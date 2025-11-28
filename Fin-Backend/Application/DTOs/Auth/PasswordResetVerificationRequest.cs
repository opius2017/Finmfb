namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for password reset verification request
    /// </summary>
    public class PasswordResetVerificationRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the token
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        public string NewPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the new password confirmation
        /// </summary>
        public string ConfirmPassword { get; set; }
    }
}
