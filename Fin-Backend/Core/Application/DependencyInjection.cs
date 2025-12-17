using Microsoft.Extensions.DependencyInjection;
using FinTech.Core.Application.Services.Integrations;
using FinTech.Core.Application.Common.Behaviors;
using System.Reflection;
using FluentValidation;
using MediatR;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services;
using FinTech.Core.Application.Services.Accounting;
using FinTech.Core.Application.Interfaces.Accounting;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Services.Loans;


using FinTech.Core.Application.Services.Customers;
using FinTech.Core.Application.Services.ClientPortal;

namespace FinTech.Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Pipeline Behaviors (order matters!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Register existing application services
        services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
        services.AddScoped<IInterestCalculationService, InterestCalculationService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IMakerCheckerService, MakerCheckerService>();
        // TaxCalculationService is in Accounting namespace and will be registered in AccountingServiceRegistration
        
        // Register integration services
        services.AddScoped<INibssService, NibssService>();
        services.AddScoped<ICreditBureauService, CreditBureauService>();
        services.AddScoped<ISmsService, SmsService>();
        // EmailService registration moved to Infrastructure layer
        services.AddScoped<IBiometricService, BiometricService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        
        // Register customer service
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IFinancialPeriodService, FinancialPeriodService>();
        
        // Register utility services
        services.AddSingleton<IDateTimeService, DateTimeService>();
        
        // Register core accounting services
        // services.AddAccountingServices(); // This method is not defined in this project
        
        // Register loan management services
        // services.AddLoanServices(); // This method is not defined in this project
        
        // Register integration services
        // services.AddIntegrationServices(); // This method is not defined in this project
        
        return services;
    }
}