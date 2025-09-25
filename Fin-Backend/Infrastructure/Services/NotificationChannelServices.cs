using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using FinTech.Core.Application.Common.Interfaces;

namespace FinTech.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = enableSsl;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(senderEmail, senderName);
                        mailMessage.To.Add(to);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = isHtml;

                        await client.SendMailAsync(mailMessage);
                    }
                }

                _logger.LogInformation("Email sent successfully to {Recipient}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailWithTemplateAsync(string to, string templateName, object templateData)
        {
            try
            {
                // In a real implementation, this would load the template from a file or database
                // and populate it with the provided data using a template engine like Razor or Handlebars

                // For now, just use a basic template approach
                var templateContent = await GetTemplateContentAsync(templateName);
                var subject = GetTemplateSubject(templateName);

                // Replace placeholders with actual values
                var body = PopulateTemplate(templateContent, templateData);

                return await SendEmailAsync(to, subject, body, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {Recipient} using template {TemplateName}", to, templateName);
                return false;
            }
        }

        private async Task<string> GetTemplateContentAsync(string templateName)
        {
            // In a real implementation, this would load the template from a file or database
            // For now, return a basic HTML template
            return "<html><body><h1>{{Title}}</h1><p>{{Content}}</p></body></html>";
        }

        private string GetTemplateSubject(string templateName)
        {
            // In a real implementation, this would get the subject for the specified template
            return "Notification from FinTech Bank";
        }

        private string PopulateTemplate(string template, object data)
        {
            // In a real implementation, this would use a template engine
            // For now, just do simple string replacement
            var properties = data.GetType().GetProperties();
            var result = template;

            foreach (var prop in properties)
            {
                var placeholder = "{{" + prop.Name + "}}";
                var value = prop.GetValue(data)?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }

            return result;
        }
    }

    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(string to, string message)
        {
            try
            {
                // In a real implementation, this would use an SMS provider API like Twilio, Nexmo, etc.
                // For now, just log the message
                _logger.LogInformation("SMS would be sent to {Recipient} with message: {Message}", to, message);

                // Simulate API call delay
                await Task.Delay(100);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {Recipient}", to);
                return false;
            }
        }
    }

    public class PushNotificationService : IPushNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPushNotificationAsync(string token, string title, string body, string data = null)
        {
            try
            {
                // In a real implementation, this would use a push notification service like Firebase Cloud Messaging
                // For now, just log the notification
                _logger.LogInformation("Push notification would be sent to token {Token} with title: {Title}, body: {Body}, data: {Data}", 
                    token, title, body, data);

                // Simulate API call delay
                await Task.Delay(100);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to token {Token}", token);
                return false;
            }
        }

        public async Task<bool> SendPushNotificationToTopicAsync(string topic, string title, string body, string data = null)
        {
            try
            {
                // In a real implementation, this would use a push notification service to send to a topic
                // For now, just log the notification
                _logger.LogInformation("Push notification would be sent to topic {Topic} with title: {Title}, body: {Body}, data: {Data}", 
                    topic, title, body, data);

                // Simulate API call delay
                await Task.Delay(100);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to topic {Topic}", topic);
                return false;
            }
        }
    }
}
