using System.Threading.Tasks;

namespace FinTech.Core.Application.Services.Integration
{
    public interface IBankingAccountingIntegrationService
    {
        Task ProcessDepositAsync(int accountId, decimal amount, string reference, string description);
        Task ProcessWithdrawalAsync(int accountId, decimal amount, string reference, string description);
        Task ProcessTransferAsync(int fromAccountId, int toAccountId, decimal amount, string reference, string description);
        Task ProcessFeeChargeAsync(int accountId, decimal amount, string feeType, string reference, string description);
        Task ProcessInterestPaymentAsync(int accountId, decimal amount, string reference, string description);
    }
}
