using MediatR;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Features.LoanConfiguration.Queries.GetConfiguration
{
    /// <summary>
    /// Query to retrieve a specific loan configuration parameter
    /// </summary>
    public class GetLoanConfigurationQuery : IRequest<Result<LoanConfigurationDto>>
    {
        public int ConfigurationId { get; set; }
    }

    /// <summary>
    /// Query to retrieve loan configuration by key
    /// </summary>
    public class GetLoanConfigurationByKeyQuery : IRequest<Result<LoanConfigurationDto>>
    {
        public string ConfigKey { get; set; }
    }

    /// <summary>
    /// Query to retrieve all loan configurations (paginated)
    /// </summary>
    public class GetAllLoanConfigurationsQuery : IRequest<Result<PaginatedList<LoanConfigurationDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Category { get; set; }
    }

    public class LoanConfigurationDto
    {
        public int Id { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public string ValueType { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public bool IsLocked { get; set; }
        public string LockReason { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string PreviousValue { get; set; }
        public bool RequiresBoardApproval { get; set; }
        public string ApprovalStatus { get; set; }
    }
}
