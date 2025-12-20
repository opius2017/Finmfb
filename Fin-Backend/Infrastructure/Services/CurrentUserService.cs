using FinTech.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace FinTech.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return !string.IsNullOrEmpty(userId) ? Guid.Parse(userId) : null;
            }
        }

        public string? TenantId
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            }
        }

        public string Username
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            }
        }

        public string[] Roles
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToArray() ?? Array.Empty<string>();
            }
        }

        public string[] Permissions
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.FindAll("permission")
                    .Select(c => c.Value)
                    .ToArray() ?? Array.Empty<string>();
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }
}
