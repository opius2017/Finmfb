using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Repositories;

public interface ISocialLoginProfileRepository : IRepository<SocialLoginProfile>
{
    Task<SocialLoginProfile?> GetByProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
    Task<SocialLoginProfile?> GetByProviderKeyAsync(string provider, string providerKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<SocialLoginProfile>> GetUserProfilesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsProviderLinkedAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
}
