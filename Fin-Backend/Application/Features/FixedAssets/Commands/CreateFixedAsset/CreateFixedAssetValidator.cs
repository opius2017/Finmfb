using FluentValidation;

namespace FinTech.Core.Application.Features.FixedAssets.Commands.CreateFixedAsset
{
    /// <summary>
    /// Validator for CreateFixedAssetCommand
    /// Ensures all required fields are valid before processing
    /// </summary>
    public class CreateFixedAssetValidator : AbstractValidator<CreateFixedAssetCommand>
    {
        public CreateFixedAssetValidator()
        {
            RuleFor(c => c.AssetCode)
                .NotEmpty()
                .WithMessage("Asset code is required")
                .MaximumLength(50)
                .WithMessage("Asset code must not exceed 50 characters")
                .Matches(@"^[A-Z0-9\-]+$")
                .WithMessage("Asset code must contain only uppercase letters, numbers, and hyphens");

            RuleFor(c => c.AssetName)
                .NotEmpty()
                .WithMessage("Asset name is required")
                .MaximumLength(200)
                .WithMessage("Asset name must not exceed 200 characters");

            RuleFor(c => c.Description)
                .MaximumLength(1000)
                .WithMessage("Description must not exceed 1000 characters");

            RuleFor(c => c.PurchasePrice)
                .GreaterThan(0)
                .WithMessage("Purchase price must be greater than 0")
                .LessThanOrEqualTo(decimal.MaxValue)
                .WithMessage("Purchase price is invalid");

            RuleFor(c => c.SalvageValue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Salvage value cannot be negative")
                .LessThan(c => c.PurchasePrice)
                .WithMessage("Salvage value must be less than purchase price");

            RuleFor(c => c.UsefulLifeYears)
                .GreaterThan(0)
                .WithMessage("Useful life must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Useful life cannot exceed 100 years");

            RuleFor(c => c.CategoryId)
                .NotEmpty()
                .WithMessage("Category is required");

            RuleFor(c => c.LocationId)
                .NotEmpty()
                .WithMessage("Location is required");

            RuleFor(c => c.AcquisitionDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Acquisition date cannot be in the future");
        }
    }
}
