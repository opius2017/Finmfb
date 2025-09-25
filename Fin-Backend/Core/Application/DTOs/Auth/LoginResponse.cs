namespace FinTech.Core.Application.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public UserDto User { get; set; } = null!;
    public TenantDto Tenant { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
}
