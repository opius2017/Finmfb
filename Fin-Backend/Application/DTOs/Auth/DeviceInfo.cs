namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for device information
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the client IP address
        /// </summary>
        public string ClientIp { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public string DeviceId { get; set; }
        
        /// <summary>
        /// Gets or sets the browser information
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the browser version
        /// </summary>
        public string BrowserVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string DeviceName { get; set; }
        
        /// <summary>
        /// Gets or sets the device type
        /// </summary>
        public string DeviceType { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }
    }
}
