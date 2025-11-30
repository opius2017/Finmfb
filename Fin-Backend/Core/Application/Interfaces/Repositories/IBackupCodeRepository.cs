using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface IBackupCodeRepository : IRepository<BackupCode>
{
    Task<BackupCode?> GetValidCodeAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<BackupCode>> GetUnusedCodesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnusedCodeCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkCodeAsUsedAsync(Guid codeId, CancellationToken cancellationToken = default);
    Task GenerateCodesAsync(Guid userId, int count, CancellationToken cancellationToken = default);
}
