using AutoMapper;
using FinTech.Core.Application.DTOs.Currency;
using FinTech.Core.Domain.Entities.Currency;

namespace FinTech.Core.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for currency-related mappings
    /// </summary>
    public class CurrencyMappingProfile : Profile
    {
        public CurrencyMappingProfile()
        {
            // Currency mappings
            CreateMap<Currency, CurrencyDto>();
            CreateMap<CreateCurrencyDto, Currency>();
            CreateMap<UpdateCurrencyDto, Currency>();
            
            // Exchange Rate mappings
            CreateMap<ExchangeRate, ExchangeRateDto>()
                .ForMember(dest => dest.InverseRate, opt => opt.MapFrom(src => 1 / src.Rate));
            CreateMap<CreateExchangeRateDto, ExchangeRate>();
            CreateMap<UpdateExchangeRateDto, ExchangeRate>();
            
            // Currency Revaluation mappings
            CreateMap<CurrencyRevaluation, CurrencyRevaluationDetailDto>()
                .ForMember(dest => dest.AffectedAccounts, opt => opt.MapFrom(src => src.Details));
            
            CreateMap<CurrencyRevaluationDetail, RevaluatedAccountDto>();
        }
    }
}
