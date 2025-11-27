using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.BankReconciliation.Commands.ImportBankStatement;

public record ImportBankStatementCommand : IRequest<Result<ImportBankStatementResponse>>
{
    public string BankAccountId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileContent { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;
}

public record ImportBankStatementResponse
{
    public string StatementId { get; init; } = string.Empty;
    public int LinesImported { get; init; }
    public DateTime StatementStartDate { get; init; }
    public DateTime StatementEndDate { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal ClosingBalance { get; init; }
}
