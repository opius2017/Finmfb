using System;

namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for authentication history item
    /// </summary>
    public class AuthHistoryItem
    {
        /// <summary>
        /// Gets or sets the login date
        /// </summary>
        public DateTime LoginDate { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the browser
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string Device { get; set; }
        
        /// <summary>
        /// Gets or sets the approximate location
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Gets or sets whether the login was successful
        /// </summary>
        public bool Successful { get; set; }
        
        /// <summary>
        /// Gets or sets whether MFA was used
        /// </summary>
        public bool UsedMfa { get; set; }
    }
}
