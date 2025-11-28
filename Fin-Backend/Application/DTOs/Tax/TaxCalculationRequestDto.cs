using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax calculation request
    /// </summary>
    public class TaxCalculationRequestDto
    {
        /// <summary>
        /// Tax type ID to calculate
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Specific tax rate ID to use (optional, will use applicable rate if not specified)
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Taxable amount before tax
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Category of the taxable item (e.g., goods, services)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Transaction date for determining applicable tax rate
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Whether the input amount is tax inclusive
        /// </summary>
        public bool IsTaxInclusive { get; set; }
        
        /// <summary>
        /// Optional customer/vendor ID for exemption checking
        /// </summary>
        public string PartyId { get; set; }
        
        /// <summary>
        /// Optional reference to related transaction
        /// </summary>
        public string TransactionReference { get; set; }
        
        /// <summary>
        /// Optional additional information for tax calculation
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
    }
}
