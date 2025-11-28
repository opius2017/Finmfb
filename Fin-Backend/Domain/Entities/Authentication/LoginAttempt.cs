namespace FinTech.Core.Domain.Entities.Authentication
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime AttemptedAt { get; set; }
        public bool WasSuccessful { get; set; }
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
    }
}
