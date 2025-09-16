using System;
using System.Threading.Tasks;
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Infrastructure.Data;
using FinTech.Application.Services.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Examples
{
    public class FixedAssetWorkflow
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IFixedAssetAccountingIntegrationService _fixedAssetAccountingService;
        private readonly ILogger<FixedAssetWorkflow> _logger;

        public FixedAssetWorkflow(
            ApplicationDbContext dbContext,
            IFixedAssetAccountingIntegrationService fixedAssetAccountingService,
            ILogger<FixedAssetWorkflow> logger)
        {
            _dbContext = dbContext;
            _fixedAssetAccountingService = fixedAssetAccountingService;
            _logger = logger;
        }

        public async Task RunFixedAssetLifecycleWorkflowAsync()
        {
            _logger.LogInformation("Starting Fixed Asset Lifecycle Workflow Example");
            
            try
            {
                // 1. Create asset categories
                var furnitureCategory = new AssetCategory(
                    categoryName: "Furniture and Fittings",
                    description: "Office furniture and fixtures",
                    defaultUsefulLifeYears: 10,
                    defaultDepreciationMethod: "STRAIGHT_LINE",
                    glAccountFixedAsset: "1200",
                    glAccountDepreciation: "5100",
                    glAccountAccumDepreciation: "1201",
                    glAccountDisposalGain: "4200",
                    glAccountDisposalLoss: "5200");

                var computerCategory = new AssetCategory(
                    categoryName: "Computer Equipment",
                    description: "Computers, servers, and IT hardware",
                    defaultUsefulLifeYears: 5,
                    defaultDepreciationMethod: "STRAIGHT_LINE",
                    glAccountFixedAsset: "1210",
                    glAccountDepreciation: "5110",
                    glAccountAccumDepreciation: "1211",
                    glAccountDisposalGain: "4210",
                    glAccountDisposalLoss: "5210");

                var vehicleCategory = new AssetCategory(
                    categoryName: "Motor Vehicles",
                    description: "Company vehicles",
                    defaultUsefulLifeYears: 8,
                    defaultDepreciationMethod: "REDUCING_BALANCE",
                    glAccountFixedAsset: "1220",
                    glAccountDepreciation: "5120",
                    glAccountAccumDepreciation: "1221",
                    glAccountDisposalGain: "4220",
                    glAccountDisposalLoss: "5220");

                _dbContext.AddRange(furnitureCategory, computerCategory, vehicleCategory);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Created asset categories: Furniture, Computer Equipment, and Motor Vehicles");

                // 2. Acquire new assets
                var officeDesks = new FixedAsset(
                    assetCode: "FUR-001",
                    assetName: "Executive Office Desks",
                    description: "Set of 10 executive office desks for the management floor",
                    assetCategory: "Furniture and Fittings",
                    acquisitionDate: DateTime.UtcNow,
                    acquisitionCost: 2500000,
                    residualValue: 250000,
                    usefulLifeYears: 10,
                    depreciationMethod: "STRAIGHT_LINE",
                    location: "Head Office - 3rd Floor",
                    serialNumber: "N/A",
                    model: "Executive Deluxe",
                    manufacturer: "OfficePro Inc.");

                var serverEquipment = new FixedAsset(
                    assetCode: "IT-001",
                    assetName: "Core Banking Servers",
                    description: "Primary server infrastructure for core banking system",
                    assetCategory: "Computer Equipment",
                    acquisitionDate: DateTime.UtcNow,
                    acquisitionCost: 15000000,
                    residualValue: 1500000,
                    usefulLifeYears: 5,
                    depreciationMethod: "STRAIGHT_LINE",
                    location: "Data Center - Basement",
                    serialNumber: "SRV-2023-5678",
                    model: "PowerEdge R740",
                    manufacturer: "Dell Technologies");

                var companyCar = new FixedAsset(
                    assetCode: "VEH-001",
                    assetName: "CEO Vehicle",
                    description: "Executive vehicle for CEO",
                    assetCategory: "Motor Vehicles",
                    acquisitionDate: DateTime.UtcNow,
                    acquisitionCost: 35000000,
                    residualValue: 7000000,
                    usefulLifeYears: 8,
                    depreciationMethod: "REDUCING_BALANCE",
                    location: "Head Office - Garage",
                    serialNumber: "WBA12345678XYZ",
                    model: "X7",
                    manufacturer: "BMW");

                _dbContext.AddRange(officeDesks, serverEquipment, companyCar);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Created new fixed assets: Office Desks, Servers, and Company Car");

                // 3. Record acquisition with tax details
                officeDesks.RecordAcquisition(
                    taxAmount: 375000, // 15% VAT
                    reference: "INV-FUR-001",
                    description: "Purchase of executive office desks");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded acquisition of office desks with cost 2,500,000 NGN + 375,000 NGN VAT");

                // NOTE: At this point, the AssetAcquiredEvent has been raised by the FixedAsset entity
                // and automatically processed by the DomainEventService during SaveChangesAsync
                // The FixedAssetAccountingIntegrationService has created journal entries for the acquisition

                serverEquipment.RecordAcquisition(
                    taxAmount: 2250000, // 15% VAT
                    reference: "INV-IT-001",
                    description: "Purchase of core banking servers");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded acquisition of servers with cost 15,000,000 NGN + 2,250,000 NGN VAT");

                companyCar.RecordAcquisition(
                    taxAmount: 5250000, // 15% VAT
                    reference: "INV-VEH-001",
                    description: "Purchase of executive vehicle");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded acquisition of company car with cost 35,000,000 NGN + 5,250,000 NGN VAT");

                // 4. Record depreciation for first month (simulating month-end process)
                // Office desks: Straight-line method (Cost - Residual) / Useful Life / 12 months
                decimal desksMonthlyDepreciation = (officeDesks.AcquisitionCost - officeDesks.ResidualValue) / officeDesks.UsefulLifeYears / 12;
                officeDesks.RecordDepreciation(
                    depreciationAmount: desksMonthlyDepreciation,
                    period: DateTime.UtcNow.ToString("MMM-yyyy"),
                    reference: "DEP-FUR-001",
                    description: "Monthly depreciation for office desks");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded first month depreciation of {Depreciation} NGN for office desks", desksMonthlyDepreciation);

                // Servers: Straight-line method
                decimal serversMonthlyDepreciation = (serverEquipment.AcquisitionCost - serverEquipment.ResidualValue) / serverEquipment.UsefulLifeYears / 12;
                serverEquipment.RecordDepreciation(
                    depreciationAmount: serversMonthlyDepreciation,
                    period: DateTime.UtcNow.ToString("MMM-yyyy"),
                    reference: "DEP-IT-001",
                    description: "Monthly depreciation for servers");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded first month depreciation of {Depreciation} NGN for servers", serversMonthlyDepreciation);

                // Car: Reducing balance method (simplified for demo)
                decimal carAnnualDepreciationRate = 0.25m; // 25% annual rate for reducing balance
                decimal carMonthlyDepreciation = companyCar.CurrentValue * carAnnualDepreciationRate / 12;
                companyCar.RecordDepreciation(
                    depreciationAmount: carMonthlyDepreciation,
                    period: DateTime.UtcNow.ToString("MMM-yyyy"),
                    reference: "DEP-VEH-001",
                    description: "Monthly depreciation for company car");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded first month depreciation of {Depreciation} NGN for company car", carMonthlyDepreciation);

                // 5. Simulate asset revaluation after 6 months
                // For demonstration, we'll jump ahead 6 months and revalue the company car
                _logger.LogInformation("Simulating passage of 6 months for asset revaluation");

                companyCar.Revalue(
                    revaluationAmount: 2000000,
                    reference: "REV-VEH-001",
                    description: "Upward revaluation due to currency fluctuation");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Revalued company car upward by 2,000,000 NGN");

                // 6. Simulate server impairment due to technical obsolescence
                serverEquipment.RecordImpairment(
                    impairmentAmount: 3000000,
                    reference: "IMP-IT-001",
                    description: "Impairment due to technological obsolescence");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded impairment of 3,000,000 NGN for server equipment");

                // 7. Simulate disposal of some office furniture
                officeDesks.Dispose(
                    disposalProceeds: 500000,
                    reference: "DISP-FUR-001",
                    description: "Partial disposal of old office furniture");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded disposal of office furniture with proceeds of 500,000 NGN");

                // 8. Retrieve assets with transactions to verify final state
                var updatedServerEquipment = await _dbContext.Set<FixedAsset>()
                    .Include(a => a.Transactions)
                    .FirstOrDefaultAsync(a => a.Id == serverEquipment.Id);

                _logger.LogInformation("Final server equipment details:");
                _logger.LogInformation("Current Value: {CurrentValue} NGN", updatedServerEquipment.CurrentValue);
                _logger.LogInformation("Accumulated Depreciation: {AccumulatedDepreciation} NGN", updatedServerEquipment.AccumulatedDepreciation);
                _logger.LogInformation("Number of transactions: {Count}", updatedServerEquipment.Transactions.Count);

                // For verification, you can query the accounting journal entries created by the integration service
                // var journalEntries = await _dbContext.JournalEntries
                //     .Where(je => je.Reference.StartsWith("INV-") || je.Reference.StartsWith("DEP-") || 
                //                 je.Reference.StartsWith("REV-") || je.Reference.StartsWith("IMP-") || 
                //                 je.Reference.StartsWith("DISP-"))
                //     .ToListAsync();
                // _logger.LogInformation("Number of fixed asset-related journal entries: {Count}", journalEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Fixed Asset Lifecycle Workflow Example");
                throw;
            }
        }
    }

    public static class FixedAssetWorkflowExtensions
    {
        public static async Task RunFixedAssetWorkflowExample(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var workflow = scope.ServiceProvider.GetRequiredService<FixedAssetWorkflow>();
            await workflow.RunFixedAssetLifecycleWorkflowAsync();
        }

        public static IServiceCollection AddFixedAssetWorkflowExample(this IServiceCollection services)
        {
            services.AddTransient<FixedAssetWorkflow>();
            return services;
        }
    }
}