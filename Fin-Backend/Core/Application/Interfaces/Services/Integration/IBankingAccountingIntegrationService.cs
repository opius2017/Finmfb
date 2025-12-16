using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Services.Integration
{
    public interface IBankingAccountingIntegrationService
    {
        Task ProcessDepositAsync(string accountId, decimal amount, string reference, string description);
        Task ProcessWithdrawalAsync(string accountId, decimal amount, string reference, string description);
        Task ProcessTransferAsync(string fromAccountId, string toAccountId, decimal amount, string reference, string description);
        Task ProcessFeeChargeAsync(string accountId, decimal amount, string feeType, string reference, string description);
        Task ProcessInterestPaymentAsync(string accountId, decimal amount, string reference, string description);
    }
}
