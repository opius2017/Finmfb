using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Services;

public interface IMfaProvider
{
    Task<string> GenerateTotpSecretAsync();
    Task<string> GenerateQrCodeUriAsync(string email, string secret);
    Task<bool> ValidateTotpCodeAsync(string secret, string code);
    Task<string> GenerateSmsCodeAsync();
    Task<string> GenerateEmailCodeAsync();
    Task<bool> ValidateCodeAsync(string code, string expectedCode);
    Task<IEnumerable<string>> GenerateBackupCodesAsync(int count = 10);
}
