using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface IMfaSettingsRepository : IRepository<MfaSettings>
{
    Task<MfaSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsMfaEnabledAsync(Guid userId, CancellationToken cancellationToken = default);
    Task EnableMfaAsync(Guid userId, MfaMethod method, string? secret = null, CancellationToken cancellationToken = default);
    Task DisableMfaAsync(Guid userId, CancellationToken cancellationToken = default);
}
