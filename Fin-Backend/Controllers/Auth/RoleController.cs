using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDtos = roles.Select(r => new {
                id = r.Id.ToString(),
                roleName = r.Name,
                displayName = r.Name, // Using Name as DisplayName for now
                level = "System", // Default/Placeholder
                permissionCount = 0, // Would need Permission queries
                isSystemRole = false,
                createdAt = DateTime.UtcNow
            });

            return Ok(new
            {
                success = true,
                data = roleDtos
            });
        }

        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            // Placeholder: Returning empty permissions but success to avoid 404/errors
            // Real implementation would query RolePermissions table
            await Task.CompletedTask; 

            var result = new
            {
                success = true,
                data = new List<object>()
            };
            return Ok(result);
        }

        [HttpGet("user-permissions")]
        public async Task<IActionResult> GetUserPermissions()
        {
            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<object>();

            // Define all available permissions matching Frontend Sidebar.tsx
            // Format: { permissionName, displayName, module }
            var allPermissions = new List<(string Name, string Module)>
            {
                // Dashboard
                ("dashboard.read", "dashboard"),
                ("executive.dashboard.read", "dashboard"),
                ("loans.dashboard.read", "dashboard"),
                ("deposits.dashboard.read", "dashboard"),
                ("inventory.dashboard.read", "dashboard"),
                ("payroll.dashboard.read", "dashboard"),

                // Core Modules
                ("customers.read", "customers"),
                ("deposits.read", "deposits"),
                ("loans.read", "loans"),
                ("maker_checker.read", "security"),
                
                // Accounting & Finance
                ("accounts_payable.read", "accounting"),
                ("accounts_receivable.read", "accounting"),
                ("general_ledger.read", "accounting"),
                ("multi_currency.read", "accounting"),
                ("financial_reports.read", "accounting"),
                ("invoicing.read", "accounting"),
                ("tax.read", "accounting"),

                // Operations
                ("payroll.read", "hr"),
                ("inventory.read", "inventory"),
                ("purchases.read", "inventory"),
                
                // Governance
                ("reports.read", "reports"),
                ("compliance.read", "compliance"),
                ("security.read", "security"),
                ("settings.read", "settings")
            };

            // Assign permissions based on Role
            if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
            {
                // Admin gets EVERYTHING
                foreach (var p in allPermissions)
                {
                    permissions.Add(new { 
                        permissionName = p.Name, 
                        resource = p.Module, 
                        action = "read", 
                        isGranted = true 
                    });
                }
            }
            else if (roles.Contains("Teller"))
            {
                // Teller gets basic ops
                var tellerPermissions = new[] { 
                    "dashboard.read", "deposits.dashboard.read", 
                    "customers.read", "deposits.read" 
                };
                foreach (var p in allPermissions.Where(p => tellerPermissions.Contains(p.Name)))
                {
                   permissions.Add(new { permissionName = p.Name, resource = p.Module, action = "read", isGranted = true });
                }
            }
            else if (roles.Contains("LoanOfficer"))
            {
                 // Loan Officer gets loans and customers
                 var loanPermissions = new[] { 
                     "dashboard.read", "loans.dashboard.read", 
                     "customers.read", "loans.read" 
                 };
                 foreach (var p in allPermissions.Where(p => loanPermissions.Contains(p.Name)))
                 {
                    permissions.Add(new { permissionName = p.Name, resource = p.Module, action = "read", isGranted = true });
                 }
            }

            var result = new {
                success = true,
                data = new {
                    permissions,
                    defaultModule = "dashboard",
                    defaultDashboard = "executive",
                    roles
                }
            };
            return Ok(result);
        }
    }
}
