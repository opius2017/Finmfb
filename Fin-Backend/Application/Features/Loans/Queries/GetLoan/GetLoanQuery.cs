using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.Loans.Queries.GetLoan
{
    public record GetLoanQuery(string LoanId) : IRequest<Result<LoanDetailDto>>;
}