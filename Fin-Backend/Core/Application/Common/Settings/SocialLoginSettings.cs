using System.Collections.Generic;

namespace Fin_Backend.Application.Common.Settings
{
    /// <summary>
    /// Social login settings
    /// </summary>
    public class SocialLoginSettings
    {
        /// <summary>
        /// Gets or sets the list of social login providers
        /// </summary>
        public List<SocialLoginProviderSettings> Providers { get; set; } = new List<SocialLoginProviderSettings>();
    }

    /// <summary>
    /// Social login provider settings
    /// </summary>
    public class SocialLoginProviderSettings
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the client ID
        /// </summary>
        public string ClientId { get; set; }
        
        /// <summary>
        /// Gets or sets the client secret
        /// </summary>
        public string ClientSecret { get; set; }
        
        /// <summary>
        /// Gets or sets the authorization endpoint
        /// </summary>
        public string AuthorizationEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets the token endpoint
        /// </summary>
        public string TokenEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets the user information endpoint
        /// </summary>
        public string UserInfoEndpoint { get; set; }
        
        /// <summary>
        /// Gets or sets the redirect URI
        /// </summary>
        public string RedirectUri { get; set; }
        
        /// <summary>
        /// Gets or sets the scope
        /// </summary>
        public string Scope { get; set; }
    }
}
