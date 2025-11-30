using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface ITrustedDeviceRepository : IRepository<TrustedDevice>
{
    Task<TrustedDevice?> GetByDeviceIdAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrustedDevice>> GetActiveDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsDeviceTrustedAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task RevokeTrustAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredDevicesAsync(CancellationToken cancellationToken = default);
}
