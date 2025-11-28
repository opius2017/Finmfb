using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// using FinTech.Core.Application.Services.Accounting;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for FixedAsset entity
    /// </summary>
    public interface IFixedAssetRepository
    {
        /// <summary>
        /// Gets a fixed asset by ID
        /// </summary>
        Task<FixedAsset> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a fixed asset by asset number
        /// </summary>
        Task<FixedAsset> GetByAssetNumberAsync(string assetNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all fixed assets
        /// </summary>
        Task<IEnumerable<FixedAsset>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all fixed assets by category
        /// </summary>
        Task<IEnumerable<FixedAsset>> GetByAssetCategoryAsync(string category, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all fixed assets by status
        /// </summary>
        Task<IEnumerable<FixedAsset>> GetByStatusAsync(FixedAssetStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all fixed assets that use a specific GL account
        /// </summary>
        Task<IEnumerable<FixedAsset>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new fixed asset
        /// </summary>
        Task<FixedAsset> AddAsync(FixedAsset fixedAsset, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing fixed asset
        /// </summary>
        Task<FixedAsset> UpdateAsync(FixedAsset fixedAsset, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a fixed asset
        /// </summary>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
