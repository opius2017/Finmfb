using AutoMapper;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.WebAPI.Application.DTOs.Accounting;

namespace FinTech.WebAPI.Application.Mappings
{
    public class AccountingProfile : Profile
    {
        public AccountingProfile()
        {
            CreateMap<FinancialPeriod, FinancialPeriodDto>();
        }
    }
}
