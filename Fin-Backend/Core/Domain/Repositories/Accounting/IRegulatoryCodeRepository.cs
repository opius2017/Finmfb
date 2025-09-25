using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Regulatory Codes
    /// </summary>
    public interface IRegulatoryCodeRepository : IRepository<RegulatoryCode>
    {
        /// <summary>
        /// Gets a regulatory code by its code
        /// </summary>
        Task<RegulatoryCode> GetByCodeAsync(
            string code, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all regulatory codes by category
        /// </summary>
        Task<IReadOnlyList<RegulatoryCode>> GetByCategoryAsync(
            string category, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all regulatory codes by authority
        /// </summary>
        Task<IReadOnlyList<RegulatoryCode>> GetByAuthorityAsync(
            string authority, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all regulatory codes by reporting form
        /// </summary>
        Task<IReadOnlyList<RegulatoryCode>> GetByReportingFormAsync(
            string reportingForm, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all active regulatory codes
        /// </summary>
        Task<IReadOnlyList<RegulatoryCode>> GetActiveCodesAsync(
            CancellationToken cancellationToken = default);
    }
}
