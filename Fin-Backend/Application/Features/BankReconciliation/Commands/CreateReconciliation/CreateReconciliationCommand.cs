using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.BankReconciliation.Commands.CreateReconciliation;

public record CreateReconciliationCommand : IRequest<Result<CreateReconciliationResponse>>
{
    public string BankAccountId { get; init; } = string.Empty;
    public DateTime ReconciliationDate { get; init; }
    public DateTime StatementStartDate { get; init; }
    public DateTime StatementEndDate { get; init; }
    public decimal StatementOpeningBalance { get; init; }
    public decimal StatementClosingBalance { get; init; }
    public string? Notes { get; init; }
}

public record CreateReconciliationResponse
{
    public string Id { get; init; } = string.Empty;
    public string BankAccountId { get; init; } = string.Empty;
    public string BankAccountName { get; init; } = string.Empty;
    public DateTime ReconciliationDate { get; init; }
    public decimal StatementClosingBalance { get; init; }
    public decimal BookClosingBalance { get; init; }
    public decimal Variance { get; init; }
    public string Status { get; init; } = string.Empty;
}
