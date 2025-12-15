using FinTech.Core.Application.Interfaces.Services.Integration;
// using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Events.FixedAssets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Events;

namespace FinTech.Infrastructure.Services.Integration
{
    public class FixedAssetDomainEventHandler : 
        IDomainEventHandler<AssetAcquiredEvent>,
        IDomainEventHandler<AssetDepreciatedEvent>,
        IDomainEventHandler<AssetDisposedEvent>,
        IDomainEventHandler<AssetRevaluedEvent>,
        IDomainEventHandler<AssetImpairedEvent>
    {
        private readonly IFixedAssetAccountingIntegrationService _integrationService;
        private readonly ILogger<FixedAssetDomainEventHandler> _logger;

        public FixedAssetDomainEventHandler(
            IFixedAssetAccountingIntegrationService integrationService,
            ILogger<FixedAssetDomainEventHandler> logger)
        {
            _integrationService = integrationService;
            _logger = logger;
        }

        public async Task HandleAsync(AssetAcquiredEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetAcquiredEvent for asset {AssetId}", domainEvent.AssetId);
            
            await _integrationService.ProcessAssetAcquisitionAsync(
                domainEvent.AssetId,
                domainEvent.AcquisitionCost,
                domainEvent.TaxAmount,
                domainEvent.AssetCategory,
                domainEvent.Reference,
                domainEvent.Description);
                
            _logger.LogInformation("Successfully processed asset acquisition for asset {AssetId}", domainEvent.AssetId);
        }

        public async Task HandleAsync(AssetDepreciatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetDepreciatedEvent for asset {AssetId}", domainEvent.AssetId);
            
            await _integrationService.ProcessAssetDepreciationAsync(
                domainEvent.AssetId,
                domainEvent.DepreciationAmount,
                domainEvent.Period,
                domainEvent.Reference,
                domainEvent.Description);
                
            _logger.LogInformation("Successfully processed asset depreciation for asset {AssetId}", domainEvent.AssetId);
        }

        public async Task HandleAsync(AssetDisposedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetDisposedEvent for asset {AssetId}", domainEvent.AssetId);
            
            await _integrationService.ProcessAssetDisposalAsync(
                domainEvent.AssetId,
                domainEvent.DisposalProceeds,
                domainEvent.NetBookValue,
                domainEvent.GainLoss,
                domainEvent.Reference,
                domainEvent.Description);
                
            _logger.LogInformation("Successfully processed asset disposal for asset {AssetId}", domainEvent.AssetId);
        }

        public async Task HandleAsync(AssetRevaluedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetRevaluedEvent for asset {AssetId}", domainEvent.AssetId);
            
            await _integrationService.ProcessAssetRevaluationAsync(
                domainEvent.AssetId,
                domainEvent.RevaluationAmount,
                domainEvent.Reference,
                domainEvent.Description);
                
            _logger.LogInformation("Successfully processed asset revaluation for asset {AssetId}", domainEvent.AssetId);
        }

        public async Task HandleAsync(AssetImpairedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetImpairedEvent for asset {AssetId}", domainEvent.AssetId);
            
            await _integrationService.ProcessAssetImpairmentAsync(
                domainEvent.AssetId,
                domainEvent.ImpairmentAmount,
                domainEvent.Reference,
                domainEvent.Description);
                
            _logger.LogInformation("Successfully processed asset impairment for asset {AssetId}", domainEvent.AssetId);
        }
    }
}
