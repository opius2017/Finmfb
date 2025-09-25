using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.Integration;
using FinTech.Core.Domain.Events.Banking;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services.EventHandlers.Banking
{
    public class BankingEventHandlers : 
        IDomainEventHandler<DepositCompletedEvent>,
        IDomainEventHandler<WithdrawalCompletedEvent>,
        IDomainEventHandler<TransferCompletedEvent>,
        IDomainEventHandler<FeeChargedEvent>,
        IDomainEventHandler<InterestPaidEvent>
    {
        private readonly IBankingAccountingIntegrationService _bankingAccountingIntegrationService;
        private readonly ILogger<BankingEventHandlers> _logger;

        public BankingEventHandlers(
            IBankingAccountingIntegrationService bankingAccountingIntegrationService,
            ILogger<BankingEventHandlers> logger)
        {
            _bankingAccountingIntegrationService = bankingAccountingIntegrationService;
            _logger = logger;
        }

        public async Task HandleAsync(DepositCompletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling DepositCompletedEvent for AccountId: {AccountId}", domainEvent.AccountId);
            await _bankingAccountingIntegrationService.ProcessDepositAsync(
                domainEvent.AccountId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(WithdrawalCompletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling WithdrawalCompletedEvent for AccountId: {AccountId}", domainEvent.AccountId);
            await _bankingAccountingIntegrationService.ProcessWithdrawalAsync(
                domainEvent.AccountId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(TransferCompletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling TransferCompletedEvent from AccountId: {FromAccountId} to AccountId: {ToAccountId}", 
                domainEvent.FromAccountId, domainEvent.ToAccountId);
            await _bankingAccountingIntegrationService.ProcessTransferAsync(
                domainEvent.FromAccountId,
                domainEvent.ToAccountId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(FeeChargedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling FeeChargedEvent for AccountId: {AccountId}, FeeType: {FeeType}", 
                domainEvent.AccountId, domainEvent.FeeType);
            await _bankingAccountingIntegrationService.ProcessFeeChargeAsync(
                domainEvent.AccountId,
                domainEvent.Amount,
                domainEvent.FeeType,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(InterestPaidEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling InterestPaidEvent for AccountId: {AccountId}", domainEvent.AccountId);
            await _bankingAccountingIntegrationService.ProcessInterestPaymentAsync(
                domainEvent.AccountId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }
    }
}
