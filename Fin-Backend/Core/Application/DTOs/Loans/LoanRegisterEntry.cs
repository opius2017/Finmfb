using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanRegisterEntry
    {
        public string SerialNumber { get; set; }
        public DateTime RegisterDate { get; set; }
        public string LoanId { get; set; }
        public string ApplicationNumber { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenorMonths { get; set; }
        public decimal MonthlyEMI { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public string Status { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string RecordedBy { get; set; }
    }
}
