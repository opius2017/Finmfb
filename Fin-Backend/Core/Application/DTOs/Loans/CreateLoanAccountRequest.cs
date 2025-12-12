using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CreateLoanAccountRequest
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Amount { get; set; }
        public int Term { get; set; }
        public string CurrencyCode { get; set; }
        public string Purpose { get; set; }
    }
}
