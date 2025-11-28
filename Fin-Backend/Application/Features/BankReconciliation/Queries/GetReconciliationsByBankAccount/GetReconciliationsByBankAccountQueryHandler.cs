using MediatR;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Core.Application.Features.BankReconciliation.Queries.GetReconciliationsByBankAccount;

public class GetReconciliationsByBankAccountQueryHandler : IRequestHandler<GetReconciliationsByBankAccountQuery, Result<List<ReconciliationSummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReconciliationsByBankAccountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ReconciliationSummaryDto>>> Handle(GetReconciliationsByBankAccountQuery request, CancellationToken cancellationToken)
    {
        var reconciliations = await _unitOfWork.Repository<Domain.Entities.Banking.BankReconciliation>()
            .GetAll()
            .Where(r => r.BankAccountId == request.BankAccountId)
            .OrderByDescending(r => r.ReconciliationDate)
            .ToListAsync(cancellationToken);

        var dtos = reconciliations.Select(r => new ReconciliationSummaryDto
        {
            Id = r.Id,
            BankAccountId = r.BankAccountId,
            BankAccountName = r.BankAccountName,
            ReconciliationDate = r.ReconciliationDate,
            StatementEndDate = r.StatementEndDate,
            StatementClosingBalance = r.StatementClosingBalance,
            BookClosingBalance = r.BookClosingBalance,
            Variance = r.Variance,
            Status = r.Status.ToString(),
            ReconciliationReference = r.ReconciliationReference,
            ReconciledBy = r.ReconciledBy,
            ApprovedDate = r.ApprovedDate
        }).ToList();

        return Result<List<ReconciliationSummaryDto>>.Success(dtos);
    }
}
