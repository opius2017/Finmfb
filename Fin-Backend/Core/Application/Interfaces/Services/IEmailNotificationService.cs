using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IEmailNotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
