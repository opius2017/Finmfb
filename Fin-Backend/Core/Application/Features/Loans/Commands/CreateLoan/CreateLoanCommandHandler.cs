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
            if (request.LoanAmount < loanProduct.MinAmount || request.LoanAmount > loanProduct.MaxAmount)
            {
                return Result.Failure<CreateLoanResponse>(
                    Error.Validation("Loan.InvalidAmount", 
                    $"Loan amount must be between {loanProduct.MinAmount} and {loanProduct.MaxAmount}"));
            }

            // Create loan entity with initialization using domain constructor
            var loanNumber = await GenerateLoanNumber();
            // Choose a default interest rate from the product (use MinInterestRate as default)
            var interestRate = loanProduct.MinInterestRate;
            var monthlyPayment = CalculateMonthlyPayment(request.LoanAmount, interestRate, request.TenorInMonths);

            var loan = new Loan
            {
                LoanNumber = loanNumber,
                CustomerId = customer.Id,
                LoanProductId = Guid.Parse(loanProduct.Id),
                PrincipalAmount = request.LoanAmount,
                InterestRate = interestRate,
                TenureMonths = request.TenorInMonths,
                DisbursementDate = DateTime.UtcNow,
                LoanType = loanProduct.Name,
                RepaymentFrequency = loanProduct.RepaymentFrequency.ToString(),
                // Currency is not in Loan entity directly, or might be handled differently. Using local var if needed or ignored if not in entity.
                // Looking at Loan.cs, it doesn't have Currency property? Wait, I saw "CurrencyCode" in Invoice/Deposit but Loan?
                // Step 330: Loan.cs does NOT have Currency property.
                LoanApplicationId = null, // or appropriate mapping
                Notes = request.Purpose, // Mapping purpose to Notes or similar
                Status = "ACTIVE",
                MemberId = customer.Id.ToString() // Assuming MemberId maps to CustomerId for now if not distinct
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

        private async Task<string> GenerateLoanNumber()
        {
            // Generate unique loan number
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"LN-{timestamp}-{random}";
        }

        private decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
        {
            if (months == 0) return 0;
            
            // Convert annual rate to monthly rate
            var monthlyRate = (annualRate / 100) / 12;
            
            if (monthlyRate == 0) return principal / months;
            
            // Calculate monthly payment using amortization formula
            // M = P * [r(1+r)^n] / [(1+r)^n - 1]
            var power = Math.Pow((double)(1 + monthlyRate), months);
            var monthlyPayment = principal * (monthlyRate * (decimal)power) / ((decimal)power - 1);
            
            return Math.Round(monthlyPayment, 2);
        }
    }
}
