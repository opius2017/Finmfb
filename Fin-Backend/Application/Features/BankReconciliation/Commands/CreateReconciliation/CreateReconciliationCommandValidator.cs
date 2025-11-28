using FluentValidation;

namespace FinTech.Core.Application.Features.BankReconciliation.Commands.CreateReconciliation;

public class CreateReconciliationCommandValidator : AbstractValidator<CreateReconciliationCommand>
{
    public CreateReconciliationCommandValidator()
    {
        RuleFor(x => x.BankAccountId)
            .NotEmpty().WithMessage("Bank account is required");

        RuleFor(x => x.ReconciliationDate)
            .NotEmpty().WithMessage("Reconciliation date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Reconciliation date cannot be in the future");

        RuleFor(x => x.StatementStartDate)
            .NotEmpty().WithMessage("Statement start date is required")
            .LessThanOrEqualTo(x => x.StatementEndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.StatementEndDate)
            .NotEmpty().WithMessage("Statement end date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Statement end date cannot be in the future");

        RuleFor(x => x.StatementOpeningBalance)
            .NotNull().WithMessage("Statement opening balance is required");

        RuleFor(x => x.StatementClosingBalance)
            .NotNull().WithMessage("Statement closing balance is required");
    }
}
