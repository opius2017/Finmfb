namespace FinTech.Core.Application.DTOs.Loans
{
    public class ThresholdAllocationResult
    {
        public bool Success { get; set; }
        public decimal AllocatedAmount { get; set; }
        public int? AllocatedMonth { get; set; }
        public int? AllocatedYear { get; set; }
        public int? QueuedForMonth { get; set; }
        public int? QueuedForYear { get; set; }
        public string Message { get; set; }
    }
}
