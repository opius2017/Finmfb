using System;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;

namespace FinTech.Infrastructure.Data.Auditing
{
    /// <summary>
    /// Represents an audit log record stored in the database
    /// to track changes made to entities.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// The name of the entity being audited
        /// </summary>
        public string EntityName { get; set; }
        
        /// <summary>
        /// The ID of the entity being audited
        /// </summary>
        public string EntityId { get; set; }
        
        /// <summary>
        /// The action being performed (Added, Modified, Deleted)
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// When the action occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// The ID of the user who performed the action
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// The tenant ID in a multi-tenant context
        /// </summary>
        public Guid? TenantId { get; set; }
        
        /// <summary>
        /// JSON-serialized changes made to the entity
        /// </summary>
        public string Changes { get; set; }
    }
}