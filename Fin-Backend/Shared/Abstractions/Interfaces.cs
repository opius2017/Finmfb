using System;
using System.Collections.Generic;

namespace FinTech.Shared.Abstractions
{
    /// <summary>
    /// Represents audit information for all entities
    /// </summary>
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
        string ModifiedBy { get; set; }
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string DeletedBy { get; set; }
    }

    /// <summary>
    /// Authorization context for current user
    /// </summary>
    public interface IAuthorizationContext
    {
        string UserId { get; }
        string UserName { get; }
        List<string> Roles { get; }
        List<string> Permissions { get; }
        string TenantId { get; }

        bool HasRole(string role);
        bool HasPermission(string permission);
        bool IsAdmin { get; }
    }

    /// <summary>
    /// Current user provider
    /// </summary>
    public interface ICurrentUserProvider
    {
        IAuthorizationContext GetCurrentUser();
        string GetCurrentUserId();
        string GetCurrentUserName();
    }
}
