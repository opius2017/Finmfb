using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.Account
{
    /// <summary>
    /// Request to create a new account
    /// </summary>
    public class CreateAccountRequest
    {
        /// <summary>
        /// Name of the account
        /// </summary>
        /// <example>My Savings Account</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string AccountName { get; set; }

        /// <summary>
        /// Type of account
        /// </summary>
        /// <example>Savings</example>
        [Required]
        [StringLength(50)]
        public string AccountType { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
    }
}
