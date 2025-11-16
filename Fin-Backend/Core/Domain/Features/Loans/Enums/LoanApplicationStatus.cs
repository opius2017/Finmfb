namespace FinTech.Core.Domain.Features.Loans.Enums
{
    public enum LoanApplicationStatus
    {
        Draft,
        Submitted,
        InReview,
        PendingDocuments,
        Approved,
        Rejected,
        Disbursed,
        Cancelled
    }
}