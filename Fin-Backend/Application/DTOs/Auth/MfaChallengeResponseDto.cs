using System;

namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaChallengeResponseDto
    {
        public bool Success { get; set; }
        public string ChallengeId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
