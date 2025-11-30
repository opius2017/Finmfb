namespace FinTech.Core.Application.Interfaces.Services;

public interface IMfaNotificationService
{
    Task SendMfaCodeBySmsAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
    Task SendMfaCodeByEmailAsync(string email, string code, CancellationToken cancellationToken = default);
    Task SendMfaEnabledNotificationAsync(string email, string method, CancellationToken cancellationToken = default);
    Task SendMfaDisabledNotificationAsync(string email, CancellationToken cancellationToken = default);
    Task SendBackupCodesAsync(string email, IEnumerable<string> codes, CancellationToken cancellationToken = default);
}
