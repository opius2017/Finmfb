using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface ISecurityAlertRepository : IRepository<SecurityAlert>
{
    Task<IEnumerable<SecurityAlert>> GetUserAlertsAsync(Guid userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteOldAlertsAsync(int daysToKeep, CancellationToken cancellationToken = default);
}
