using MediatR;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Features.LoanConfiguration.Commands.CreateConfiguration
{
    /// <summary>
    /// Command to create or update a system-wide loan configuration parameter
    /// Only Super Admins can execute this command
    /// </summary>
    public class CreateLoanConfigurationCommand : IRequest<Result<CreateLoanConfigurationResponse>>
    {
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public string ValueType { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public bool RequiresBoardApproval { get; set; }
    }

    public class CreateLoanConfigurationResponse
    {
        public int ConfigurationId { get; set; }
        public string ConfigKey { get; set; }
        public string Message { get; set; }
    }
}
