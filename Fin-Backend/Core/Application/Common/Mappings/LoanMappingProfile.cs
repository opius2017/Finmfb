using System;
using System.Collections.Generic;
using AutoMapper;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.DTOs.ClientPortal;

namespace FinTech.Core.Application.Common.Mappings
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            // Loan Account mappings
            CreateMap<LoanAccount, LoanAccountSummaryDto>()
                .ForMember(dest => dest.LoanProductName, opt => opt.MapFrom(src => src.Loan != null && src.Loan.LoanProduct != null ? src.Loan.LoanProduct.ProductName : "Unknown"));

            // Loan Application mappings
            CreateMap<LoanApplicationRequest, LoanApplicationSummaryDto>()
                .ForMember(dest => dest.LoanProductName, opt => opt.MapFrom(src => src.LoanProduct != null ? src.LoanProduct.ProductName : "Unknown"));

            // Loan Repayment Schedule mappings
            CreateMap<LoanRepaymentSchedule, LoanRepaymentScheduleDto>();

            // Loan Transaction mappings
            CreateMap<LoanTransaction, LoanTransactionDto>();

            // DTO to Entity mappings
            CreateMap<LoanApplicationDto, LoanApplicationRequest>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.ApplicationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.LoanProduct, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
