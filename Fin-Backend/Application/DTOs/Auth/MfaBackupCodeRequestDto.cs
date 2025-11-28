namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaBackupCodeRequestDto
    {
        public string Code { get; set; }
        public string MfaToken { get; set; }
    }
}
