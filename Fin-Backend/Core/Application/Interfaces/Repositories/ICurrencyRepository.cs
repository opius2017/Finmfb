using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Currency;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for currency and exchange rate operations
    /// </summary>
    public interface ICurrencyRepository
    {
        /// <summary>
        /// Gets all currencies in the system
        /// </summary>
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a currency by its ISO code
        /// </summary>
        Task<Currency> GetCurrencyByCodeAsync(string currencyCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new currency to the system
        /// </summary>
        Task<Currency> AddCurrencyAsync(Currency currency, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing currency
        /// </summary>
        Task<Currency> UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current base currency for the system
        /// </summary>
        Task<Currency> GetBaseCurrencyAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a currency exists
        /// </summary>
        Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active exchange rates for a specific date
        /// </summary>
        Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the exchange rate between two currencies for a specific date
        /// </summary>
        Task<ExchangeRate> GetExchangeRateAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime date,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets exchange rate history for a currency pair over a date range
        /// </summary>
        Task<IEnumerable<ExchangeRate>> GetExchangeRateHistoryAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new exchange rate
        /// </summary>
        Task<ExchangeRate> AddExchangeRateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing exchange rate
        /// </summary>
        Task<ExchangeRate> UpdateExchangeRateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all account balances in foreign currencies
        /// </summary>
        Task<IEnumerable<ForeignCurrencyBalance>> GetForeignCurrencyBalancesAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a currency revaluation transaction
        /// </summary>
        Task<CurrencyRevaluation> AddCurrencyRevaluationAsync(
            CurrencyRevaluation revaluation, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets currency revaluation history for a specific period
        /// </summary>
        Task<IEnumerable<CurrencyRevaluation>> GetCurrencyRevaluationsForPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
    }
}
