using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Tax;
using FinTech.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers
{
    /// <summary>
    /// Controller for tax-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaxController : ControllerBase
    {
        private readonly ITaxCalculationService _taxCalculationService;
        private readonly ILogger<TaxController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxController(
            ITaxCalculationService taxCalculationService,
            ILogger<TaxController> logger)
        {
            _taxCalculationService = taxCalculationService ?? throw new ArgumentNullException(nameof(taxCalculationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all tax types
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of tax types</returns>
        [HttpGet("types")]
        [ProducesResponseType(typeof(IEnumerable<TaxTypeDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllTaxTypesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting all tax types");
                var taxTypes = await _taxCalculationService.GetAllTaxTypesAsync(cancellationToken);
                return Ok(taxTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tax types");
                return StatusCode(500, "An error occurred while retrieving tax types");
            }
        }

        /// <summary>
        /// Gets a tax type by ID
        /// </summary>
        /// <param name="id">Tax type ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tax type</returns>
        [HttpGet("types/{id}")]
        [ProducesResponseType(typeof(TaxTypeDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTaxTypeByIdAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting tax type with ID {TaxTypeId}", id);
                var taxType = await _taxCalculationService.GetTaxTypeByIdAsync(id, cancellationToken);
                return Ok(taxType);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for tax type ID {TaxTypeId}", id);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type with ID {TaxTypeId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax type with ID {TaxTypeId}", id);
                return StatusCode(500, "An error occurred while retrieving the tax type");
            }
        }

        /// <summary>
        /// Creates a new tax type
        /// </summary>
        /// <param name="createTaxTypeDto">Tax type creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created tax type</returns>
        [HttpPost("types")]
        [ProducesResponseType(typeof(TaxTypeDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,TaxManager")]
        public async Task<IActionResult> CreateTaxTypeAsync([FromBody] CreateTaxTypeDto createTaxTypeDto, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new tax type with code {TaxTypeCode}", createTaxTypeDto.Code);
                var taxType = await _taxCalculationService.CreateTaxTypeAsync(createTaxTypeDto, cancellationToken);
                return CreatedAtAction(nameof(GetTaxTypeByIdAsync), new { id = taxType.Id }, taxType);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for creating tax type with code {TaxTypeCode}", createTaxTypeDto?.Code);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("already exists"))
            {
                _logger.LogWarning(ex, "Tax type with code {TaxTypeCode} already exists", createTaxTypeDto?.Code);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax type with code {TaxTypeCode}", createTaxTypeDto?.Code);
                return StatusCode(500, "An error occurred while creating the tax type");
            }
        }

        /// <summary>
        /// Updates an existing tax type
        /// </summary>
        /// <param name="id">Tax type ID</param>
        /// <param name="updateTaxTypeDto">Tax type update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated tax type</returns>
        [HttpPut("types/{id}")]
        [ProducesResponseType(typeof(TaxTypeDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,TaxManager")]
        public async Task<IActionResult> UpdateTaxTypeAsync(string id, [FromBody] UpdateTaxTypeDto updateTaxTypeDto, CancellationToken cancellationToken)
        {
            try
            {
                // Ensure the ID in the route matches the ID in the body
                if (updateTaxTypeDto.Id != id)
                {
                    _logger.LogWarning("Tax type ID mismatch. Route ID: {RouteId}, Body ID: {BodyId}", id, updateTaxTypeDto.Id);
                    return BadRequest("The tax type ID in the route must match the ID in the request body");
                }
                
                _logger.LogInformation("Updating tax type with ID {TaxTypeId}", id);
                var taxType = await _taxCalculationService.UpdateTaxTypeAsync(updateTaxTypeDto, cancellationToken);
                return Ok(taxType);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for updating tax type with ID {TaxTypeId}", id);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type with ID {TaxTypeId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax type with ID {TaxTypeId}", id);
                return StatusCode(500, "An error occurred while updating the tax type");
            }
        }

        /// <summary>
        /// Gets tax rates for a specific tax type
        /// </summary>
        /// <param name="taxTypeId">Tax type ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of tax rates</returns>
        [HttpGet("types/{taxTypeId}/rates")]
        [ProducesResponseType(typeof(IEnumerable<TaxRateDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTaxRatesForTypeAsync(string taxTypeId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting tax rates for tax type {TaxTypeId}", taxTypeId);
                var taxRates = await _taxCalculationService.GetTaxRatesForTypeAsync(taxTypeId, cancellationToken);
                return Ok(taxRates);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for tax type ID {TaxTypeId}", taxTypeId);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type with ID {TaxTypeId} not found", taxTypeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax rates for tax type {TaxTypeId}", taxTypeId);
                return StatusCode(500, "An error occurred while retrieving tax rates");
            }
        }

        /// <summary>
        /// Gets a tax rate by ID
        /// </summary>
        /// <param name="id">Tax rate ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tax rate</returns>
        [HttpGet("rates/{id}")]
        [ProducesResponseType(typeof(TaxRateDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTaxRateByIdAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting tax rate with ID {TaxRateId}", id);
                var taxRate = await _taxCalculationService.GetTaxRateByIdAsync(id, cancellationToken);
                return Ok(taxRate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for tax rate ID {TaxRateId}", id);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax rate with ID {TaxRateId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax rate with ID {TaxRateId}", id);
                return StatusCode(500, "An error occurred while retrieving the tax rate");
            }
        }

        /// <summary>
        /// Creates a new tax rate
        /// </summary>
        /// <param name="createTaxRateDto">Tax rate creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created tax rate</returns>
        [HttpPost("rates")]
        [ProducesResponseType(typeof(TaxRateDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,TaxManager")]
        public async Task<IActionResult> CreateTaxRateAsync([FromBody] CreateTaxRateDto createTaxRateDto, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new tax rate for tax type {TaxTypeId}", createTaxRateDto.TaxTypeId);
                var taxRate = await _taxCalculationService.CreateTaxRateAsync(createTaxRateDto, cancellationToken);
                return CreatedAtAction(nameof(GetTaxRateByIdAsync), new { id = taxRate.Id }, taxRate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for creating tax rate for tax type {TaxTypeId}", createTaxRateDto?.TaxTypeId);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type with ID {TaxTypeId} not found", createTaxRateDto?.TaxTypeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax rate for tax type {TaxTypeId}", createTaxRateDto?.TaxTypeId);
                return StatusCode(500, "An error occurred while creating the tax rate");
            }
        }

        /// <summary>
        /// Updates an existing tax rate
        /// </summary>
        /// <param name="id">Tax rate ID</param>
        /// <param name="updateTaxRateDto">Tax rate update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated tax rate</returns>
        [HttpPut("rates/{id}")]
        [ProducesResponseType(typeof(TaxRateDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,TaxManager")]
        public async Task<IActionResult> UpdateTaxRateAsync(string id, [FromBody] UpdateTaxRateDto updateTaxRateDto, CancellationToken cancellationToken)
        {
            try
            {
                // Ensure the ID in the route matches the ID in the body
                if (updateTaxRateDto.Id != id)
                {
                    _logger.LogWarning("Tax rate ID mismatch. Route ID: {RouteId}, Body ID: {BodyId}", id, updateTaxRateDto.Id);
                    return BadRequest("The tax rate ID in the route must match the ID in the request body");
                }
                
                _logger.LogInformation("Updating tax rate with ID {TaxRateId}", id);
                var taxRate = await _taxCalculationService.UpdateTaxRateAsync(updateTaxRateDto, cancellationToken);
                return Ok(taxRate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for updating tax rate with ID {TaxRateId}", id);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax rate with ID {TaxRateId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax rate with ID {TaxRateId}", id);
                return StatusCode(500, "An error occurred while updating the tax rate");
            }
        }

        /// <summary>
        /// Calculates tax for a transaction
        /// </summary>
        /// <param name="request">Tax calculation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tax calculation result</returns>
        [HttpPost("calculate")]
        [ProducesResponseType(typeof(TaxCalculationResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CalculateTaxAsync([FromBody] TaxCalculationRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculating tax for type {TaxTypeId} on amount {Amount}", request.TaxTypeId, request.TaxableAmount);
                var result = await _taxCalculationService.CalculateTaxAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for calculating tax for type {TaxTypeId}", request?.TaxTypeId);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type or rate not found for calculation request with tax type {TaxTypeId}", request?.TaxTypeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax for type {TaxTypeId} on amount {Amount}", request?.TaxTypeId, request?.TaxableAmount);
                return StatusCode(500, "An error occurred while calculating tax");
            }
        }

        /// <summary>
        /// Calculates tax for multiple transactions in a batch
        /// </summary>
        /// <param name="requests">Collection of tax calculation requests</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of tax calculation results</returns>
        [HttpPost("calculate/batch")]
        [ProducesResponseType(typeof(IEnumerable<TaxCalculationResultDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CalculateBatchTaxAsync([FromBody] IEnumerable<TaxCalculationRequestDto> requests, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculating batch tax for {Count} items", requests);
                var results = await _taxCalculationService.CalculateBatchTaxAsync(requests, cancellationToken);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for batch tax calculation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating batch tax");
                return StatusCode(500, "An error occurred while calculating batch tax");
            }
        }

        /// <summary>
        /// Calculates withholding tax
        /// </summary>
        /// <param name="request">Withholding tax calculation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Withholding tax calculation result</returns>
        [HttpPost("withholding")]
        [ProducesResponseType(typeof(WithholdingTaxResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CalculateWithholdingTaxAsync([FromBody] WithholdingTaxRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Calculating withholding tax for income type {IncomeType} on amount {Amount}", 
                    request.IncomeType, request.GrossAmount);
                
                var result = await _taxCalculationService.CalculateWithholdingTaxAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for calculating withholding tax for income type {IncomeType}", 
                    request?.IncomeType);
                
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Withholding tax type or rate not found for income type {IncomeType}", 
                    request?.IncomeType);
                
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating withholding tax for income type {IncomeType} on amount {Amount}", 
                    request?.IncomeType, request?.GrossAmount);
                
                return StatusCode(500, "An error occurred while calculating withholding tax");
            }
        }

        /// <summary>
        /// Records a tax transaction
        /// </summary>
        /// <param name="createTaxTransactionDto">Tax transaction creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created tax transaction</returns>
        [HttpPost("transactions")]
        [ProducesResponseType(typeof(TaxTransactionDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,TaxManager,Accountant")]
        public async Task<IActionResult> RecordTaxTransactionAsync([FromBody] CreateTaxTransactionDto createTaxTransactionDto, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Recording tax transaction for tax type {TaxTypeId}", createTaxTransactionDto.TaxTypeId);
                var transaction = await _taxCalculationService.RecordTaxTransactionAsync(createTaxTransactionDto, cancellationToken);
                return CreatedAtAction(nameof(GetTaxTransactionByIdAsync), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for recording tax transaction for tax type {TaxTypeId}", 
                    createTaxTransactionDto?.TaxTypeId);
                
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type or rate not found for recording tax transaction with tax type {TaxTypeId}", 
                    createTaxTransactionDto?.TaxTypeId);
                
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording tax transaction for tax type {TaxTypeId}", 
                    createTaxTransactionDto?.TaxTypeId);
                
                return StatusCode(500, "An error occurred while recording the tax transaction");
            }
        }

        /// <summary>
        /// Gets tax transactions for a financial period
        /// </summary>
        /// <param name="financialPeriodId">Financial period ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of tax transactions</returns>
        [HttpGet("transactions/period/{financialPeriodId}")]
        [ProducesResponseType(typeof(IEnumerable<TaxTransactionDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTaxTransactionsForPeriodAsync(string financialPeriodId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting tax transactions for financial period {FinancialPeriodId}", financialPeriodId);
                var transactions = await _taxCalculationService.GetTaxTransactionsForPeriodAsync(financialPeriodId, cancellationToken);
                return Ok(transactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for financial period ID {FinancialPeriodId}", financialPeriodId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax transactions for financial period {FinancialPeriodId}", financialPeriodId);
                return StatusCode(500, "An error occurred while retrieving tax transactions");
            }
        }

        /// <summary>
        /// Gets tax transactions for a specific tax type in a financial period
        /// </summary>
        /// <param name="taxTypeId">Tax type ID</param>
        /// <param name="financialPeriodId">Financial period ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of tax transactions</returns>
        [HttpGet("transactions/period/{financialPeriodId}/type/{taxTypeId}")]
        [ProducesResponseType(typeof(IEnumerable<TaxTransactionDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTaxTransactionsByTypeForPeriodAsync(string taxTypeId, string financialPeriodId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting tax transactions for tax type {TaxTypeId} in financial period {FinancialPeriodId}", 
                    taxTypeId, financialPeriodId);
                
                var transactions = await _taxCalculationService.GetTaxTransactionsByTypeForPeriodAsync(
                    taxTypeId, financialPeriodId, cancellationToken);
                
                return Ok(transactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for getting tax transactions for tax type {TaxTypeId} in financial period {FinancialPeriodId}", 
                    taxTypeId, financialPeriodId);
                
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax type with ID {TaxTypeId} not found", taxTypeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax transactions for tax type {TaxTypeId} in financial period {FinancialPeriodId}", 
                    taxTypeId, financialPeriodId);
                
                return StatusCode(500, "An error occurred while retrieving tax transactions");
            }
        }

        /// <summary>
        /// Gets a tax transaction by ID
        /// </summary>
        /// <param name="id">Tax transaction ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tax transaction</returns>
        [HttpGet("transactions/{id}")]
        [ProducesResponseType(typeof(TaxTransactionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTaxTransactionByIdAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting tax transaction with ID {TransactionId}", id);
                var transaction = await _taxCalculationService.GetTaxTransactionByIdAsync(id, cancellationToken);
                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for tax transaction ID {TransactionId}", id);
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Tax transaction with ID {TransactionId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax transaction with ID {TransactionId}", id);
                return StatusCode(500, "An error occurred while retrieving the tax transaction");
            }
        }

        /// <summary>
        /// Generates a tax report
        /// </summary>
        /// <param name="request">Tax report request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tax report</returns>
        [HttpPost("reports/generate")]
        [ProducesResponseType(typeof(TaxReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "Admin,TaxManager,Accountant,Auditor")]
        public async Task<IActionResult> GenerateTaxReportAsync([FromBody] TaxReportRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Generating tax report from {StartDate} to {EndDate}", request.StartDate, request.EndDate);
                var report = await _taxCalculationService.GenerateTaxReportAsync(request, cancellationToken);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for generating tax report");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tax report");
                return StatusCode(500, "An error occurred while generating the tax report");
            }
        }
    }
}
