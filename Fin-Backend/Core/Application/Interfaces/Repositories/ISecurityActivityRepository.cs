using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Identity; // Using Identity version

namespace FinTech.Core.Application.Interfaces.Repositories
{
    public interface ISecurityActivityRepository : IBaseAuthRepository<SecurityActivity>
    {
        Task<SecurityActivity> LogActivityAsync(string userId, string eventType, string details, string ipAddress, string userAgent);
        Task<IEnumerable<SecurityActivity>> GetRecentActivityAsync(string userId, int count = 10);
        Task<IEnumerable<SecurityActivity>> GetByUserIdAsync(string userId);
    }
}
