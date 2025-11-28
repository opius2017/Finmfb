using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for currency revaluation details
    /// </summary>
    public class CurrencyRevaluationDetailDto
    {
        /// <summary>
        /// Foreign currency code
        /// </summary>
        public string CurrencyCode { get; set; }
        
        /// <summary>
        /// Previous exchange rate
        /// </summary>
        public decimal PreviousRate { get; set; }
        
        /// <summary>
        /// Current exchange rate
        /// </summary>
        public decimal CurrentRate { get; set; }
        
        /// <summary>
        /// Total balance in foreign currency
        /// </summary>
        public decimal ForeignCurrencyBalance { get; set; }
        
        /// <summary>
        /// Previous balance in base currency
        /// </summary>
        public decimal PreviousBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Current balance in base currency
        /// </summary>
        public decimal CurrentBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Unrealized gain amount
        /// </summary>
        public decimal UnrealizedGain { get; set; }
        
        /// <summary>
        /// Unrealized loss amount
        /// </summary>
        public decimal UnrealizedLoss { get; set; }
        
        /// <summary>
        /// Accounts affected by the revaluation
        /// </summary>
        public List<RevaluatedAccountDto> AffectedAccounts { get; set; } = new List<RevaluatedAccountDto>();
    }
}
