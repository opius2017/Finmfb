using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Application.Interfaces.Services.Accounting
{
    public interface IChartOfAccountService
    {
        Task<ChartOfAccount> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<ChartOfAccount> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetByClassificationAsync(AccountClassification classification, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
        Task<string> CreateAccountAsync(ChartOfAccount account, CancellationToken cancellationToken = default);
        Task UpdateAccountAsync(ChartOfAccount account, CancellationToken cancellationToken = default);
        Task ActivateAccountAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task DeactivateAccountAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<string> GenerateAccountNumberAsync(AccountType accountType, AccountClassification classification, CancellationToken cancellationToken = default);
        Task<string?> GetPayrollExpenseAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetTaxPayableAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetPensionPayableAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetPayrollLiabilityAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetCashAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetTaxExpenseAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetBenefitsExpenseAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetBenefitsPayableAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetLoanReceivableAccountIdAsync(string? productType = null, CancellationToken cancellationToken = default);
        Task<string?> GetLoanWriteOffAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetBankAccountIdForCustomerAsync(string customerId, CancellationToken cancellationToken = default);
        Task<string?> GetInterestIncomeAccountIdAsync(CancellationToken cancellationToken = default);

        Task<string?> GetInterestReceivableAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetBadDebtExpenseAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetLoanLossProvisionAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetFeeReceivableAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetFeeIncomeAccountIdAsync(string? feeType = null, CancellationToken cancellationToken = default);
        Task<string?> GetFixedAssetAccountIdAsync(string assetCategory, CancellationToken cancellationToken = default);
        Task<string?> GetAccumulatedDepreciationAccountIdAsync(string assetCategory, CancellationToken cancellationToken = default);
        Task<string?> GetDepreciationExpenseAccountIdAsync(string assetCategory, CancellationToken cancellationToken = default);
        Task<string?> GetAssetRevaluationReserveAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetAssetImpairmentAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetAssetDisposalGainAccountIdAsync(CancellationToken cancellationToken = default);
        Task<string?> GetAssetDisposalLossAccountIdAsync(CancellationToken cancellationToken = default);
    }
}
