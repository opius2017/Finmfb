using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Features.LoanCommittee.Commands.ApproveApplication;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Loans;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Features.LoanCommittee.Commands.ApproveApplication
{
    /// <summary>
    /// Handler for ApproveLoanByCommitteeCommand
    /// Processes loan committee approvals, rejections, or conditional approvals
    /// </summary>
    public class ApproveLoanByCommitteeCommandHandler : 
        IRequestHandler<ApproveLoanByCommitteeCommand, Result<ApproveLoanByCommitteeResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ApproveLoanByCommitteeCommandHandler> _logger;

        public ApproveLoanByCommitteeCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ApproveLoanByCommitteeCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ApproveLoanByCommitteeResponse>> Handle(
            ApproveLoanByCommitteeCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Find the approval record
                var approval = await _context.LoanCommitteeApprovals
                    .FirstOrDefaultAsync(x => x.ApprovalRefNumber == request.ApprovalRefNumber, cancellationToken);

                if (approval == null)
                {
                    _logger.LogWarning($"Committee approval not found: {request.ApprovalRefNumber}");
                    return Result<ApproveLoanByCommitteeResponse>.Failure(
                        $"Approval with reference number '{request.ApprovalRefNumber}' not found");
                }

                // Update approval record
                approval.CommitteeDecision = request.CommitteeDecision;
                approval.ApprovalStatus = request.CommitteeDecision;
                approval.MeetingDate = request.MeetingDate ?? System.DateTime.UtcNow;
                approval.VotingDetails = request.VotingDetails;
                approval.ConditionsText = request.ConditionsText;
                
                if (request.CommitteeDecision == "Rejected")
                {
                    approval.RejectionReason = request.RejectionReason;
                }

                approval.LastModifiedDate = System.DateTime.UtcNow;

                // Update database
                _context.LoanCommitteeApprovals.Update(approval);

                // If approved, update loan application status
                if (request.CommitteeDecision == "Approved" || request.CommitteeDecision == "ApprovedWithConditions")
                {
                    var loanApplication = await _context.LoanApplications
                        .FirstOrDefaultAsync(x => x.Id == approval.LoanApplicationId, cancellationToken);

                    if (loanApplication != null)
                    {
                        loanApplication.Status = "CommitteeApproved";
                        _context.LoanApplications.Update(loanApplication);
                    }
                }
                else if (request.CommitteeDecision == "Rejected")
                {
                    var loanApplication = await _context.LoanApplications
                        .FirstOrDefaultAsync(x => x.Id == approval.LoanApplicationId, cancellationToken);

                    if (loanApplication != null)
                    {
                        loanApplication.Status = "CommitteeRejected";
                        loanApplication.RejectionReason = request.RejectionReason;
                        _context.LoanApplications.Update(loanApplication);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Committee approval processed: {request.ApprovalRefNumber} - Decision: {request.CommitteeDecision}");

                return Result<ApproveLoanByCommitteeResponse>.Success(
                    new ApproveLoanByCommitteeResponse
                    {
                        ApprovalRefNumber = approval.ApprovalRefNumber,
                        CommitteeDecision = approval.CommitteeDecision,
                        Message = $"Loan application approval processed successfully with decision: {request.CommitteeDecision}",
                        ProcessedDate = System.DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing committee approval: {ex.Message}");
                return Result<ApproveLoanByCommitteeResponse>.Failure(
                    $"Error processing approval: {ex.Message}");
            }
        }
    }

    public class ApproveLoanByCommitteeResponse
    {
        public string ApprovalRefNumber { get; set; }
        public string CommitteeDecision { get; set; }
        public string Message { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
}
