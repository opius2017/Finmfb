using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Currency
{
    /// <summary>
    /// Represents a currency in the system
    /// </summary>
    public class Currency : BaseEntity
    {
        /// <summary>
        /// ISO 4217 currency code (e.g., NGN, USD)
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Currency name (e.g., Nigerian Naira, US Dollar)
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Currency symbol (e.g., ₦, $)
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of decimal places typically used (e.g., 2 for NGN, USD)
        /// </summary>
        public int DecimalPlaces { get; set; }
        
        /// <summary>
        /// Whether the currency is currently active in the system
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Whether this is the system's base currency
        /// </summary>
        public bool IsBaseCurrency { get; set; }
        
        /// <summary>
        /// Navigation property for exchange rates where this currency is the source
        /// </summary>
        public virtual ICollection<ExchangeRate> FromExchangeRates { get; set; } = new List<ExchangeRate>();
        
        /// <summary>
        /// Navigation property for exchange rates where this currency is the target
        /// </summary>
        public virtual ICollection<ExchangeRate> ToExchangeRates { get; set; } = new List<ExchangeRate>();
    }

    /// <summary>
    /// Represents an exchange rate between two currencies at a specific date
    /// </summary>
    public class ExchangeRate : BaseEntity
    {
        /// <summary>
        /// From currency code
        /// </summary>
        public string FromCurrencyCode { get; set; } = string.Empty;
        
        /// <summary>
        /// To currency code
        /// </summary>
        public string ToCurrencyCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Exchange rate date
        /// </summary>
        public DateTime RateDate { get; set; }
        
        /// <summary>
        /// Exchange rate value (how much of ToCurrency equals 1 unit of FromCurrency)
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Source of the exchange rate (e.g., "CBN", "Manual", "API")
        /// </summary>
        public string Source { get; set; } = string.Empty;
        
        /// <summary>
        /// Navigation property for the source currency
        /// </summary>
        public virtual Currency? FromCurrency { get; set; }
        
        /// <summary>
        /// Navigation property for the target currency
        /// </summary>
        public virtual Currency? ToCurrency { get; set; }
    }

    /// <summary>
    /// Represents a foreign currency balance for an account at a specific date
    /// </summary>
    public class ForeignCurrencyBalance
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public string AccountId { get; set; } = string.Empty;
        
        /// <summary>
        /// Account number
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; set; } = string.Empty;
        
        /// <summary>
        /// Currency code
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Balance in foreign currency
        /// </summary>
        public decimal ForeignAmount { get; set; }
        
        /// <summary>
        /// Balance in base currency
        /// </summary>
        public decimal BaseAmount { get; set; }
        
        /// <summary>
        /// Last revaluation date
        /// </summary>
        public DateTime? LastRevaluationDate { get; set; }
        
        /// <summary>
        /// Exchange rate used for the last revaluation
        /// </summary>
        public decimal? LastRevaluationRate { get; set; }
        
        /// <summary>
        /// Financial period ID
        /// </summary>
        public string FinancialPeriodId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a currency revaluation transaction
    /// </summary>
    public class CurrencyRevaluation : BaseEntity
    {
        /// <summary>
        /// Financial period ID
        /// </summary>
        public string FinancialPeriodId { get; set; } = string.Empty; // Added init
        
        /// <summary>
        /// Revaluation date
        /// </summary>
        public DateTime RevaluationDate { get; set; }
        
        /// <summary>
        /// Currency code
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Previous exchange rate
        /// </summary>
        public decimal PreviousRate { get; set; }
        
        /// <summary>
        /// Current exchange rate
        /// </summary>
        public decimal CurrentRate { get; set; }
        
        /// <summary>
        /// Total foreign currency amount revaluated
        /// </summary>
        public decimal ForeignCurrencyAmount { get; set; }
        
        /// <summary>
        /// Previous base currency value
        /// </summary>
        public decimal PreviousBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Current base currency value
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
        /// Journal entry ID for the revaluation
        /// </summary>
        public string JournalEntryId { get; set; } = string.Empty;
        
        /// <summary>
        /// Status of the revaluation
        /// </summary>
        public RevaluationStatus Status { get; set; }
        
        /// <summary>
        /// Processed by user ID
        /// </summary>
        public string ProcessedById { get; set; } = string.Empty;
        
        /// <summary>
        /// Processing notes
        /// </summary>
        public string Notes { get; set; } = string.Empty;
        
        /// <summary>
        /// Navigation property for revaluation details
        /// </summary>
        public virtual ICollection<CurrencyRevaluationDetail> Details { get; set; } = new List<CurrencyRevaluationDetail>();
    }

    /// <summary>
    /// Represents a detail line for a currency revaluation
    /// </summary>
    public class CurrencyRevaluationDetail : BaseEntity
    {
        /// <summary>
        /// Revaluation ID
        /// </summary>
        public string RevaluationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Account ID
        /// </summary>
        public string AccountId { get; set; } = string.Empty;
        
        /// <summary>
        /// Account number
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Foreign currency amount
        /// </summary>
        public decimal ForeignCurrencyAmount { get; set; }
        
        /// <summary>
        /// Previous base currency value
        /// </summary>
        public decimal PreviousBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Current base currency value
        /// </summary>
        public decimal CurrentBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Revaluation effect (gain or loss)
        /// </summary>
        public decimal RevaluationEffect { get; set; }
        
        /// <summary>
        /// Navigation property for the parent revaluation
        /// </summary>
        public virtual CurrencyRevaluation? Revaluation { get; set; }
    }

    /// <summary>
    /// Represents the status of a currency revaluation
    /// </summary>
    public enum RevaluationStatus
    {
        /// <summary>
        /// Revaluation is draft
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Revaluation is processed but not posted to GL
        /// </summary>
        Processed = 1,
        
        /// <summary>
        /// Revaluation is posted to GL
        /// </summary>
        Posted = 2,
        
        /// <summary>
        /// Revaluation is reversed
        /// </summary>
        Reversed = 3
    }
}
