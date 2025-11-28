using MediatR;
using AutoMapper;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Entities.FixedAssets;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.FixedAssets.Commands.CreateFixedAsset
{
    /// <summary>
    /// Handler for creating a new fixed asset
    /// </summary>
    public class CreateFixedAssetHandler : IRequestHandler<CreateFixedAssetCommand, Result<CreateFixedAssetResponse>>
    {
        private readonly IRepository<FixedAsset> _repository;
        private readonly IValidator<CreateFixedAssetCommand> _validator;
        private readonly IMapper _mapper;

        public CreateFixedAssetHandler(
            IRepository<FixedAsset> repository,
            IValidator<CreateFixedAssetCommand> validator,
            IMapper mapper)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<Result<CreateFixedAssetResponse>> Handle(
            CreateFixedAssetCommand request,
            CancellationToken cancellationToken)
        {
            // 1. VALIDATE COMMAND
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors[0];
                return Result.Failure<CreateFixedAssetResponse>(
                    Error.Validation(firstError.PropertyName, firstError.ErrorMessage));
            }

            // 2. CREATE DOMAIN ENTITY using the proper constructor
            try
            {
                var asset = new FixedAsset(
                    assetCode: request.AssetCode,
                    assetName: request.AssetName,
                    description: request.Description ?? string.Empty,
                    assetCategory: request.CategoryId,
                    acquisitionDate: request.AcquisitionDate,
                    acquisitionCost: request.PurchasePrice,
                    residualValue: request.SalvageValue,
                    usefulLifeYears: request.UsefulLifeYears,
                    depreciationMethod: "STRAIGHT_LINE",
                    location: request.LocationId ?? string.Empty,
                    serialNumber: string.Empty,
                    model: string.Empty,
                    manufacturer: string.Empty);

                // 3. PERSIST TO DATABASE
                await _repository.AddAsync(asset, cancellationToken);

                // 4. MAP TO RESPONSE DTO
                var response = new CreateFixedAssetResponse
                {
                    Id = asset.Id,
                    AssetCode = asset.AssetCode,
                    AssetName = asset.AssetName,
                    BookValue = asset.CurrentValue,
                    Status = asset.Status.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                return Result.Success(response);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<CreateFixedAssetResponse>(
                    Error.Validation("ValidationError", ex.Message));
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateFixedAssetResponse>(
                    Error.Failure("InternalError", $"An error occurred while creating the asset: {ex.Message}"));
            }
        }
    }
}
