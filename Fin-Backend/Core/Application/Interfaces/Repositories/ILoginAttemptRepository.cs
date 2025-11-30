using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface ILoginAttemptRepository : IRepository<LoginAttempt>
{
    Task<IEnumerable<LoginAttempt>> GetRecentAttemptsAsync(Guid userId, int minutes, CancellationToken cancellationToken = default);
    Task<int> GetFailedAttemptsCountAsync(Guid userId, int minutes, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoginAttempt>> GetAttemptsByIpAsync(string ipAddress, int minutes, CancellationToken cancellationToken = default);
    Task<int> DeleteOldAttemptsAsync(int daysToKeep, CancellationToken cancellationToken = default);
}
