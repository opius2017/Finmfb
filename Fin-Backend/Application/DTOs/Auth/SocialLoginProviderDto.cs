using System;

namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for social login provider information
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
        /// Gets or sets whether the provider is enabled
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
