using System;
using FinTech.Core.Domain.Common;
// using FinTech.Infrastructure.Security.Authorization;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Security
{
    public class ResourcePermission : BaseEntity, IAuditable
    {
        public string Resource { get; set; }
        public string Operation { get; set; }
        public string Description { get; set; }
    }
    
    public class UserPermission : BaseEntity, IAuditable
    {
        public Guid UserId { get; set; }
        public Guid ResourcePermissionId { get; set; }
        public bool IsGranted { get; set; }
        public string Reason { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class SecurityPolicy : BaseEntity, IAuditable
    {
        public string PolicyName { get; set; }
        public string PolicyValue { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastModified { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
    
    public class DataAccessLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime AccessTime { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string AccessType { get; set; }
        public string IPAddress { get; set; }
        public string Reason { get; set; }
        public bool IsAuthorized { get; set; }
    }
}
