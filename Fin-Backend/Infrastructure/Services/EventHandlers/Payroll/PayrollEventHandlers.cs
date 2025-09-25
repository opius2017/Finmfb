using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.Integration;
using FinTech.Core.Domain.Events.Payroll;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services.EventHandlers.Payroll
{
    public class PayrollEventHandlers : 
        IDomainEventHandler<SalaryPaymentProcessedEvent>,
        IDomainEventHandler<PayrollTaxRemittedEvent>,
        IDomainEventHandler<PensionRemittedEvent>,
        IDomainEventHandler<BonusPaymentProcessedEvent>,
        IDomainEventHandler<PayrollExpenseAccruedEvent>
    {
        private readonly IPayrollAccountingIntegrationService _payrollAccountingIntegrationService;
        private readonly ILogger<PayrollEventHandlers> _logger;

        public PayrollEventHandlers(
            IPayrollAccountingIntegrationService payrollAccountingIntegrationService,
            ILogger<PayrollEventHandlers> logger)
        {
            _payrollAccountingIntegrationService = payrollAccountingIntegrationService;
            _logger = logger;
        }

        public async Task HandleAsync(SalaryPaymentProcessedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling SalaryPaymentProcessedEvent for EmployeeId: {EmployeeId}", domainEvent.EmployeeId);
            await _payrollAccountingIntegrationService.ProcessSalaryPaymentAsync(
                domainEvent.EmployeeId,
                domainEvent.GrossAmount,
                domainEvent.TaxAmount,
                domainEvent.PensionAmount,
                domainEvent.OtherDeductions,
                domainEvent.PayPeriod,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(PayrollTaxRemittedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling PayrollTaxRemittedEvent for TaxType: {TaxType}, Period: {TaxPeriod}", 
                domainEvent.TaxType, domainEvent.TaxPeriod);
            await _payrollAccountingIntegrationService.ProcessPayrollTaxRemittanceAsync(
                domainEvent.Amount,
                domainEvent.TaxType,
                domainEvent.TaxPeriod,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(PensionRemittedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling PensionRemittedEvent for Provider: {PensionProvider}, Period: {PensionPeriod}", 
                domainEvent.PensionProvider, domainEvent.PensionPeriod);
            await _payrollAccountingIntegrationService.ProcessPensionRemittanceAsync(
                domainEvent.Amount,
                domainEvent.PensionProvider,
                domainEvent.PensionPeriod,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(BonusPaymentProcessedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling BonusPaymentProcessedEvent for EmployeeId: {EmployeeId}", domainEvent.EmployeeId);
            await _payrollAccountingIntegrationService.ProcessBonusPaymentAsync(
                domainEvent.EmployeeId,
                domainEvent.Amount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(PayrollExpenseAccruedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling PayrollExpenseAccruedEvent for ExpenseType: {ExpenseType}, Period: {Period}", 
                domainEvent.ExpenseType, domainEvent.Period);
            await _payrollAccountingIntegrationService.ProcessPayrollExpenseAccrualAsync(
                domainEvent.Amount,
                domainEvent.ExpenseType,
                domainEvent.Period,
                domainEvent.Reference,
                domainEvent.Description);
        }
    }
}
