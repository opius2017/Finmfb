using System;

namespace FinTech.Core.Domain.Common
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}
