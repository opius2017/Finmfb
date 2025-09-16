using System;

namespace FinTech.Domain.Common
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}