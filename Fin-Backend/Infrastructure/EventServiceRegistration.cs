using FinTech.Application.Interfaces.Services;
using FinTech.Domain.Events.Banking;
using FinTech.Domain.Events.FixedAssets;
using FinTech.Domain.Events.Loans;
using FinTech.Domain.Events.Payroll;
using FinTech.Infrastructure.Services;
using FinTech.Infrastructure.Services.EventHandlers.Banking;
using FinTech.Infrastructure.Services.EventHandlers.FixedAssets;
using FinTech.Infrastructure.Services.EventHandlers.Loans;
using FinTech.Infrastructure.Services.EventHandlers.Payroll;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class EventServiceRegistration
    {
        public static IServiceCollection AddEventServices(this IServiceCollection services)
        {
            // Register domain event service
            services.AddScoped<IDomainEventService, DomainEventService>();
            
            // Register Banking event handlers
            services.AddScoped<IDomainEventHandler<DepositCompletedEvent>, BankingEventHandlers>();
            services.AddScoped<IDomainEventHandler<WithdrawalCompletedEvent>, BankingEventHandlers>();
            services.AddScoped<IDomainEventHandler<TransferCompletedEvent>, BankingEventHandlers>();
            services.AddScoped<IDomainEventHandler<FeeChargedEvent>, BankingEventHandlers>();
            services.AddScoped<IDomainEventHandler<InterestPaidEvent>, BankingEventHandlers>();
            
            // Register Loan event handlers
            services.AddScoped<IDomainEventHandler<LoanDisbursedEvent>, LoanEventHandlers>();
            services.AddScoped<IDomainEventHandler<LoanRepaymentReceivedEvent>, LoanEventHandlers>();
            services.AddScoped<IDomainEventHandler<LoanWrittenOffEvent>, LoanEventHandlers>();
            services.AddScoped<IDomainEventHandler<LoanInterestAccruedEvent>, LoanEventHandlers>();
            services.AddScoped<IDomainEventHandler<LoanFeeChargedEvent>, LoanEventHandlers>();
            
            // Register Payroll event handlers
            services.AddScoped<IDomainEventHandler<SalaryPaymentProcessedEvent>, PayrollEventHandlers>();
            services.AddScoped<IDomainEventHandler<PayrollTaxRemittedEvent>, PayrollEventHandlers>();
            services.AddScoped<IDomainEventHandler<PensionRemittedEvent>, PayrollEventHandlers>();
            services.AddScoped<IDomainEventHandler<BonusPaymentProcessedEvent>, PayrollEventHandlers>();
            services.AddScoped<IDomainEventHandler<PayrollExpenseAccruedEvent>, PayrollEventHandlers>();
            
            // Register Fixed Asset event handlers
            services.AddScoped<IDomainEventHandler<AssetAcquiredEvent>, FixedAssetEventHandlers>();
            services.AddScoped<IDomainEventHandler<AssetDepreciatedEvent>, FixedAssetEventHandlers>();
            services.AddScoped<IDomainEventHandler<AssetDisposedEvent>, FixedAssetEventHandlers>();
            services.AddScoped<IDomainEventHandler<AssetRevaluedEvent>, FixedAssetEventHandlers>();
            services.AddScoped<IDomainEventHandler<AssetImpairedEvent>, FixedAssetEventHandlers>();
            
            return services;
        }
    }
}