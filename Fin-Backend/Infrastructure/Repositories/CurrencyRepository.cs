using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Domain.Entities.Currency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for currency and exchange rate operations
    /// </summary>
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<CurrencyRepository> _logger;

        public CurrencyRepository(
            IApplicationDbContext dbContext,
            ILogger<CurrencyRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Currencies
                    .OrderBy(c => c.Code)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all currencies");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Currency> GetCurrencyByCodeAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Currencies
                    .FirstOrDefaultAsync(c => c.Code == currencyCode, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with code {CurrencyCode}", currencyCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Currency> AddCurrencyAsync(Currency currency, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.Currencies.Add(currency);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return currency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding currency with code {CurrencyCode}", currency.Code);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Currency> UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.Currencies.Update(currency);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return currency;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency with code {CurrencyCode}", currency.Code);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Currency> GetBaseCurrencyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Currencies
                    .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving base currency");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Currencies
                    .AnyAsync(c => c.Code == currencyCode, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if currency with code {CurrencyCode} exists", currencyCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the rates for the exact date first
                var rates = await _dbContext.ExchangeRates
                    .Where(er => er.RateDate.Date == date.Date)
                    .Include(er => er.FromCurrency)
                    .Include(er => er.ToCurrency)
                    .ToListAsync(cancellationToken);
                
                // If no rates found for the exact date, get the most recent rates before the date
                if (!rates.Any())
                {
                    // Get unique currency pairs
                    var currencyPairs = await _dbContext.ExchangeRates
                        .Select(er => new { er.FromCurrencyCode, er.ToCurrencyCode })
                        .Distinct()
                        .ToListAsync(cancellationToken);
                    
                    foreach (var pair in currencyPairs)
                    {
                        // Get the most recent rate for this pair before the given date
                        var mostRecentRate = await _dbContext.ExchangeRates
                            .Where(er => 
                                er.FromCurrencyCode == pair.FromCurrencyCode && 
                                er.ToCurrencyCode == pair.ToCurrencyCode &&
                                er.RateDate.Date <= date.Date)
                            .OrderByDescending(er => er.RateDate)
                            .Include(er => er.FromCurrency)
                            .Include(er => er.ToCurrency)
                            .FirstOrDefaultAsync(cancellationToken);
                        
                        if (mostRecentRate != null)
                        {
                            rates.Add(mostRecentRate);
                        }
                    }
                }
                
                return rates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rates for date {Date}", date);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRate> GetExchangeRateAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to get the exact rate for the date
                var rate = await _dbContext.ExchangeRates
                    .Where(er => 
                        er.FromCurrencyCode == fromCurrencyCode && 
                        er.ToCurrencyCode == toCurrencyCode &&
                        er.RateDate.Date == date.Date)
                    .OrderByDescending(er => er.RateDate)
                    .Include(er => er.FromCurrency)
                    .Include(er => er.ToCurrency)
                    .FirstOrDefaultAsync(cancellationToken);
                
                // If not found, get the most recent rate before the date
                if (rate == null)
                {
                    rate = await _dbContext.ExchangeRates
                        .Where(er => 
                            er.FromCurrencyCode == fromCurrencyCode && 
                            er.ToCurrencyCode == toCurrencyCode &&
                            er.RateDate.Date <= date.Date)
                        .OrderByDescending(er => er.RateDate)
                        .Include(er => er.FromCurrency)
                        .Include(er => er.ToCurrency)
                        .FirstOrDefaultAsync(cancellationToken);
                }
                
                return rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate from {FromCurrency} to {ToCurrency} for date {Date}", 
                    fromCurrencyCode, toCurrencyCode, date);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeRate>> GetExchangeRateHistoryAsync(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.ExchangeRates
                    .Where(er => 
                        er.FromCurrencyCode == fromCurrencyCode && 
                        er.ToCurrencyCode == toCurrencyCode &&
                        er.RateDate >= fromDate &&
                        er.RateDate <= toDate)
                    .OrderBy(er => er.RateDate)
                    .Include(er => er.FromCurrency)
                    .Include(er => er.ToCurrency)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate history from {FromCurrency} to {ToCurrency} between {FromDate} and {ToDate}", 
                    fromCurrencyCode, toCurrencyCode, fromDate, toDate);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRate> AddExchangeRateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.ExchangeRates.Add(exchangeRate);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return exchangeRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exchange rate from {FromCurrency} to {ToCurrency}", 
                    exchangeRate.FromCurrencyCode, exchangeRate.ToCurrencyCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRate> UpdateExchangeRateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.ExchangeRates.Update(exchangeRate);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return exchangeRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rate with ID {ExchangeRateId}", exchangeRate.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ForeignCurrencyBalance>> GetForeignCurrencyBalancesAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get base currency
                var baseCurrency = await GetBaseCurrencyAsync(cancellationToken);
                if (baseCurrency == null)
                {
                    throw new InvalidOperationException("Base currency not found");
                }
                
                // For this method, we need to join several tables to get foreign currency balances
                // This is a more complex query involving chart of accounts, general ledger, etc.
                // Here's a simplified example:
                
                var balances = await (from account in _dbContext.ChartOfAccounts
                                    join ledger in _dbContext.GeneralLedgerEntries on account.Id equals ledger.AccountId.ToString()
                                    where account.CurrencyCode != baseCurrency.Code
                                          && account.CurrencyCode != null
                                          && ledger.FinancialPeriodId == financialPeriodId
                                    group ledger by new { account.Id, account.AccountNumber, account.AccountName, account.CurrencyCode } into g
                                    select new ForeignCurrencyBalance
                                    {
                                        AccountId = g.Key.Id,
                                        AccountNumber = g.Key.AccountNumber,
                                        AccountName = g.Key.AccountName,
                                        CurrencyCode = g.Key.CurrencyCode,
                                        ForeignAmount = g.Sum(l => l.Amount),
                                        BaseAmount = g.Sum(l => l.BaseAmount),
                                        FinancialPeriodId = financialPeriodId
                                    }).ToListAsync(cancellationToken);
                
                // Get last revaluation rates
                var lastRevaluations = await (from rev in _dbContext.CurrencyRevaluations
                                           where rev.FinancialPeriodId == financialPeriodId
                                           group rev by rev.CurrencyCode into g
                                           select new 
                                           {
                                               CurrencyCode = g.Key,
                                               LastRevaluationDate = g.Max(r => r.RevaluationDate),
                                               LastRevaluationRate = g.OrderByDescending(r => r.RevaluationDate)
                                                                     .First().CurrentRate
                                           }).ToListAsync(cancellationToken);
                
                // Set last revaluation information
                foreach (var balance in balances)
                {
                    var lastRevaluation = lastRevaluations.FirstOrDefault(r => r.CurrencyCode == balance.CurrencyCode);
                    if (lastRevaluation != null)
                    {
                        balance.LastRevaluationDate = lastRevaluation.LastRevaluationDate;
                        balance.LastRevaluationRate = lastRevaluation.LastRevaluationRate;
                    }
                }
                
                return balances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving foreign currency balances for financial period {FinancialPeriodId}", 
                    financialPeriodId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CurrencyRevaluation> AddCurrencyRevaluationAsync(
            CurrencyRevaluation revaluation, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.CurrencyRevaluations.Add(revaluation);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return revaluation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding currency revaluation for currency {CurrencyCode}", 
                    revaluation.CurrencyCode);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CurrencyRevaluation>> GetCurrencyRevaluationsForPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.CurrencyRevaluations
                    .Where(cr => cr.FinancialPeriodId == financialPeriodId)
                    .Include(cr => cr.Details)
                    .OrderByDescending(cr => cr.RevaluationDate)
                    .ThenBy(cr => cr.CurrencyCode)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency revaluations for financial period {FinancialPeriodId}", 
                    financialPeriodId);
                throw;
            }
        }
    }
}
