using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for registration result
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Gets or sets whether the registration was successful
        /// </summary>
        public bool Succeeded { get; set; }
        
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
        /// Gets or sets the error messages if registration failed
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
