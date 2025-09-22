using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class SecurityPreferences : BaseEntity
    {
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorMethod { get; set; }
        public bool LoginAlerts { get; set; }
        public string UserId { get; set; }
    }
}
