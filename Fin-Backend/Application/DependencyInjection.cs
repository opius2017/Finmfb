using Microsoft.Extensions.DependencyInjection;
using FinTech.Core.Application.Services.Integrations;

namespace FinTech.Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
        services.AddScoped<IInterestCalculationService, InterestCalculationService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IMakerCheckerService, MakerCheckerService>();
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();
        
        // Register integration services
        services.AddScoped<INibssService, NibssService>();
        services.AddScoped<ICreditBureauService, CreditBureauService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBiometricService, BiometricService>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        
        // Register customer service
        services.AddScoped<ICustomerService, CustomerService>();
        
        return services;
    }
}