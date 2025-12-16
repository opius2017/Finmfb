using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;
using FinTech.Core.Application.DTOs.Email;
using FinTech.Core.Application.DTOs.Notification; // Just in case
using FinTech.Core.Application.Interfaces;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Email service for sending transactional emails, statements, and marketing communications
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(
            ILogger<EmailService> logger,
            IOptions<EmailSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<EmailResponse> SendEmailAsync(EmailRequest request)
        {
            try
            {
                _logger.LogInformation("Sending email to: {ToEmail}, Subject: {Subject}", 
                    MaskEmail(request.To), request.Subject);

                using var client = CreateSmtpClient();
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = request.IsHtml
                };

                message.To.Add(request.To);

                if (request.Cc != null)
                {
                    foreach (var cc in request.Cc)
                    {
                        message.CC.Add(cc);
                    }
                }

                if (request.Bcc != null)
                {
                    foreach (var bcc in request.Bcc)
                    {
                        message.Bcc.Add(bcc);
                    }
                }

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email successfully sent to: {ToEmail}", MaskEmail(request.To));
                
                return new EmailResponse
                {
                    Success = true,
                    ErrorMessage = "Email sent successfully" 
                    // EmailId = Guid.NewGuid().ToString() // not in DTO
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Exception occurred while sending email to: {ToEmail}", 
                    MaskEmail(request.To));
                
                return new EmailResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to send email: {ex.Message}"
                };
            }
        }

        public async Task<EmailResponse> SendTemplatedEmailAsync(TemplatedEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Sending templated email to: {ToEmail}, Template: {TemplateName}", 
                    MaskEmail(request.To), request.TemplateName);

                string templateContent = await LoadTemplateAsync(request.TemplateName);

                foreach (var param in request.TemplateData)
                {
                    templateContent = templateContent.Replace($"{{{param.Key}}}", param.Value);
                }

                var emailRequest = new EmailRequest
                {
                    To = request.To,
                    Subject = request.Subject,
                    Body = templateContent,
                    IsHtml = true,
                    Cc = request.Cc,
                    Bcc = request.Bcc
                };

                return await SendEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending templated email to: {ToEmail}, Template: {TemplateName}", 
                    MaskEmail(request.To), request.TemplateName);
                
                return new EmailResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to send templated email: {ex.Message}"
                };
            }
        }

        public async Task<EmailResponse> SendEmailWithAttachmentAsync(EmailWithAttachmentRequest request)
        {
            try
            {
                _logger.LogInformation("Sending email with attachment to: {ToEmail}, Subject: {Subject}", 
                    MaskEmail(request.To), request.Subject);

                using var client = CreateSmtpClient();
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = request.IsHtml
                };

                message.To.Add(request.To);

                if (request.Cc != null)
                {
                    foreach (var cc in request.Cc)
                    {
                        message.CC.Add(cc);
                    }
                }

                if (request.Bcc != null)
                {
                    foreach (var bcc in request.Bcc)
                    {
                        message.Bcc.Add(bcc);
                    }
                }

                foreach (var attachment in request.Attachments)
                {
                    var ms = new MemoryStream(attachment.Content);
                    message.Attachments.Add(new Attachment(ms, attachment.FileName, attachment.ContentType));
                }

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email with attachment successfully sent to: {ToEmail}", 
                    MaskEmail(request.To));
                
                return new EmailResponse
                {
                    Success = true,
                    // MessageId = Guid.NewGuid().ToString() // not in default response but could be added
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email with attachment to: {ToEmail}", 
                    MaskEmail(request.To));
                
                return new EmailResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to send email with attachment: {ex.Message}"
                };
            }
        }

        public async Task<EmailResponse> SendBulkEmailAsync(BulkEmailRequest request)
        {
            try
            {
                _logger.LogInformation("Sending bulk email to {Count} recipients, Subject: {Subject}", 
                    request.Recipients.Count, request.Subject);

                int successCount = 0;
                int failureCount = 0;

                foreach (var email in request.Recipients)
                {
                    var singleRequest = new EmailRequest
                    {
                        To = email,
                        Subject = request.Subject,
                        Body = request.Body,
                        IsHtml = request.IsHtml,
                        Bcc = request.Bcc
                    };

                    var result = await SendEmailAsync(singleRequest);
                    
                    if (result.Success)
                        successCount++;
                    else
                        failureCount++;
                }
                
                _logger.LogInformation("Bulk email sent: {SuccessCount} successful, {FailureCount} failed", 
                    successCount, failureCount);
                
                return new EmailResponse
                {
                    Success = successCount > 0,
                    ErrorMessage = $"Bulk email sent: {successCount} successful, {failureCount} failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending bulk email to {Count} recipients", 
                    request.Recipients.Count);
                
                return new EmailResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to send bulk email: {ex.Message}"
                };
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };
            
            return client;
        }

        private async Task<string> LoadTemplateAsync(string templateName)
        {
            string templatePath = Path.Combine(_settings.TemplatesPath, $"{templateName}.html");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templateName}");
            }
            
            return await File.ReadAllTextAsync(templatePath);
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;
            
            var parts = email.Split('@');
            
            if (parts[0].Length <= 2)
                return email;
            
            var maskedUsername = parts[0].Substring(0, 2) + new string('*', parts[0].Length - 2);
            
            return $"{maskedUsername}@{parts[1]}";
        }
    }
}
