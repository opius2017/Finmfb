using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Account
{
    public class AccountBalanceDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public AccountClassification Classification { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Status { get; set; }
        public string Currency { get; set; }
        public string CurrencyCode { get; set; }
        public string CBNReportingCode { get; set; }
        public string NDICReportingCode { get; set; }
        public string IFRSCategory { get; set; }
    }
}
