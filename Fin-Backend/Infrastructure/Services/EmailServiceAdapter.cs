using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces;
using FinTech.Core.Application.DTOs.Email;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Infrastructure.Services
{
    /// <summary>
    /// Adapter implementation of IEmailService that delegates to the core email service
    /// </summary>
    public class EmailServiceAdapter : IEmailService
    {
        private readonly IEmailService _coreEmailService;
        private readonly ILogger<EmailServiceAdapter> _logger;

        public EmailServiceAdapter(
            IEmailService coreEmailService,
            ILogger<EmailServiceAdapter> logger)
        {
            _coreEmailService = coreEmailService;
            _logger = logger;
        }

        public async Task<EmailResponse> SendTemplatedEmailAsync(TemplatedEmailRequest request)
        {
            _logger.LogInformation("Delegating templated email to core email service");
            return await _coreEmailService.SendTemplatedEmailAsync(request);
        }

        public async Task<EmailResponse> SendEmailAsync(EmailRequest request)
        {
             _logger.LogInformation("Delegating standard email to core email service");
             return await _coreEmailService.SendEmailAsync(request);
        }
    }
}
