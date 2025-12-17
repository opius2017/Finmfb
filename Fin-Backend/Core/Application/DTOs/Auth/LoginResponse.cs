namespace FinTech.Core.Application.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public UserDto User { get; set; } = null!;
    public TenantDto Tenant { get; set; } = null!;
    public bool RequiresMfa { get; set; }
    public string MfaType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string MfaChallengeId { get; set; } = string.Empty;
    public object MfaChallenge { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
