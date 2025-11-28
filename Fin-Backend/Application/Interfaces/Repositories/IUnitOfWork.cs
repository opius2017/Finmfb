using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : FinTech.Core.Domain.Repositories.IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : BaseEntity;
    }
}
