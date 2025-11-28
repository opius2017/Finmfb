using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.Reports.Queries.GetTrialBalance;

public record GetTrialBalanceQuery : IRequest<Result<TrialBalanceDto>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IncludeZeroBalances { get; init; }
    public string? AccountType { get; init; }
}

public record TrialBalanceDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime GeneratedDate { get; init; }
    public string PeriodDescription { get; init; } = string.Empty;
    
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal Difference { get; init; }
    public bool IsBalanced { get; init; }
    
    public List<TrialBalanceLineDto> Lines { get; init; } = new();
    public TrialBalanceSummaryDto Summary { get; init; } = new();
}

public record TrialBalanceLineDto
{
    public string AccountId { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public string AccountClassification { get; init; } = string.Empty;
    
    public decimal OpeningDebit { get; init; }
    public decimal OpeningCredit { get; init; }
    public decimal OpeningBalance { get; init; }
    
    public decimal PeriodDebit { get; init; }
    public decimal PeriodCredit { get; init; }
    public decimal PeriodMovement { get; init; }
    
    public decimal ClosingDebit { get; init; }
    public decimal ClosingCredit { get; init; }
    public decimal ClosingBalance { get; init; }
}

public record TrialBalanceSummaryDto
{
    public decimal TotalAssets { get; init; }
    public decimal TotalLiabilities { get; init; }
    public decimal TotalEquity { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetIncome { get; init; }
    
    public bool AccountingEquationBalanced { get; init; }
    public string? ValidationMessage { get; init; }
}
