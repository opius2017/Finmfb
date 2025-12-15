using MediatR;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Entities.Banking;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Core.Application.Features.BankReconciliation.Commands.ImportBankStatement;

public class ImportBankStatementCommandHandler : IRequestHandler<ImportBankStatementCommand, Result<ImportBankStatementResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ImportBankStatementCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ImportBankStatementResponse>> Handle(ImportBankStatementCommand request, CancellationToken cancellationToken)
    {
        // Get bank account
        var bankAccount = await _unitOfWork.Repository<Domain.Entities.Banking.BankAccount>()
            .GetByIdAsync(request.BankAccountId);
            
        if (bankAccount == null)
        {
            return Result.Failure<ImportBankStatementResponse>(
                Error.NotFound("BankAccount.NotFound", "Bank account not found"));
        }

        // Parse statement based on file type
        var statementLines = ParseStatementFile(request.FileContent, request.FileType);

        if (!statementLines.Any())
        {
            return Result.Failure<ImportBankStatementResponse>(
                Error.Validation("Import.NoData", "No valid data found in the file"));
        }

        // Calculate statement dates and balances
        var startDate = statementLines.Min(l => l.TransactionDate);
        var endDate = statementLines.Max(l => l.TransactionDate);
        var openingBalance = statementLines.First().Balance - statementLines.First().DebitAmount + statementLines.First().CreditAmount;
        var closingBalance = statementLines.Last().Balance;

        var statement = new BankStatement
        {
            BankAccountId = request.BankAccountId,
            BankAccountName = bankAccount.AccountName,
            BankAccountNumber = bankAccount.AccountNumber,
            StatementDate = DateTime.UtcNow,
            StatementStartDate = startDate,
            StatementEndDate = endDate,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            TotalDebits = statementLines.Sum(l => l.DebitAmount),
            TotalCredits = statementLines.Sum(l => l.CreditAmount),
            FileName = request.FileName,
            Status = BankStatementStatus.Imported,
            ImportedBy = "System", // Get from current user context
            ImportedDate = DateTime.UtcNow,
            StatementLines = statementLines,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _unitOfWork.Repository<BankStatement>().AddAsync(statement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ImportBankStatementResponse
        {
            StatementId = statement.Id,
            LinesImported = statementLines.Count,
            StatementStartDate = startDate,
            StatementEndDate = endDate,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance
        };

        return Result<ImportBankStatementResponse>.Success(response);
    }

    private List<BankStatementLine> ParseStatementFile(string fileContent, string fileType)
    {
        var lines = new List<BankStatementLine>();

        if (fileType == ".csv")
        {
            lines = ParseCsvFile(fileContent);
        }
        else if (fileType == ".txt")
        {
            lines = ParseTextFile(fileContent);
        }

        return lines;
    }

    private List<BankStatementLine> ParseCsvFile(string content)
    {
        var lines = new List<BankStatementLine>();
        var rows = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Skip header row
        for (int i = 1; i < rows.Length; i++)
        {
            var columns = rows[i].Split(',');
            
            if (columns.Length < 5) continue;

            try
            {
                var line = new BankStatementLine
                {
                    TransactionDate = DateTime.Parse(columns[0].Trim()),
                    Description = columns[1].Trim(),
                    Reference = columns.Length > 2 ? columns[2].Trim() : null,
                    DebitAmount = decimal.TryParse(columns[3].Trim(), out var debit) ? debit : 0,
                    CreditAmount = decimal.TryParse(columns[4].Trim(), out var credit) ? credit : 0,
                    Balance = columns.Length > 5 && decimal.TryParse(columns[5].Trim(), out var balance) ? balance : 0,
                    IsReconciled = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                lines.Add(line);
            }
            catch
            {
                // Skip invalid lines
                continue;
            }
        }

        return lines;
    }

    private List<BankStatementLine> ParseTextFile(string content)
    {
        // Implement text file parsing logic
        // This is a placeholder - actual implementation depends on bank statement format
        return new List<BankStatementLine>();
    }
}
