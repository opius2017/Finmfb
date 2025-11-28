using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Common
{
    /// <summary>
    /// Base class for auditable entities
    /// </summary>
    public abstract class AuditableEntity : AggregateRoot
    {
        // Inherits all properties from canonical BaseEntity in src/Core/Domain/Entities/Common/BaseEntity.cs
        // Add any additional audit properties here if needed
    }
}
