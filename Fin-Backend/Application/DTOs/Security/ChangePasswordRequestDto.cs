namespace FinTech.Core.Application.DTOs.Security
{
    /// <summary>
    /// Request to change password
    /// </summary>
    public class ChangePasswordRequestDto
    {
        /// <summary>
        /// Gets or sets the current password
        /// </summary>
        public string CurrentPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        public string NewPassword { get; set; }
    }
}
