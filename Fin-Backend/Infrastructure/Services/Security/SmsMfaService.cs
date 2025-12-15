using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services.Security
{
    public class SmsMfaService : IMfaProvider
    {
        private readonly ILogger<SmsMfaService> _logger;

        public SmsMfaService(ILogger<SmsMfaService> logger)
        {
            _logger = logger;
        }

        public string GenerateSecretKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string GetMfaSetupQrCodeUri(string secretKey, string label)
        {
            return string.Empty;
        }

        public bool ValidateCode(string secretKey, string code)
        {
             // Similar to Email, return true or implement TOTP
             return true;
        }

        public async Task<string> SendMfaCodeAsync(string destination)
        {
            var code = new Random().Next(100000, 999999).ToString();
            _logger.LogInformation("Sending SMS code {Code} to {Destination}", code, destination);
            return Task.FromResult(code).Result;
        }
    }
}
