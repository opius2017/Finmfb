using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Domain.Entities.Tax;
using FinTech.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for tax operations
    /// </summary>
    public class TaxRepository : ITaxRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<TaxRepository> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxRepository(ApplicationDbContext dbContext, ILogger<TaxRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxType>> GetAllTaxTypesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving all tax types");
            return await _dbContext.TaxTypes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxType> GetTaxTypeByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax type with ID {TaxTypeId}", id);
            return await _dbContext.TaxTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxType> GetTaxTypeByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax type with code {TaxTypeCode}", code);
            return await _dbContext.TaxTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Code == code, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxType> AddTaxTypeAsync(TaxType taxType, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding new tax type with code {TaxTypeCode}", taxType.Code);
            
            // Generate ID if not provided
            if (string.IsNullOrEmpty(taxType.Id))
            {
                taxType.Id = Guid.NewGuid().ToString();
            }
            
            // Set creation timestamp
            taxType.CreatedDate = DateTime.UtcNow;
            
            await _dbContext.TaxTypes.AddAsync(taxType, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return taxType;
        }

        /// <inheritdoc/>
        public async Task<TaxType> UpdateTaxTypeAsync(TaxType taxType, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating tax type with ID {TaxTypeId}", taxType.Id);
            
            // Set last modified timestamp
            taxType.LastModifiedDate = DateTime.UtcNow;
            
            _dbContext.TaxTypes.Update(taxType);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return taxType;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxRate>> GetTaxRatesForTypeAsync(string taxTypeId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax rates for tax type {TaxTypeId}", taxTypeId);
            return await _dbContext.TaxRates
                .AsNoTracking()
                .Where(r => r.TaxTypeId == taxTypeId)
                .OrderByDescending(r => r.EffectiveDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxRate> GetTaxRateByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax rate with ID {TaxRateId}", id);
            return await _dbContext.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxRate>> GetApplicableTaxRatesAsync(
            string taxTypeId, 
            DateTime date, 
            string category = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving applicable tax rates for tax type {TaxTypeId} on date {Date}", taxTypeId, date);
            
            var query = _dbContext.TaxRates
                .AsNoTracking()
                .Where(r => r.TaxTypeId == taxTypeId && 
                           r.IsActive &&
                           r.EffectiveDate <= date &&
                           (!r.EndDate.HasValue || r.EndDate >= date));
            
            // Filter by category if provided
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.ApplicableCategory == category || r.ApplicableCategory == null);
            }
            
            return await query
                .OrderByDescending(r => r.EffectiveDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxRate> AddTaxRateAsync(TaxRate taxRate, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding new tax rate for tax type {TaxTypeId}", taxRate.TaxTypeId);
            
            // Generate ID if not provided
            if (string.IsNullOrEmpty(taxRate.Id))
            {
                taxRate.Id = Guid.NewGuid().ToString();
            }
            
            // Set creation timestamp
            taxRate.CreatedDate = DateTime.UtcNow;
            
            await _dbContext.TaxRates.AddAsync(taxRate, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return taxRate;
        }

        /// <inheritdoc/>
        public async Task<TaxRate> UpdateTaxRateAsync(TaxRate taxRate, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating tax rate with ID {TaxRateId}", taxRate.Id);
            
            // Set last modified timestamp
            taxRate.LastModifiedDate = DateTime.UtcNow;
            
            _dbContext.TaxRates.Update(taxRate);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return taxRate;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxExemption>> GetTaxExemptionsForPartyAsync(
            string partyId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax exemptions for party {PartyId}", partyId);
            return await _dbContext.TaxExemptions
                .AsNoTracking()
                .Where(e => e.PartyId == partyId && e.IsActive)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxTransaction> AddTaxTransactionAsync(
            TaxTransaction taxTransaction, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding new tax transaction for tax type {TaxTypeId}", taxTransaction.TaxTypeId);
            
            // Generate ID if not provided
            if (string.IsNullOrEmpty(taxTransaction.Id))
            {
                taxTransaction.Id = Guid.NewGuid().ToString();
            }
            
            // Set creation timestamp
            taxTransaction.CreatedDate = DateTime.UtcNow;
            
            await _dbContext.TaxTransactions.AddAsync(taxTransaction, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return taxTransaction;
        }

        /// <inheritdoc/>
        public async Task<TaxTransaction> GetTaxTransactionByIdAsync(
            string id, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax transaction with ID {TransactionId}", id);
            return await _dbContext.TaxTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxTransaction>> GetTaxTransactionsForPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax transactions for financial period {FinancialPeriodId}", financialPeriodId);
            return await _dbContext.TaxTransactions
                .AsNoTracking()
                .Where(t => t.FinancialPeriodId == financialPeriodId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxTransaction>> GetTaxTransactionsByTypeForPeriodAsync(
            string taxTypeId, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax transactions for tax type {TaxTypeId} in financial period {FinancialPeriodId}", 
                taxTypeId, financialPeriodId);
            
            return await _dbContext.TaxTransactions
                .AsNoTracking()
                .Where(t => t.TaxTypeId == taxTypeId && t.FinancialPeriodId == financialPeriodId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TaxTransaction>> GetTaxTransactionsByDateRangeAsync(
            DateTime startDate, 
            DateTime endDate, 
            string taxTypeId = null,
            bool? isSettled = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving tax transactions between {StartDate} and {EndDate}", startDate, endDate);
            
            var query = _dbContext.TaxTransactions
                .AsNoTracking()
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
            
            // Filter by tax type if provided
            if (!string.IsNullOrEmpty(taxTypeId))
            {
                query = query.Where(t => t.TaxTypeId == taxTypeId);
            }
            
            // Filter by settlement status if provided
            if (isSettled.HasValue)
            {
                query = query.Where(t => t.IsSettled == isSettled.Value);
            }
            
            return await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TaxTransaction> UpdateTaxTransactionSettlementAsync(
            string id, 
            bool isSettled, 
            DateTime? settlementDate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating settlement status of tax transaction {TransactionId} to {IsSettled}", 
                id, isSettled);
            
            var transaction = await _dbContext.TaxTransactions
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            
            if (transaction == null)
            {
                return null;
            }
            
            transaction.IsSettled = isSettled;
            transaction.SettlementDate = settlementDate;
            transaction.LastModifiedDate = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return transaction;
        }

        /// <inheritdoc/>
        public async Task<TaxRate> GetWithholdingTaxRateAsync(
            string incomeType, 
            bool isResident, 
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving withholding tax rate for income type {IncomeType}, residency {IsResident}", 
                incomeType, isResident);
            
            // First, find the WHT tax type
            var whtTaxType = await _dbContext.TaxTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Code == "WHT", cancellationToken);
            
            if (whtTaxType == null)
            {
                return null;
            }
            
            // Get the applicable WHT rate
            var whtRate = await _dbContext.TaxRates
                .AsNoTracking()
                .Where(r => r.TaxTypeId == whtTaxType.Id &&
                          r.IsActive &&
                          r.EffectiveDate <= date &&
                          (!r.EndDate.HasValue || r.EndDate >= date) &&
                          r.ApplicableCategory == incomeType &&
                          r.Name.Contains(isResident ? "Resident" : "Non-Resident"))
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync(cancellationToken);
            
            return whtRate;
        }
    }
}
