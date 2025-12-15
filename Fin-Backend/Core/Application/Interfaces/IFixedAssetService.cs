using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Accounting;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IFixedAssetService
    {
        Task<FixedAssetDto> CreateFixedAssetAsync(
            CreateFixedAssetDto assetDto,
            CancellationToken cancellationToken = default);

        Task<FixedAssetDto> GetFixedAssetAsync(
            string assetId,
            CancellationToken cancellationToken = default);

        Task<FixedAssetDto> UpdateFixedAssetAsync(
            string assetId,
            UpdateFixedAssetDto assetDto,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteFixedAssetAsync(
            string assetId,
            CancellationToken cancellationToken = default);

        Task<List<FixedAssetDto>> GetAllFixedAssetsAsync(
            CancellationToken cancellationToken = default);

        Task<FixedAssetDto> CalculateDepreciationAsync(
            string assetId,
            DateTime asOfDate,
            CancellationToken cancellationToken = default);

        Task<DepreciationScheduleDto> GenerateDepreciationScheduleAsync(
            string assetId,
            CancellationToken cancellationToken = default);

        Task<bool> RecordDepreciationAsync(
            string assetId,
            string financialPeriodId,
            CancellationToken cancellationToken = default);

        Task<FixedAssetDisposalDto> RecordAssetDisposalAsync(
            string assetId,
            AssetDisposalDto disposalDto,
            CancellationToken cancellationToken = default);

        Task<FixedAssetReportDto> GenerateFixedAssetReportAsync(
            string financialPeriodId,
            CancellationToken cancellationToken = default);
    }
}
