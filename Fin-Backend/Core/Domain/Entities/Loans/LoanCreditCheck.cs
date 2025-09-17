using System;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanCreditCheck
    {
        public Guid Id { get; set; }
        public string? LoanId { get; set; }
        public string? Bureau { get; set; }
        public string? Result { get; set; }
        public DateTime DateChecked { get; set; }
    }
}
