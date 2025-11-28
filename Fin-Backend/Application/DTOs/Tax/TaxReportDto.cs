using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax report result
    /// </summary>
    public class TaxReportDto
    {
        /// <summary>
        /// Report title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Report period description
        /// </summary>
        public string PeriodDescription { get; set; }
        
        /// <summary>
        /// Start date of the report period
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the report period
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Date the report was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
        
        /// <summary>
        /// Summaries by tax type
        /// </summary>
        public List<TaxReportSummaryDto> Summaries { get; set; } = new List<TaxReportSummaryDto>();
        
        /// <summary>
        /// Details of tax transactions
        /// </summary>
        public List<TaxTransactionDto> Details { get; set; } = new List<TaxTransactionDto>();
        
        /// <summary>
        /// Total taxable amount across all tax types
        /// </summary>
        public decimal TotalTaxableAmount { get; set; }
        
        /// <summary>
        /// Total tax amount across all tax types
        /// </summary>
        public decimal TotalTaxAmount { get; set; }
        
        /// <summary>
        /// Total settled tax amount
        /// </summary>
        public decimal TotalSettledAmount { get; set; }
        
        /// <summary>
        /// Total outstanding tax amount
        /// </summary>
        public decimal TotalOutstandingAmount { get; set; }
    }
}
