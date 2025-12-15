using FinTech.Core.Domain.Enums;
using DomainAccountType = FinTech.Core.Domain.Enums.AccountType;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Account
{
    public class AccountBalanceDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public AccountClassification Classification { get; set; }
        public DomainAccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
        public string CBNReportingCode { get; set; }
        public string NDICReportingCode { get; set; }
        public string IFRSCategory { get; set; }
        
        // FinTech Best Practice: Add AsOfDate for balance reporting
        public System.DateTime AsOfDate { get; set; }
    }
}
