namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaChallengeRequestDto
    {
        public string Operation { get; set; }
        public string UserId { get; set; }
    }
}
