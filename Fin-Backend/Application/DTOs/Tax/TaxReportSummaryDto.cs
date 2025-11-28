namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax report summary by tax type
    /// </summary>
    public class TaxReportSummaryDto
    {
        /// <summary>
        /// Tax type ID
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
        /// Total taxable amount for this tax type
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Total tax amount for this tax type
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Total settled amount for this tax type
        /// </summary>
        public decimal SettledAmount { get; set; }
        
        /// <summary>
        /// Total outstanding amount for this tax type
        /// </summary>
        public decimal OutstandingAmount { get; set; }
        
        /// <summary>
        /// Number of transactions for this tax type
        /// </summary>
        public int TransactionCount { get; set; }
    }
}
