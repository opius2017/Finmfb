using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class SecurityPreferences : BaseEntity
    {
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorMethod { get; set; } = string.Empty;
        public bool LoginAlerts { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime? LastPasswordChangeDate { get; set; }
        public bool SecurityQuestionsConfigured { get; set; }
    }
}
