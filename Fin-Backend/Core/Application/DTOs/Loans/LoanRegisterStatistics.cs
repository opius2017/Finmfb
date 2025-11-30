namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanRegisterStatistics
    {
        public int Year { get; set; }
        public int TotalLoansRegistered { get; set; }
        public decimal TotalPrincipalAmount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public int ActiveLoans { get; set; }
        public int ClosedLoans { get; set; }
    }
}
