using AutoMapper;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Application.DTOs.Accounting;

namespace FinTech.Core.Application.Mappings
{
    public class AccountingProfile : Profile
    {
        public AccountingProfile()
        {
            CreateMap<FinancialPeriod, FinancialPeriodDto>();
        }
    }
}
