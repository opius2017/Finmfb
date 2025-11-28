using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaSetupResponseDto
    {
        public string Secret { get; set; }
        public string QrCodeUrl { get; set; }
        public List<string> BackupCodes { get; set; }
    }
}
