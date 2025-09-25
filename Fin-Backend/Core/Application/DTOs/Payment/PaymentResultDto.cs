using System;

namespace FinTech.Core.Application.DTOs.Payment
{
    /// <summary>
    /// Payment result data transfer object
    /// </summary>
    public class PaymentResultDto
    {
        /// <summary>
        /// Gets or sets the payment ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        /// <example>Successful</example>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the transaction reference
        /// </summary>
        /// <example>PMT-20250916-123456</example>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Gets or sets the processed date and time
        /// </summary>
        /// <example>2025-09-16T10:30:00Z</example>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// Gets or sets the payment amount
        /// </summary>
        /// <example>1000.00</example>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the result message
        /// </summary>
        /// <example>Payment processed successfully</example>
        public string Message { get; set; }
    }
}
