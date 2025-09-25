using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;

namespace FinTech.Core.Application.Services.Integrations;

/// <summary>
/// Email service for sending transactional emails, statements, and marketing communications
/// </summary>
public interface IEmailService
{
    Task<EmailResponse> SendEmailAsync(EmailRequest request);
    Task<EmailResponse> SendTemplatedEmailAsync(TemplatedEmailRequest request);
    Task<EmailResponse> SendEmailWithAttachmentAsync(EmailWithAttachmentRequest request);
    Task<EmailResponse> SendBulkEmailAsync(BulkEmailRequest request);
}

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
                MaskEmail(request.ToEmail), request.Subject);

            using var client = CreateSmtpClient();
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsHtml
            };

            message.To.Add(request.ToEmail);

            if (request.CcEmails != null)
            {
                foreach (var cc in request.CcEmails)
                {
                    message.CC.Add(cc);
                }
            }

            if (request.BccEmails != null)
            {
                foreach (var bcc in request.BccEmails)
                {
                    message.Bcc.Add(bcc);
                }
            }

            await client.SendMailAsync(message);
            
            _logger.LogInformation("Email successfully sent to: {ToEmail}", MaskEmail(request.ToEmail));
            
            return new EmailResponse
            {
                Success = true,
                Message = "Email sent successfully",
                EmailId = Guid.NewGuid().ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to: {ToEmail}", 
                MaskEmail(request.ToEmail));
            
            return new EmailResponse
            {
                Success = false,
                Message = $"Failed to send email: {ex.Message}"
            };
        }
    }

    public async Task<EmailResponse> SendTemplatedEmailAsync(TemplatedEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending templated email to: {ToEmail}, Template: {TemplateName}", 
                MaskEmail(request.ToEmail), request.TemplateName);

            string templateContent = await LoadTemplateAsync(request.TemplateName);

            foreach (var param in request.TemplateParameters)
            {
                templateContent = templateContent.Replace($"{{{param.Key}}}", param.Value);
            }

            var emailRequest = new EmailRequest
            {
                ToEmail = request.ToEmail,
                Subject = request.Subject,
                Body = templateContent,
                IsHtml = true,
                CcEmails = request.CcEmails,
                BccEmails = request.BccEmails
            };

            return await SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending templated email to: {ToEmail}, Template: {TemplateName}", 
                MaskEmail(request.ToEmail), request.TemplateName);
            
            return new EmailResponse
            {
                Success = false,
                Message = $"Failed to send templated email: {ex.Message}"
            };
        }
    }

    public async Task<EmailResponse> SendEmailWithAttachmentAsync(EmailWithAttachmentRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email with attachment to: {ToEmail}, Subject: {Subject}", 
                MaskEmail(request.ToEmail), request.Subject);

            using var client = CreateSmtpClient();
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsHtml
            };

            message.To.Add(request.ToEmail);

            if (request.CcEmails != null)
            {
                foreach (var cc in request.CcEmails)
                {
                    message.CC.Add(cc);
                }
            }

            if (request.BccEmails != null)
            {
                foreach (var bcc in request.BccEmails)
                {
                    message.Bcc.Add(bcc);
                }
            }

            foreach (var attachment in request.Attachments)
            {
                var ms = new MemoryStream(attachment.FileContent);
                message.Attachments.Add(new Attachment(ms, attachment.FileName, attachment.ContentType));
            }

            await client.SendMailAsync(message);
            
            _logger.LogInformation("Email with attachment successfully sent to: {ToEmail}", 
                MaskEmail(request.ToEmail));
            
            return new EmailResponse
            {
                Success = true,
                Message = "Email with attachment sent successfully",
                EmailId = Guid.NewGuid().ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email with attachment to: {ToEmail}", 
                MaskEmail(request.ToEmail));
            
            return new EmailResponse
            {
                Success = false,
                Message = $"Failed to send email with attachment: {ex.Message}"
            };
        }
    }

    public async Task<EmailResponse> SendBulkEmailAsync(BulkEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending bulk email to {Count} recipients, Subject: {Subject}", 
                request.ToEmails.Count, request.Subject);

            int successCount = 0;
            int failureCount = 0;

            foreach (var email in request.ToEmails)
            {
                var singleRequest = new EmailRequest
                {
                    ToEmail = email,
                    Subject = request.Subject,
                    Body = request.Body,
                    IsHtml = request.IsHtml,
                    BccEmails = request.BccEmails
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
                Message = $"Bulk email sent: {successCount} successful, {failureCount} failed",
                BatchId = Guid.NewGuid().ToString(),
                SuccessCount = successCount,
                FailedCount = failureCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending bulk email to {Count} recipients", 
                request.ToEmails.Count);
            
            return new EmailResponse
            {
                Success = false,
                Message = $"Failed to send bulk email: {ex.Message}"
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

public class EmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? CcEmails { get; set; }
    public List<string>? BccEmails { get; set; }
}

public class TemplatedEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, string> TemplateParameters { get; set; } = new();
    public List<string>? CcEmails { get; set; }
    public List<string>? BccEmails { get; set; }
}

public class EmailWithAttachmentRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? CcEmails { get; set; }
    public List<string>? BccEmails { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new();
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

public class BulkEmailRequest
{
    public List<string> ToEmails { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? BccEmails { get; set; }
}

public class EmailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? EmailId { get; set; }
    public string? BatchId { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}
