using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Tax;

namespace FinTech.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Interface for tax calculation and management service
    /// </summary>
    public interface ITaxCalculationService
    {
        /// <summary>
        /// Gets all tax types defined in the system
        /// </summary>
        Task<IEnumerable<TaxTypeDto>> GetAllTaxTypesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax type by its ID
        /// </summary>
        Task<TaxTypeDto> GetTaxTypeByIdAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new tax type
        /// </summary>
        Task<TaxTypeDto> CreateTaxTypeAsync(CreateTaxTypeDto createTaxTypeDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing tax type
        /// </summary>
        Task<TaxTypeDto> UpdateTaxTypeAsync(UpdateTaxTypeDto updateTaxTypeDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all tax rates for a specific tax type
        /// </summary>
        Task<IEnumerable<TaxRateDto>> GetTaxRatesForTypeAsync(string taxTypeId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax rate by its ID
        /// </summary>
        Task<TaxRateDto> GetTaxRateByIdAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new tax rate for a tax type
        /// </summary>
        Task<TaxRateDto> CreateTaxRateAsync(CreateTaxRateDto createTaxRateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing tax rate
        /// </summary>
        Task<TaxRateDto> UpdateTaxRateAsync(UpdateTaxRateDto updateTaxRateDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates tax for a single taxable item
        /// </summary>
        Task<TaxCalculationResultDto> CalculateTaxAsync(
            TaxCalculationRequestDto request, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates tax for multiple taxable items in a batch
        /// </summary>
        Task<IEnumerable<TaxCalculationResultDto>> CalculateBatchTaxAsync(
            IEnumerable<TaxCalculationRequestDto> requests, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates withholding tax for a transaction
        /// </summary>
        Task<WithholdingTaxResultDto> CalculateWithholdingTaxAsync(
            WithholdingTaxRequestDto request, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Records a tax transaction
        /// </summary>
        Task<TaxTransactionDto> RecordTaxTransactionAsync(
            CreateTaxTransactionDto createTaxTransactionDto, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tax transactions for a financial period
        /// </summary>
        Task<IEnumerable<TaxTransactionDto>> GetTaxTransactionsForPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tax transactions for a specific tax type in a financial period
        /// </summary>
        Task<IEnumerable<TaxTransactionDto>> GetTaxTransactionsByTypeForPeriodAsync(
            string taxTypeId, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax transaction by its ID
        /// </summary>
        Task<TaxTransactionDto> GetTaxTransactionByIdAsync(
            string id, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generates a tax report for a financial period
        /// </summary>
        Task<TaxReportDto> GenerateTaxReportAsync(
            TaxReportRequestDto request, 
            CancellationToken cancellationToken = default);
    }
}
