using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.Account
{
    /// <summary>
    /// Request to update an account
    /// </summary>
    public class UpdateAccountRequest
    {
        /// <summary>
        /// Name of the account
        /// </summary>
        /// <example>My Updated Savings Account</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string AccountName { get; set; } = string.Empty;
    }
}
