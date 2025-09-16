using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.Account
{
    /// <summary>
    /// Account creation request data transfer object
    /// </summary>
    public class AccountCreationRequestDto
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        /// <example>c1d2e3f4-g5h6-7i8j-9k0l-m1n2o3p4q5r6</example>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the account name
        /// </summary>
        /// <example>My Savings Account</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the account type
        /// </summary>
        /// <example>Savings</example>
        [Required]
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
    }
}