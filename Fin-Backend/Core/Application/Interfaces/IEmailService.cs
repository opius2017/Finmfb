using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Services.Integrations;
using FinTech.Core.Application.DTOs.Email;

namespace FinTech.Core.Application.Interfaces
{
    /// <summary>
    /// Interface for email service used within the application
    /// This interface is a facade over the core email service
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send a template-based email with the specified parameters
        /// </summary>
        Task<EmailResponse> SendTemplatedEmailAsync(TemplatedEmailRequest request);
    }
}
