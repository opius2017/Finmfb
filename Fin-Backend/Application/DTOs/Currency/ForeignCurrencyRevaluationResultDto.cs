using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for foreign currency revaluation results
    /// </summary>
    public class ForeignCurrencyRevaluationResultDto
    {
        /// <summary>
        /// ID of the financial period for the revaluation
        /// </summary>
        public string FinancialPeriodId { get; set; }
        
        /// <summary>
        /// Revaluation date
        /// </summary>
        public DateTime RevaluationDate { get; set; }
        
        /// <summary>
        /// Base currency code
        /// </summary>
        public string BaseCurrencyCode { get; set; }
        
        /// <summary>
        /// Total unrealized gain amount in base currency
        /// </summary>
        public decimal TotalUnrealizedGain { get; set; }
        
        /// <summary>
        /// Total unrealized loss amount in base currency
        /// </summary>
        public decimal TotalUnrealizedLoss { get; set; }
        
        /// <summary>
        /// Net effect of the revaluation in base currency
        /// </summary>
        public decimal NetEffect { get; set; }
        
        /// <summary>
        /// Journal entry ID for the revaluation if posted
        /// </summary>
        public string JournalEntryId { get; set; }
        
        /// <summary>
        /// Detailed results by currency
        /// </summary>
        public List<CurrencyRevaluationDetailDto> Details { get; set; } = new List<CurrencyRevaluationDetailDto>();
    }
}
