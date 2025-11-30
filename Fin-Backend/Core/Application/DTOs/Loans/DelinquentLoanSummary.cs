namespace FinTech.Core.Application.DTOs.Loans
{
    public class DelinquentLoanSummary
    {
        public string LoanId { get; set; }
        public string LoanNumber { get; set; }
        public string MemberId { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Classification { get; set; }
    }
}
