using MediatR;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Features.LoanConfiguration.Commands.UpdateConfiguration
{
    /// <summary>
    /// Command to update an existing loan configuration parameter
    /// Tracks previous values for audit trail
    /// </summary>
    public class UpdateLoanConfigurationCommand : IRequest<Result<UpdateLoanConfigurationResponse>>
    {
        public int ConfigurationId { get; set; }
        public string ConfigValue { get; set; }
        public string Reason { get; set; }
        public bool RequiresBoardApproval { get; set; }
    }

    public class UpdateLoanConfigurationResponse
    {
        public int ConfigurationId { get; set; }
        public string ConfigKey { get; set; }
        public string PreviousValue { get; set; }
        public string NewValue { get; set; }
        public string ApprovalStatus { get; set; }
        public string Message { get; set; }
    }
}
