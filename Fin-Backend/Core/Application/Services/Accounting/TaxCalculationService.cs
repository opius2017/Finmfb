using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Core.Application.DTOs.Tax;
using FinTech.Core.Application.Exceptions;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Entities.Tax;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Accounting
{
    /// <summary>
    /// Service implementation for tax calculation and management
    /// </summary>
    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly ITaxRepository _taxRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaxCalculationService> _logger;
        private readonly IJournalEntryService _journalEntryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxCalculationService(
            ITaxRepository taxRepository,
            IMapper mapper,
            ILogger<TaxCalculationService> logger,
            IJournalEntryService journalEntryService)
        {
            _taxRepository = taxRepository ?? throw new ArgumentNullException(nameof(taxRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxTypeDto>> GetAllTaxTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting all tax types");
                var taxTypes = await _taxRepository.GetAllTaxTypesAsync(cancellationToken);
                return _mapper.Map<IEnumerable<TaxTypeDto>>(taxTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tax types");
                throw new ApplicationException("Failed to retrieve tax types", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxTypeDto> GetTaxTypeByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax type with ID {TaxTypeId}", id);
                
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(id));
                }
                
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(id, cancellationToken);
                
                if (taxType == null)
                {
                    _logger.LogWarning("Tax type with ID {TaxTypeId} not found", id);
                    throw new NotFoundException($"Tax type with ID {id} not found");
                }
                
                return _mapper.Map<TaxTypeDto>(taxType);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax type with ID {TaxTypeId}", id);
                throw new ApplicationException($"Failed to retrieve tax type with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxTypeDto> CreateTaxTypeAsync(CreateTaxTypeDto createTaxTypeDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating new tax type with code {TaxTypeCode}", createTaxTypeDto.Code);
                
                // Validate input
                if (createTaxTypeDto == null)
                {
                    throw new ArgumentNullException(nameof(createTaxTypeDto));
                }
                
                if (string.IsNullOrWhiteSpace(createTaxTypeDto.Code))
                {
                    throw new ArgumentException("Tax type code cannot be null or empty", nameof(createTaxTypeDto.Code));
                }
                
                // Check if tax type with the same code already exists
                var existingTaxType = await _taxRepository.GetTaxTypeByCodeAsync(createTaxTypeDto.Code, cancellationToken);
                if (existingTaxType != null)
                {
                    _logger.LogWarning("Tax type with code {TaxTypeCode} already exists", createTaxTypeDto.Code);
                    throw new DuplicateEntityException($"Tax type with code {createTaxTypeDto.Code} already exists");
                }
                
                // Map and save
                var taxType = _mapper.Map<TaxType>(createTaxTypeDto);
                taxType.IsActive = true;
                
                var result = await _taxRepository.AddTaxTypeAsync(taxType, cancellationToken);
                return _mapper.Map<TaxTypeDto>(result);
            }
            catch (DuplicateEntityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax type with code {TaxTypeCode}", createTaxTypeDto?.Code);
                throw new ApplicationException($"Failed to create tax type with code {createTaxTypeDto?.Code}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxTypeDto> UpdateTaxTypeAsync(UpdateTaxTypeDto updateTaxTypeDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating tax type with ID {TaxTypeId}", updateTaxTypeDto.Id);
                
                // Validate input
                if (updateTaxTypeDto == null)
                {
                    throw new ArgumentNullException(nameof(updateTaxTypeDto));
                }
                
                if (string.IsNullOrWhiteSpace(updateTaxTypeDto.Id))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(updateTaxTypeDto.Id));
                }
                
                // Get existing tax type
                var existingTaxType = await _taxRepository.GetTaxTypeByIdAsync(updateTaxTypeDto.Id, cancellationToken);
                if (existingTaxType == null)
                {
                    _logger.LogWarning("Tax type with ID {TaxTypeId} not found", updateTaxTypeDto.Id);
                    throw new NotFoundException($"Tax type with ID {updateTaxTypeDto.Id} not found");
                }
                
                // Update properties
                existingTaxType.Name = updateTaxTypeDto.Name;
                existingTaxType.Description = updateTaxTypeDto.Description;
                existingTaxType.IsActive = updateTaxTypeDto.IsActive;
                existingTaxType.LiabilityAccountId = updateTaxTypeDto.LiabilityAccountId;
                existingTaxType.ReceivableAccountId = updateTaxTypeDto.ReceivableAccountId;
                existingTaxType.Direction = updateTaxTypeDto.Direction;
                existingTaxType.IsReclaimable = updateTaxTypeDto.IsReclaimable;
                existingTaxType.RegulatoryAuthority = updateTaxTypeDto.RegulatoryAuthority;
                
                // Save changes
                var result = await _taxRepository.UpdateTaxTypeAsync(existingTaxType, cancellationToken);
                return _mapper.Map<TaxTypeDto>(result);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax type with ID {TaxTypeId}", updateTaxTypeDto?.Id);
                throw new ApplicationException($"Failed to update tax type with ID {updateTaxTypeDto?.Id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxRateDto>> GetTaxRatesForTypeAsync(string taxTypeId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax rates for tax type {TaxTypeId}", taxTypeId);
                
                if (string.IsNullOrWhiteSpace(taxTypeId))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(taxTypeId));
                }
                
                var taxRates = await _taxRepository.GetTaxRatesForTypeAsync(taxTypeId, cancellationToken);
                return _mapper.Map<IEnumerable<TaxRateDto>>(taxRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax rates for tax type {TaxTypeId}", taxTypeId);
                throw new ApplicationException($"Failed to retrieve tax rates for tax type {taxTypeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxRateDto> GetTaxRateByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax rate with ID {TaxRateId}", id);
                
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Tax rate ID cannot be null or empty", nameof(id));
                }
                
                var taxRate = await _taxRepository.GetTaxRateByIdAsync(id, cancellationToken);
                
                if (taxRate == null)
                {
                    _logger.LogWarning("Tax rate with ID {TaxRateId} not found", id);
                    throw new NotFoundException($"Tax rate with ID {id} not found");
                }
                
                return _mapper.Map<TaxRateDto>(taxRate);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax rate with ID {TaxRateId}", id);
                throw new ApplicationException($"Failed to retrieve tax rate with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxRateDto> CreateTaxRateAsync(CreateTaxRateDto createTaxRateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating new tax rate for tax type {TaxTypeId}", createTaxRateDto.TaxTypeId);
                
                // Validate input
                if (createTaxRateDto == null)
                {
                    throw new ArgumentNullException(nameof(createTaxRateDto));
                }
                
                if (string.IsNullOrWhiteSpace(createTaxRateDto.TaxTypeId))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(createTaxRateDto.TaxTypeId));
                }
                
                // Check if tax type exists
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(createTaxRateDto.TaxTypeId, cancellationToken);
                if (taxType == null)
                {
                    _logger.LogWarning("Tax type with ID {TaxTypeId} not found", createTaxRateDto.TaxTypeId);
                    throw new NotFoundException($"Tax type with ID {createTaxRateDto.TaxTypeId} not found");
                }
                
                // Validate rate
                if (createTaxRateDto.Rate < 0)
                {
                    throw new ArgumentException("Tax rate cannot be negative", nameof(createTaxRateDto.Rate));
                }
                
                // Validate amount range
                if (createTaxRateDto.MinimumAmount.HasValue && createTaxRateDto.MaximumAmount.HasValue 
                    && createTaxRateDto.MinimumAmount > createTaxRateDto.MaximumAmount)
                {
                    throw new ArgumentException("Minimum amount cannot be greater than maximum amount");
                }
                
                // Validate date range
                if (createTaxRateDto.EndDate.HasValue && createTaxRateDto.EffectiveDate > createTaxRateDto.EndDate)
                {
                    throw new ArgumentException("Effective date cannot be later than end date");
                }
                
                // Map and save
                var taxRate = _mapper.Map<TaxRate>(createTaxRateDto);
                taxRate.IsActive = true;
                taxRate.TaxTypeId = createTaxRateDto.TaxTypeId;
                
                var result = await _taxRepository.AddTaxRateAsync(taxRate, cancellationToken);
                
                // Fetch the tax type code for the response
                var mappedResult = _mapper.Map<TaxRateDto>(result);
                mappedResult.TaxTypeCode = taxType.Code;
                
                return mappedResult;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax rate for tax type {TaxTypeId}", createTaxRateDto?.TaxTypeId);
                throw new ApplicationException($"Failed to create tax rate for tax type {createTaxRateDto?.TaxTypeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxRateDto> UpdateTaxRateAsync(UpdateTaxRateDto updateTaxRateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating tax rate with ID {TaxRateId}", updateTaxRateDto.Id);
                
                // Validate input
                if (updateTaxRateDto == null)
                {
                    throw new ArgumentNullException(nameof(updateTaxRateDto));
                }
                
                if (string.IsNullOrWhiteSpace(updateTaxRateDto.Id))
                {
                    throw new ArgumentException("Tax rate ID cannot be null or empty", nameof(updateTaxRateDto.Id));
                }
                
                // Get existing tax rate
                var existingTaxRate = await _taxRepository.GetTaxRateByIdAsync(updateTaxRateDto.Id, cancellationToken);
                if (existingTaxRate == null)
                {
                    _logger.LogWarning("Tax rate with ID {TaxRateId} not found", updateTaxRateDto.Id);
                    throw new NotFoundException($"Tax rate with ID {updateTaxRateDto.Id} not found");
                }
                
                // Validate rate
                if (updateTaxRateDto.Rate < 0)
                {
                    throw new ArgumentException("Tax rate cannot be negative", nameof(updateTaxRateDto.Rate));
                }
                
                // Validate amount range
                if (updateTaxRateDto.MinimumAmount.HasValue && updateTaxRateDto.MaximumAmount.HasValue 
                    && updateTaxRateDto.MinimumAmount > updateTaxRateDto.MaximumAmount)
                {
                    throw new ArgumentException("Minimum amount cannot be greater than maximum amount");
                }
                
                // Validate date range
                if (updateTaxRateDto.EndDate.HasValue && updateTaxRateDto.EffectiveDate > updateTaxRateDto.EndDate)
                {
                    throw new ArgumentException("Effective date cannot be later than end date");
                }
                
                // Update properties
                existingTaxRate.Name = updateTaxRateDto.Name;
                existingTaxRate.Rate = updateTaxRateDto.Rate;
                existingTaxRate.MinimumAmount = updateTaxRateDto.MinimumAmount;
                existingTaxRate.MaximumAmount = updateTaxRateDto.MaximumAmount;
                existingTaxRate.EffectiveDate = updateTaxRateDto.EffectiveDate;
                existingTaxRate.EndDate = updateTaxRateDto.EndDate;
                existingTaxRate.IsActive = updateTaxRateDto.IsActive;
                existingTaxRate.ApplicableCategory = updateTaxRateDto.ApplicableCategory;
                
                // Save changes
                var result = await _taxRepository.UpdateTaxRateAsync(existingTaxRate, cancellationToken);
                
                // Fetch the tax type code for the response
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(result.TaxTypeId, cancellationToken);
                var mappedResult = _mapper.Map<TaxRateDto>(result);
                mappedResult.TaxTypeCode = taxType.Code;
                
                return mappedResult;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax rate with ID {TaxRateId}", updateTaxRateDto?.Id);
                throw new ApplicationException($"Failed to update tax rate with ID {updateTaxRateDto?.Id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxCalculationResultDto> CalculateTaxAsync(TaxCalculationRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calculating tax for type {TaxTypeId} on amount {Amount}", request.TaxTypeId, request.TaxableAmount);
                
                // Validate input
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }
                
                if (string.IsNullOrWhiteSpace(request.TaxTypeId))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(request.TaxTypeId));
                }
                
                if (request.TaxableAmount < 0)
                {
                    throw new ArgumentException("Taxable amount cannot be negative", nameof(request.TaxableAmount));
                }
                
                // Get tax type
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(request.TaxTypeId, cancellationToken);
                if (taxType == null)
                {
                    _logger.LogWarning("Tax type with ID {TaxTypeId} not found", request.TaxTypeId);
                    throw new NotFoundException($"Tax type with ID {request.TaxTypeId} not found");
                }
                
                // Prepare result
                var result = new TaxCalculationResultDto
                {
                    TaxTypeId = taxType.Id,
                    TaxTypeCode = taxType.Code,
                    TaxTypeName = taxType.Name,
                    TaxableAmount = request.TaxableAmount,
                    TransactionDate = request.TransactionDate,
                    TaxAccountId = taxType.Direction == TaxDirection.Input ? taxType.ReceivableAccountId : taxType.LiabilityAccountId
                };
                
                // Check for exemptions if party ID is provided
                if (!string.IsNullOrWhiteSpace(request.PartyId))
                {
                    var exemptions = await _taxRepository.GetTaxExemptionsForPartyAsync(request.PartyId, cancellationToken);
                    var validExemption = exemptions.FirstOrDefault(e => 
                        (e.TaxTypeId == request.TaxTypeId || e.TaxTypeId == null) &&
                        e.IsActive &&
                        e.StartDate <= request.TransactionDate &&
                        (!e.EndDate.HasValue || e.EndDate >= request.TransactionDate));
                    
                    if (validExemption != null)
                    {
                        result.IsExempt = true;
                        result.ExemptionReason = validExemption.Reason;
                        result.AppliedRate = 0;
                        result.TaxAmount = 0;
                        result.TotalAmount = request.TaxableAmount;
                        result.Notes = $"Tax exempt: {validExemption.Reason}";
                        
                        return result;
                    }
                }
                
                // Get applicable tax rate
                TaxRate taxRate;
                
                if (!string.IsNullOrWhiteSpace(request.TaxRateId))
                {
                    // Use specified tax rate
                    taxRate = await _taxRepository.GetTaxRateByIdAsync(request.TaxRateId, cancellationToken);
                    if (taxRate == null)
                    {
                        _logger.LogWarning("Tax rate with ID {TaxRateId} not found", request.TaxRateId);
                        throw new NotFoundException($"Tax rate with ID {request.TaxRateId} not found");
                    }
                    
                    // Verify the tax rate belongs to the specified tax type
                    if (taxRate.TaxTypeId != request.TaxTypeId)
                    {
                        throw new InvalidOperationException($"Tax rate with ID {request.TaxRateId} does not belong to tax type with ID {request.TaxTypeId}");
                    }
                    
                    // Check if the rate is active and valid for the transaction date
                    if (!taxRate.IsActive || 
                        taxRate.EffectiveDate > request.TransactionDate ||
                        (taxRate.EndDate.HasValue && taxRate.EndDate < request.TransactionDate))
                    {
                        _logger.LogWarning("Tax rate with ID {TaxRateId} is not active or not valid for date {TransactionDate}", 
                            request.TaxRateId, request.TransactionDate);
                        throw new InvalidOperationException($"Tax rate with ID {request.TaxRateId} is not active or not valid for the transaction date");
                    }
                }
                else
                {
                    // Find applicable tax rate based on date and category
                    var applicableRates = await _taxRepository.GetApplicableTaxRatesAsync(
                        request.TaxTypeId, 
                        request.TransactionDate, 
                        request.Category,
                        cancellationToken);
                    
                    // Filter by amount range if provided
                    taxRate = applicableRates
                        .Where(r => 
                            (!r.MinimumAmount.HasValue || r.MinimumAmount <= request.TaxableAmount) &&
                            (!r.MaximumAmount.HasValue || r.MaximumAmount >= request.TaxableAmount))
                        .OrderByDescending(r => r.EffectiveDate)
                        .FirstOrDefault();
                    
                    if (taxRate == null)
                    {
                        _logger.LogWarning("No applicable tax rate found for tax type {TaxTypeId}, date {TransactionDate}, and category {Category}", 
                            request.TaxTypeId, request.TransactionDate, request.Category);
                        throw new NotFoundException($"No applicable tax rate found for the given parameters");
                    }
                }
                
                // Calculate tax
                result.TaxRateId = taxRate.Id;
                result.AppliedRate = taxRate.Rate;
                
                if (request.IsTaxInclusive)
                {
                    // Calculate tax from tax-inclusive amount
                    // Formula: Tax Amount = Total Amount - (Total Amount / (1 + Rate))
                    result.TaxAmount = Math.Round(request.TaxableAmount - (request.TaxableAmount / (1 + (taxRate.Rate / 100))), 2);
                    result.TaxableAmount = Math.Round(request.TaxableAmount - result.TaxAmount, 2);
                    result.TotalAmount = request.TaxableAmount;
                }
                else
                {
                    // Calculate tax from tax-exclusive amount
                    // Formula: Tax Amount = Taxable Amount * Rate / 100
                    result.TaxAmount = Math.Round(request.TaxableAmount * (taxRate.Rate / 100), 2);
                    result.TotalAmount = Math.Round(request.TaxableAmount + result.TaxAmount, 2);
                }
                
                return result;
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
                _logger.LogError(ex, "Error calculating tax for type {TaxTypeId} on amount {Amount}", 
                    request?.TaxTypeId, request?.TaxableAmount);
                throw new ApplicationException("Failed to calculate tax", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxCalculationResultDto>> CalculateBatchTaxAsync(IEnumerable<TaxCalculationRequestDto> requests, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calculating batch tax for {Count} items", requests.Count());
                
                // Validate input
                if (requests == null)
                {
                    throw new ArgumentNullException(nameof(requests));
                }
                
                if (!requests.Any())
                {
                    return Enumerable.Empty<TaxCalculationResultDto>();
                }
                
                // Calculate tax for each request
                var results = new List<TaxCalculationResultDto>();
                foreach (var request in requests)
                {
                    try
                    {
                        var result = await CalculateTaxAsync(request, cancellationToken);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error calculating tax for item in batch with tax type {TaxTypeId}. Skipping.", 
                            request.TaxTypeId);
                        
                        // Add error result
                        results.Add(new TaxCalculationResultDto
                        {
                            TaxTypeId = request.TaxTypeId,
                            TaxableAmount = request.TaxableAmount,
                            TransactionDate = request.TransactionDate,
                            TaxAmount = 0,
                            TotalAmount = request.TaxableAmount,
                            Notes = $"Error: {ex.Message}"
                        });
                    }
                }
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating batch tax");
                throw new ApplicationException("Failed to calculate batch tax", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<WithholdingTaxResultDto> CalculateWithholdingTaxAsync(WithholdingTaxRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calculating withholding tax for income type {IncomeType} on amount {Amount}", 
                    request.IncomeType, request.GrossAmount);
                
                // Validate input
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }
                
                if (string.IsNullOrWhiteSpace(request.IncomeType))
                {
                    throw new ArgumentException("Income type cannot be null or empty", nameof(request.IncomeType));
                }
                
                if (request.GrossAmount < 0)
                {
                    throw new ArgumentException("Gross amount cannot be negative", nameof(request.GrossAmount));
                }
                
                // Get WHT tax type
                var whtTaxType = await _taxRepository.GetTaxTypeByCodeAsync("WHT", cancellationToken);
                if (whtTaxType == null)
                {
                    _logger.LogWarning("Withholding tax type not found");
                    throw new NotFoundException("Withholding tax type not found");
                }
                
                // Get applicable WHT rate for the income type and residency status
                var whtRate = await _taxRepository.GetWithholdingTaxRateAsync(
                    request.IncomeType, 
                    request.IsResident, 
                    request.TransactionDate,
                    cancellationToken);
                
                if (whtRate == null)
                {
                    _logger.LogWarning("No withholding tax rate found for income type {IncomeType} and residency status {IsResident}", 
                        request.IncomeType, request.IsResident);
                    throw new NotFoundException($"No withholding tax rate found for income type {request.IncomeType}");
                }
                
                // Calculate WHT
                decimal whtAmount = Math.Round(request.GrossAmount * (whtRate.Rate / 100), 2);
                decimal netAmount = Math.Round(request.GrossAmount - whtAmount, 2);
                
                // Prepare result
                var result = new WithholdingTaxResultDto
                {
                    TaxTypeId = whtTaxType.Id,
                    TaxTypeCode = whtTaxType.Code,
                    TaxRateId = whtRate.Id,
                    AppliedRate = whtRate.Rate,
                    GrossAmount = request.GrossAmount,
                    WithholdingAmount = whtAmount,
                    NetAmount = netAmount,
                    IncomeType = request.IncomeType,
                    WhtLiabilityAccountId = whtTaxType.LiabilityAccountId,
                    Notes = $"WHT {whtRate.Rate}% for {request.IncomeType}"
                };
                
                return result;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating withholding tax for income type {IncomeType} on amount {Amount}", 
                    request?.IncomeType, request?.GrossAmount);
                throw new ApplicationException("Failed to calculate withholding tax", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxTransactionDto> RecordTaxTransactionAsync(CreateTaxTransactionDto createTaxTransactionDto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Recording tax transaction for tax type {TaxTypeId}", createTaxTransactionDto.TaxTypeId);
                
                // Validate input
                if (createTaxTransactionDto == null)
                {
                    throw new ArgumentNullException(nameof(createTaxTransactionDto));
                }
                
                if (string.IsNullOrWhiteSpace(createTaxTransactionDto.TaxTypeId))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(createTaxTransactionDto.TaxTypeId));
                }
                
                if (string.IsNullOrWhiteSpace(createTaxTransactionDto.TaxRateId))
                {
                    throw new ArgumentException("Tax rate ID cannot be null or empty", nameof(createTaxTransactionDto.TaxRateId));
                }
                
                // Get tax type and rate
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(createTaxTransactionDto.TaxTypeId, cancellationToken);
                if (taxType == null)
                {
                    _logger.LogWarning("Tax type with ID {TaxTypeId} not found", createTaxTransactionDto.TaxTypeId);
                    throw new NotFoundException($"Tax type with ID {createTaxTransactionDto.TaxTypeId} not found");
                }
                
                var taxRate = await _taxRepository.GetTaxRateByIdAsync(createTaxTransactionDto.TaxRateId, cancellationToken);
                if (taxRate == null)
                {
                    _logger.LogWarning("Tax rate with ID {TaxRateId} not found", createTaxTransactionDto.TaxRateId);
                    throw new NotFoundException($"Tax rate with ID {createTaxTransactionDto.TaxRateId} not found");
                }
                
                // Map and save
                var taxTransaction = _mapper.Map<TaxTransaction>(createTaxTransactionDto);
                taxTransaction.IsSettled = false;
                
                // Create journal entry if requested
                if (createTaxTransactionDto.CreateJournalEntry)
                {
                    // Determine accounts based on tax direction
                    string taxAccountId = taxType.Direction == TaxDirection.Input 
                        ? taxType.ReceivableAccountId 
                        : taxType.LiabilityAccountId;
                    
                    // For this simplified example, we'll just log the journal entry creation
                    // In a real implementation, this would create a proper journal entry
                    _logger.LogInformation("Creating journal entry for tax transaction: Debit {DebitAccount} {Amount}, Credit {CreditAccount} {Amount}",
                        taxType.Direction == TaxDirection.Input ? taxAccountId : "SourceAccount",
                        taxTransaction.TaxAmount,
                        taxType.Direction == TaxDirection.Input ? "SourceAccount" : taxAccountId,
                        taxTransaction.TaxAmount);
                    
                    // Set journal entry ID (would come from the actual journal entry creation)
                    taxTransaction.JournalEntryId = Guid.NewGuid().ToString();
                }
                
                var result = await _taxRepository.AddTaxTransactionAsync(taxTransaction, cancellationToken);
                
                // Map to DTO
                var mappedResult = _mapper.Map<TaxTransactionDto>(result);
                mappedResult.TaxTypeCode = taxType.Code;
                
                return mappedResult;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording tax transaction for tax type {TaxTypeId}", 
                    createTaxTransactionDto?.TaxTypeId);
                throw new ApplicationException("Failed to record tax transaction", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxTransactionDto>> GetTaxTransactionsForPeriodAsync(string financialPeriodId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax transactions for financial period {FinancialPeriodId}", financialPeriodId);
                
                if (string.IsNullOrWhiteSpace(financialPeriodId))
                {
                    throw new ArgumentException("Financial period ID cannot be null or empty", nameof(financialPeriodId));
                }
                
                var transactions = await _taxRepository.GetTaxTransactionsForPeriodAsync(financialPeriodId, cancellationToken);
                
                // Get all tax types to include codes in response
                var taxTypes = await _taxRepository.GetAllTaxTypesAsync(cancellationToken);
                var taxTypeDict = taxTypes.ToDictionary(t => t.Id, t => t.Code);
                
                var result = _mapper.Map<IEnumerable<TaxTransactionDto>>(transactions);
                
                // Set tax type codes
                foreach (var transaction in result)
                {
                    if (taxTypeDict.TryGetValue(transaction.TaxTypeId, out var code))
                    {
                        transaction.TaxTypeCode = code;
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax transactions for financial period {FinancialPeriodId}", 
                    financialPeriodId);
                throw new ApplicationException($"Failed to retrieve tax transactions for financial period {financialPeriodId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxTransactionDto>> GetTaxTransactionsByTypeForPeriodAsync(string taxTypeId, string financialPeriodId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax transactions for tax type {TaxTypeId} in financial period {FinancialPeriodId}", 
                    taxTypeId, financialPeriodId);
                
                if (string.IsNullOrWhiteSpace(taxTypeId))
                {
                    throw new ArgumentException("Tax type ID cannot be null or empty", nameof(taxTypeId));
                }
                
                if (string.IsNullOrWhiteSpace(financialPeriodId))
                {
                    throw new ArgumentException("Financial period ID cannot be null or empty", nameof(financialPeriodId));
                }
                
                // Get tax type to include code in response
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(taxTypeId, cancellationToken);
                if (taxType == null)
                {
                    _logger.LogWarning("Tax type with ID {TaxTypeId} not found", taxTypeId);
                    throw new NotFoundException($"Tax type with ID {taxTypeId} not found");
                }
                
                var transactions = await _taxRepository.GetTaxTransactionsByTypeForPeriodAsync(
                    taxTypeId, financialPeriodId, cancellationToken);
                
                var result = _mapper.Map<IEnumerable<TaxTransactionDto>>(transactions);
                
                // Set tax type code
                foreach (var transaction in result)
                {
                    transaction.TaxTypeCode = taxType.Code;
                }
                
                return result;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax transactions for tax type {TaxTypeId} in financial period {FinancialPeriodId}", 
                    taxTypeId, financialPeriodId);
                throw new ApplicationException($"Failed to retrieve tax transactions", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxTransactionDto> GetTaxTransactionByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting tax transaction with ID {TransactionId}", id);
                
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Tax transaction ID cannot be null or empty", nameof(id));
                }
                
                var transaction = await _taxRepository.GetTaxTransactionByIdAsync(id, cancellationToken);
                
                if (transaction == null)
                {
                    _logger.LogWarning("Tax transaction with ID {TransactionId} not found", id);
                    throw new NotFoundException($"Tax transaction with ID {id} not found");
                }
                
                // Get tax type to include code in response
                var taxType = await _taxRepository.GetTaxTypeByIdAsync(transaction.TaxTypeId, cancellationToken);
                
                var result = _mapper.Map<TaxTransactionDto>(transaction);
                result.TaxTypeCode = taxType.Code;
                
                return result;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax transaction with ID {TransactionId}", id);
                throw new ApplicationException($"Failed to retrieve tax transaction with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TaxReportDto> GenerateTaxReportAsync(TaxReportRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating tax report from {StartDate} to {EndDate}", request.StartDate, request.EndDate);
                
                // Validate input
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }
                
                // Determine date range
                DateTime startDate, endDate;
                string periodDescription;
                
                if (!string.IsNullOrWhiteSpace(request.FinancialPeriodId))
                {
                    // Use financial period
                    // In a real implementation, we would get the start and end dates from the financial period
                    // For this example, we'll just use the provided dates
                    startDate = request.StartDate;
                    endDate = request.EndDate;
                    periodDescription = $"Financial Period: {request.FinancialPeriodId}";
                }
                else
                {
                    // Use date range
                    startDate = request.StartDate;
                    endDate = request.EndDate;
                    periodDescription = $"From {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
                }
                
                // Get tax transactions
                IEnumerable<TaxTransaction> transactions;
                if (!string.IsNullOrWhiteSpace(request.FinancialPeriodId))
                {
                    if (!string.IsNullOrWhiteSpace(request.TaxTypeId))
                    {
                        transactions = await _taxRepository.GetTaxTransactionsByTypeForPeriodAsync(
                            request.TaxTypeId, request.FinancialPeriodId, cancellationToken);
                    }
                    else
                    {
                        transactions = await _taxRepository.GetTaxTransactionsForPeriodAsync(
                            request.FinancialPeriodId, cancellationToken);
                    }
                }
                else
                {
                    transactions = await _taxRepository.GetTaxTransactionsByDateRangeAsync(
                        startDate, endDate, request.TaxTypeId, request.IsSettled, cancellationToken);
                }
                
                // Filter by settlement status if specified
                if (request.IsSettled.HasValue)
                {
                    transactions = transactions.Where(t => t.IsSettled == request.IsSettled.Value);
                }
                
                // Prepare report
                var report = new TaxReportDto
                {
                    Title = !string.IsNullOrWhiteSpace(request.TaxTypeId) 
                        ? $"Tax Report - {request.TaxTypeId}" 
                        : "Tax Report - All Taxes",
                    PeriodDescription = periodDescription,
                    StartDate = startDate,
                    EndDate = endDate,
                    GeneratedDate = DateTime.UtcNow
                };
                
                // Get all tax types for the report
                var taxTypes = await _taxRepository.GetAllTaxTypesAsync(cancellationToken);
                var taxTypeDict = taxTypes.ToDictionary(t => t.Id, t => t);
                
                // Add details
                report.Details = _mapper.Map<List<TaxTransactionDto>>(transactions);
                
                // Set tax type codes in details
                foreach (var transaction in report.Details)
                {
                    if (taxTypeDict.TryGetValue(transaction.TaxTypeId, out var taxType))
                    {
                        transaction.TaxTypeCode = taxType.Code;
                    }
                }
                
                // Generate summaries by tax type
                var summaryGroups = transactions
                    .GroupBy(t => t.TaxTypeId)
                    .Select(g => new TaxReportSummaryDto
                    {
                        TaxTypeId = g.Key,
                        TaxTypeCode = taxTypeDict.ContainsKey(g.Key) ? taxTypeDict[g.Key].Code : "Unknown",
                        TaxTypeName = taxTypeDict.ContainsKey(g.Key) ? taxTypeDict[g.Key].Name : "Unknown",
                        TaxableAmount = g.Sum(t => t.TaxableAmount),
                        TaxAmount = g.Sum(t => t.TaxAmount),
                        SettledAmount = g.Where(t => t.IsSettled).Sum(t => t.TaxAmount),
                        OutstandingAmount = g.Where(t => !t.IsSettled).Sum(t => t.TaxAmount),
                        TransactionCount = g.Count()
                    })
                    .ToList();
                
                report.Summaries = summaryGroups;
                
                // Calculate totals
                report.TotalTaxableAmount = summaryGroups.Sum(s => s.TaxableAmount);
                report.TotalTaxAmount = summaryGroups.Sum(s => s.TaxAmount);
                report.TotalSettledAmount = summaryGroups.Sum(s => s.SettledAmount);
                report.TotalOutstandingAmount = summaryGroups.Sum(s => s.OutstandingAmount);
                
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tax report");
                throw new ApplicationException("Failed to generate tax report", ex);
            }
        }
    }
}
