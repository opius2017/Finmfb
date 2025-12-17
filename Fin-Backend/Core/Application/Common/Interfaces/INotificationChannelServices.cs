using System.Threading.Tasks;

namespace FinTech.Core.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task<bool> SendEmailWithTemplateAsync(string to, string templateName, object templateData);
    }

    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string to, string message);
    }

    public interface IPushNotificationService
    {
        Task<bool> SendPushNotificationAsync(string token, string title, string body, string? data = null);
        Task<bool> SendPushNotificationToTopicAsync(string topic, string title, string body, string? data = null);
    }
}
