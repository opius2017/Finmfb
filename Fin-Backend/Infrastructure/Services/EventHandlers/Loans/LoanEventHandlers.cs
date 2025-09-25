using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.Integration;
using FinTech.Core.Domain.Events.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services.EventHandlers.Loans
{
    public class LoanEventHandlers : 
        IDomainEventHandler<LoanDisbursedEvent>,
        IDomainEventHandler<LoanRepaymentReceivedEvent>,
        IDomainEventHandler<LoanWrittenOffEvent>,
        IDomainEventHandler<LoanInterestAccruedEvent>,
        IDomainEventHandler<LoanFeeChargedEvent>
    {
        private readonly ILoanAccountingIntegrationService _loanAccountingIntegrationService;
        private readonly ILogger<LoanEventHandlers> _logger;

        public LoanEventHandlers(
            ILoanAccountingIntegrationService loanAccountingIntegrationService,
            ILogger<LoanEventHandlers> logger)
        {
            _loanAccountingIntegrationService = loanAccountingIntegrationService;
            _logger = logger;
        }

        public async Task HandleAsync(LoanDisbursedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling LoanDisbursedEvent for LoanId: {LoanId}", domainEvent.LoanId);
            await _loanAccountingIntegrationService.ProcessLoanDisbursementAsync(
                domainEvent.LoanId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(LoanRepaymentReceivedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling LoanRepaymentReceivedEvent for LoanId: {LoanId}", domainEvent.LoanId);
            await _loanAccountingIntegrationService.ProcessLoanRepaymentAsync(
                domainEvent.LoanId,
                domainEvent.PrincipalAmount,
                domainEvent.InterestAmount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(LoanWrittenOffEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling LoanWrittenOffEvent for LoanId: {LoanId}", domainEvent.LoanId);
            await _loanAccountingIntegrationService.ProcessLoanWriteOffAsync(
                domainEvent.LoanId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(LoanInterestAccruedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling LoanInterestAccruedEvent for LoanId: {LoanId}", domainEvent.LoanId);
            await _loanAccountingIntegrationService.ProcessLoanInterestAccrualAsync(
                domainEvent.LoanId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(LoanFeeChargedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling LoanFeeChargedEvent for LoanId: {LoanId}, FeeType: {FeeType}", 
                domainEvent.LoanId, domainEvent.FeeType);
            await _loanAccountingIntegrationService.ProcessLoanFeeChargeAsync(
                domainEvent.LoanId,
                domainEvent.Amount,
                domainEvent.FeeType,
                domainEvent.Reference,
                domainEvent.Description);
        }
    }
}
