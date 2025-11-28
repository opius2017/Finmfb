namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaBackupCodeResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; }
    }
}
