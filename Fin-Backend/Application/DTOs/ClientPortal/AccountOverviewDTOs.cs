using System;
using System.Collections.Generic;
using FinTech.Domain.Entities.Deposits;

namespace FinTech.Application.DTOs.ClientPortal
{
    // Account Overview DTOs
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Status { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }

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

    public class AccountBalanceDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime AsOfDate { get; set; }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public decimal RunningBalance { get; set; }
        public string ReferenceNumber { get; set; }
        public string Channel { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BeneficiaryBank { get; set; }
    }

    public class AccountSummaryDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalCurrent { get; set; }
        public decimal TotalFixed { get; set; }
        public int AccountCount { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class AccountActivityDto
    {
        public DateTime Date { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TransactionSearchDto
    {
        public string AccountNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string TransactionType { get; set; } // Credit, Debit
        public string SearchTerm { get; set; } // Search in description, reference, beneficiary
        public string Category { get; set; }
        public string Channel { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TransactionFilterDto
    {
        public string AccountNumber { get; set; }
        public List<string> TransactionTypes { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Channels { get; set; } = new List<string>();
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

    public class TransactionExportDto
    {
        public string AccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "csv"; // csv, excel, pdf
        public bool IncludeRunningBalance { get; set; } = true;
    }
}