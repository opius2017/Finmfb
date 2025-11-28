using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Shared.Abstractions;
using FinTech.Shared.Common;
using FluentValidation;
using AutoMapper;

namespace FinTech.Shared.Application.Services
{
    /// <summary>
    /// Base CRUD command handler template
    /// </summary>
    public abstract class CreateCommandHandler<TCommand, TEntity, TDto> : IRequestHandler<TCommand, Result<TDto>>
        where TCommand : IRequest<Result<TDto>>
        where TEntity : BaseEntity
    {
        protected readonly ICrudRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ICurrentUserProvider _userProvider;
        protected readonly IValidator<TCommand> _validator;

        protected CreateCommandHandler(
            ICrudRepository<TEntity> repository,
            IMapper mapper,
            ICurrentUserProvider userProvider,
            IValidator<TCommand> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _userProvider = userProvider;
            _validator = validator;
        }

        public async Task<Result<TDto>> Handle(TCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                    .ToList();
                return Result<TDto>.ValidationFailed(errors);
            }

            // Authorization check
            var authContext = _userProvider.GetCurrentUser();
            if (!HasCreatePermission(authContext))
                return Result<TDto>.Forbidden("You do not have permission to create this resource");

            // Create entity
            var entity = await MapAndCreateEntity(request, cancellationToken);

            // Add to repository
            await _repository.AddAsync(entity, cancellationToken);

            // Map to DTO and return
            var dto = _mapper.Map<TDto>(entity);
            return Result<TDto>.Success(dto, "Resource created successfully", 201);
        }

        protected abstract Task<TEntity> MapAndCreateEntity(TCommand request, CancellationToken cancellationToken);

        protected virtual bool HasCreatePermission(IAuthorizationContext authContext)
            => authContext.IsAdmin || authContext.HasPermission($"Create_{typeof(TEntity).Name}");
    }

    /// <summary>
    /// Base update command handler template
    /// </summary>
    public abstract class UpdateCommandHandler<TCommand, TEntity, TDto> : IRequestHandler<TCommand, Result<TDto>>
        where TCommand : IRequest<Result<TDto>>
        where TEntity : BaseEntity
    {
        protected readonly ICrudRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ICurrentUserProvider _userProvider;
        protected readonly IValidator<TCommand> _validator;

        protected UpdateCommandHandler(
            ICrudRepository<TEntity> repository,
            IMapper mapper,
            ICurrentUserProvider userProvider,
            IValidator<TCommand> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _userProvider = userProvider;
            _validator = validator;
        }

        public async Task<Result<TDto>> Handle(TCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new Error(e.PropertyName, e.ErrorMessage))
                    .ToList();
                return Result<TDto>.ValidationFailed(errors);
            }

            // Get entity
            var entityId = GetEntityId(request);
            var entity = await _repository.GetByIdAsync(entityId, cancellationToken: cancellationToken);
            if (entity == null)
                return Result<TDto>.NotFound($"{typeof(TEntity).Name} with ID {entityId} not found");

            // Authorization check
            var authContext = _userProvider.GetCurrentUser();
            if (!HasUpdatePermission(authContext, entity))
                return Result<TDto>.Forbidden("You do not have permission to update this resource");

            // Update entity
            await MapAndUpdateEntity(entity, request, cancellationToken);
            await _repository.UpdateAsync(entity, cancellationToken: cancellationToken);

            // Map to DTO and return
            var dto = _mapper.Map<TDto>(entity);
            return Result<TDto>.Success(dto, "Resource updated successfully");
        }

        protected abstract string GetEntityId(TCommand request);
        protected abstract Task MapAndUpdateEntity(TEntity entity, TCommand request, CancellationToken cancellationToken);

        protected virtual bool HasUpdatePermission(IAuthorizationContext authContext, TEntity entity)
            => authContext.IsAdmin || authContext.HasPermission($"Update_{typeof(TEntity).Name}");
    }

    /// <summary>
    /// Base delete command handler template
    /// </summary>
    public abstract class DeleteCommandHandler<TCommand, TEntity> : IRequestHandler<TCommand, Result>
        where TCommand : IRequest<Result>
        where TEntity : BaseEntity
    {
        protected readonly ICrudRepository<TEntity> _repository;
        protected readonly ICurrentUserProvider _userProvider;

        protected DeleteCommandHandler(
            ICrudRepository<TEntity> repository,
            ICurrentUserProvider userProvider)
        {
            _repository = repository;
            _userProvider = userProvider;
        }

        public async Task<Result> Handle(TCommand request, CancellationToken cancellationToken)
        {
            // Get entity
            var entityId = GetEntityId(request);
            var entity = await _repository.GetByIdAsync(entityId, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound($"{typeof(TEntity).Name} with ID {entityId} not found");

            // Authorization check
            var authContext = _userProvider.GetCurrentUser();
            if (!HasDeletePermission(authContext, entity))
                return Result.Forbidden("You do not have permission to delete this resource");

            // Delete entity
            await _repository.DeleteAsync(entity, cancellationToken);

            return Result.Success("Resource deleted successfully");
        }

        protected abstract string GetEntityId(TCommand request);

        protected virtual bool HasDeletePermission(IAuthorizationContext authContext, TEntity entity)
            => authContext.IsAdmin || authContext.HasPermission($"Delete_{typeof(TEntity).Name}");
    }

    /// <summary>
    /// Base list query handler template
    /// </summary>
    public abstract class ListQueryHandler<TQuery, TEntity, TDto> : IRequestHandler<TQuery, Result<PaginatedResult<TDto>>>
        where TQuery : IRequest<Result<PaginatedResult<TDto>>>
        where TEntity : BaseEntity
    {
        protected readonly ICrudRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ICurrentUserProvider _userProvider;

        protected ListQueryHandler(
            ICrudRepository<TEntity> repository,
            IMapper mapper,
            ICurrentUserProvider userProvider)
        {
            _repository = repository;
            _mapper = mapper;
            _userProvider = userProvider;
        }

        public async Task<Result<PaginatedResult<TDto>>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            // Authorization check
            var authContext = _userProvider.GetCurrentUser();
            if (!HasListPermission(authContext))
                return Result<PaginatedResult<TDto>>.Forbidden("You do not have permission to list this resource");

            // Get paginated entities
            var pageNumber = GetPageNumber(query);
            var pageSize = GetPageSize(query);
            var (entities, totalCount) = await _repository.GetAllAsync(pageNumber, pageSize, cancellationToken);

            // Map to DTOs
            var dtos = entities.Select(e => _mapper.Map<TDto>(e)).ToList();
            var paginatedResult = PaginatedResult<TDto>.Success(dtos, pageNumber, pageSize, totalCount);

            return Result<PaginatedResult<TDto>>.Success(paginatedResult);
        }

        protected virtual int GetPageNumber(TQuery query) => 1;
        protected virtual int GetPageSize(TQuery query) => 10;

        protected virtual bool HasListPermission(IAuthorizationContext authContext)
            => authContext.IsAdmin || authContext.HasPermission($"List_{typeof(TEntity).Name}");
    }
}
