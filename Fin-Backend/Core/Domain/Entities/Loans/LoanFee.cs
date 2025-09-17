using System;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanFee
    {
        public Guid Id { get; set; }
        public string? LoanId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime DateApplied { get; set; }
    }
}
