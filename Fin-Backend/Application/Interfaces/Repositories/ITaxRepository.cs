using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Tax;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for tax operations
    /// </summary>
    public interface ITaxRepository
    {
        /// <summary>
        /// Gets all tax types
        /// </summary>
        Task<IEnumerable<TaxType>> GetAllTaxTypesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax type by ID
        /// </summary>
        Task<TaxType> GetTaxTypeByIdAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax type by code
        /// </summary>
        Task<TaxType> GetTaxTypeByCodeAsync(string code, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new tax type
        /// </summary>
        Task<TaxType> AddTaxTypeAsync(TaxType taxType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing tax type
        /// </summary>
        Task<TaxType> UpdateTaxTypeAsync(TaxType taxType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all tax rates for a specific tax type
        /// </summary>
        Task<IEnumerable<TaxRate>> GetTaxRatesForTypeAsync(string taxTypeId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax rate by ID
        /// </summary>
        Task<TaxRate> GetTaxRateByIdAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets applicable tax rates for a date and category
        /// </summary>
        Task<IEnumerable<TaxRate>> GetApplicableTaxRatesAsync(
            string taxTypeId, 
            DateTime date, 
            string category = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new tax rate
        /// </summary>
        Task<TaxRate> AddTaxRateAsync(TaxRate taxRate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing tax rate
        /// </summary>
        Task<TaxRate> UpdateTaxRateAsync(TaxRate taxRate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tax exemptions for a party
        /// </summary>
        Task<IEnumerable<TaxExemption>> GetTaxExemptionsForPartyAsync(
            string partyId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new tax transaction
        /// </summary>
        Task<TaxTransaction> AddTaxTransactionAsync(
            TaxTransaction taxTransaction, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a tax transaction by ID
        /// </summary>
        Task<TaxTransaction> GetTaxTransactionByIdAsync(
            string id, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tax transactions for a financial period
        /// </summary>
        Task<IEnumerable<TaxTransaction>> GetTaxTransactionsForPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tax transactions for a specific tax type in a financial period
        /// </summary>
        Task<IEnumerable<TaxTransaction>> GetTaxTransactionsByTypeForPeriodAsync(
            string taxTypeId, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tax transactions for a specific time period
        /// </summary>
        Task<IEnumerable<TaxTransaction>> GetTaxTransactionsByDateRangeAsync(
            DateTime startDate, 
            DateTime endDate, 
            string taxTypeId = null,
            bool? isSettled = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates the settlement status of a tax transaction
        /// </summary>
        Task<TaxTransaction> UpdateTaxTransactionSettlementAsync(
            string id, 
            bool isSettled, 
            DateTime? settlementDate,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets withholding tax rate for a specific income type
        /// </summary>
        Task<TaxRate> GetWithholdingTaxRateAsync(
            string incomeType, 
            bool isResident, 
            DateTime date,
            CancellationToken cancellationToken = default);
    }
}
