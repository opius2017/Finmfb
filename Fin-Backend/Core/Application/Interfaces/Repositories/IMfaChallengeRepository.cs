using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface IMfaChallengeRepository : IRepository<MfaChallenge>
{
    Task<MfaChallenge?> GetValidChallengeAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredChallengesAsync(CancellationToken cancellationToken = default);
    Task InvalidateChallengeAsync(Guid challengeId, CancellationToken cancellationToken = default);
}
