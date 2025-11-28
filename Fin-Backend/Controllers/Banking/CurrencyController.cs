using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Currency;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(
            ICurrencyService currencyService,
            ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetAllCurrencies(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all currencies");
            var currencies = await _currencyService.GetAllCurrenciesAsync(cancellationToken);
            return Ok(currencies);
        }

        [HttpGet("base")]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<CurrencyDto>> GetBaseCurrency(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting base currency");
            var currency = await _currencyService.GetBaseCurrencyAsync(cancellationToken);
            return Ok(currency);
        }

        [HttpPost("base/{currencyCode}")]
        [ResourceAuthorization("Currency", ResourceOperation.Update)]
        public async Task<IActionResult> SetBaseCurrency(string currencyCode, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Setting {CurrencyCode} as base currency", currencyCode);
            var result = await _currencyService.SetBaseCurrencyAsync(currencyCode, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{currencyCode}")]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<CurrencyDto>> GetCurrencyByCode(string currencyCode, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting currency with code {CurrencyCode}", currencyCode);
            var currency = await _currencyService.GetCurrencyByCodeAsync(currencyCode, cancellationToken);
            return Ok(currency);
        }

        [HttpPost]
        [ResourceAuthorization("Currency", ResourceOperation.Create)]
        public async Task<ActionResult<CurrencyDto>> CreateCurrency(CreateCurrencyDto createCurrencyDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new currency with code {CurrencyCode}", createCurrencyDto.Code);
            var currency = await _currencyService.AddCurrencyAsync(createCurrencyDto, cancellationToken);
            return CreatedAtAction(nameof(GetCurrencyByCode), new { currencyCode = currency.Code }, currency);
        }

        [HttpPut("{currencyCode}")]
        [ResourceAuthorization("Currency", ResourceOperation.Update)]
        public async Task<ActionResult<CurrencyDto>> UpdateCurrency(string currencyCode, UpdateCurrencyDto updateCurrencyDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating currency with code {CurrencyCode}", currencyCode);
            
            if (currencyCode != updateCurrencyDto.Code)
            {
                _logger.LogWarning("Currency code mismatch: URL {UrlCode} vs DTO {DtoCode}", currencyCode, updateCurrencyDto.Code);
                return BadRequest("Currency code in URL must match the code in the request body");
            }
            
            var currency = await _currencyService.UpdateCurrencyAsync(updateCurrencyDto, cancellationToken);
            return Ok(currency);
        }

        [HttpPatch("{currencyCode}/status")]
        [ResourceAuthorization("Currency", ResourceOperation.Update)]
        public async Task<IActionResult> SetCurrencyStatus(string currencyCode, [FromBody] bool isActive, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Setting currency {CurrencyCode} status to {IsActive}", currencyCode, isActive);
            var result = await _currencyService.SetCurrencyStatusAsync(currencyCode, isActive, cancellationToken);
            return Ok(result);
        }

        [HttpGet("exchange-rates/{fromCurrencyCode}/{toCurrencyCode}")]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<ExchangeRateDto>> GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting current exchange rate from {FromCurrency} to {ToCurrency}", fromCurrencyCode, toCurrencyCode);
            var exchangeRate = await _currencyService.GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode, cancellationToken);
            return Ok(exchangeRate);
        }

        [HttpGet("exchange-rates/{fromCurrencyCode}/{toCurrencyCode}/historical")]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<ExchangeRateDto>> GetHistoricalExchangeRate(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            [FromQuery] DateTime rateDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting historical exchange rate from {FromCurrency} to {ToCurrency} for date {RateDate}", 
                fromCurrencyCode, toCurrencyCode, rateDate);
            var exchangeRate = await _currencyService.GetHistoricalExchangeRateAsync(fromCurrencyCode, toCurrencyCode, rateDate, cancellationToken);
            return Ok(exchangeRate);
        }

        [HttpGet("exchange-rates/{fromCurrencyCode}/{toCurrencyCode}/history")]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<IEnumerable<ExchangeRateDto>>> GetExchangeRateHistory(
            string fromCurrencyCode, 
            string toCurrencyCode, 
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting exchange rate history from {FromCurrency} to {ToCurrency} between {FromDate} and {ToDate}", 
                fromCurrencyCode, toCurrencyCode, fromDate, toDate);
            var exchangeRates = await _currencyService.GetExchangeRateHistoryAsync(fromCurrencyCode, toCurrencyCode, fromDate, toDate, cancellationToken);
            return Ok(exchangeRates);
        }

        [HttpPost("exchange-rates")]
        [ResourceAuthorization("Currency", ResourceOperation.Create)]
        public async Task<ActionResult<ExchangeRateDto>> CreateExchangeRate(
            CreateExchangeRateDto createExchangeRateDto, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new exchange rate from {FromCurrency} to {ToCurrency}", 
                createExchangeRateDto.FromCurrencyCode, createExchangeRateDto.ToCurrencyCode);
            var exchangeRate = await _currencyService.AddExchangeRateAsync(createExchangeRateDto, cancellationToken);
            return CreatedAtAction(nameof(GetExchangeRate), 
                new { fromCurrencyCode = exchangeRate.FromCurrencyCode, toCurrencyCode = exchangeRate.ToCurrencyCode }, 
                exchangeRate);
        }

        [HttpPut("exchange-rates")]
        [ResourceAuthorization("Currency", ResourceOperation.Update)]
        public async Task<ActionResult<ExchangeRateDto>> UpdateExchangeRate(
            UpdateExchangeRateDto updateExchangeRateDto, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating exchange rate with ID {ExchangeRateId}", updateExchangeRateDto.Id);
            var exchangeRate = await _currencyService.UpdateExchangeRateAsync(updateExchangeRateDto, cancellationToken);
            return Ok(exchangeRate);
        }

        [HttpPost("convert")]
        [ResourceAuthorization("Currency", ResourceOperation.Read)]
        public async Task<ActionResult<ConversionResultDto>> ConvertCurrency(
            [FromQuery] decimal amount,
            [FromQuery] string fromCurrencyCode,
            [FromQuery] string toCurrencyCode,
            [FromQuery] DateTime? conversionDate = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Converting {Amount} {FromCurrency} to {ToCurrency}", amount, fromCurrencyCode, toCurrencyCode);
            var result = await _currencyService.ConvertCurrencyAsync(amount, fromCurrencyCode, toCurrencyCode, conversionDate, cancellationToken);
            return Ok(result);
        }

        [HttpPost("revalue/{financialPeriodId}")]
        [ResourceAuthorization("Currency", ResourceOperation.Process)]
        public async Task<ActionResult<ForeignCurrencyRevaluationResultDto>> RevalueForeignCurrencyBalances(
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Revaluing foreign currency balances for financial period {FinancialPeriodId}", financialPeriodId);
            var result = await _currencyService.RevalueForeignCurrencyBalancesAsync(financialPeriodId, cancellationToken);
            return Ok(result);
        }
    }
}
