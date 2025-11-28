using FinTech.Core.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Common.Interfaces
{
    public interface IJwtService
    {
        Task<(string token, string refreshToken, System.DateTime expiresAt)> GenerateTokensAsync(ApplicationUser user, IList<string> roles);
    }
}
