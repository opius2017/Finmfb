using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Interfaces.Services;

namespace FinTech.Infrastructure.Services
{
    public class SmsNotificationService : ISmsNotificationService
    {
        private readonly ILogger<SmsNotificationService> _logger;

        public SmsNotificationService(ILogger<SmsNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string phoneNumber, string message)
        {
            _logger.LogInformation($"Sending SMS to {phoneNumber}: {message}");
            return Task.CompletedTask;
        }
    }
}
