using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanRepaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid TransactionId { get; set; }
    }
}
