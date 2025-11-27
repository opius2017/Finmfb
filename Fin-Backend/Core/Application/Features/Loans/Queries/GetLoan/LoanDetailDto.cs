using System;

namespace FinTech.Core.Application.Features.Loans.Queries.GetLoan
{
    public record LoanDetailDto
    {
        public string Id { get; init; } = string.Empty;
        public string LoanNumber { get; init; } = string.Empty;
        public string CustomerId { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string LoanProductId { get; init; } = string.Empty;
        public string LoanProductName { get; init; } = string.Empty;
        public decimal LoanAmount { get; init; }
        public decimal OutstandingBalance { get; init; }
        public decimal InterestRate { get; init; }
        public int TenorInMonths { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime ApplicationDate { get; init; }
        public DateTime? ApprovalDate { get; init; }
        public DateTime? DisbursementDate { get; init; }
    }
}