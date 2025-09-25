using System;

namespace FinTech.Core.Application.DTOs.Security
{
    /// <summary>
    /// Active session DTO
    /// </summary>
    public class ActiveSessionDto
    {
        /// <summary>
        /// Gets or sets the session ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public string Device { get; set; }
        
        /// <summary>
        /// Gets or sets the browser information
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the login date
        /// </summary>
        public DateTime LoginDate { get; set; }
        
        /// <summary>
        /// Gets or sets the approximate location
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is the current session
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}
