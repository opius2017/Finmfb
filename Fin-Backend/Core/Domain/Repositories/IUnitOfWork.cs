using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Repositories
{
    /// <summary>
    /// Unit of work interface for transaction management
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        IRepository<T> Repository<T>() where T : BaseEntity;
    }
}
