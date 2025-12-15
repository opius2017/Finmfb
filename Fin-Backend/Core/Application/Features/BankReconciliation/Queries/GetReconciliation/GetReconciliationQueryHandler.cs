using MediatR;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Core.Application.Features.BankReconciliation.Queries.GetReconciliation;

public class GetReconciliationQueryHandler : IRequestHandler<GetReconciliationQuery, Result<ReconciliationDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReconciliationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReconciliationDetailDto>> Handle(GetReconciliationQuery request, CancellationToken cancellationToken)
    {
        var reconciliation = await _unitOfWork.Repository<Domain.Entities.Banking.BankReconciliation>()
            .GetByIdAsync(request.Id);

        if (reconciliation == null)
        {
            return Result.Failure<ReconciliationDetailDto>(
                Error.NotFound("Reconciliation.NotFound", "Bank reconciliation not found"));
        }

        var dto = new ReconciliationDetailDto
        {
            Id = reconciliation.Id,
            BankAccountId = reconciliation.BankAccountId,
            BankAccountName = reconciliation.BankAccountName,
            BankAccountNumber = reconciliation.BankAccountNumber,
            ReconciliationDate = reconciliation.ReconciliationDate,
            StatementStartDate = reconciliation.StatementStartDate,
            StatementEndDate = reconciliation.StatementEndDate,
            StatementOpeningBalance = reconciliation.StatementOpeningBalance,
            StatementClosingBalance = reconciliation.StatementClosingBalance,
            BookOpeningBalance = reconciliation.BookOpeningBalance,
            BookClosingBalance = reconciliation.BookClosingBalance,
            TotalDepositsInTransit = reconciliation.TotalDepositsInTransit,
            TotalOutstandingChecks = reconciliation.TotalOutstandingChecks,
            TotalBankCharges = reconciliation.TotalBankCharges,
            TotalBankInterest = reconciliation.TotalBankInterest,
            TotalAdjustments = reconciliation.TotalAdjustments,
            ReconciledBalance = reconciliation.ReconciledBalance,
            Variance = reconciliation.Variance,
            Status = reconciliation.Status.ToString(),
            Notes = reconciliation.Notes,
            ReconciliationReference = reconciliation.ReconciliationReference,
            ReconciledBy = reconciliation.ReconciledBy,
            ReconciledDate = reconciliation.ReconciledDate,
            ApprovedBy = reconciliation.ApprovedBy,
            ApprovedDate = reconciliation.ApprovedDate,
            Items = reconciliation.ReconciliationItems.Select(item => new ReconciliationItemDto
            {
                Id = item.Id,
                TransactionDate = item.TransactionDate,
                Description = item.Description,
                Reference = item.Reference,
                Amount = item.Amount,
                ItemType = item.ItemType.ToString(),
                ItemStatus = item.ItemStatus.ToString(),
                IsMatched = item.IsMatched
            }).ToList()
        };

        return Result<ReconciliationDetailDto>.Success(dto);
    }
}
