using System.Threading.Tasks;

namespace FinTech.Application.Interfaces.Services
{
    public interface IChartOfAccountService
    {
        Task<int> GetCashAccountIdAsync();
        Task<int> GetBankAccountIdForCustomerAsync(int customerAccountId);
        Task<int> GetFeeIncomeAccountIdAsync(string feeType);
        Task<int> GetInterestExpenseAccountIdAsync();
        Task<int> GetInterestIncomeAccountIdAsync();
        Task<int> GetLoanReceivableAccountIdAsync();
        Task<int> GetLoanWriteOffAccountIdAsync();
        Task<int> GetPayrollExpenseAccountIdAsync();
        Task<int> GetPayrollLiabilityAccountIdAsync();
        Task<int> GetTaxPayableAccountIdAsync();
        Task<int> GetPensionPayableAccountIdAsync();
        Task<int> GetFixedAssetAccountIdAsync(string assetCategory);
        Task<int> GetAccumulatedDepreciationAccountIdAsync(string assetCategory);
        Task<int> GetDepreciationExpenseAccountIdAsync(string assetCategory);
        Task<int> GetAssetDisposalGainAccountIdAsync();
        Task<int> GetAssetDisposalLossAccountIdAsync();
        Task<int> GetAssetRevaluationReserveAccountIdAsync();
        Task<int> GetAssetImpairmentAccountIdAsync();
    }
}