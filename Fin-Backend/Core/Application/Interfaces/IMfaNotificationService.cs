using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.WebAPI.Application.Interfaces
{
    /// <summary>
    /// Service responsible for sending MFA-related email notifications
    /// </summary>
    public interface IMfaNotificationService
    {
        /// <summary>
        /// Sends a notification email when MFA setup is successful
        /// </summary>
        Task SendMfaSetupSuccessEmail(string email, string username);
        
        /// <summary>
        /// Sends a notification email for a new login
        /// </summary>
        Task SendLoginNotificationEmail(string email, string username, string browser, string operatingSystem, string ipAddress, DateTime timestamp, bool isNewTrustedDevice = false);
        
        /// <summary>
        /// Sends an alert email for suspicious login activity
        /// </summary>
        Task SendSuspiciousActivityAlertEmail(string email, string username, string browser, string operatingSystem, string ipAddress, DateTime timestamp);
        
        /// <summary>
        /// Sends a notification email when backup codes are generated
        /// </summary>
        Task SendBackupCodesGeneratedEmail(string email, string username, List<string> backupCodes);
        
        /// <summary>
        /// Sends a security alert email for various security events
        /// </summary>
        Task SendSecurityAlertEmail(string email, string username, string subject, string message);
    }
}
