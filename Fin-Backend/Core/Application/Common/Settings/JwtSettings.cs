namespace Fin_Backend.Application.Common.Settings
{
    /// <summary>
    /// JWT settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Gets or sets the issuer
        /// </summary>
        public string Issuer { get; set; }
        
        /// <summary>
        /// Gets or sets the audience
        /// </summary>
        public string Audience { get; set; }
        
        /// <summary>
        /// Gets or sets the secret
        /// </summary>
        public string Secret { get; set; }
        
        /// <summary>
        /// Gets or sets the expiration in minutes
        /// </summary>
        public int ExpirationInMinutes { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token expiration in days
        /// </summary>
        public int RefreshTokenExpirationInDays { get; set; }
    }
}
