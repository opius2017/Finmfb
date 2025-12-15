namespace FinTech.Core.Application.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public UserDto User { get; set; } = null!;
    public TenantDto Tenant { get; set; } = null!;
    public bool RequiresTwoFactor { get; set; }
}
