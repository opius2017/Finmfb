using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services.Integration;
using FinTech.Core.Domain.Events.FixedAssets;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services.EventHandlers.FixedAssets
{
    public class FixedAssetEventHandlers : 
        IDomainEventHandler<AssetAcquiredEvent>,
        IDomainEventHandler<AssetDepreciatedEvent>,
        IDomainEventHandler<AssetDisposedEvent>,
        IDomainEventHandler<AssetRevaluedEvent>,
        IDomainEventHandler<AssetImpairedEvent>
    {
        private readonly IFixedAssetAccountingIntegrationService _fixedAssetAccountingIntegrationService;
        private readonly ILogger<FixedAssetEventHandlers> _logger;

        public FixedAssetEventHandlers(
            IFixedAssetAccountingIntegrationService fixedAssetAccountingIntegrationService,
            ILogger<FixedAssetEventHandlers> logger)
        {
            _fixedAssetAccountingIntegrationService = fixedAssetAccountingIntegrationService;
            _logger = logger;
        }

        public async Task HandleAsync(AssetAcquiredEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetAcquiredEvent for AssetId: {AssetId}, Category: {AssetCategory}", 
                domainEvent.AssetId, domainEvent.AssetCategory);
            await _fixedAssetAccountingIntegrationService.ProcessAssetAcquisitionAsync(
                domainEvent.AssetId,
                domainEvent.AcquisitionCost,
                domainEvent.TaxAmount,
                domainEvent.AssetCategory,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(AssetDepreciatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetDepreciatedEvent for AssetId: {AssetId}, Period: {Period}", 
                domainEvent.AssetId, domainEvent.Period);
            await _fixedAssetAccountingIntegrationService.ProcessAssetDepreciationAsync(
                domainEvent.AssetId,
                domainEvent.DepreciationAmount,
                domainEvent.Period,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(AssetDisposedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetDisposedEvent for AssetId: {AssetId}", domainEvent.AssetId);
            await _fixedAssetAccountingIntegrationService.ProcessAssetDisposalAsync(
                domainEvent.AssetId,
                domainEvent.DisposalProceeds,
                domainEvent.NetBookValue,
                domainEvent.GainLoss,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(AssetRevaluedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetRevaluedEvent for AssetId: {AssetId}", domainEvent.AssetId);
            await _fixedAssetAccountingIntegrationService.ProcessAssetRevaluationAsync(
                domainEvent.AssetId,
                domainEvent.RevaluationAmount,
                domainEvent.Reference,
                domainEvent.Description);
        }

        public async Task HandleAsync(AssetImpairedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling AssetImpairedEvent for AssetId: {AssetId}", domainEvent.AssetId);
            await _fixedAssetAccountingIntegrationService.ProcessAssetImpairmentAsync(
                domainEvent.AssetId,
                domainEvent.ImpairmentAmount,
                domainEvent.Reference,
                domainEvent.Description);
        }
    }
}
