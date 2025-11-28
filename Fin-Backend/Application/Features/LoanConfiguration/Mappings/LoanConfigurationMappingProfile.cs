using AutoMapper;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.Features.LoanConfiguration.Queries.GetConfiguration;

namespace FinTech.Core.Application.Features.LoanConfiguration.Mappings
{
    /// <summary>
    /// AutoMapper profile for LoanConfiguration entity mapping to DTOs
    /// </summary>
    public class LoanConfigurationMappingProfile : Profile
    {
        public LoanConfigurationMappingProfile()
        {
            // Map LoanConfiguration to LoanConfigurationDto
            CreateMap<LoanConfiguration, LoanConfigurationDto>()
                .ForMember(dest => dest.ConfigurationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RequiresBoardApproval, opt => opt.MapFrom(src => src.RequiresBoardApproval));
        }
    }
}
