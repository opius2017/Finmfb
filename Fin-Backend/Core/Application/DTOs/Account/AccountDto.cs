using System;

namespace FinTech.Core.Application.DTOs.Account
{
    /// <summary>
    /// Account information
    /// </summary>
    public class AccountDto
    {
        /// <summary>
        /// Unique identifier for the account
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Account number
        /// </summary>
        /// <example>1234567890</example>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Name of the account
        /// </summary>
        /// <example>Savings Account</example>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Type of account
        /// </summary>
        /// <example>Savings</example>
        public string AccountType { get; set; } = string.Empty;

        /// <summary>
        /// Current balance
        /// </summary>
        /// <example>1000.00</example>
        public decimal Balance { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Account status
        /// </summary>
        /// <example>Active</example>
        public string Status { get; set; } = string.Empty;
    }
}
