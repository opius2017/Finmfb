using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Domain.Entities.Identity;
using FinTech.Infrastructure.Data;
using FinTech.Application.Common.Models;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoleController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<RoleDto>>>> GetRoles()
    {
        var tenantId = GetTenantId();
        
        var roles = await _context.Set<Role>()
            .Where(r => !r.IsDeleted && r.IsActive)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                RoleName = r.RoleName,
                DisplayName = r.DisplayName,
                Description = r.Description,
                Level = r.Level.ToString(),
                DefaultModule = r.DefaultModule,
                DefaultDashboard = r.DefaultDashboard,
                PermissionCount = r.RolePermissions.Count(rp => rp.IsGranted),
                IsSystemRole = r.IsSystemRole,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(BaseResponse<List<RoleDto>>.SuccessResponse(roles));
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<BaseResponse<List<PermissionDto>>>> GetRolePermissions(Guid id)
    {
        var tenantId = GetTenantId();
        
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == id && rp.TenantId == tenantId)
            .Include(rp => rp.Permission)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                PermissionName = rp.Permission.PermissionName,
                DisplayName = rp.Permission.DisplayName,
                Module = rp.Permission.Module,
                Resource = rp.Permission.Resource,
                Action = rp.Permission.Action.ToString(),
                IsGranted = rp.IsGranted,
                Conditions = rp.Conditions
            })
            .ToListAsync();

        return Ok(BaseResponse<List<PermissionDto>>.SuccessResponse(permissions));
    }

    [HttpGet("user-permissions")]
    public async Task<ActionResult<BaseResponse<UserPermissionsDto>>> GetUserPermissions()
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();
        
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId && ur.IsActive)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .ToListAsync();

        var permissions = userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.IsGranted && rp.Permission.IsActive)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                PermissionName = rp.Permission.PermissionName,
                DisplayName = rp.Permission.DisplayName,
                Module = rp.Permission.Module,
                Resource = rp.Permission.Resource,
                Action = rp.Permission.Action.ToString(),
                IsGranted = true
            })
            .GroupBy(p => p.PermissionName)
            .Select(g => g.First())
            .ToList();

        var defaultModule = userRoles.FirstOrDefault()?.Role?.DefaultModule ?? "dashboard";
        var defaultDashboard = userRoles.FirstOrDefault()?.Role?.DefaultDashboard ?? "overview";

        var result = new UserPermissionsDto
        {
            Permissions = permissions,
            DefaultModule = defaultModule,
            DefaultDashboard = defaultDashboard,
            Roles = userRoles.Select(ur => ur.Role.RoleName).ToList()
        };

        return Ok(BaseResponse<UserPermissionsDto>.SuccessResponse(result));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Level { get; set; } = string.Empty;
    public string? DefaultModule { get; set; }
    public string? DefaultDashboard { get; set; }
    public int PermissionCount { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public string? Conditions { get; set; }
}

public class UserPermissionsDto
{
    public List<PermissionDto> Permissions { get; set; } = [];
    public string DefaultModule { get; set; } = string.Empty;
    public string DefaultDashboard { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}