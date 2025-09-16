using System;

namespace FinTech.Core.Application.DTOs.Transaction
{
    /// <summary>
    /// Transaction information
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// Unique identifier for the transaction
        /// </summary>
        /// <example>t1u2v3w4-x5y6-7z8a-9b0c-d1e2f3g4h5i6</example>
        public string TransactionId { get; set; }

        /// <summary>
        /// Account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; }

        /// <summary>
        /// Type of transaction
        /// </summary>
        /// <example>Credit</example>
        public string TransactionType { get; set; }

        /// <summary>
        /// Transaction amount
        /// </summary>
        /// <example>500.00</example>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Transaction description
        /// </summary>
        /// <example>Salary payment</example>
        public string Description { get; set; }

        /// <summary>
        /// Date and time of the transaction
        /// </summary>
        /// <example>2025-09-15T14:30:00Z</example>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Transaction status
        /// </summary>
        /// <example>Completed</example>
        public string Status { get; set; }
    }
}