using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Repositories
{
    /// <summary>
    /// Generic repository interface for domain entities
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);
        IQueryable<T> GetAll();
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<T> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }
}
