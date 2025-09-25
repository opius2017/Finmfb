using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Regulatory Mappings
    /// </summary>
    public interface IRegulatoryMappingRepository : IRepository<RegulatoryMapping>
    {
        /// <summary>
        /// Gets all mappings for a specific chart of account
        /// </summary>
        Task<IReadOnlyList<RegulatoryMapping>> GetByChartOfAccountAsync(
            string chartOfAccountId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all mappings for a specific regulatory code
        /// </summary>
        Task<IReadOnlyList<RegulatoryMapping>> GetByRegulatoryCodeAsync(
            string regulatoryCodeId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all active mappings
        /// </summary>
        Task<IReadOnlyList<RegulatoryMapping>> GetActiveMappingsAsync(
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets a specific mapping between a chart of account and a regulatory code
        /// </summary>
        Task<RegulatoryMapping> GetMappingAsync(
            string chartOfAccountId, 
            string regulatoryCodeId, 
            CancellationToken cancellationToken = default);
    }
}
