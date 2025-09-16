using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Accounting
{
    public interface IRegulatoryMappingService
    {
        Task<RegulatoryMappingDto> GetMappingAsync(
            string mappingId, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<RegulatoryMappingDto>> GetMappingsByAccountAsync(
            string chartOfAccountId, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<RegulatoryMappingDto>> GetMappingsByRegulatoryCodeAsync(
            string regulatoryCodeId, 
            CancellationToken cancellationToken = default);
            
        Task<RegulatoryMappingDto> CreateMappingAsync(
            CreateRegulatoryMappingDto request, 
            CancellationToken cancellationToken = default);
            
        Task<RegulatoryMappingDto> UpdateMappingAsync(
            string mappingId, 
            UpdateRegulatoryMappingDto request, 
            CancellationToken cancellationToken = default);
            
        Task<bool> DeactivateMappingAsync(
            string mappingId, 
            CancellationToken cancellationToken = default);
            
        Task<bool> ActivateMappingAsync(
            string mappingId, 
            CancellationToken cancellationToken = default);
            
        Task<RegulatoryReportDto> GenerateRegulatoryReportAsync(
            string reportingForm,
            string financialPeriodId,
            bool includeZeroBalances = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default);
    }

    public class RegulatoryMappingService : IRegulatoryMappingService
    {
        private readonly IRegulatoryMappingRepository _regulatoryMappingRepository;
        private readonly IRegulatoryCodeRepository _regulatoryCodeRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly ITrialBalanceService _trialBalanceService;
        
        public RegulatoryMappingService(
            IRegulatoryMappingRepository regulatoryMappingRepository,
            IRegulatoryCodeRepository regulatoryCodeRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            ITrialBalanceService trialBalanceService)
        {
            _regulatoryMappingRepository = regulatoryMappingRepository ?? throw new ArgumentNullException(nameof(regulatoryMappingRepository));
            _regulatoryCodeRepository = regulatoryCodeRepository ?? throw new ArgumentNullException(nameof(regulatoryCodeRepository));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
        }

        /// <summary>
        /// Gets a specific regulatory mapping by ID
        /// </summary>
        public async Task<RegulatoryMappingDto> GetMappingAsync(
            string mappingId, 
            CancellationToken cancellationToken = default)
        {
            var mapping = await _regulatoryMappingRepository.GetByIdAsync(mappingId, cancellationToken);
            if (mapping == null)
            {
                return null;
            }
            
            var regulatoryCode = await _regulatoryCodeRepository.GetByIdAsync(mapping.RegulatoryCodeId, cancellationToken);
            var chartOfAccount = await _chartOfAccountRepository.GetByIdAsync(mapping.ChartOfAccountId, cancellationToken);
            
            return CreateMappingDto(mapping, regulatoryCode, chartOfAccount);
        }

        /// <summary>
        /// Gets all regulatory mappings for a specific chart of account
        /// </summary>
        public async Task<IReadOnlyList<RegulatoryMappingDto>> GetMappingsByAccountAsync(
            string chartOfAccountId, 
            CancellationToken cancellationToken = default)
        {
            var mappings = await _regulatoryMappingRepository.GetByChartOfAccountAsync(chartOfAccountId, cancellationToken);
            
            var result = new List<RegulatoryMappingDto>();
            foreach (var mapping in mappings)
            {
                var regulatoryCode = await _regulatoryCodeRepository.GetByIdAsync(mapping.RegulatoryCodeId, cancellationToken);
                var chartOfAccount = await _chartOfAccountRepository.GetByIdAsync(mapping.ChartOfAccountId, cancellationToken);
                
                result.Add(CreateMappingDto(mapping, regulatoryCode, chartOfAccount));
            }
            
            return result;
        }

        /// <summary>
        /// Gets all regulatory mappings for a specific regulatory code
        /// </summary>
        public async Task<IReadOnlyList<RegulatoryMappingDto>> GetMappingsByRegulatoryCodeAsync(
            string regulatoryCodeId, 
            CancellationToken cancellationToken = default)
        {
            var mappings = await _regulatoryMappingRepository.GetByRegulatoryCodeAsync(regulatoryCodeId, cancellationToken);
            
            var result = new List<RegulatoryMappingDto>();
            foreach (var mapping in mappings)
            {
                var regulatoryCode = await _regulatoryCodeRepository.GetByIdAsync(mapping.RegulatoryCodeId, cancellationToken);
                var chartOfAccount = await _chartOfAccountRepository.GetByIdAsync(mapping.ChartOfAccountId, cancellationToken);
                
                result.Add(CreateMappingDto(mapping, regulatoryCode, chartOfAccount));
            }
            
            return result;
        }

        /// <summary>
        /// Creates a new regulatory mapping
        /// </summary>
        public async Task<RegulatoryMappingDto> CreateMappingAsync(
            CreateRegulatoryMappingDto request, 
            CancellationToken cancellationToken = default)
        {
            // Validate the chart of account
            var chartOfAccount = await _chartOfAccountRepository.GetByIdAsync(request.ChartOfAccountId, cancellationToken);
            if (chartOfAccount == null)
            {
                throw new InvalidOperationException($"Chart of account with ID {request.ChartOfAccountId} not found");
            }
            
            // Validate the regulatory code
            var regulatoryCode = await _regulatoryCodeRepository.GetByIdAsync(request.RegulatoryCodeId, cancellationToken);
            if (regulatoryCode == null)
            {
                throw new InvalidOperationException($"Regulatory code with ID {request.RegulatoryCodeId} not found");
            }
            
            // Check if the mapping already exists
            var existingMapping = await _regulatoryMappingRepository.GetMappingAsync(
                request.ChartOfAccountId, 
                request.RegulatoryCodeId, 
                cancellationToken);
                
            if (existingMapping != null)
            {
                throw new InvalidOperationException("A mapping between this chart of account and regulatory code already exists");
            }
            
            // Create the new mapping
            var mapping = new RegulatoryMapping(
                request.ChartOfAccountId,
                request.RegulatoryCodeId,
                request.MappingWeight,
                request.Notes,
                request.EffectiveDate);
                
            await _regulatoryMappingRepository.AddAsync(mapping, cancellationToken);
            
            return CreateMappingDto(mapping, regulatoryCode, chartOfAccount);
        }

        /// <summary>
        /// Updates an existing regulatory mapping
        /// </summary>
        public async Task<RegulatoryMappingDto> UpdateMappingAsync(
            string mappingId, 
            UpdateRegulatoryMappingDto request, 
            CancellationToken cancellationToken = default)
        {
            var mapping = await _regulatoryMappingRepository.GetByIdAsync(mappingId, cancellationToken);
            if (mapping == null)
            {
                throw new InvalidOperationException($"Regulatory mapping with ID {mappingId} not found");
            }
            
            mapping.Update(
                request.MappingWeight,
                request.Notes,
                request.ExpiryDate);
                
            await _regulatoryMappingRepository.UpdateAsync(mapping, cancellationToken);
            
            var regulatoryCode = await _regulatoryCodeRepository.GetByIdAsync(mapping.RegulatoryCodeId, cancellationToken);
            var chartOfAccount = await _chartOfAccountRepository.GetByIdAsync(mapping.ChartOfAccountId, cancellationToken);
            
            return CreateMappingDto(mapping, regulatoryCode, chartOfAccount);
        }

        /// <summary>
        /// Deactivates a regulatory mapping
        /// </summary>
        public async Task<bool> DeactivateMappingAsync(
            string mappingId, 
            CancellationToken cancellationToken = default)
        {
            var mapping = await _regulatoryMappingRepository.GetByIdAsync(mappingId, cancellationToken);
            if (mapping == null)
            {
                throw new InvalidOperationException($"Regulatory mapping with ID {mappingId} not found");
            }
            
            mapping.Deactivate();
            await _regulatoryMappingRepository.UpdateAsync(mapping, cancellationToken);
            
            return true;
        }

        /// <summary>
        /// Activates a regulatory mapping
        /// </summary>
        public async Task<bool> ActivateMappingAsync(
            string mappingId, 
            CancellationToken cancellationToken = default)
        {
            var mapping = await _regulatoryMappingRepository.GetByIdAsync(mappingId, cancellationToken);
            if (mapping == null)
            {
                throw new InvalidOperationException($"Regulatory mapping with ID {mappingId} not found");
            }
            
            mapping.Activate();
            await _regulatoryMappingRepository.UpdateAsync(mapping, cancellationToken);
            
            return true;
        }

        /// <summary>
        /// Generates a regulatory report for a specific form and financial period
        /// </summary>
        public async Task<RegulatoryReportDto> GenerateRegulatoryReportAsync(
            string reportingForm,
            string financialPeriodId,
            bool includeZeroBalances = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get all regulatory codes for the specified form
            var regulatoryCodes = await _regulatoryCodeRepository.GetByReportingFormAsync(reportingForm, cancellationToken);
            if (!regulatoryCodes.Any())
            {
                throw new InvalidOperationException($"No regulatory codes found for reporting form {reportingForm}");
            }
            
            // Get the trial balance for the specified financial period
            var trialBalance = await _trialBalanceService.GenerateTrialBalanceByFinancialPeriodAsync(
                financialPeriodId, 
                true, // Include zero balances for the calculation
                currencyCode,
                cancellationToken);
                
            // Create the report structure
            var report = new RegulatoryReportDto
            {
                ReportingForm = reportingForm,
                FinancialPeriodId = financialPeriodId,
                GeneratedAt = DateTime.UtcNow,
                CurrencyCode = currencyCode ?? "NGN",
                Sections = new List<RegulatoryReportSectionDto>()
            };
            
            // Group regulatory codes by category
            var codesByCategory = regulatoryCodes
                .GroupBy(c => c.Category)
                .OrderBy(g => g.Key)
                .ToList();
                
            // Process each category
            foreach (var categoryGroup in codesByCategory)
            {
                var section = new RegulatoryReportSectionDto
                {
                    Category = categoryGroup.Key,
                    Items = new List<RegulatoryReportItemDto>(),
                    Total = 0
                };
                
                // Process each regulatory code in the category
                foreach (var code in categoryGroup.OrderBy(c => c.Code))
                {
                    // Get all mappings for this regulatory code
                    var mappings = await _regulatoryMappingRepository.GetByRegulatoryCodeAsync(code.Id, cancellationToken);
                    if (!mappings.Any())
                    {
                        // No mappings for this code, add it with zero balance if requested
                        if (includeZeroBalances)
                        {
                            section.Items.Add(new RegulatoryReportItemDto
                            {
                                RegulatoryCodeId = code.Id,
                                RegulatoryCode = code.Code,
                                Description = code.Description,
                                Amount = 0,
                                MappedAccounts = new List<RegulatoryReportMappedAccountDto>()
                            });
                        }
                        
                        continue;
                    }
                    
                    // Calculate the balance for this regulatory code
                    decimal totalAmount = 0;
                    var mappedAccounts = new List<RegulatoryReportMappedAccountDto>();
                    
                    foreach (var mapping in mappings.Where(m => m.IsActive))
                    {
                        // Find the account in the trial balance
                        var account = trialBalance.Accounts.FirstOrDefault(a => a.AccountId == mapping.ChartOfAccountId);
                        if (account == null)
                        {
                            continue; // Account not found in trial balance
                        }
                        
                        // Calculate the mapped amount based on the account type and mapping weight
                        decimal accountBalance = 0;
                        
                        // Different account classifications have different balance calculations
                        switch (account.Classification)
                        {
                            case "Asset":
                                accountBalance = account.DebitBalance - account.CreditBalance;
                                break;
                            case "Liability":
                            case "Equity":
                                accountBalance = account.CreditBalance - account.DebitBalance;
                                break;
                            case "Income":
                                accountBalance = account.CreditBalance - account.DebitBalance;
                                break;
                            case "Expense":
                                accountBalance = account.DebitBalance - account.CreditBalance;
                                break;
                            default:
                                accountBalance = account.DebitBalance - account.CreditBalance;
                                break;
                        }
                        
                        // Apply the mapping weight
                        decimal mappedAmount = accountBalance * mapping.MappingWeight;
                        
                        // Add to the total for this regulatory code
                        totalAmount += mappedAmount;
                        
                        // Add to the mapped accounts list
                        mappedAccounts.Add(new RegulatoryReportMappedAccountDto
                        {
                            AccountId = account.AccountId,
                            AccountNumber = account.AccountNumber,
                            AccountName = account.AccountName,
                            AccountBalance = accountBalance,
                            MappingWeight = mapping.MappingWeight,
                            MappedAmount = mappedAmount
                        });
                    }
                    
                    // Only add the item if it has a non-zero balance or if including zero balances
                    if (totalAmount != 0 || includeZeroBalances)
                    {
                        section.Items.Add(new RegulatoryReportItemDto
                        {
                            RegulatoryCodeId = code.Id,
                            RegulatoryCode = code.Code,
                            Description = code.Description,
                            Amount = totalAmount,
                            MappedAccounts = mappedAccounts
                                .OrderBy(a => a.AccountNumber)
                                .ToList()
                        });
                        
                        // Add to the section total
                        section.Total += totalAmount;
                    }
                }
                
                // Only add the section if it has items
                if (section.Items.Any())
                {
                    report.Sections.Add(section);
                }
            }
            
            // Calculate the report totals
            report.TotalAssets = report.Sections
                .FirstOrDefault(s => s.Category == "Asset")?.Total ?? 0;
                
            report.TotalLiabilities = report.Sections
                .FirstOrDefault(s => s.Category == "Liability")?.Total ?? 0;
                
            report.TotalEquity = report.Sections
                .FirstOrDefault(s => s.Category == "Equity")?.Total ?? 0;
                
            report.TotalIncome = report.Sections
                .FirstOrDefault(s => s.Category == "Income")?.Total ?? 0;
                
            report.TotalExpenses = report.Sections
                .FirstOrDefault(s => s.Category == "Expense")?.Total ?? 0;
            
            return report;
        }

        // Helper method to create a RegulatoryMappingDto
        private RegulatoryMappingDto CreateMappingDto(
            RegulatoryMapping mapping, 
            RegulatoryCode regulatoryCode, 
            ChartOfAccount chartOfAccount)
        {
            return new RegulatoryMappingDto
            {
                Id = mapping.Id,
                ChartOfAccountId = mapping.ChartOfAccountId,
                AccountNumber = chartOfAccount?.AccountNumber,
                AccountName = chartOfAccount?.AccountName,
                AccountClassification = chartOfAccount?.Classification,
                RegulatoryCodeId = mapping.RegulatoryCodeId,
                RegulatoryCode = regulatoryCode?.Code,
                RegulatoryDescription = regulatoryCode?.Description,
                RegulatoryCategory = regulatoryCode?.Category,
                RegulatoryAuthority = regulatoryCode?.Authority,
                ReportingForm = regulatoryCode?.ReportingForm,
                MappingWeight = mapping.MappingWeight,
                Notes = mapping.Notes,
                IsActive = mapping.IsActive,
                EffectiveDate = mapping.EffectiveDate,
                ExpiryDate = mapping.ExpiryDate,
                CreatedAt = mapping.CreatedAt,
                UpdatedAt = mapping.UpdatedAt
            };
        }
    }

    // DTOs
    public class RegulatoryMappingDto
    {
        public string Id { get; set; }
        
        // Chart of Account info
        public string ChartOfAccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountClassification { get; set; }
        
        // Regulatory Code info
        public string RegulatoryCodeId { get; set; }
        public string RegulatoryCode { get; set; }
        public string RegulatoryDescription { get; set; }
        public string RegulatoryCategory { get; set; }
        public string RegulatoryAuthority { get; set; }
        public string ReportingForm { get; set; }
        
        // Mapping info
        public decimal MappingWeight { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateRegulatoryMappingDto
    {
        public string ChartOfAccountId { get; set; }
        public string RegulatoryCodeId { get; set; }
        public decimal MappingWeight { get; set; } = 1.0m;
        public string Notes { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    public class UpdateRegulatoryMappingDto
    {
        public decimal MappingWeight { get; set; }
        public string Notes { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class RegulatoryReportDto
    {
        public string ReportingForm { get; set; }
        public string FinancialPeriodId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string CurrencyCode { get; set; }
        public List<RegulatoryReportSectionDto> Sections { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
    }

    public class RegulatoryReportSectionDto
    {
        public string Category { get; set; }
        public List<RegulatoryReportItemDto> Items { get; set; }
        public decimal Total { get; set; }
    }

    public class RegulatoryReportItemDto
    {
        public string RegulatoryCodeId { get; set; }
        public string RegulatoryCode { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public List<RegulatoryReportMappedAccountDto> MappedAccounts { get; set; }
    }

    public class RegulatoryReportMappedAccountDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal MappingWeight { get; set; }
        public decimal MappedAmount { get; set; }
    }
}