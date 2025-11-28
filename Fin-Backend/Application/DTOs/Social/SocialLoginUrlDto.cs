namespace FinTech.Core.Application.DTOs.Social
{
    /// <summary>
    /// Social login authorization URL DTO
    /// </summary>
    public class SocialLoginUrlDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Provider { get; set; }
        
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
