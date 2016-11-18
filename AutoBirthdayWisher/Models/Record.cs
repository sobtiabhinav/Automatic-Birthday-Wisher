// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Record.cs" company="">
//   
// </copyright>
// <summary>
//   The record.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace AutoBirthdayWisher.Models
{
    /// <summary>
    /// The record.
    /// </summary>
    public class Record
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public int Date { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}