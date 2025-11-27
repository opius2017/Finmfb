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

            // Create loan entity with complete initialization
            var loanNumber = await GenerateLoanNumber();
            var interestRate = loanProduct.InterestRate;
            var monthlyPayment = CalculateMonthlyPayment(request.LoanAmount, interestRate, request.TenorInMonths);
            var totalInterest = (monthlyPayment * request.TenorInMonths) - request.LoanAmount;
            var totalRepayment = request.LoanAmount + totalInterest;

            var loan = new Loan
            {
                Id = Guid.NewGuid().ToString(),
                LoanNumber = loanNumber,
                CustomerId = request.CustomerId,
                CustomerName = customer.FullName,
                LoanProductId = request.LoanProductId,
                LoanProductName = loanProduct.ProductName,
                PrincipalAmount = request.LoanAmount,
                LoanAmount = request.LoanAmount,
                InterestRate = interestRate,
                TenorInMonths = request.TenorInMonths,
                MonthlyPayment = monthlyPayment,
                TotalInterest = totalInterest,
                TotalRepayment = totalRepayment,
                OutstandingBalance = request.LoanAmount,
                Purpose = request.Purpose,
                ApplicationDate = DateTime.UtcNow,
                Status = LoanStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System", // Get from current user context
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "System"
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
