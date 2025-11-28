namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for token revocation request
    /// </summary>
    public class RevokeTokenRequest
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
