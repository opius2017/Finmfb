namespace FinTech.Core.Application.DTOs.Social
{
    /// <summary>
    /// Request to link a social account
    /// </summary>
    public class SocialLinkRequest
    {
        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        public string AccessToken { get; set; }
        
        /// <summary>
        /// Gets or sets the token secret (required for some providers like Twitter)
        /// </summary>
        public string TokenSecret { get; set; }
    }
}
