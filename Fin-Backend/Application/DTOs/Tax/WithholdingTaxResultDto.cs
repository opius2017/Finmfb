namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for withholding tax calculation result
    /// </summary>
    public class WithholdingTaxResultDto
    {
        /// <summary>
        /// Applied WHT rate percentage
        /// </summary>
        public decimal AppliedRate { get; set; }
        
        /// <summary>
        /// Gross amount subject to WHT
        /// </summary>
        public decimal GrossAmount { get; set; }
        
        /// <summary>
        /// Calculated WHT amount to withhold
        /// </summary>
        public decimal WithholdingAmount { get; set; }
        
        /// <summary>
        /// Net amount after withholding tax
        /// </summary>
        public decimal NetAmount { get; set; }
        
        /// <summary>
        /// Tax type ID used (typically WHT)
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (typically WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Specific WHT tax rate ID used
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Type of income for WHT
        /// </summary>
        public string IncomeType { get; set; }
        
        /// <summary>
        /// Liability account ID for recording WHT
        /// </summary>
        public string WhtLiabilityAccountId { get; set; }
        
        /// <summary>
        /// Calculation notes or messages
        /// </summary>
        public string Notes { get; set; }
    }
}
