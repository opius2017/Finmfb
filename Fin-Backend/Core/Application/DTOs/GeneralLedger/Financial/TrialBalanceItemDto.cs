using FinTech.Core.Domain.Enums;
using DomainAccountType = FinTech.Core.Domain.Enums.AccountType;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Financial
{
    public class TrialBalanceItemDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public AccountClassification Classification { get; set; }
        public DomainAccountType AccountType { get; set; }
    }
}
