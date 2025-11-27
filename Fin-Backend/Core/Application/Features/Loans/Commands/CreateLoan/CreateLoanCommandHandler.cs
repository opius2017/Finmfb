using MediatR;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.Customers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Result<CreateLoanResponse>>
    {
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<LoanProduct> _loanProductRepository;

        public CreateLoanCommandHandler(
            IRepository<Loan> loanRepository,
            IRepository<Customer> customerRepository,
            IRepository<LoanProduct> loanProductRepository)
        {
            _loanRepository = loanRepository;
            _customerRepository = customerRepository;
            _loanProductRepository = loanProductRepository;
        }

        public async Task<Result<CreateLoanResponse>> Handle(
            CreateLoanCommand request,
            CancellationToken cancellationToken)
        {
            // Validate customer exists
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                return Result.Failure<CreateLoanResponse>(
                    Error.NotFound("Customer.NotFound", $"Customer with ID {request.CustomerId} not found"));
            }

            // Validate loan product exists
            var loanProduct = await _loanProductRepository.GetByIdAsync(request.LoanProductId, cancellationToken);
            if (loanProduct == null)
            {
                return Result.Failure<CreateLoanResponse>(
                    Error.NotFound("LoanProduct.NotFound", $"Loan product with ID {request.LoanProductId} not found"));
            }

            // Validate loan amount against product limits
            if (request.LoanAmount < loanProduct.MinLoanAmount || request.LoanAmount > loanProduct.MaxLoanAmount)
            {
                return Result.Failure<CreateLoanResponse>(
                    Error.Validation("Loan.InvalidAmount", 
                    $"Loan amount must be between {loanProduct.MinLoanAmount} and {loanProduct.MaxLoanAmount}"));
            }

            // TODO: Create loan entity (adjust according to your Loan entity structure)
            // This is a placeholder - adapt to your actual Loan entity creation method
            var loan = new Loan
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = request.CustomerId,
                LoanProductId = request.LoanProductId,
                LoanAmount = request.LoanAmount,
                TenorInMonths = request.TenorInMonths,
                Purpose = request.Purpose,
                ApplicationDate = DateTime.UtcNow,
                Status = LoanStatus.Pending
            };

            await _loanRepository.AddAsync(loan, cancellationToken);

            var response = new CreateLoanResponse
            {
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber ?? "PENDING",
                Status = loan.Status.ToString(),
                Message = "Loan application created successfully"
            };

            return Result.Success(response);
        }
    }
}
