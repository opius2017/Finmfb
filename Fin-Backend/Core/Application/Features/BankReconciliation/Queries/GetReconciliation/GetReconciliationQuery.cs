using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.BankReconciliation.Queries.GetReconciliation;

public record GetReconciliationQuery(string Id) : IRequest<Result<ReconciliationDetailDto>>;

public record ReconciliationDetailDto
{
    public string Id { get; init; } = string.Empty;
    public string BankAccountId { get; init; } = string.Empty;
    public string BankAccountName { get; init; } = string.Empty;
    public string BankAccountNumber { get; init; } = string.Empty;
    public DateTime ReconciliationDate { get; init; }
    public DateTime StatementStartDate { get; init; }
    public DateTime StatementEndDate { get; init; }
    
    public decimal StatementOpeningBalance { get; init; }
    public decimal StatementClosingBalance { get; init; }
    public decimal BookOpeningBalance { get; init; }
    public decimal BookClosingBalance { get; init; }
    
    public decimal TotalDepositsInTransit { get; init; }
    public decimal TotalOutstandingChecks { get; init; }
    public decimal TotalBankCharges { get; init; }
    public decimal TotalBankInterest { get; init; }
    public decimal TotalAdjustments { get; init; }
    
    public decimal ReconciledBalance { get; init; }
    public decimal Variance { get; init; }
    
    public string Status { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string? ReconciliationReference { get; init; }
    
    public string ReconciledBy { get; init; } = string.Empty;
    public DateTime? ReconciledDate { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedDate { get; init; }
    
    public List<ReconciliationItemDto> Items { get; init; } = new();
}

public record ReconciliationItemDto
{
    public string Id { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public decimal Amount { get; init; }
    public string ItemType { get; init; } = string.Empty;
    public string ItemStatus { get; init; } = string.Empty;
    public bool IsMatched { get; init; }
}
