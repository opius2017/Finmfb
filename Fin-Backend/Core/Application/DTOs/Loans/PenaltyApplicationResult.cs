namespace FinTech.Core.Application.DTOs.Loans
{
    public class PenaltyApplicationResult
    {
        public bool Success { get; set; }
        public string LoanId { get; set; }
        public decimal PreviousPenalty { get; set; }
        public decimal NewPenalty { get; set; }
        public decimal PenaltyIncrease { get; set; }
        public int DaysOverdue { get; set; }
        public string Message { get; set; }
    }
}
