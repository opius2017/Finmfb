using System;

namespace FinTech.Core.Domain.Common
{
    public interface ITenantEntity
    {
        string TenantId { get; set; }
    }
}
