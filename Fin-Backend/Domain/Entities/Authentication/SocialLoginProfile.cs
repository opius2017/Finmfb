namespace FinTech.Core.Domain.Entities.Authentication
{
    public class SocialLoginProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProfileDataJson { get; set; }
    }
}
