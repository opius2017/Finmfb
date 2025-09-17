using System;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Common
{
    /// <summary>
    /// Base class for auditable entities
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    }
}
