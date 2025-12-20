using System;
using FinTech.Core.Domain.Common;
// using FinTech.Infrastructure.Security.Authorization;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Security
{
    public class ResourcePermission : BaseEntity, IAuditable
    {
        public string Resource { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    
    public class UserPermission : BaseEntity, IAuditable
    {
        public string UserId { get; set; } = string.Empty;
        public string ResourcePermissionId { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class SecurityPolicy : BaseEntity, IAuditable
    {
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime? LastModified { get; set; }
        public new string? LastModifiedBy { get; set; }
    }
    
    public class DataAccessLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime AccessTime { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsAuthorized { get; set; }
    }
}
