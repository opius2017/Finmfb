using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class OverdueLoan
    {
        public string LoanId { get; set; }
        public string LoanNumber { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberNumber { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string Classification { get; set; }
    }
}
