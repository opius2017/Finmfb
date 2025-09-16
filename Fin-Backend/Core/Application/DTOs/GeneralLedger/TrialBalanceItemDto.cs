using FinTech.Domain.Entities.Accounting;

namespace FinTech.Core.Application.DTOs.GeneralLedger
{
    public class TrialBalanceItemDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public AccountClassification Classification { get; set; }
        public AccountType AccountType { get; set; }
    }
}