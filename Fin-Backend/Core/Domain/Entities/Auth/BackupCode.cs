using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class BackupCode : BaseEntity
{
    public Guid MfaSettingsId { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual MfaSettings? MfaSettings { get; set; }
}
