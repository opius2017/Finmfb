using AutoMapper;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Application.DTOs.ClientPortal;

namespace FinTech.Application.Common.Mappings
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // Fund Transfer mappings
            CreateMap<FundTransferDto, ExternalTransfer>()
                .ForMember(dest => dest.SourceAccountId, opt => opt.Ignore())
                .ForMember(dest => dest.ReferenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.InitiatedAt, opt => opt.MapFrom(src => System.DateTime.UtcNow));

            // Bill Payment mappings
            CreateMap<BillPaymentDto, BillPayment>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore())
                .ForMember(dest => dest.ReferenceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => System.DateTime.UtcNow));

            // Recurring Payment mappings
            CreateMap<RecurringPaymentDto, RecurringPayment>()
                .ForMember(dest => dest.SourceAccountId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
                .ForMember(dest => dest.LastExecutionDate, opt => opt.Ignore())
                .ForMember(dest => dest.NextExecutionDate, opt => opt.MapFrom(src => src.StartDate));

            // Saved Templates and Payees mappings
            CreateMap<SaveTransferTemplateDto, SavedTransferTemplate>();
            CreateMap<SavePayeeDto, SavedPayee>();
        }
    }
}