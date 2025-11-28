using MediatR;
using AutoMapper;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Shared.Abstractions;
using FluentValidation;
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
        private readonly ICurrentUserProvider _userProvider;
        private readonly IValidator<CreateFixedAssetCommand> _validator;
        private readonly IMapper _mapper;

        public CreateFixedAssetHandler(
            IRepository<FixedAsset> repository,
            ICurrentUserProvider userProvider,
            IValidator<CreateFixedAssetCommand> validator,
            IMapper mapper)
        {
            _repository = repository;
            _userProvider = userProvider;
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
                    Error.New(firstError.PropertyName, firstError.ErrorMessage));
            }

            // 2. AUTHORIZE
            var currentUser = _userProvider.GetCurrentUser();
            if (!currentUser.IsAdmin && !currentUser.HasPermission("FixedAssets.Create"))
            {
                return Result.Failure<CreateFixedAssetResponse>(
                    Error.New("Unauthorized", "You do not have permission to create fixed assets"));
            }

            // 3. CREATE DOMAIN ENTITY
            try
            {
                var asset = FixedAsset.Create(
                    request.AssetCode,
                    request.AssetName,
                    request.Description,
                    request.PurchasePrice,
                    request.SalvageValue,
                    request.UsefulLifeYears,
                    request.CategoryId,
                    request.LocationId,
                    request.DepartmentId,
                    request.AcquisitionDate,
                    currentUser.UserId);

                // 4. PERSIST TO DATABASE
                await _repository.AddAsync(asset, cancellationToken);

                // 5. MAP TO RESPONSE DTO
                var response = new CreateFixedAssetResponse
                {
                    Id = asset.Id,
                    AssetCode = asset.AssetCode,
                    AssetName = asset.AssetName,
                    BookValue = asset.BookValue,
                    Status = asset.Status.ToString(),
                    CreatedAt = asset.CreatedAt
                };

                return Result.Success(response);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<CreateFixedAssetResponse>(
                    Error.New("ValidationError", ex.Message));
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateFixedAssetResponse>(
                    Error.New("InternalError", $"An error occurred while creating the asset: {ex.Message}"));
            }
        }
    }
}
