using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface ISecurityEventService
    {
        Task LogEventAsync(SecurityEvent securityEvent);
        Task LogSuspiciousActivityAsync(string username, string ipAddress, string userAgent, string reason, string severity);
    }
}
