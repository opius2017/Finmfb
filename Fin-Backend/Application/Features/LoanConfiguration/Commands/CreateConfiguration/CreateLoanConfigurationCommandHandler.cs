using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Features.LoanConfiguration.Commands.CreateConfiguration;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.LoanConfiguration.Commands.CreateConfiguration
{
    /// <summary>
    /// Handler for CreateLoanConfigurationCommand
    /// Creates or updates a system-wide loan configuration parameter
    /// </summary>
    public class CreateLoanConfigurationCommandHandler : 
        IRequestHandler<CreateLoanConfigurationCommand, Result<CreateLoanConfigurationResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateLoanConfigurationCommandHandler> _logger;

        public CreateLoanConfigurationCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<CreateLoanConfigurationCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CreateLoanConfigurationResponse>> Handle(
            CreateLoanConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Check for duplicate ConfigKey
                var existingConfig = await _context.LoanConfigurations
                    .FirstOrDefaultAsync(x => x.ConfigKey == request.ConfigKey, cancellationToken);

                if (existingConfig != null)
                {
                    _logger.LogWarning($"Loan configuration with key {request.ConfigKey} already exists");
                    return Result<CreateLoanConfigurationResponse>.Failure(
                        $"Configuration key '{request.ConfigKey}' already exists");
                }

                // Create new configuration entity
                var loanConfig = new LoanConfiguration
                {
                    ConfigKey = request.ConfigKey,
                    ConfigValue = request.ConfigValue,
                    ValueType = request.ValueType,
                    Label = request.Label,
                    Description = request.Description,
                    Category = request.Category,
                    MinValue = request.MinValue,
                    MaxValue = request.MaxValue,
                    RequiresBoardApproval = request.RequiresBoardApproval,
                    IsLocked = false,
                    ApprovalStatus = request.RequiresBoardApproval ? "Pending" : "Approved"
                };

                // Add to database
                _context.LoanConfigurations.Add(loanConfig);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Loan configuration created: {request.ConfigKey} with ID {loanConfig.Id}");

                // Return response
                return Result<CreateLoanConfigurationResponse>.Success(
                    new CreateLoanConfigurationResponse
                    {
                        ConfigurationId = loanConfig.Id,
                        ConfigKey = loanConfig.ConfigKey,
                        Message = $"Configuration '{loanConfig.ConfigKey}' created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating loan configuration: {ex.Message}");
                return Result<CreateLoanConfigurationResponse>.Failure(
                    $"Error creating configuration: {ex.Message}");
            }
        }
    }
}
