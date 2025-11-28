using MediatR;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Application.Features.BankReconciliation.Queries.GetReconciliation;

namespace FinTech.Core.Application.Features.BankReconciliation.Queries.GetReconciliationsByBankAccount;

public record GetReconciliationsByBankAccountQuery(string BankAccountId) : IRequest<Result<List<ReconciliationSummaryDto>>>;

public record ReconciliationSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string BankAccountId { get; init; } = string.Empty;
    public string BankAccountName { get; init; } = string.Empty;
    public DateTime ReconciliationDate { get; init; }
    public DateTime StatementEndDate { get; init; }
    public decimal StatementClosingBalance { get; init; }
    public decimal BookClosingBalance { get; init; }
    public decimal Variance { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ReconciliationReference { get; init; }
    public string ReconciledBy { get; init; } = string.Empty;
    public DateTime? ApprovedDate { get; init; }
}
