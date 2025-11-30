using System;
using FinTech.Configuration;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinTech.Tests.Integration
{
    /// <summary>
    /// Test fixture for integration tests
    /// Sets up in-memory database and all services
    /// </summary>
    public class IntegrationTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public IntegrationTestFixture()
        {
            SetupServices();
        }

        private void SetupServices()
        {
            // Build configuration
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection", 
                        "Server=(localdb)\\mssqllocaldb;Database=CooperativeLoanTest;Trusted_Connection=true;MultipleActiveResultSets=true"),
                    new KeyValuePair<string, string>("Logging:LogLevel:Default", "Information")
                });

            Configuration = configBuilder.Build();

            // Setup services
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add Entity Framework with In-Memory database
            services.AddDbContext<ModularApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Add application services
            services.AddApplicationServices(Configuration);

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();

            // Initialize database
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ModularApplicationDbContext>();
            
            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed test data if needed
            SeedTestData(context);
        }

        private void SeedTestData(ModularApplicationDbContext context)
        {
            // Add any common test data here
            // For now, we'll create data in individual tests
        }

        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
