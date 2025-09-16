using System;

namespace FinTech.Core.Application.DTOs.Security
{
    /// <summary>
    /// Security activity DTO
    /// </summary>
    public class SecurityActivityDto
    {
        /// <summary>
        /// Gets or sets the activity ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the activity type
        /// </summary>
        public string ActivityType { get; set; }
        
        /// <summary>
        /// Gets or sets the activity description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public string Device { get; set; }
        
        /// <summary>
        /// Gets or sets the browser information
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the location
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is suspicious activity
        /// </summary>
        public bool IsSuspicious { get; set; }
    }
}