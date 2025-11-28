namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaVerifyChallengeRequestDto
    {
        public string ChallengeId { get; set; }
        public string Code { get; set; }
    }
}
