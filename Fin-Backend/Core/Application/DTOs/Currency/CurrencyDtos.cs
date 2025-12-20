using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for currency information
    /// </summary>
    public class CurrencyDto
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
    }

    /// <summary>
    /// DTO for creating a new currency
    /// </summary>
    public class CreateCurrencyDto
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
    }

    /// <summary>
    /// DTO for updating an existing currency
    /// </summary>
    public class UpdateCurrencyDto
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
    }

    /// <summary>
    /// DTO for exchange rate information
    /// </summary>
    public class ExchangeRateDto
    {
        /// <summary>
        /// Exchange rate ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
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
        /// Inverse rate (how much of FromCurrency equals 1 unit of ToCurrency)
        /// </summary>
        public decimal InverseRate { get; set; }
        
        /// <summary>
        /// Source of the exchange rate (e.g., "CBN", "Manual", "API")
        /// </summary>
        public string Source { get; set; } = string.Empty;
        
        /// <summary>
        /// When the exchange rate was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for creating a new exchange rate
    /// </summary>
    public class CreateExchangeRateDto
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
    }

    /// <summary>
    /// DTO for updating an existing exchange rate
    /// </summary>
    public class UpdateExchangeRateDto
    {
        /// <summary>
        /// Exchange rate ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
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
    }

    /// <summary>
    /// DTO for currency conversion results
    /// </summary>
    public class ConversionResultDto
    {
        /// <summary>
        /// Original amount in source currency
        /// </summary>
        public decimal OriginalAmount { get; set; }
        
        /// <summary>
        /// Source currency code
        /// </summary>
        public string FromCurrencyCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Converted amount in target currency
        /// </summary>
        public decimal ConvertedAmount { get; set; }
        
        /// <summary>
        /// Target currency code
        /// </summary>
        public string ToCurrencyCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Exchange rate used for conversion
        /// </summary>
        public decimal ExchangeRate { get; set; }
        
        /// <summary>
        /// Date of the exchange rate used
        /// </summary>
        public DateTime ConversionDate { get; set; }
    }

    /// <summary>
    /// DTO for foreign currency revaluation results
    /// </summary>
    public class ForeignCurrencyRevaluationResultDto
    {
        /// <summary>
        /// ID of the financial period for the revaluation
        /// </summary>
        public string FinancialPeriodId { get; set; } = string.Empty;
        
        /// <summary>
        /// Revaluation date
        /// </summary>
        public DateTime RevaluationDate { get; set; }
        
        /// <summary>
        /// Base currency code
        /// </summary>
        public string BaseCurrencyCode { get; set; } = string.Empty;
        
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
        public string JournalEntryId { get; set; } = string.Empty;
        
        /// <summary>
        /// Detailed results by currency
        /// </summary>
        public List<CurrencyRevaluationDetailDto> Details { get; set; } = new List<CurrencyRevaluationDetailDto>();
    }

    /// <summary>
    /// DTO for currency revaluation details
    /// </summary>
    public class CurrencyRevaluationDetailDto
    {
        /// <summary>
        /// Foreign currency code
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

    /// <summary>
    /// DTO for revaluated account information
    /// </summary>
    public class RevaluatedAccountDto
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
        /// Foreign currency balance
        /// </summary>
        public decimal ForeignCurrencyBalance { get; set; }
        
        /// <summary>
        /// Previous base currency value
        /// </summary>
        public decimal PreviousBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Current base currency value
        /// </summary>
        public decimal CurrentBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Unrealized gain or loss amount (positive for gain, negative for loss)
        /// </summary>
        public decimal RevaluationEffect { get; set; }
    }
}
