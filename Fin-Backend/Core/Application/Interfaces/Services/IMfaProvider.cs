using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Services;

public interface IMfaProvider
{
    string GenerateSecretKey();
    string GetMfaSetupQrCodeUri(string secretKey, string label);
    bool ValidateCode(string secretKey, string code);
    Task<string> SendMfaCodeAsync(string destination);
}
