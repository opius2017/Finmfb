using FluentValidation;
using FinTech.Core.Application.Features.LoanConfiguration.Commands.CreateConfiguration;

namespace FinTech.Core.Application.Features.LoanConfiguration.Commands.CreateConfiguration
{
    /// <summary>
    /// Validator for CreateLoanConfigurationCommand
    /// Ensures all configuration parameters are valid and within acceptable ranges
    /// </summary>
    public class CreateLoanConfigurationValidator : AbstractValidator<CreateLoanConfigurationCommand>
    {
        public CreateLoanConfigurationValidator()
        {
            RuleFor(x => x.ConfigKey)
                .NotEmpty().WithMessage("Configuration key is required")
                .MaximumLength(100).WithMessage("Configuration key cannot exceed 100 characters");

            RuleFor(x => x.ConfigValue)
                .NotEmpty().WithMessage("Configuration value is required");

            RuleFor(x => x.ValueType)
                .NotEmpty().WithMessage("Value type is required")
                .Must(x => x == "Decimal" || x == "Integer" || x == "Boolean" || x == "String")
                .WithMessage("Value type must be one of: Decimal, Integer, Boolean, String");

            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Label is required")
                .MaximumLength(200).WithMessage("Label cannot exceed 200 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .Must(x => x == "Interest" || x == "Deduction" || x == "Multiplier" || x == "Thresholds" || x == "Compliance")
                .WithMessage("Category must be one of: Interest, Deduction, Multiplier, Thresholds, Compliance");

            // Validate decimal ranges if min/max provided
            When(x => !string.IsNullOrEmpty(x.MinValue) && x.ValueType == "Decimal", () =>
            {
                RuleFor(x => x.MinValue)
                    .Must(x => decimal.TryParse(x, out _))
                    .WithMessage("Minimum value must be a valid decimal");
            });

            When(x => !string.IsNullOrEmpty(x.MaxValue) && x.ValueType == "Decimal", () =>
            {
                RuleFor(x => x.MaxValue)
                    .Must(x => decimal.TryParse(x, out _))
                    .WithMessage("Maximum value must be a valid decimal");
            });

            // Specific business rule validations
            When(x => x.ConfigKey.Contains("InterestRate"), () =>
            {
                RuleFor(x => x.ConfigValue)
                    .Must(x => decimal.TryParse(x, out var val) && val >= 0 && val <= 100)
                    .WithMessage("Interest rate must be between 0 and 100 percent");
            });

            When(x => x.ConfigKey.Contains("DeductionRate"), () =>
            {
                RuleFor(x => x.ConfigValue)
                    .Must(x => decimal.TryParse(x, out var val) && val >= 0 && val <= 100)
                    .WithMessage("Deduction rate must be between 0 and 100 percent");
            });

            When(x => x.ConfigKey.Contains("LoanMultiplier"), () =>
            {
                RuleFor(x => x.ConfigValue)
                    .Must(x => decimal.TryParse(x, out var val) && val > 0 && val <= 10)
                    .WithMessage("Loan multiplier must be between 0.1 and 10 times savings");
            });
        }
    }
}
