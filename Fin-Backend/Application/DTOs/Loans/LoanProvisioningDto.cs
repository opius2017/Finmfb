using System;

namespace FinTech.Application.DTOs.Loans
{
    public class LoanProvisioningDto
    {
        public string LoanId { get; set; }
        public decimal ExpectedCreditLoss { get; set; }
        public DateTime CalculationDate { get; set; }
        public string Stage { get; set; } // IFRS 9 stage (1, 2, 3)
        public string Notes { get; set; }
    }
}
