using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Features.LoanConfiguration.Commands.UpdateConfiguration;
using FinTech.Core.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.LoanConfiguration.Commands.UpdateConfiguration
{
    /// <summary>
    /// Handler for UpdateLoanConfigurationCommand
    /// Updates an existing loan configuration parameter with audit trail
    /// </summary>
    public class UpdateLoanConfigurationCommandHandler : 
        IRequestHandler<UpdateLoanConfigurationCommand, Result<UpdateLoanConfigurationResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateLoanConfigurationCommandHandler> _logger;

        public UpdateLoanConfigurationCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<UpdateLoanConfigurationCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<UpdateLoanConfigurationResponse>> Handle(
            UpdateLoanConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Find existing configuration
                var loanConfig = await _context.LoanConfigurations
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (loanConfig == null)
                {
                    _logger.LogWarning($"Loan configuration with ID {request.Id} not found");
                    return Result<UpdateLoanConfigurationResponse>.Failure(
                        $"Configuration with ID {request.Id} not found");
                }

                // Check if locked
                if (loanConfig.IsLocked)
                {
                    _logger.LogWarning($"Attempt to update locked configuration: {loanConfig.ConfigKey}");
                    return Result<UpdateLoanConfigurationResponse>.Failure(
                        $"Configuration '{loanConfig.ConfigKey}' is locked and cannot be modified");
                }

                // Store previous value for audit trail
                var previousValue = loanConfig.ConfigValue;

                // Update configuration
                loanConfig.ConfigValue = request.ConfigValue;
                loanConfig.Label = request.Label ?? loanConfig.Label;
                loanConfig.Description = request.Description ?? loanConfig.Description;
                loanConfig.MinValue = request.MinValue ?? loanConfig.MinValue;
                loanConfig.MaxValue = request.MaxValue ?? loanConfig.MaxValue;
                loanConfig.PreviousValue = previousValue;
                loanConfig.ChangeReason = request.ChangeReason;
                loanConfig.ApprovalStatus = request.RequiresBoardApproval ? "Pending" : "Approved";
                loanConfig.LastModifiedDate = System.DateTime.UtcNow;

                // Save changes
                _context.LoanConfigurations.Update(loanConfig);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Loan configuration updated: {loanConfig.ConfigKey} (ID: {loanConfig.Id})");

                return Result<UpdateLoanConfigurationResponse>.Success(
                    new UpdateLoanConfigurationResponse
                    {
                        ConfigurationId = loanConfig.Id,
                        ConfigKey = loanConfig.ConfigKey,
                        Message = $"Configuration '{loanConfig.ConfigKey}' updated successfully",
                        PreviousValue = previousValue,
                        NewValue = request.ConfigValue
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating loan configuration: {ex.Message}");
                return Result<UpdateLoanConfigurationResponse>.Failure(
                    $"Error updating configuration: {ex.Message}");
            }
        }
    }

    public class UpdateLoanConfigurationResponse
    {
        public int ConfigurationId { get; set; }
        public string ConfigKey { get; set; }
        public string Message { get; set; }
        public string PreviousValue { get; set; }
        public string NewValue { get; set; }
    }
}
