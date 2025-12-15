using MediatR;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Entities.Banking;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Core.Application.Features.BankReconciliation.Commands.CreateReconciliation;

public class CreateReconciliationCommandHandler : IRequestHandler<CreateReconciliationCommand, Result<CreateReconciliationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateReconciliationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateReconciliationResponse>> Handle(CreateReconciliationCommand request, CancellationToken cancellationToken)
    {
        // Get bank account
        var bankAccount = await _unitOfWork.Repository<Domain.Entities.Banking.BankAccount>()
            .GetByIdAsync(request.BankAccountId);
            
        if (bankAccount == null)
        {
            return Result.Failure<CreateReconciliationResponse>(
                Error.NotFound("BankAccount.NotFound", "Bank account not found"));
        }

        // Calculate book balance from general ledger
        var bookBalance = await CalculateBookBalance(request.BankAccountId, request.StatementEndDate);

        // Create reconciliation
        var reconciliation = new Domain.Entities.Banking.BankReconciliation
        {
            BankAccountId = request.BankAccountId,
            BankAccountName = bankAccount.AccountName,
            BankAccountNumber = bankAccount.AccountNumber,
            ReconciliationDate = request.ReconciliationDate,
            StatementStartDate = request.StatementStartDate,
            StatementEndDate = request.StatementEndDate,
            StatementOpeningBalance = request.StatementOpeningBalance,
            StatementClosingBalance = request.StatementClosingBalance,
            BookOpeningBalance = 0, // Calculate from previous reconciliation
            BookClosingBalance = bookBalance,
            ReconciledBalance = 0,
            Variance = request.StatementClosingBalance - bookBalance,
            Status = ReconciliationStatus.Draft,
            Notes = request.Notes,
            ReconciliationReference = GenerateReconciliationReference(),
            ReconciledBy = "System", // Get from current user context
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _unitOfWork.Repository<Domain.Entities.Banking.BankReconciliation>().AddAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateReconciliationResponse
        {
            Id = reconciliation.Id,
            BankAccountId = reconciliation.BankAccountId,
            BankAccountName = reconciliation.BankAccountName,
            ReconciliationDate = reconciliation.ReconciliationDate,
            StatementClosingBalance = reconciliation.StatementClosingBalance,
            BookClosingBalance = reconciliation.BookClosingBalance,
            Variance = reconciliation.Variance,
            Status = reconciliation.Status.ToString()
        };

        return Result<CreateReconciliationResponse>.Success(response);
    }

    private async Task<decimal> CalculateBookBalance(string bankAccountId, DateTime asOfDate)
    {
        // Query general ledger entries for this bank account up to the date
        // This is a simplified version - actual implementation would query GL
        return 0; // Placeholder
    }

    private string GenerateReconciliationReference()
    {
        return $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
