using System;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for withholding tax calculation request
    /// </summary>
    public class WithholdingTaxRequestDto
    {
        /// <summary>
        /// Gross amount subject to withholding tax
        /// </summary>
        public decimal GrossAmount { get; set; }
        
        /// <summary>
        /// Type of income for WHT (e.g., professional-services, rent, dividend)
        /// </summary>
        public string IncomeType { get; set; }
        
        /// <summary>
        /// ID of the vendor/payee
        /// </summary>
        public string VendorId { get; set; }
        
        /// <summary>
        /// Whether the vendor is a resident entity
        /// </summary>
        public bool IsResident { get; set; }
        
        /// <summary>
        /// Optional tax identification number
        /// </summary>
        public string TaxIdentificationNumber { get; set; }
        
        /// <summary>
        /// Transaction date for determining applicable rate
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Optional reference to related transaction
        /// </summary>
        public string TransactionReference { get; set; }
    }
}
