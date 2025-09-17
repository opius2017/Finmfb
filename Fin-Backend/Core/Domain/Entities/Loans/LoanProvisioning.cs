using System;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanProvisioning
    {
        public Guid Id { get; set; }
    public string? LoanId { get; set; }
        public decimal ExpectedCreditLoss { get; set; }
        public DateTime CalculationDate { get; set; }
        public int Stage { get; set; } // IFRS 9 stage (1, 2, 3)
    public string? Notes { get; set; }
    }
}
