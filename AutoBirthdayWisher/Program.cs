// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AutoBirthdayWisher
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using AutoBirthdayWisher.Models;

    using Microsoft.Exchange.WebServices.Data;
    using Microsoft.SharePoint.Client;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        public static ExchangeService Service { get; set; }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            var dateToday = DateTime.UtcNow.AddHours(5).AddMinutes(30);
            List<Record> todaysBirthdays = GetTodaysBirthdays(dateToday);
            int photoId = GetRandomPhotoId();
            SendEmail(todaysBirthdays, photoId);
        }

        /// <summary>
        /// The send email.
        /// </summary>
        /// <param name="todaysBirthdays">
        /// The todays birthdays.
        /// </param>
        /// <param name="photo">
        /// The photo.
        /// </param>
        private static void SendEmail(List<Record> todaysBirthdays, int photo)
        {
            InitializeExchangeService();

            foreach (var item in todaysBirthdays)
            {
                if (item.IsActive == true)
                {
                    EmailMessage email = new EmailMessage(Service);

                    email.From = ConfigurationManager.AppSettings["From"];

                    email.Subject = "Wish you a very Happy  Birthday";

                    string html = @"<html>
                     <head>
                     </head>
                     <body>
                        <h3 style='font-family:Segoe UI;'>From the entire Dynamics CRM team , wish you a very Happy Birthday "
                                  + item.Name + "!";
                    html +=
                        @"</h3> <img height=600 id=""1"" src=""cid:HappyBirthday.jpg""> <br/><br/> <p style='font-family:Segoe UI;'>Love,<br/>

                    IGD CRM Movers and Shakers</p><br/><img height=80 id=""2"" src=""cid:logo.jpg"">
                        </body> </html>";

                    email.Body = new MessageBody(BodyType.HTML, html);
                    email.ToRecipients.Add("igdcrm@microsoft.com");
                    email.CcRecipients.Add(item.Alias + "@microsoft.com");

                    // Add the attachment to the local copy of the email message.
                    string file = @"..\..\Resources\_" + photo + ".jpg";
                    email.Attachments.AddFileAttachment("HappyBirthday.jpg", file);
                    string file1 = @"..\..\Resources\logo.jpg";
                    email.Attachments.AddFileAttachment("logo.jpg", file1);
                    email.Attachments[0].IsInline = true;
                    email.Attachments[0].ContentId = "HappyBirthday.jpg";
                    email.Attachments[1].IsInline = true;
                    email.Attachments[1].ContentId = "logo.jpg";

                    // Save a copy of the email, add the attachment, and then send the email. This method results in three calls to EWS.
                    email.SendAndSaveCopy();
                    Console.WriteLine(@"Email sent successfully to {0}@microsoft.com", item.Alias);
                }
            }
        }

        /// <summary>
        /// The initialize exchange service.
        /// </summary>
        public static void InitializeExchangeService()
        {
            // Create the binding.
            Service = new ExchangeService();

            // Set the credentials for the on-premises server.
            Service.Credentials = new WebCredentials(
                ConfigurationManager.AppSettings["Username"], 
                ConfigurationManager.AppSettings["Password"]);

            // Set the URL
            // Service.Url = new Uri(ConfigurationManager.AppSettings["ExchangeServiceUrl"]);
            Service.TraceEnabled = true;
            Service.TraceFlags = TraceFlags.All;
            Service.AutodiscoverUrl(ConfigurationManager.AppSettings["Username"], RedirectionUrlValidationCallback);
        }

        /// <summary>
        /// The redirection url validation callback.
        /// </summary>
        /// <param name="redirectionUrl">
        /// The redirection url.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// The get random photo.
        /// </summary>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        private static int GetRandomPhotoId()
        {
            Random random = new Random();
            int picNumber = random.Next(1, 8);
            return picNumber;
        }

        /// <summary>
        /// The get todays birthdays.
        /// </summary>
        /// <param name="dateToday">
        /// The date today.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<Record> GetTodaysBirthdays(DateTime dateToday)
        {
            List<Record> birthdaysToday = new List<Record>();

            var username = ConfigurationManager.AppSettings["Username"];
            var password = ConfigurationManager.AppSettings["Password"];
            var sharePointSiteUrl = ConfigurationManager.AppSettings["SharePointSiteUrl"];
            var sharePointListName = ConfigurationManager.AppSettings["SharePointListName"];

            using (ClientContext ctx = new ClientContext(sharePointSiteUrl))
            {
                ctx.Credentials = new SharePointOnlineCredentials(username, Common.GetSecureString(password));

                var list = ctx.Web.Lists.GetByTitle(sharePointListName);

                var query = new CamlQuery()
                                {
                                    ViewXml =
                                        "<View><Query><Where><And><Eq><FieldRef Name='Month' /><Value Type='Number'>"
                                        + dateToday.Month.ToString()
                                        + "</Value></Eq><Eq><FieldRef Name='Date' /><Value Type='Number'>"
                                        + dateToday.Day.ToString()
                                        + "</Value></Eq></And></Where></Query><ViewFields><FieldRef Name = 'Title'/><FieldRef Name ='Alias'/><FieldRef Name = 'Month'/><FieldRef Name ='Date'/><FieldRef Name='IsActive'/></ViewFields></View>"
                                };
                var listItemCollection = list.GetItems(query);
                ctx.Load(listItemCollection);
                ctx.ExecuteQuery();

                foreach (var item in listItemCollection)
                {
                    Record birthday = new Record();

                    birthday.Name = item["Title"].ToString();
                    birthday.Alias = item["Alias"].ToString();
                    birthday.Month = Convert.ToInt32(item["Month"].ToString());
                    birthday.Date = Convert.ToInt32(item["Date"].ToString());
                    birthday.IsActive = Convert.ToBoolean(item["IsActive"].ToString());

                    birthdaysToday.Add(birthday);
                }
            }

            return birthdaysToday;
        }
    }
}