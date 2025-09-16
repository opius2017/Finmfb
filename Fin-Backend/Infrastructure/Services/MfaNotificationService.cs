using FinTech.WebAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Services.Integrations;

namespace FinTech.WebAPI.Infrastructure.Services
{
    public class MfaNotificationService : IMfaNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<MfaNotificationService> _logger;
        
        public MfaNotificationService(
            IEmailService emailService,
            ILogger<MfaNotificationService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }
        
        public async Task SendMfaSetupSuccessEmail(string email, string username)
        {
            try
            {
                var subject = "Two-Factor Authentication Enabled";
                var templateRequest = new TemplatedEmailRequest
                {
                    ToEmail = email,
                    Subject = subject,
                    TemplateName = "mfa-setup-success",
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "Username", username },
                        { "SetupDate", DateTime.UtcNow.ToString("MMMM dd, yyyy, HH:mm UTC") }
                    }
                };
                
                await _emailService.SendTemplatedEmailAsync(templateRequest);
                _logger.LogInformation("Sent MFA setup success email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending MFA setup success email to {Email}", email);
            }
        }
        
        public async Task SendLoginNotificationEmail(string email, string username, string browser, string operatingSystem, string ipAddress, DateTime timestamp, bool isNewTrustedDevice = false)
        {
            try
            {
                var subject = isNewTrustedDevice 
                    ? "New Device Added to Trusted Devices" 
                    : "New Login to Your Account";
                
                var templateRequest = new TemplatedEmailRequest
                {
                    ToEmail = email,
                    Subject = subject,
                    TemplateName = "new-login-notification",
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "Username", username },
                        { "LoginDate", timestamp.ToString("MMMM dd, yyyy, HH:mm UTC") },
                        { "Browser", browser },
                        { "OperatingSystem", operatingSystem },
                        { "IpAddress", ipAddress },
                        { "IsTrustedDevice", isNewTrustedDevice.ToString() }
                    }
                };
                
                await _emailService.SendTemplatedEmailAsync(templateRequest);
                _logger.LogInformation("Sent login notification email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending login notification email to {Email}", email);
            }
        }
        
        public async Task SendSuspiciousActivityAlertEmail(string email, string username, string browser, string operatingSystem, string ipAddress, DateTime timestamp)
        {
            try
            {
                var subject = "Suspicious Login Attempt Detected";
                var templateRequest = new TemplatedEmailRequest
                {
                    ToEmail = email,
                    Subject = subject,
                    TemplateName = "suspicious-activity-alert",
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "Username", username },
                        { "ActivityDate", timestamp.ToString("MMMM dd, yyyy, HH:mm UTC") },
                        { "Browser", browser },
                        { "OperatingSystem", operatingSystem },
                        { "IpAddress", ipAddress }
                    }
                };
                
                await _emailService.SendTemplatedEmailAsync(templateRequest);
                _logger.LogInformation("Sent suspicious activity alert email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending suspicious activity alert email to {Email}", email);
            }
        }
        
        public async Task SendBackupCodesGeneratedEmail(string email, string username, List<string> backupCodes)
        {
            try
            {
                var subject = "New Backup Codes Generated";
                
                // Create a formatted list of backup codes
                var codesHtml = string.Empty;
                foreach (var code in backupCodes)
                {
                    codesHtml += $"<li style=\"font-family: monospace; font-size: 16px; padding: 4px 0;\">{code}</li>";
                }
                
                var templateRequest = new TemplatedEmailRequest
                {
                    ToEmail = email,
                    Subject = subject,
                    TemplateName = "backup-codes-generated",
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "Username", username },
                        { "GenerationDate", DateTime.UtcNow.ToString("MMMM dd, yyyy, HH:mm UTC") },
                        { "BackupCodesList", codesHtml }
                    }
                };
                
                await _emailService.SendTemplatedEmailAsync(templateRequest);
                _logger.LogInformation("Sent backup codes generated email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending backup codes generated email to {Email}", email);
            }
        }
        
        public async Task SendSecurityAlertEmail(string email, string username, string subject, string message)
        {
            try
            {
                var templateRequest = new TemplatedEmailRequest
                {
                    ToEmail = email,
                    Subject = subject,
                    TemplateName = "security-alert",
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "Username", username },
                        { "AlertDate", DateTime.UtcNow.ToString("MMMM dd, yyyy, HH:mm UTC") },
                        { "AlertMessage", message }
                    }
                };
                
                await _emailService.SendTemplatedEmailAsync(templateRequest);
                _logger.LogInformation("Sent security alert email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security alert email to {Email}", email);
            }
        }
    }
}