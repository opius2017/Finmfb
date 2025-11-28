namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaVerifyRequestDto
    {
        public string Code { get; set; }
        public string Secret { get; set; }
    }
}
