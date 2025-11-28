namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaVerifyResponseDto
    {
        public bool Verified { get; set; }
        public string Token { get; set; }
    }
}
