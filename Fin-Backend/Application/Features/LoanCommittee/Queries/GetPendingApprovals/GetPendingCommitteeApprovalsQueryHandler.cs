using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Features.LoanCommittee.Queries.GetPendingApprovals;
using FinTech.Core.Domain.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.LoanCommittee.Queries.GetPendingApprovals
{
    /// <summary>
    /// Handler for GetPendingCommitteeApprovalsQuery
    /// Retrieves pending loan committee approvals with pagination and filtering
    /// </summary>
    public class GetPendingCommitteeApprovalsQueryHandler : 
        IRequestHandler<GetPendingCommitteeApprovalsQuery, Result<PendingApprovalsResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPendingCommitteeApprovalsQueryHandler> _logger;

        public GetPendingCommitteeApprovalsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<GetPendingCommitteeApprovalsQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PendingApprovalsResponse>> Handle(
            GetPendingCommitteeApprovalsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Build query for pending approvals
                var query = _context.LoanCommitteeApprovals
                    .Where(x => x.ApprovalStatus == "Pending");

                // Filter by risk rating if provided
                if (!string.IsNullOrEmpty(request.RiskRating))
                {
                    query = query.Where(x => x.ReferralReason.Contains(request.RiskRating));
                }

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
                var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
                var skip = (pageNumber - 1) * pageSize;

                // Get paginated results, ordered by date descending
                var approvals = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // Map to DTOs
                var approvalDtos = new List<CommitteeApprovalDto>();
                foreach (var approval in approvals)
                {
                    var loanApp = await _context.LoanApplications
                        .FirstOrDefaultAsync(x => x.Id == approval.LoanApplicationId, cancellationToken);

                    approvalDtos.Add(new CommitteeApprovalDto
                    {
                        ApprovalRefNumber = approval.ApprovalRefNumber,
                        LoanApplicationId = approval.LoanApplicationId,
                        LoanAmount = loanApp?.RequestedAmount ?? 0,
                        MemberId = loanApp?.CustomerId ?? "N/A",
                        MemberName = "Member", // Would need customer lookup for actual name
                        Status = approval.ApprovalStatus,
                        DateSubmitted = approval.CreatedDate,
                        RiskRating = approval.ReferralReason,
                        RepaymentScore = approval.RepaymentHistoryScore
                    });
                }

                _logger.LogInformation($"Retrieved {approvals.Count} pending committee approvals (Total: {totalCount})");

                return Result<PendingApprovalsResponse>.Success(
                    new PendingApprovalsResponse
                    {
                        Approvals = approvalDtos,
                        TotalCount = totalCount,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = (int)System.Math.Ceiling((double)totalCount / pageSize)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving pending committee approvals: {ex.Message}");
                return Result<PendingApprovalsResponse>.Failure(
                    $"Error retrieving pending approvals: {ex.Message}");
            }
        }
    }

    public class CommitteeApprovalDto
    {
        public string ApprovalRefNumber { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal LoanAmount { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public string Status { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string RiskRating { get; set; }
        public int RepaymentScore { get; set; }
    }

    public class PendingApprovalsResponse
    {
        public List<CommitteeApprovalDto> Approvals { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
