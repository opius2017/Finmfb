namespace FinTech.Core.Application.Features.Loans.Commands.CreateLoan
{
    public record CreateLoanResponse
    {
        public string LoanId { get; init; } = string.Empty;
        public string LoanNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}