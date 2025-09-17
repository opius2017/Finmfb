using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.DTOs.Currency;

namespace FinTech.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for currency management and foreign exchange operations
    /// </summary>
    public interface ICurrencyService
    {
        /// <summary>
        /// Gets a list of all active currencies in the system
        /// </summary>
        Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets currency by its ISO code
        /// </summary>
        Task<CurrencyDto> GetCurrencyByCodeAsync(string currencyCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new currency to the system
        /// </summary>
        Task<CurrencyDto> AddCurrencyAsync(CreateCurrencyDto createCurrencyDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing currency
        /// </summary>
        Task<CurrencyDto> UpdateCurrencyAsync(UpdateCurrencyDto updateCurrencyDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sets a currency as active or inactive
        /// </summary>
        Task<bool> SetCurrencyStatusAsync(string currencyCode, bool isActive, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current exchange rate between two currencies
        /// </summary>
        Task<ExchangeRateDto> GetExchangeRateAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the exchange rate between two currencies for a specific date
        /// </summary>
        Task<ExchangeRateDto> GetHistoricalExchangeRateAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime rateDate,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets exchange rate history for a currency pair over a date range
        /// </summary>
        Task<IEnumerable<ExchangeRateDto>> GetExchangeRateHistoryAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates the exchange rate between two currencies
        /// </summary>
        Task<ExchangeRateDto> UpdateExchangeRateAsync(
            UpdateExchangeRateDto updateExchangeRateDto, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new exchange rate between two currencies
        /// </summary>
        Task<ExchangeRateDto> AddExchangeRateAsync(
            CreateExchangeRateDto createExchangeRateDto, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Converts an amount from one currency to another
        /// </summary>
        Task<ConversionResultDto> ConvertCurrencyAsync(
            decimal amount, 
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime? conversionDate = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Revalues foreign currency balances for period end processing
        /// </summary>
        Task<ForeignCurrencyRevaluationResultDto> RevalueForeignCurrencyBalancesAsync(
            string financialPeriodId,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the base currency for the system
        /// </summary>
        Task<CurrencyDto> GetBaseCurrencyAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sets the base currency for the system
        /// </summary>
        Task<bool> SetBaseCurrencyAsync(string currencyCode, CancellationToken cancellationToken = default);
    }
}