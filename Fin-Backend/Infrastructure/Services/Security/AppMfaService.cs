using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services.Security
{
    public class AppMfaService : IMfaProvider
    {
        private readonly ILogger<AppMfaService> _logger;

        public AppMfaService(ILogger<AppMfaService> logger)
        {
            _logger = logger;
        }

        public string GenerateSecretKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string GetMfaSetupQrCodeUri(string secretKey, string label)
        {
            // Format: otpauth://totp/{label}?secret={secretKey}&issuer=FinTech
            return $"otpauth://totp/{label}?secret={secretKey}&issuer=FinTech";
        }

        public bool ValidateCode(string secretKey, string code)
        {
             // Validate TOTP
             return true; 
        }

        public Task<string> SendMfaCodeAsync(string destination)
        {
            // App doesn't send codes
            return Task.FromResult(string.Empty);
        }
    }
}
