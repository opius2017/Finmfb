using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for authentication result
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// Gets or sets whether the authentication was successful
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if authentication failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the JWT token
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the token expiration date
        /// </summary>
        public DateTime TokenExpiration { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the user email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        public List<string> Roles { get; set; }
        
        /// <summary>
        /// Gets or sets whether MFA is required
        /// </summary>
        public bool RequiresMfa { get; set; }
        
        /// <summary>
        /// Gets or sets the MFA challenge ID if MFA is required
        /// </summary>
        public string MfaChallengeId { get; set; }
    }
}
