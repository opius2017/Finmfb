using FinTech.Application.Services;
using FinTech.Domain.Events.FixedAssets;
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Infrastructure.Data;
using FinTech.Infrastructure.Services.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinTech.Tests.EndToEnd
{
    public class FixedAssetWorkflowTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbContext;

        public FixedAssetWorkflowTests()
        {
            // Set up the service collection
            var services = new ServiceCollection();

            // Register DbContext with in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            // Register services needed for the test
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
            services.AddScoped<IFixedAssetAccountingIntegrationService, FixedAssetAccountingIntegrationService>();
            
            // Add event handlers
            services.AddScoped<IDomainEventHandler<AssetAcquiredEvent>, FixedAssetDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<AssetDepreciatedEvent>, FixedAssetDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<AssetDisposedEvent>, FixedAssetDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<AssetRevaluedEvent>, FixedAssetDomainEventHandler>();
            services.AddScoped<IDomainEventHandler<AssetImpairedEvent>, FixedAssetDomainEventHandler>();
            
            // Add the domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Register logging
            services.AddLogging(builder => builder.AddConsole());

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Get the database context
            _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Seed the database with necessary data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add chart of accounts
            _dbContext.ChartOfAccounts.AddRange(
                new ChartOfAccount { Id = 1001, AccountNumber = "1001", Name = "Cash", AccountType = "Asset", IsActive = true },
                new ChartOfAccount { Id = 7001, AccountNumber = "7001", Name = "Fixed Assets", AccountType = "Asset", IsActive = true },
                new ChartOfAccount { Id = 7002, AccountNumber = "7002", Name = "Accumulated Depreciation", AccountType = "Contra Asset", IsActive = true },
                new ChartOfAccount { Id = 7003, AccountNumber = "7003", Name = "Depreciation Expense", AccountType = "Expense", IsActive = true },
                new ChartOfAccount { Id = 7004, AccountNumber = "7004", Name = "Gain on Asset Disposal", AccountType = "Income", IsActive = true },
                new ChartOfAccount { Id = 7005, AccountNumber = "7005", Name = "Loss on Asset Disposal", AccountType = "Expense", IsActive = true },
                new ChartOfAccount { Id = 7006, AccountNumber = "7006", Name = "Revaluation Reserve", AccountType = "Equity", IsActive = true },
                new ChartOfAccount { Id = 7007, AccountNumber = "7007", Name = "Impairment Loss", AccountType = "Expense", IsActive = true }
            );
            
            // Add asset categories
            _dbContext.AssetCategories.AddRange(
                new AssetCategory { Id = 1, Name = "Computer Equipment", DepreciationRate = 0.25m, UsefulLifeYears = 4 },
                new AssetCategory { Id = 2, Name = "Office Furniture", DepreciationRate = 0.1m, UsefulLifeYears = 10 },
                new AssetCategory { Id = 3, Name = "Vehicles", DepreciationRate = 0.2m, UsefulLifeYears = 5 }
            );
            
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CompleteAssetLifecycle_ShouldCreateAllExpectedJournalEntries()
        {
            // Arrange
            var dispatcher = _serviceProvider.GetRequiredService<IDomainEventDispatcher>();
            
            // 1. Create a new fixed asset
            var asset = new FixedAsset
            {
                Id = 101,
                Name = "Server Equipment",
                Description = "High-performance server",
                AcquisitionDate = DateTime.Now.AddDays(-365),
                AcquisitionCost = 100000,
                CategoryId = 1, // Computer Equipment
                Location = "Headquarters",
                AssetTag = "SVR-2024-001",
                SerialNumber = "SRV123456789",
                Status = AssetStatus.Active,
                CurrentValue = 100000
            };
            
            // 2. Raise the asset acquired event
            var acquiredEvent = new AssetAcquiredEvent
            {
                AssetId = asset.Id,
                AcquisitionCost = asset.AcquisitionCost,
                TaxAmount = 15000, // VAT
                AssetCategory = "Computer Equipment",
                Reference = "PO-2024-12345",
                Description = $"Purchase of {asset.Name}"
            };
            
            await dispatcher.DispatchAsync(acquiredEvent);
            
            // 3. Raise a depreciation event
            var depreciationEvent = new AssetDepreciatedEvent
            {
                AssetId = asset.Id,
                DepreciationAmount = 2395.83m, // Monthly depreciation
                Period = "SEP-2025",
                Reference = $"DEP-{asset.Id}-SEP2025",
                Description = $"Monthly depreciation for {asset.Name}"
            };
            
            await dispatcher.DispatchAsync(depreciationEvent);
            
            // 4. Raise a revaluation event
            var revaluationEvent = new AssetRevaluedEvent
            {
                AssetId = asset.Id,
                RevaluationAmount = 10000, // Positive revaluation
                Reference = $"REV-{asset.Id}-2025",
                Description = $"Annual revaluation of {asset.Name}"
            };
            
            await dispatcher.DispatchAsync(revaluationEvent);
            
            // 5. Raise an impairment event
            var impairmentEvent = new AssetImpairedEvent
            {
                AssetId = asset.Id,
                ImpairmentAmount = 5000,
                Reference = $"IMP-{asset.Id}-2025",
                Description = $"Impairment due to technology obsolescence for {asset.Name}"
            };
            
            await dispatcher.DispatchAsync(impairmentEvent);
            
            // 6. Finally, dispose of the asset
            var disposalEvent = new AssetDisposedEvent
            {
                AssetId = asset.Id,
                DisposalProceeds = 80000,
                NetBookValue = 75000,
                GainLoss = 5000, // Gain
                Reference = $"DISP-{asset.Id}-2025",
                Description = $"Disposal of {asset.Name}"
            };
            
            await dispatcher.DispatchAsync(disposalEvent);
            
            // Assert - Check that all journal entries were created correctly
            var journalEntries = _dbContext.JournalEntries.Include(je => je.Lines).ToList();
            
            // Should have 5 journal entries (acquisition, depreciation, revaluation, impairment, disposal)
            Assert.Equal(5, journalEntries.Count);
            
            // Verify acquisition entry
            var acquisitionEntry = journalEntries.FirstOrDefault(je => je.Reference == "PO-2024-12345");
            Assert.NotNull(acquisitionEntry);
            Assert.Equal(2, acquisitionEntry.Lines.Count);
            Assert.Equal(115000, acquisitionEntry.Lines.Sum(l => l.DebitAmount));
            Assert.Equal(115000, acquisitionEntry.Lines.Sum(l => l.CreditAmount));
            
            // Verify depreciation entry
            var depreciationEntry = journalEntries.FirstOrDefault(je => je.Reference == $"DEP-{asset.Id}-SEP2025");
            Assert.NotNull(depreciationEntry);
            Assert.Equal(2, depreciationEntry.Lines.Count);
            Assert.Equal(2395.83m, depreciationEntry.Lines.Sum(l => l.DebitAmount));
            Assert.Equal(2395.83m, depreciationEntry.Lines.Sum(l => l.CreditAmount));
            
            // Verify revaluation entry
            var revaluationEntry = journalEntries.FirstOrDefault(je => je.Reference == $"REV-{asset.Id}-2025");
            Assert.NotNull(revaluationEntry);
            Assert.Equal(2, revaluationEntry.Lines.Count);
            Assert.Equal(10000, revaluationEntry.Lines.Sum(l => l.DebitAmount));
            Assert.Equal(10000, revaluationEntry.Lines.Sum(l => l.CreditAmount));
            
            // Verify impairment entry
            var impairmentEntry = journalEntries.FirstOrDefault(je => je.Reference == $"IMP-{asset.Id}-2025");
            Assert.NotNull(impairmentEntry);
            Assert.Equal(2, impairmentEntry.Lines.Count);
            Assert.Equal(5000, impairmentEntry.Lines.Sum(l => l.DebitAmount));
            Assert.Equal(5000, impairmentEntry.Lines.Sum(l => l.CreditAmount));
            
            // Verify disposal entry
            var disposalEntry = journalEntries.FirstOrDefault(je => je.Reference == $"DISP-{asset.Id}-2025");
            Assert.NotNull(disposalEntry);
            Assert.Equal(3, disposalEntry.Lines.Count);
            Assert.Equal(80000, disposalEntry.Lines.Sum(l => l.DebitAmount));
            Assert.Equal(80000, disposalEntry.Lines.Sum(l => l.CreditAmount));
        }
    }
}