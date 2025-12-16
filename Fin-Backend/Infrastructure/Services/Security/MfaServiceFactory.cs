using System;
using Microsoft.Extensions.DependencyInjection;
using FinTech.Infrastructure.Services;
using FinTech.Core.Application.Interfaces.Services;

namespace FinTech.Infrastructure.Services.Security
{
    public class MfaServiceFactory : IMfaProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MfaServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public FinTech.Core.Application.Interfaces.Services.IMfaProvider GetMfaProvider(string method)
        {
            return method.ToLower() switch
            {
                "email" => (FinTech.Core.Application.Interfaces.Services.IMfaProvider)_serviceProvider.GetRequiredService<EmailMfaService>(),
                "sms" => (FinTech.Core.Application.Interfaces.Services.IMfaProvider)_serviceProvider.GetRequiredService<SmsMfaService>(),
                "app" => (FinTech.Core.Application.Interfaces.Services.IMfaProvider)_serviceProvider.GetRequiredService<AppMfaService>(),
                _ => throw new ArgumentException($"Unsupported MFA method: {method}")
            };
        }
    }
}
