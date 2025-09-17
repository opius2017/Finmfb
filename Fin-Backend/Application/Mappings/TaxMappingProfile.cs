using AutoMapper;
using FinTech.Application.DTOs.Tax;
using FinTech.Domain.Entities.Tax;

namespace FinTech.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for tax entities
    /// </summary>
    public class TaxMappingProfile : Profile
    {
        /// <summary>
        /// Constructor with mapping configuration
        /// </summary>
        public TaxMappingProfile()
        {
            // TaxType mappings
            CreateMap<TaxType, TaxTypeDto>();
            CreateMap<CreateTaxTypeDto, TaxType>();
            CreateMap<UpdateTaxTypeDto, TaxType>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // TaxRate mappings
            CreateMap<TaxRate, TaxRateDto>()
                .ForMember(dest => dest.TaxTypeCode, opt => opt.Ignore()); // TaxTypeCode is set manually
            CreateMap<CreateTaxRateDto, TaxRate>();
            CreateMap<UpdateTaxRateDto, TaxRate>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // TaxTransaction mappings
            CreateMap<TaxTransaction, TaxTransactionDto>()
                .ForMember(dest => dest.TaxTypeCode, opt => opt.Ignore()); // TaxTypeCode is set manually
            CreateMap<CreateTaxTransactionDto, TaxTransaction>();
        }
    }
}