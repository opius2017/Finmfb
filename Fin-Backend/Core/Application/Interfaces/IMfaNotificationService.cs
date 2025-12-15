using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces
{
    public interface IMfaNotificationService
    {
        Task SendMfaSetupSuccessEmailAsync(string userId, string username, string email);
        Task SendNewLoginNotificationAsync(string userId, string email, string ipAddress, string deviceInfo, string location);
        Task SendSuspiciousActivityAlertAsync(string userId, string email, string activityType, string ipAddress, string deviceInfo, string location);
        Task SendBackupCodesGeneratedEmailAsync(string userId, string username, string email);
        Task SendSecurityPreferencesUpdatedEmailAsync(string userId, string email);
        Task SendTrustedDeviceAddedEmailAsync(string userId, string email, string deviceInfo);
        Task SendTrustedDeviceRemovedEmailAsync(string userId, string email, string deviceInfo);
        Task SendSecurityAlertEmailAsync(string userId, string email, string title, string message);
    }
}
