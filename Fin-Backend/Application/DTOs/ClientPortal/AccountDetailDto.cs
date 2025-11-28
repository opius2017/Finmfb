using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class AccountDetailDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProductDescription { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal HoldAmount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Status { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal InterestRate { get; set; }
        public string InterestPaymentFrequency { get; set; }
        public decimal MinimumBalance { get; set; }
        public decimal MonthlyServiceCharge { get; set; }
        public decimal OverdraftLimit { get; set; }
        public bool IsJointAccount { get; set; }
        public string Branch { get; set; }
        public string RelationshipManager { get; set; }
    }
}
