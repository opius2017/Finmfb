using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.Payment
{
    /// <summary>
    /// Payment request data transfer object
    /// </summary>
    public class PaymentRequestDto
    {
        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        [Required]
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the payment amount
        /// </summary>
        /// <example>1000.00</example>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the payment method
        /// </summary>
        /// <example>CreditCard</example>
        [Required]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the payment description
        /// </summary>
        /// <example>Monthly bill payment</example>
        public string Description { get; set; }
    }
}