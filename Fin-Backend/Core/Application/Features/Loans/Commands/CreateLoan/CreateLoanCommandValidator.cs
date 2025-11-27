using FluentValidation;

namespace FinTech.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
    {
        public CreateLoanCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required");

            RuleFor(x => x.LoanProductId)
                .NotEmpty().WithMessage("Loan Product ID is required");

            RuleFor(x => x.LoanAmount)
                .GreaterThan(0).WithMessage("Loan amount must be greater than zero");

            RuleFor(x => x.TenorInMonths)
                .GreaterThan(0).WithMessage("Tenor must be greater than zero")
                .LessThanOrEqualTo(360).WithMessage("Tenor cannot exceed 360 months");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Loan purpose is required")
                .MaximumLength(500).WithMessage("Purpose cannot exceed 500 characters");
        }
    }
}