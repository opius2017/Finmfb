namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for authentication request
    /// </summary>
    public class AuthRequest
    {
        /// <summary>
        /// Gets or sets the username or email
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets whether to remember the device
        /// </summary>
        public bool RememberDevice { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
    }
}
