using System;

namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for linked social account information
    /// </summary>
    public class LinkedSocialAccountDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Gets or sets the provider display name
        /// </summary>
        public string ProviderDisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the external user ID
        /// </summary>
        public string ExternalUserId { get; set; }
        
        /// <summary>
        /// Gets or sets the external user name or email
        /// </summary>
        public string ExternalUserName { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the account was linked
        /// </summary>
        public DateTime LinkedDate { get; set; }
    }
}
