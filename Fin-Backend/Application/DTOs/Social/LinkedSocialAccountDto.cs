using System;

namespace FinTech.Core.Application.DTOs.Social
{
    /// <summary>
    /// Linked social account DTO
    /// </summary>
    public class LinkedSocialAccountDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Gets or sets the provider's user ID
        /// </summary>
        public string ProviderUserId { get; set; }
        
        /// <summary>
        /// Gets or sets the display name from the provider
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the profile picture URL
        /// </summary>
        public string ProfilePictureUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the account was linked
        /// </summary>
        public DateTime LinkedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the token was last refreshed
        /// </summary>
        public DateTime? LastRefreshed { get; set; }
    }
}
