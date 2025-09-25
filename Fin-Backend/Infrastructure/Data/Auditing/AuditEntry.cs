using System;
using System.Collections.Generic;

namespace FinTech.Infrastructure.Data.Auditing
{
    /// <summary>
    /// Represents an audit entry tracking changes to an entity for auditing purposes.
    /// This is used to track changes before committing them to the database.
    /// </summary>
    public class AuditEntry
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
        /// Dictionary of property changes with property names as keys
        /// </summary>
        public Dictionary<string, object> Changes { get; set; }
    }
}
