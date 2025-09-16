using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services;

namespace FinTech.Core.Application
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register application services
            services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
            
            // Register validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register MediatR behaviors (for cross-cutting concerns)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            
            return services;
        }
    }
}