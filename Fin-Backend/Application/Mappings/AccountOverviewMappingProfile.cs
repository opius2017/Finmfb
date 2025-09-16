using System;
using AutoMapper;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Domain.Entities.Deposits;
using FinTech.Application.Services;

namespace FinTech.Application.Mappings
{
    public class AccountOverviewMappingProfile : Profile
    {
        public AccountOverviewMappingProfile()
        {
            // Account Mappings
            CreateMap<DepositAccount, AccountDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.DepositProduct.ProductName))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.DepositProduct.ProductType))
                .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.CurrentBalance - src.HoldAmount));

            CreateMap<DepositAccount, AccountDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.DepositProduct.ProductName))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.DepositProduct.ProductType))
                .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.DepositProduct.Description))
                .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.CurrentBalance - src.HoldAmount))
                .ForMember(dest => dest.InterestRate, opt => opt.MapFrom(src => src.DepositProduct.InterestRate))
                .ForMember(dest => dest.InterestPaymentFrequency, opt => opt.MapFrom(src => src.DepositProduct.InterestPaymentFrequency))
                .ForMember(dest => dest.MinimumBalance, opt => opt.MapFrom(src => src.DepositProduct.MinimumBalance))
                .ForMember(dest => dest.MonthlyServiceCharge, opt => opt.MapFrom(src => src.DepositProduct.MonthlyServiceCharge));

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