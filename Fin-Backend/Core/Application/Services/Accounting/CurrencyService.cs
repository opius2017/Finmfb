using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Core.Application.DTOs.Currency;
using FinTech.Core.Application.Exceptions;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Entities.Currency;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Accounting
{
    /// <summary>
    /// Implementation of the currency service for handling multi-currency operations
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;

        public CurrencyService(
            ICurrencyRepository currencyRepository,
            IMapper mapper,
            ILogger<CurrencyService> logger,
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService)
        {
            _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _chartOfAccountService = chartOfAccountService ?? throw new ArgumentNullException(nameof(chartOfAccountService));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting all currencies");
                var currencies = await _currencyRepository.GetAllCurrenciesAsync(cancellationToken);
                return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all currencies");
                throw new ApplicationException("Failed to retrieve currencies", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CurrencyDto> GetCurrencyByCodeAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting currency with code {CurrencyCode}", currencyCode);
                
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    throw new ArgumentException("Currency code cannot be null or empty", nameof(currencyCode));
                }
                
                var currency = await _currencyRepository.GetCurrencyByCodeAsync(currencyCode, cancellationToken);
                
                if (currency == null)
                {
                    _logger.LogWarning("Currency with code {CurrencyCode} not found", currencyCode);
                    throw new NotFoundException($"Currency with code {currencyCode} not found");
                }
                
                return _mapper.Map<CurrencyDto>(currency);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with code {CurrencyCode}", currencyCode);
                throw new ApplicationException($"Failed to retrieve currency with code {currencyCode}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CurrencyDto> AddCurrencyAsync(CreateCurrencyDto createCurrencyDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding new currency with code {CurrencyCode}", createCurrencyDto.Code);
                
                // Validate input
                if (createCurrencyDto == null)
                {
                    throw new ArgumentNullException(nameof(createCurrencyDto));
                }
                
                if (string.IsNullOrWhiteSpace(createCurrencyDto.Code))
                {
                    throw new ArgumentException("Currency code cannot be null or empty", nameof(createCurrencyDto.Code));
                }
                
                // Check if currency already exists
                var exists = await _currencyRepository.CurrencyExistsAsync(createCurrencyDto.Code, cancellationToken);
                if (exists)
                {
                    _logger.LogWarning("Currency with code {CurrencyCode} already exists", createCurrencyDto.Code);
                    throw new DuplicateEntityException($"Currency with code {createCurrencyDto.Code} already exists");
                }
                
                // Map and save
                var currency = _mapper.Map<Currency>(createCurrencyDto);
                currency.IsActive = true;
                
                // If this is the first currency, make it the base currency
                var allCurrencies = await _currencyRepository.GetAllCurrenciesAsync(cancellationToken);
                if (!allCurrencies.Any())
                {
                    currency.IsBaseCurrency = true;
                }
                else
                {
                    currency.IsBaseCurrency = false;
                }
                
                var result = await _currencyRepository.AddCurrencyAsync(currency, cancellationToken);
                return _mapper.Map<CurrencyDto>(result);
            }
            catch (DuplicateEntityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding currency with code {CurrencyCode}", createCurrencyDto?.Code);
                throw new ApplicationException($"Failed to add currency with code {createCurrencyDto?.Code}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CurrencyDto> UpdateCurrencyAsync(UpdateCurrencyDto updateCurrencyDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating currency with code {CurrencyCode}", updateCurrencyDto.Code);
                
                // Validate input
                if (updateCurrencyDto == null)
                {
                    throw new ArgumentNullException(nameof(updateCurrencyDto));
                }
                
                if (string.IsNullOrWhiteSpace(updateCurrencyDto.Code))
                {
                    throw new ArgumentException("Currency code cannot be null or empty", nameof(updateCurrencyDto.Code));
                }
                
                // Get existing currency
                var existingCurrency = await _currencyRepository.GetCurrencyByCodeAsync(updateCurrencyDto.Code, cancellationToken);
                if (existingCurrency == null)
                {
                    _logger.LogWarning("Currency with code {CurrencyCode} not found", updateCurrencyDto.Code);
                    throw new NotFoundException($"Currency with code {updateCurrencyDto.Code} not found");
                }
                
                // Update properties
                existingCurrency.Name = updateCurrencyDto.Name;
                existingCurrency.Symbol = updateCurrencyDto.Symbol;
                existingCurrency.DecimalPlaces = updateCurrencyDto.DecimalPlaces;
                existingCurrency.IsActive = updateCurrencyDto.IsActive;
                
                // Save changes
                var result = await _currencyRepository.UpdateCurrencyAsync(existingCurrency, cancellationToken);
                return _mapper.Map<CurrencyDto>(result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating currency with code {CurrencyCode}", updateCurrencyDto?.Code);
                throw new ApplicationException($"Failed to update currency with code {updateCurrencyDto?.Code}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetCurrencyStatusAsync(string currencyCode, bool isActive, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Setting currency {CurrencyCode} status to {IsActive}", currencyCode, isActive);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    throw new ArgumentException("Currency code cannot be null or empty", nameof(currencyCode));
                }
                
                // Get existing currency
                var existingCurrency = await _currencyRepository.GetCurrencyByCodeAsync(currencyCode, cancellationToken);
                if (existingCurrency == null)
                {
                    _logger.LogWarning("Currency with code {CurrencyCode} not found", currencyCode);
                    throw new NotFoundException($"Currency with code {currencyCode} not found");
                }
                
                // Cannot deactivate base currency
                if (existingCurrency.IsBaseCurrency && !isActive)
                {
                    _logger.LogWarning("Cannot deactivate base currency {CurrencyCode}", currencyCode);
                    throw new InvalidOperationException($"Cannot deactivate base currency {currencyCode}");
                }
                
                // Update status
                existingCurrency.IsActive = isActive;
                
                // Save changes
                await _currencyRepository.UpdateCurrencyAsync(existingCurrency, cancellationToken);
                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting currency {CurrencyCode} status to {IsActive}", currencyCode, isActive);
                throw new ApplicationException($"Failed to set currency {currencyCode} status", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateDto> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting current exchange rate from {FromCurrency} to {ToCurrency}", 
                    fromCurrencyCode, toCurrencyCode);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(fromCurrencyCode))
                {
                    throw new ArgumentException("From currency code cannot be null or empty", nameof(fromCurrencyCode));
                }
                
                if (string.IsNullOrWhiteSpace(toCurrencyCode))
                {
                    throw new ArgumentException("To currency code cannot be null or empty", nameof(toCurrencyCode));
                }
                
                // Get the latest exchange rate
                var exchangeRate = await _currencyRepository.GetExchangeRateAsync(
                    fromCurrencyCode, 
                    toCurrencyCode, 
                    DateTime.UtcNow,
                    cancellationToken);
                
                if (exchangeRate == null)
                {
                    _logger.LogWarning("Exchange rate from {FromCurrency} to {ToCurrency} not found", 
                        fromCurrencyCode, toCurrencyCode);
                    throw new NotFoundException($"Exchange rate from {fromCurrencyCode} to {toCurrencyCode} not found");
                }
                
                return _mapper.Map<ExchangeRateDto>(exchangeRate);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate from {FromCurrency} to {ToCurrency}", 
                    fromCurrencyCode, toCurrencyCode);
                throw new ApplicationException($"Failed to get exchange rate from {fromCurrencyCode} to {toCurrencyCode}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateDto> GetHistoricalExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode, DateTime rateDate, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting historical exchange rate from {FromCurrency} to {ToCurrency} for date {RateDate}", 
                    fromCurrencyCode, toCurrencyCode, rateDate);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(fromCurrencyCode))
                {
                    throw new ArgumentException("From currency code cannot be null or empty", nameof(fromCurrencyCode));
                }
                
                if (string.IsNullOrWhiteSpace(toCurrencyCode))
                {
                    throw new ArgumentException("To currency code cannot be null or empty", nameof(toCurrencyCode));
                }
                
                // Get historical exchange rate
                var exchangeRate = await _currencyRepository.GetExchangeRateAsync(
                    fromCurrencyCode, 
                    toCurrencyCode, 
                    rateDate,
                    cancellationToken);
                
                if (exchangeRate == null)
                {
                    _logger.LogWarning("Historical exchange rate from {FromCurrency} to {ToCurrency} for date {RateDate} not found", 
                        fromCurrencyCode, toCurrencyCode, rateDate);
                    throw new NotFoundException($"Exchange rate from {fromCurrencyCode} to {toCurrencyCode} for date {rateDate} not found");
                }
                
                return _mapper.Map<ExchangeRateDto>(exchangeRate);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical exchange rate from {FromCurrency} to {ToCurrency} for date {RateDate}", 
                    fromCurrencyCode, toCurrencyCode, rateDate);
                throw new ApplicationException($"Failed to get historical exchange rate", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeRateDto>> GetExchangeRateHistoryAsync(string fromCurrencyCode, string toCurrencyCode, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting exchange rate history from {FromCurrency} to {ToCurrency} between {FromDate} and {ToDate}", 
                    fromCurrencyCode, toCurrencyCode, fromDate, toDate);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(fromCurrencyCode))
                {
                    throw new ArgumentException("From currency code cannot be null or empty", nameof(fromCurrencyCode));
                }
                
                if (string.IsNullOrWhiteSpace(toCurrencyCode))
                {
                    throw new ArgumentException("To currency code cannot be null or empty", nameof(toCurrencyCode));
                }
                
                if (fromDate > toDate)
                {
                    throw new ArgumentException("From date must be earlier than or equal to to date");
                }
                
                // Get exchange rate history
                var exchangeRates = await _currencyRepository.GetExchangeRateHistoryAsync(
                    fromCurrencyCode, 
                    toCurrencyCode, 
                    fromDate,
                    toDate,
                    cancellationToken);
                
                return _mapper.Map<IEnumerable<ExchangeRateDto>>(exchangeRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate history from {FromCurrency} to {ToCurrency} between {FromDate} and {ToDate}", 
                    fromCurrencyCode, toCurrencyCode, fromDate, toDate);
                throw new ApplicationException("Failed to get exchange rate history", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateDto> UpdateExchangeRateAsync(UpdateExchangeRateDto updateExchangeRateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating exchange rate with ID {ExchangeRateId}", updateExchangeRateDto.Id);
                
                // Validate input
                if (updateExchangeRateDto == null)
                {
                    throw new ArgumentNullException(nameof(updateExchangeRateDto));
                }
                
                if (string.IsNullOrWhiteSpace(updateExchangeRateDto.Id))
                {
                    throw new ArgumentException("Exchange rate ID cannot be null or empty", nameof(updateExchangeRateDto.Id));
                }
                
                // Get existing exchange rate
                var exchangeRates = await _currencyRepository.GetExchangeRateHistoryAsync(
                    updateExchangeRateDto.FromCurrencyCode,
                    updateExchangeRateDto.ToCurrencyCode,
                    updateExchangeRateDto.RateDate,
                    updateExchangeRateDto.RateDate,
                    cancellationToken);
                
                var existingRate = exchangeRates.FirstOrDefault(er => er.Id == updateExchangeRateDto.Id);
                
                if (existingRate == null)
                {
                    _logger.LogWarning("Exchange rate with ID {ExchangeRateId} not found", updateExchangeRateDto.Id);
                    throw new NotFoundException($"Exchange rate with ID {updateExchangeRateDto.Id} not found");
                }
                
                // Update properties
                existingRate.Rate = updateExchangeRateDto.Rate;
                existingRate.Source = updateExchangeRateDto.Source;
                
                // Save changes
                var result = await _currencyRepository.UpdateExchangeRateAsync(existingRate, cancellationToken);
                return _mapper.Map<ExchangeRateDto>(result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rate with ID {ExchangeRateId}", updateExchangeRateDto?.Id);
                throw new ApplicationException($"Failed to update exchange rate with ID {updateExchangeRateDto?.Id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ExchangeRateDto> AddExchangeRateAsync(CreateExchangeRateDto createExchangeRateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding new exchange rate from {FromCurrency} to {ToCurrency} for date {RateDate}", 
                    createExchangeRateDto.FromCurrencyCode, createExchangeRateDto.ToCurrencyCode, createExchangeRateDto.RateDate);
                
                // Validate input
                if (createExchangeRateDto == null)
                {
                    throw new ArgumentNullException(nameof(createExchangeRateDto));
                }
                
                if (string.IsNullOrWhiteSpace(createExchangeRateDto.FromCurrencyCode))
                {
                    throw new ArgumentException("From currency code cannot be null or empty", nameof(createExchangeRateDto.FromCurrencyCode));
                }
                
                if (string.IsNullOrWhiteSpace(createExchangeRateDto.ToCurrencyCode))
                {
                    throw new ArgumentException("To currency code cannot be null or empty", nameof(createExchangeRateDto.ToCurrencyCode));
                }
                
                if (createExchangeRateDto.Rate <= 0)
                {
                    throw new ArgumentException("Exchange rate must be greater than zero", nameof(createExchangeRateDto.Rate));
                }
                
                // Check if currencies exist
                var fromCurrencyExists = await _currencyRepository.CurrencyExistsAsync(createExchangeRateDto.FromCurrencyCode, cancellationToken);
                var toCurrencyExists = await _currencyRepository.CurrencyExistsAsync(createExchangeRateDto.ToCurrencyCode, cancellationToken);
                
                if (!fromCurrencyExists)
                {
                    _logger.LogWarning("From currency {FromCurrency} not found", createExchangeRateDto.FromCurrencyCode);
                    throw new NotFoundException($"Currency with code {createExchangeRateDto.FromCurrencyCode} not found");
                }
                
                if (!toCurrencyExists)
                {
                    _logger.LogWarning("To currency {ToCurrency} not found", createExchangeRateDto.ToCurrencyCode);
                    throw new NotFoundException($"Currency with code {createExchangeRateDto.ToCurrencyCode} not found");
                }
                
                // Check if exchange rate already exists for the date
                var existingRates = await _currencyRepository.GetExchangeRateHistoryAsync(
                    createExchangeRateDto.FromCurrencyCode,
                    createExchangeRateDto.ToCurrencyCode,
                    createExchangeRateDto.RateDate.Date,
                    createExchangeRateDto.RateDate.Date.AddDays(1).AddSeconds(-1),
                    cancellationToken);
                
                if (existingRates.Any())
                {
                    _logger.LogWarning("Exchange rate already exists for {FromCurrency} to {ToCurrency} on {RateDate}", 
                        createExchangeRateDto.FromCurrencyCode, createExchangeRateDto.ToCurrencyCode, createExchangeRateDto.RateDate.Date);
                    throw new DuplicateEntityException($"Exchange rate already exists for this date");
                }
                
                // Map and save
                var exchangeRate = _mapper.Map<ExchangeRate>(createExchangeRateDto);
                
                var result = await _currencyRepository.AddExchangeRateAsync(exchangeRate, cancellationToken);
                return _mapper.Map<ExchangeRateDto>(result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (DuplicateEntityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding exchange rate from {FromCurrency} to {ToCurrency}", 
                    createExchangeRateDto?.FromCurrencyCode, createExchangeRateDto?.ToCurrencyCode);
                throw new ApplicationException("Failed to add exchange rate", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ConversionResultDto> ConvertCurrencyAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode, DateTime? conversionDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var date = conversionDate ?? DateTime.UtcNow;
                _logger.LogInformation("Converting {Amount} {FromCurrency} to {ToCurrency} as of {Date}", 
                    amount, fromCurrencyCode, toCurrencyCode, date);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(fromCurrencyCode))
                {
                    throw new ArgumentException("From currency code cannot be null or empty", nameof(fromCurrencyCode));
                }
                
                if (string.IsNullOrWhiteSpace(toCurrencyCode))
                {
                    throw new ArgumentException("To currency code cannot be null or empty", nameof(toCurrencyCode));
                }
                
                // If currencies are the same, no conversion needed
                if (fromCurrencyCode.Equals(toCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    return new ConversionResultDto
                    {
                        OriginalAmount = amount,
                        FromCurrencyCode = fromCurrencyCode,
                        ConvertedAmount = amount,
                        ToCurrencyCode = toCurrencyCode,
                        ExchangeRate = 1,
                        ConversionDate = date
                    };
                }
                
                // Get exchange rate
                var exchangeRate = await _currencyRepository.GetExchangeRateAsync(
                    fromCurrencyCode, 
                    toCurrencyCode, 
                    date,
                    cancellationToken);
                
                if (exchangeRate == null)
                {
                    _logger.LogWarning("Exchange rate from {FromCurrency} to {ToCurrency} for date {Date} not found", 
                        fromCurrencyCode, toCurrencyCode, date);
                    
                    // Try to get the inverse rate and use its reciprocal
                    var inverseRate = await _currencyRepository.GetExchangeRateAsync(
                        toCurrencyCode, 
                        fromCurrencyCode, 
                        date,
                        cancellationToken);
                    
                    if (inverseRate == null)
                    {
                        throw new NotFoundException($"Exchange rate from {fromCurrencyCode} to {toCurrencyCode} for date {date} not found");
                    }
                    
                    // Convert using inverse rate
                    decimal convertedAmount = amount / inverseRate.Rate;
                    
                    return new ConversionResultDto
                    {
                        OriginalAmount = amount,
                        FromCurrencyCode = fromCurrencyCode,
                        ConvertedAmount = convertedAmount,
                        ToCurrencyCode = toCurrencyCode,
                        ExchangeRate = 1 / inverseRate.Rate,
                        ConversionDate = date
                    };
                }
                
                // Convert amount
                decimal result = amount * exchangeRate.Rate;
                
                // Prepare result
                return new ConversionResultDto
                {
                    OriginalAmount = amount,
                    FromCurrencyCode = fromCurrencyCode,
                    ConvertedAmount = result,
                    ToCurrencyCode = toCurrencyCode,
                    ExchangeRate = exchangeRate.Rate,
                    ConversionDate = date
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting {Amount} {FromCurrency} to {ToCurrency}", 
                    amount, fromCurrencyCode, toCurrencyCode);
                throw new ApplicationException("Failed to convert currency", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ForeignCurrencyRevaluationResultDto> RevalueForeignCurrencyBalancesAsync(string financialPeriodId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Revaluing foreign currency balances for financial period {FinancialPeriodId}", financialPeriodId);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(financialPeriodId))
                {
                    throw new ArgumentException("Financial period ID cannot be null or empty", nameof(financialPeriodId));
                }
                
                // Get base currency
                var baseCurrency = await _currencyRepository.GetBaseCurrencyAsync(cancellationToken);
                if (baseCurrency == null)
                {
                    _logger.LogWarning("Base currency not found");
                    throw new NotFoundException("Base currency not found");
                }
                
                // Get all foreign currency balances
                var foreignCurrencyBalances = await _currencyRepository.GetForeignCurrencyBalancesAsync(financialPeriodId, cancellationToken);
                if (!foreignCurrencyBalances.Any())
                {
                    _logger.LogInformation("No foreign currency balances found for period {FinancialPeriodId}", financialPeriodId);
                    return new ForeignCurrencyRevaluationResultDto
                    {
                        FinancialPeriodId = financialPeriodId,
                        RevaluationDate = DateTime.UtcNow,
                        BaseCurrencyCode = baseCurrency.Code,
                        TotalUnrealizedGain = 0,
                        TotalUnrealizedLoss = 0,
                        NetEffect = 0
                    };
                }
                
                // Prepare result
                var result = new ForeignCurrencyRevaluationResultDto
                {
                    FinancialPeriodId = financialPeriodId,
                    RevaluationDate = DateTime.UtcNow,
                    BaseCurrencyCode = baseCurrency.Code
                };
                
                // Process each currency group
                foreach (var currencyGroup in foreignCurrencyBalances.GroupBy(b => b.CurrencyCode))
                {
                    string currencyCode = currencyGroup.Key;
                    
                    // Get current exchange rate
                    var currentRate = await _currencyRepository.GetExchangeRateAsync(
                        currencyCode, 
                        baseCurrency.Code, 
                        DateTime.UtcNow,
                        cancellationToken);
                    
                    if (currentRate == null)
                    {
                        _logger.LogWarning("Current exchange rate from {CurrencyCode} to {BaseCurrency} not found, skipping", 
                            currencyCode, baseCurrency.Code);
                        continue;
                    }
                    
                    // Create currency detail
                    var currencyDetail = new CurrencyRevaluationDetailDto
                    {
                        CurrencyCode = currencyCode,
                        CurrentRate = currentRate.Rate,
                        ForeignCurrencyBalance = currencyGroup.Sum(b => b.ForeignAmount)
                    };
                    
                    // Process each account
                    foreach (var balance in currencyGroup)
                    {
                        // Calculate new base currency value
                        decimal currentBaseCurrencyValue = balance.ForeignAmount * currentRate.Rate;
                        decimal previousBaseCurrencyValue = balance.BaseAmount;
                        decimal revaluationEffect = currentBaseCurrencyValue - previousBaseCurrencyValue;
                        
                        // Create account detail
                        var accountDetail = new RevaluatedAccountDto
                        {
                            AccountId = balance.AccountId,
                            AccountNumber = balance.AccountNumber,
                            AccountName = balance.AccountName,
                            ForeignCurrencyBalance = balance.ForeignAmount,
                            PreviousBaseCurrencyValue = previousBaseCurrencyValue,
                            CurrentBaseCurrencyValue = currentBaseCurrencyValue,
                            RevaluationEffect = revaluationEffect
                        };
                        
                        // Add to currency detail
                        currencyDetail.AffectedAccounts.Add(accountDetail);
                        
                        // Update totals
                        if (revaluationEffect > 0)
                        {
                            currencyDetail.UnrealizedGain += revaluationEffect;
                        }
                        else if (revaluationEffect < 0)
                        {
                            currencyDetail.UnrealizedLoss += Math.Abs(revaluationEffect);
                        }
                    }
                    
                    // Set previous rate if available
                    if (currencyGroup.First().LastRevaluationRate.HasValue)
                    {
                        currencyDetail.PreviousRate = currencyGroup.First().LastRevaluationRate.Value;
                    }
                    else
                    {
                        // Use last known rate or default to current rate
                        currencyDetail.PreviousRate = currencyDetail.CurrentRate;
                    }
                    
                    // Calculate totals
                    currencyDetail.PreviousBaseCurrencyValue = currencyGroup.Sum(b => b.BaseAmount);
                    currencyDetail.CurrentBaseCurrencyValue = currencyDetail.ForeignCurrencyBalance * currencyDetail.CurrentRate;
                    
                    // Add to result
                    result.Details.Add(currencyDetail);
                    result.TotalUnrealizedGain += currencyDetail.UnrealizedGain;
                    result.TotalUnrealizedLoss += currencyDetail.UnrealizedLoss;
                }
                
                // Calculate net effect
                result.NetEffect = result.TotalUnrealizedGain - result.TotalUnrealizedLoss;
                
                // Generate a revaluation for each currency
                foreach (var detail in result.Details)
                {
                    var revaluation = new CurrencyRevaluation
                    {
                        FinancialPeriodId = financialPeriodId,
                        RevaluationDate = result.RevaluationDate,
                        CurrencyCode = detail.CurrencyCode,
                        PreviousRate = detail.PreviousRate,
                        CurrentRate = detail.CurrentRate,
                        ForeignCurrencyAmount = detail.ForeignCurrencyBalance,
                        PreviousBaseCurrencyValue = detail.PreviousBaseCurrencyValue,
                        CurrentBaseCurrencyValue = detail.CurrentBaseCurrencyValue,
                        UnrealizedGain = detail.UnrealizedGain,
                        UnrealizedLoss = detail.UnrealizedLoss,
                        Status = RevaluationStatus.Processed,
                        ProcessedById = "System" // Replace with actual user ID
                    };
                    
                    // Add details
                    revaluation.Details = detail.AffectedAccounts.Select(a => new CurrencyRevaluationDetail
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        ForeignCurrencyAmount = a.ForeignCurrencyBalance,
                        PreviousBaseCurrencyValue = a.PreviousBaseCurrencyValue,
                        CurrentBaseCurrencyValue = a.CurrentBaseCurrencyValue,
                        RevaluationEffect = a.RevaluationEffect
                    }).ToList();
                    
                    // Save revaluation
                    await _currencyRepository.AddCurrencyRevaluationAsync(revaluation, cancellationToken);
                }
                
                return result;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revaluing foreign currency balances for financial period {FinancialPeriodId}", financialPeriodId);
                throw new ApplicationException($"Failed to revalue foreign currency balances", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<CurrencyDto> GetBaseCurrencyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting base currency");
                
                var baseCurrency = await _currencyRepository.GetBaseCurrencyAsync(cancellationToken);
                
                if (baseCurrency == null)
                {
                    _logger.LogWarning("Base currency not found");
                    throw new NotFoundException("Base currency not found");
                }
                
                return _mapper.Map<CurrencyDto>(baseCurrency);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting base currency");
                throw new ApplicationException("Failed to get base currency", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetBaseCurrencyAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Setting {CurrencyCode} as base currency", currencyCode);
                
                // Validate input
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    throw new ArgumentException("Currency code cannot be null or empty", nameof(currencyCode));
                }
                
                // Get current currency
                var currency = await _currencyRepository.GetCurrencyByCodeAsync(currencyCode, cancellationToken);
                
                if (currency == null)
                {
                    _logger.LogWarning("Currency with code {CurrencyCode} not found", currencyCode);
                    throw new NotFoundException($"Currency with code {currencyCode} not found");
                }
                
                // If already base currency, nothing to do
                if (currency.IsBaseCurrency)
                {
                    return true;
                }
                
                // Currency must be active
                if (!currency.IsActive)
                {
                    _logger.LogWarning("Cannot set inactive currency {CurrencyCode} as base currency", currencyCode);
                    throw new InvalidOperationException($"Cannot set inactive currency as base currency");
                }
                
                // Get current base currency
                var currentBaseCurrency = await _currencyRepository.GetBaseCurrencyAsync(cancellationToken);
                
                // Update currencies
                if (currentBaseCurrency != null)
                {
                    currentBaseCurrency.IsBaseCurrency = false;
                    await _currencyRepository.UpdateCurrencyAsync(currentBaseCurrency, cancellationToken);
                }
                
                currency.IsBaseCurrency = true;
                await _currencyRepository.UpdateCurrencyAsync(currency, cancellationToken);
                
                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting {CurrencyCode} as base currency", currencyCode);
                throw new ApplicationException($"Failed to set {currencyCode} as base currency", ex);
            }
        }
    }
}
