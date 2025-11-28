using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Features.LoanConfiguration.Queries.GetConfiguration;
using FinTech.Core.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.LoanConfiguration.Queries.GetConfiguration
{
    /// <summary>
    /// Handler for GetLoanConfigurationQuery
    /// Retrieves loan configuration parameters
    /// </summary>
    public class GetLoanConfigurationQueryHandler : 
        IRequestHandler<GetLoanConfigurationQuery, Result<LoanConfigurationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLoanConfigurationQueryHandler> _logger;

        public GetLoanConfigurationQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<GetLoanConfigurationQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<LoanConfigurationDto>> Handle(
            GetLoanConfigurationQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Try to find by ID first
                var loanConfig = await _context.LoanConfigurations
                    .FirstOrDefaultAsync(x => x.Id == request.ConfigurationId, cancellationToken);

                // If not found and ConfigKey is provided, search by key
                if (loanConfig == null && !string.IsNullOrEmpty(request.ConfigKey))
                {
                    loanConfig = await _context.LoanConfigurations
                        .FirstOrDefaultAsync(x => x.ConfigKey == request.ConfigKey, cancellationToken);
                }

                if (loanConfig == null)
                {
                    _logger.LogWarning($"Loan configuration not found. ID: {request.ConfigurationId}, Key: {request.ConfigKey}");
                    return Result<LoanConfigurationDto>.Failure(
                        $"Configuration not found");
                }

                // Map to DTO
                var dto = _mapper.Map<LoanConfigurationDto>(loanConfig);

                _logger.LogInformation($"Loan configuration retrieved: {loanConfig.ConfigKey}");

                return Result<LoanConfigurationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving loan configuration: {ex.Message}");
                return Result<LoanConfigurationDto>.Failure(
                    $"Error retrieving configuration: {ex.Message}");
            }
        }
    }

    public class LoanConfigurationDto
    {
        public int ConfigurationId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public string ValueType { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public bool RequiresBoardApproval { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
