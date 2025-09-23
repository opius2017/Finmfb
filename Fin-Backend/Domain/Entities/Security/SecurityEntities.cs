using System;
using FinTech.Domain.Common;
using FinTech.Infrastructure.Security.Authorization;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Security
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
namespace FinTech.Domain.Entities.Authentication
{
    public class LoginAttempt
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Username { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime AttemptTime { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string LoginMethod { get; set; }
    }
}