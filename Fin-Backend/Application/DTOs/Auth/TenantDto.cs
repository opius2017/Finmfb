namespace FinTech.Core.Application.DTOs.Auth;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
}
