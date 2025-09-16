using System;

namespace FinTech.Domain.Entities.Common
{
    /// <summary>
    /// Base entity for all domain entities with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
    }
}