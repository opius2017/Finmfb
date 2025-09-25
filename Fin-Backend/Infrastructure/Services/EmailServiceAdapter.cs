using System.Threading.Tasks;
using FinTech.Core.Application.Services.Integrations;
using FinTech.WebAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Infrastructure.Services
{
    /// <summary>
    /// Adapter implementation of IEmailService that delegates to the core email service
    /// </summary>
    public class EmailServiceAdapter : IEmailService
    {
        private readonly FinTech.Core.Application.Services.Integrations.IEmailService _coreEmailService;
        private readonly ILogger<EmailServiceAdapter> _logger;

        public EmailServiceAdapter(
            FinTech.Core.Application.Services.Integrations.IEmailService coreEmailService,
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
    }
}
