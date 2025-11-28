namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for registration request
    /// </summary>
    public class RegistrationRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the password confirmation
        /// </summary>
        public string ConfirmPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
