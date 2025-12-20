using System;

namespace FinTech.Core.Application.DTOs.Account
{
    /// <summary>
    /// Account creation result data transfer object
    /// </summary>
    public class AccountCreationResultDto
    {
        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account number
        /// </summary>
        /// <example>12345678</example>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account name
        /// </summary>
        /// <example>My Savings Account</example>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account type
        /// </summary>
        /// <example>Savings</example>
        public string AccountType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account status
        /// </summary>
        /// <example>Active</example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the open date
        /// </summary>
        /// <example>2025-09-16T10:30:00Z</example>
        public DateTime OpenDate { get; set; }

        /// <summary>
        /// Gets or sets the result message
        /// </summary>
        /// <example>Account created successfully</example>
        public string Message { get; set; } = string.Empty;
    }
}
