namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for social login initiation result
    /// </summary>
    public class SocialLoginInitiationResult
    {
        /// <summary>
        /// Gets or sets the authorization URL
        /// </summary>
        public string AuthorizationUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the state parameter
        /// </summary>
        public string State { get; set; }
    }
}
