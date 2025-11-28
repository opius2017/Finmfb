using System;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax report request
    /// </summary>
    public class TaxReportRequestDto
    {
        /// <summary>
        /// Tax type ID to report on (null for all tax types)
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Start date for the report period
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date for the report period
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Financial period ID (alternative to date range)
        /// </summary>
        public string FinancialPeriodId { get; set; }
        
        /// <summary>
        /// Whether to include only settled transactions
        /// </summary>
        public bool? IsSettled { get; set; }
        
        /// <summary>
        /// How to group the report data (by date, tax type, etc.)
        /// </summary>
        public string GroupBy { get; set; }
        
        /// <summary>
        /// Report format (detailed or summary)
        /// </summary>
        public string Format { get; set; }
    }
}
