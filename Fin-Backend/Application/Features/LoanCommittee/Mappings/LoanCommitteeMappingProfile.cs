using AutoMapper;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.Features.LoanCommittee.Queries.GetPendingApprovals;

namespace FinTech.Core.Application.Features.LoanCommittee.Mappings
{
    /// <summary>
    /// AutoMapper profile for LoanCommitteeApproval entity mapping to DTOs
    /// </summary>
    public class LoanCommitteeMappingProfile : Profile
    {
        public LoanCommitteeMappingProfile()
        {
            // Map LoanCommitteeApproval to CommitteeApprovalDto (list view)
            CreateMap<LoanCommitteeApproval, CommitteeApprovalDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.ApprovalStatus))
                .ForMember(dest => dest.RiskRating, opt => opt.MapFrom(src => src.ReferralReason))
                .ForMember(dest => dest.DateSubmitted, opt => opt.MapFrom(src => src.CreatedDate));
        }
    }
}
