using MediatR;
using AutoMapper;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Entities.Loans;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.Loans.Queries.GetLoan
{
    public class GetLoanQueryHandler : IRequestHandler<GetLoanQuery, Result<LoanDetailDto>>
    {
        private readonly IRepository<Loan> _loanRepository;
        private readonly IMapper _mapper;

        public GetLoanQueryHandler(IRepository<Loan> loanRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<Result<LoanDetailDto>> Handle(
            GetLoanQuery request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRepository.GetByIdAsync(request.LoanId, cancellationToken);

            if (loan == null)
            {
                return Result.Failure<LoanDetailDto>(
                    Error.NotFound("Loan.NotFound", $"Loan with ID {request.LoanId} not found"));
            }

            var loanDto = _mapper.Map<LoanDetailDto>(loan);
            return Result.Success(loanDto);
        }
    }
}
