using System;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? TenantId { get; }
        string Username { get; }
        string[] Roles { get; }
        string[] Permissions { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        bool HasPermission(string permission);
    }
}
