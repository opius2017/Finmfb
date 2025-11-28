using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Shared.Abstractions
{
    /// <summary>
    /// Enhanced repository with CRUD operations
    /// </summary>
    public interface ICrudRepository<T> : IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Get entity by ID with tracking
        /// </summary>
        Task<T> GetByIdAsync(string id, bool trackChanges = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities with pagination
        /// </summary>
        Task<(List<T> items, int totalCount)> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entities by predicate
        /// </summary>
        Task<List<T>> GetByPredicateAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if entity exists
        /// </summary>
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update with conflict detection
        /// </summary>
        Task UpdateAsync(T entity, int expectedVersion = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft delete (mark as deleted)
        /// </summary>
        Task SoftDeleteAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Batch operations
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    }
}
