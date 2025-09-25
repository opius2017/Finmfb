namespace FinTech.Core.Application.DTOs.Social
{
    /// <summary>
    /// Social login provider DTO
    /// </summary>
    public class SocialLoginProviderDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the provider display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the provider icon URL
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the color associated with the provider (for UI)
        /// </summary>
        public string Color { get; set; }
        
        /// <summary>
        /// Gets or sets whether the provider is enabled
        /// </summary>
        public bool Enabled { get; set; }
    }
}
