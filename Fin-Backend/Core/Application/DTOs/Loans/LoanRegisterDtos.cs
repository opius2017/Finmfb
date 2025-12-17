using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class RegisterLoanCommand
    {
        public string LoanId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class MonthlyRegisterReport
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalRegistrations { get; set; }
        public decimal TotalAmount { get; set; }
        public List<LoanRegisterEntryDto> Entries { get; set; } = new List<LoanRegisterEntryDto>();
    }

    public class RegisterStatistics
    {
        public int TotalLoansRegistered { get; set; }
        public decimal TotalPrincipalRegistered { get; set; }
        public int ActiveRegistrations { get; set; }
        public DateTime LastRegistrationDate { get; set; }
    }
}
