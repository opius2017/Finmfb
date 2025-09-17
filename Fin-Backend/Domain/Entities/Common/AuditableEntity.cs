using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Common
{
    /// <summary>
    /// Base class for auditable entities
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
