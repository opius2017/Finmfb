using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface IUserSecurityPreferencesRepository : IRepository<UserSecurityPreferences>
{
    Task<UserSecurityPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSecurityPreferences> GetOrCreateAsync(Guid userId, CancellationToken cancellationToken = default);
}
