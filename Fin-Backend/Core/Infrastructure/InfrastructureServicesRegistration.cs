using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Infrastructure.Persistence;
using FinTech.Core.Infrastructure.Repositories;

namespace FinTech.Core.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            
            // Register repositories
            services.AddScoped<IGeneralLedgerRepository, GeneralLedgerRepository>();
            
            // Register other infrastructure services
            services.AddScoped<IDomainEventService, DomainEventService>();
            services.AddScoped<IDateTime, DateTimeService>();
            
            return services;
        }
    }
}