using AutoMapper;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.Features.Loans.Queries.GetLoan;

namespace FinTech.Core.Application.Features.Loans.Mappings
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            CreateMap<Loan, LoanDetailDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}" : ""))
                .ForMember(dest => dest.LoanProductName, opt => opt.MapFrom(src => src.LoanProduct != null ? src.LoanProduct.ProductName : ""))
                .ForMember(dest => dest.OutstandingBalance, opt => opt.MapFrom(src => src.OutstandingPrincipal + src.OutstandingInterest));
        }
    }
}
