namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class RiskProfileDto
    {
        public Guid CustomerId { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public decimal RiskScore { get; set; }
        public string[] Factors { get; set; } = System.Array.Empty<string>();
        public DateTime EvaluatedAt { get; set; }
    }
}
