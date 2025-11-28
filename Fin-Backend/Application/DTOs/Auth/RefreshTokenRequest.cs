namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for refresh token request
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
    }
}
