using System.Threading.Tasks;

namespace FinTech.Infrastructure.Services.Security
{
    public interface IMfaProvider
    {
        string GenerateSecretKey();
        string GetMfaSetupQrCodeUri(string secretKey, string label);
        bool ValidateCode(string secretKey, string code);
        Task<string> SendMfaCodeAsync(string destination);
    }
}
