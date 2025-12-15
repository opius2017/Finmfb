using System;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure.Services.Security
{
    public class MfaServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MfaServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMfaProvider GetMfaService(string method)
        {
            return method.ToLower() switch
            {
                "email" => _serviceProvider.GetRequiredService<EmailMfaService>(),
                "sms" => _serviceProvider.GetRequiredService<SmsMfaService>(),
                "app" => _serviceProvider.GetRequiredService<AppMfaService>(), // Need to create AppMfaService too
                _ => throw new ArgumentException($"Unsupported MFA method: {method}")
            };
        }
    }
}
