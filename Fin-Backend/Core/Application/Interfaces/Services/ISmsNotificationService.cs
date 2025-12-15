using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface ISmsNotificationService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }
}
