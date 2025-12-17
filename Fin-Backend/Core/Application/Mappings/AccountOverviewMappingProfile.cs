using System;
using AutoMapper;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Application.Services;

namespace FinTech.Core.Application.Mappings
{
    public class AccountOverviewMappingProfile : Profile
    {
        public AccountOverviewMappingProfile()
        {
            // Account Mappings
            CreateMap<DepositAccount, AccountDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.Product.ProductType))
                .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.CurrentBalance - src.LienAmount));

            CreateMap<DepositAccount, AccountDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.Product.ProductType))
                .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
                .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.CurrentBalance - src.LienAmount))
                .ForMember(dest => dest.InterestRate, opt => opt.MapFrom(src => src.Product.InterestRate))
                .ForMember(dest => dest.InterestPaymentFrequency, opt => opt.MapFrom(src => src.Product.InterestPostingFrequency))
                .ForMember(dest => dest.MinimumBalance, opt => opt.MapFrom(src => src.Product.MinimumBalance))
                .ForMember(dest => dest.MonthlyServiceCharge, opt => opt.MapFrom(src => src.Product.MaintenanceFee));

            // Transaction Mappings
            CreateMap<DepositTransaction, TransactionDto>()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionReference))
                .ForMember(dest => dest.ReferenceNumber, opt => opt.MapFrom(src => src.ExternalReference))
                .ForMember(dest => dest.Channel, opt => opt.MapFrom(src => src.Channel ?? "Branch"))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => DetermineCategory(src)));

            // Account Summary Mapping
            CreateMap<AccountSummary, AccountSummaryDto>();

            // Account Activity Mapping
            CreateMap<AccountActivity, AccountActivityDto>();
        }

        private string DetermineCategory(DepositTransaction transaction)
        {
            // This is a simplified version of transaction categorization
            // In a real system, this would be more sophisticated and might involve machine learning
            var description = (transaction.Description ?? "").ToLower();
            
            if (description.Contains("salary") || description.Contains("payroll"))
                return "Income";
            
            if (description.Contains("transfer") || description.Contains("xfer"))
                return "Transfer";
            
            if (description.Contains("withdrawal") || description.Contains("atm"))
                return "Withdrawal";
            
            if (description.Contains("deposit") || description.Contains("credit"))
                return "Deposit";
            
            if (description.Contains("fee") || description.Contains("charge"))
                return "Fee";
            
            if (description.Contains("loan") || description.Contains("repayment"))
                return "Loan";
            
            if (description.Contains("bill") || description.Contains("payment"))
                return "Bill Payment";
            
            return "Other";
        }
    }
}
