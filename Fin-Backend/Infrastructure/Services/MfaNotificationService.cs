using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using FinTech.Core.Application.DTOs.Email;

namespace FinTech.Infrastructure.Services
{
    public class MfaNotificationService : IMfaNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<MfaNotificationService> _logger;

        public MfaNotificationService(
            IEmailService emailService,
            UserManager<IdentityUser> userManager,
            ILogger<MfaNotificationService> logger)
        {
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SendMfaSetupSuccessEmailAsync(string userId, string username, string email)
        {
            try
            {
                _logger.LogInformation("Sending MFA setup success email to user {UserId}", userId);

                var parameters = new Dictionary<string, string>
                {
                    { "UserName", username },
                    { "SecurityPageUrl", "https://fintechmfb.com/account/security" },
                    { "PrivacyPolicyUrl", "https://fintechmfb.com/privacy" },
                    { "TermsOfServiceUrl", "https://fintechmfb.com/terms" },
                    { "UnsubscribeUrl", "https://fintechmfb.com/unsubscribe" }
                };

                var request = new TemplatedEmailRequest
                {
                    To = email,
                    Subject = "Two-Factor Authentication Enabled on Your Account",
                    TemplateName = "mfa-setup-success",
                    TemplateData = parameters
                };

                var result = await _emailService.SendTemplatedEmailAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("MFA setup success email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send MFA setup success email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending MFA setup success email to user {UserId}", userId);
            }
        }

        public async Task SendNewLoginNotificationAsync(string userId, string email, string ipAddress, string deviceInfo, string location)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for sending login notification: {UserId}", userId);
                    return;
                }

                _logger.LogInformation("Sending new login notification email to user {UserId}", userId);

                var loginTime = DateTime.UtcNow.ToString("f", CultureInfo.InvariantCulture);

                var parameters = new Dictionary<string, string>
                {
                    { "UserName", user.UserName },
                    { "LoginTime", loginTime },
                    { "DeviceInfo", deviceInfo },
                    { "Location", location },
                    { "IpAddress", ipAddress },
                    { "SecurityPageUrl", "https://fintechmfb.com/account/security" },
                    { "SecurityCenterUrl", "https://fintechmfb.com/security-center" },
                    { "PrivacyPolicyUrl", "https://fintechmfb.com/privacy" },
                    { "TermsOfServiceUrl", "https://fintechmfb.com/terms" },
                    { "UnsubscribeUrl", "https://fintechmfb.com/unsubscribe" }
                };

                var request = new TemplatedEmailRequest
                {
                    To = email,
                    Subject = "New Login to Your FinTech MFB Account",
                    TemplateName = "new-login-notification",
                    TemplateData = parameters
                };

                var result = await _emailService.SendTemplatedEmailAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("New login notification email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send new login notification email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new login notification email to user {UserId}", userId);
            }
        }

        public async Task SendSuspiciousActivityAlertAsync(string userId, string email, string activityType, string ipAddress, string deviceInfo, string location)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for sending suspicious activity alert: {UserId}", userId);
                    return;
                }

                _logger.LogInformation("Sending suspicious activity alert email to user {UserId}", userId);

                var activityTime = DateTime.UtcNow.ToString("f", CultureInfo.InvariantCulture);

                var parameters = new Dictionary<string, string>
                {
                    { "UserName", user.UserName },
                    { "ActivityType", activityType },
                    { "ActivityTime", activityTime },
                    { "DeviceInfo", deviceInfo },
                    { "Location", location },
                    { "IpAddress", ipAddress },
                    { "SecurityPageUrl", "https://fintechmfb.com/account/security" },
                    { "PrivacyPolicyUrl", "https://fintechmfb.com/privacy" },
                    { "TermsOfServiceUrl", "https://fintechmfb.com/terms" },
                    { "UnsubscribeUrl", "https://fintechmfb.com/unsubscribe" }
                };

                var request = new TemplatedEmailRequest
                {
                    To = email,
                    Subject = "SECURITY ALERT: Suspicious Activity Detected on Your Account",
                    TemplateName = "suspicious-activity-alert",
                    TemplateData = parameters
                };

                var result = await _emailService.SendTemplatedEmailAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("Suspicious activity alert email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send suspicious activity alert email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending suspicious activity alert email to user {UserId}", userId);
            }
        }

        public async Task SendBackupCodesGeneratedEmailAsync(string userId, string username, string email)
        {
            try
            {
                _logger.LogInformation("Sending backup codes generated email to user {UserId}", userId);

                var parameters = new Dictionary<string, string>
                {
                    { "UserName", username },
                    { "SecurityPageUrl", "https://fintechmfb.com/account/security/backup-codes" },
                    { "PrivacyPolicyUrl", "https://fintechmfb.com/privacy" },
                    { "TermsOfServiceUrl", "https://fintechmfb.com/terms" },
                    { "UnsubscribeUrl", "https://fintechmfb.com/unsubscribe" }
                };

                var request = new TemplatedEmailRequest
                {
                    To = email,
                    Subject = "Your FinTech MFB Account Backup Codes Have Been Generated",
                    TemplateName = "backup-codes-generated",
                    TemplateData = parameters
                };

                var result = await _emailService.SendTemplatedEmailAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("Backup codes generated email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send backup codes generated email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending backup codes generated email to user {UserId}", userId);
            }
        }

        public async Task SendSecurityPreferencesUpdatedEmailAsync(string userId, string email)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for sending security preferences update: {UserId}", userId);
                    return;
                }

                _logger.LogInformation("Sending security preferences updated email to user {UserId}", userId);

                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = "Your FinTech MFB Security Preferences Have Been Updated",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <div style='background-color: #10b981; padding: 20px; text-align: center; color: white; border-radius: 5px 5px 0 0;'>
                                    <h1>Security Preferences Updated</h1>
                                </div>
                                <div style='background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px;'>
                                    <p>Dear {user.UserName},</p>
                                    
                                    <p>We're writing to inform you that the security preferences for your FinTech MFB account have been updated.</p>
                                    
                                    <p>If you made these changes, no further action is required.</p>
                                    
                                    <p>If you did NOT make these changes, please contact our support team immediately at <a href='mailto:support@fintechmfb.com'>support@fintechmfb.com</a> or call +234 800 123 4567.</p>
                                    
                                    <p>You can review your current security settings by visiting your <a href='https://fintechmfb.com/account/security'>Account Security page</a>.</p>
                                    
                                    <p>Thank you for helping us keep your account secure.</p>
                                    
                                    <p>Best regards,<br>
                                    The FinTech MFB Security Team</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                    IsHtml = true
                };

                var result = await _emailService.SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Security preferences updated email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send security preferences updated email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security preferences updated email to user {UserId}", userId);
            }
        }

        public async Task SendTrustedDeviceAddedEmailAsync(string userId, string email, string deviceInfo)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for sending trusted device added notification: {UserId}", userId);
                    return;
                }

                _logger.LogInformation("Sending trusted device added email to user {UserId}", userId);

                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = "New Trusted Device Added to Your FinTech MFB Account",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <div style='background-color: #10b981; padding: 20px; text-align: center; color: white; border-radius: 5px 5px 0 0;'>
                                    <h1>New Trusted Device Added</h1>
                                </div>
                                <div style='background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px;'>
                                    <p>Dear {user.UserName},</p>
                                    
                                    <p>A new device has been added to your list of trusted devices for your FinTech MFB account.</p>
                                    
                                    <p><strong>Device Information:</strong> {deviceInfo}</p>
                                    <p><strong>Added On:</strong> {DateTime.UtcNow.ToString("f", CultureInfo.InvariantCulture)}</p>
                                    
                                    <p>If you authorized this device, no further action is required.</p>
                                    
                                    <p>If you did NOT authorize this device, please:</p>
                                    <ol>
                                        <li>Log in to your account</li>
                                        <li>Go to Security Settings</li>
                                        <li>Remove this device from your trusted devices list</li>
                                        <li>Change your password immediately</li>
                                        <li>Contact our support team at <a href='mailto:support@fintechmfb.com'>support@fintechmfb.com</a></li>
                                    </ol>
                                    
                                    <p>You can manage your trusted devices by visiting your <a href='https://fintechmfb.com/account/security/trusted-devices'>Trusted Devices page</a>.</p>
                                    
                                    <p>Thank you for helping us keep your account secure.</p>
                                    
                                    <p>Best regards,<br>
                                    The FinTech MFB Security Team</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                    IsHtml = true
                };

                var result = await _emailService.SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Trusted device added email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send trusted device added email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending trusted device added email to user {UserId}", userId);
            }
        }

        public async Task SendTrustedDeviceRemovedEmailAsync(string userId, string email, string deviceInfo)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for sending trusted device removed notification: {UserId}", userId);
                    return;
                }

                _logger.LogInformation("Sending trusted device removed email to user {UserId}", userId);

                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = "Trusted Device Removed From Your FinTech MFB Account",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <div style='background-color: #f97316; padding: 20px; text-align: center; color: white; border-radius: 5px 5px 0 0;'>
                                    <h1>Trusted Device Removed</h1>
                                </div>
                                <div style='background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px;'>
                                    <p>Dear {user.UserName},</p>
                                    
                                    <p>A device has been removed from your list of trusted devices for your FinTech MFB account.</p>
                                    
                                    <p><strong>Device Information:</strong> {deviceInfo}</p>
                                    <p><strong>Removed On:</strong> {DateTime.UtcNow.ToString("f", CultureInfo.InvariantCulture)}</p>
                                    
                                    <p>If you removed this device, no further action is required.</p>
                                    
                                    <p>If you did NOT remove this device, please:</p>
                                    <ol>
                                        <li>Change your password immediately</li>
                                        <li>Review all your trusted devices</li>
                                        <li>Contact our support team at <a href='mailto:support@fintechmfb.com'>support@fintechmfb.com</a> or call +234 800 123 4567</li>
                                    </ol>
                                    
                                    <p>You can manage your trusted devices by visiting your <a href='https://fintechmfb.com/account/security/trusted-devices'>Trusted Devices page</a>.</p>
                                    
                                    <p>Stay secure,<br>
                                    The FinTech MFB Security Team</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                    IsHtml = true
                };

                var result = await _emailService.SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Trusted device removed email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send trusted device removed email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending trusted device removed email to user {UserId}", userId);
            }
        }

        public async Task SendSecurityAlertEmailAsync(string userId, string email, string title, string message)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for sending security alert: {UserId}", userId);
                    return;
                }

                _logger.LogInformation("Sending security alert email to user {UserId}", userId);

                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = title,
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <div style='background-color: #ef4444; padding: 20px; text-align: center; color: white; border-radius: 5px 5px 0 0;'>
                                    <h1>Security Alert</h1>
                                </div>
                                <div style='background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; border-radius: 0 0 5px 5px;'>
                                    <p>Dear {user.UserName},</p>
                                    
                                    <p>{message}</p>
                                    
                                    <p>If you did not initiate this action, please secure your account immediately or contact support.</p>
                                    
                                    <p>Best regards,<br>
                                    The FinTech MFB Security Team</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ",
                    IsHtml = true
                };

                var result = await _emailService.SendEmailAsync(emailRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Security alert email sent to user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to send security alert email to user {UserId}: {Message}", userId, result.ErrorMessage ?? "Unknown Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security alert email to user {UserId}", userId);
            }
        }
    }
}
