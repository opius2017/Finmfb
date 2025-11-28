using System;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax calculation result
    /// </summary>
    public class TaxCalculationResultDto
    {
        /// <summary>
        /// Tax type ID used for calculation
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Tax type name
        /// </summary>
        public string TaxTypeName { get; set; }
        
        /// <summary>
        /// Tax rate ID used for calculation
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Applied tax rate percentage
        /// </summary>
        public decimal AppliedRate { get; set; }
        
        /// <summary>
        /// Original taxable amount
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Calculated tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Total amount including tax
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Whether the item is exempt from this tax
        /// </summary>
        public bool IsExempt { get; set; }
        
        /// <summary>
        /// Reason for exemption, if applicable
        /// </summary>
        public string ExemptionReason { get; set; }
        
        /// <summary>
        /// Transaction date used for tax calculation
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Account ID for recording this tax
        /// </summary>
        public string TaxAccountId { get; set; }
        
        /// <summary>
        /// Calculation notes or messages
        /// </summary>
        public string Notes { get; set; }
    }
}
