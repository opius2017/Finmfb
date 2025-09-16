using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.Common;

namespace FinTech.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Interface for General Ledger operations supporting double-entry accounting,
    /// financial reporting, and regulatory compliance for Nigerian Microfinance Banks
    /// </summary>
    public interface IGeneralLedgerService
    {
        // Journal Entry Management
        Task<string> CreateJournalEntryAsync(JournalEntryDto journalEntry, string userId);
        Task<bool> SubmitJournalEntryForApprovalAsync(string journalEntryId, string userId);
        Task<bool> ApproveJournalEntryAsync(string journalEntryId, string userId);
        Task<bool> RejectJournalEntryAsync(string journalEntryId, string userId, string reason);
        Task<bool> PostJournalEntryAsync(string journalEntryId, string userId);
        Task<string> ReverseJournalEntryAsync(string journalEntryId, string userId, string reason);
        Task<JournalEntryDto> GetJournalEntryAsync(string journalEntryId);
        Task<IEnumerable<JournalEntryDto>> GetJournalEntriesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<JournalEntryDto>> GetPendingJournalEntriesAsync();
        Task<bool> ValidateJournalEntryAsync(JournalEntryDto journalEntry);
        
        // Account Management
        Task<bool> UpdateAccountBalanceAsync(string accountId, Money amount, bool isDebit);
        Task<Money> GetAccountBalanceAsync(string accountId);
        Task<AccountBalanceDto> GetAccountWithBalanceAsync(string accountId);
        Task<IEnumerable<AccountBalanceDto>> GetAccountBalancesAsync(IEnumerable<string> accountIds = null);
        Task<IEnumerable<JournalEntryLineDto>> GetAccountLedgerEntriesAsync(string accountId, DateTime fromDate, DateTime toDate);
        
        // Financial Period Management
        Task<bool> IsAccountingPeriodOpenAsync(DateTime transactionDate);
        Task<bool> ClosePeriodAsync(string fiscalPeriodId, string userId);
        Task<bool> ReopenPeriodAsync(string fiscalPeriodId, string userId);
        Task<bool> CreateNewFiscalYearAsync(FiscalYearDto fiscalYear, string userId);
        Task<bool> CloseFiscalYearAsync(string fiscalYearId, string userId);
        Task<FinancialPeriodDto> GetCurrentPeriodAsync();
        Task<FiscalYearDto> GetCurrentFiscalYearAsync();
        
        // Financial Reporting
        Task<TrialBalanceDto> GenerateTrialBalanceAsync(DateTime asOfDate, string fiscalPeriodId = null);
        Task<BalanceSheetDto> GenerateBalanceSheetAsync(DateTime asOfDate);
        Task<IncomeStatementDto> GenerateIncomeStatementAsync(DateTime fromDate, DateTime toDate);
        Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(DateTime fromDate, DateTime toDate);
        
        // Regulatory Reporting
        Task<CBNReturnsDto> GenerateCBNReturnsAsync(DateTime asOfDate, string returnType);
        Task<NDICReturnsDto> GenerateNDICReturnsAsync(DateTime asOfDate);
        Task<IFRSDisclosureDto> GenerateIFRSDisclosuresAsync(DateTime asOfDate, string disclosureType);
        
        // Business Module Integration
        Task<bool> PostLoanDisbursementAsync(string loanAccountId, Money amount, string userId);
        Task<bool> PostLoanRepaymentAsync(string loanAccountId, Money principal, Money interest, string userId);
        Task<bool> PostDepositTransactionAsync(string depositAccountId, Money amount, bool isDeposit, string userId);
        Task<bool> PostInterestAccrualAsync(string accountId, Money interestAmount, string userId);
        Task<bool> PostFeeIncomeAsync(string accountId, Money feeAmount, string feeType, string userId);
    }

    #region DTOs

    public class JournalEntryDto
    {
        public string Id { get; set; }
        public string JournalEntryNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int EntryType { get; set; }
        public string Reference { get; set; }
        public string SourceDocument { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string PostedBy { get; set; }
        public DateTime? PostedDate { get; set; }
        public string FinancialPeriodId { get; set; }
        public string ModuleSource { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; }
        public string Notes { get; set; }
        public IEnumerable<JournalEntryLineDto> JournalEntryLines { get; set; }
    }

    public class JournalEntryLineDto
    {
        public string Id { get; set; }
        public string JournalEntryId { get; set; }
        public string JournalEntryNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsDebit { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
    }

    public class AccountBalanceDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public AccountClassification Classification { get; set; }
        public AccountType AccountType { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
        public string CBNReportingCode { get; set; }
        public string NDICReportingCode { get; set; }
        public string IFRSCategory { get; set; }
    }

    public class TrialBalanceDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<TrialBalanceItemDto> AccountBalances { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
    }

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

    public class BalanceSheetDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<FinancialStatementItemDto> Assets { get; set; }
        public decimal TotalAssets { get; set; }
        public IEnumerable<FinancialStatementItemDto> Liabilities { get; set; }
        public decimal TotalLiabilities { get; set; }
        public IEnumerable<FinancialStatementItemDto> Equity { get; set; }
        public decimal TotalEquity { get; set; }
        public bool IsBalanced => Math.Abs(TotalAssets - (TotalLiabilities + TotalEquity)) < 0.01m;
    }

    public class IncomeStatementDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<FinancialStatementItemDto> Income { get; set; }
        public decimal TotalIncome { get; set; }
        public IEnumerable<FinancialStatementItemDto> Expenses { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome => TotalIncome - TotalExpenses;
    }

    public class CashFlowStatementDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<FinancialStatementItemDto> OperatingActivities { get; set; }
        public decimal NetOperatingCashFlow { get; set; }
        public IEnumerable<FinancialStatementItemDto> InvestingActivities { get; set; }
        public decimal NetInvestingCashFlow { get; set; }
        public IEnumerable<FinancialStatementItemDto> FinancingActivities { get; set; }
        public decimal NetFinancingCashFlow { get; set; }
        public decimal NetCashFlow => NetOperatingCashFlow + NetInvestingCashFlow + NetFinancingCashFlow;
        public decimal BeginningCashBalance { get; set; }
        public decimal EndingCashBalance { get; set; }
    }

    public class FinancialStatementItemDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string ParentCategory { get; set; }
        public string SubCategory { get; set; }
        public int SortOrder { get; set; }
        public bool IsBold { get; set; }
        public bool IsSubtotal { get; set; }
        public int IndentLevel { get; set; }
    }

    public class FiscalYearDto
    {
        public string Id { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedBy { get; set; }
        public bool IsCurrentYear { get; set; }
        public IEnumerable<FinancialPeriodDto> Periods { get; set; }
    }

    public class FinancialPeriodDto
    {
        public string Id { get; set; }
        public string PeriodCode { get; set; }
        public string PeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedBy { get; set; }
        public int FiscalYear { get; set; }
        public int FiscalMonth { get; set; }
        public bool IsAdjustmentPeriod { get; set; }
    }

    public class CBNReturnsDto
    {
        public string ReturnType { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IDictionary<string, decimal> Values { get; set; }
        public IEnumerable<CBNReturnItemDto> ReturnItems { get; set; }
    }

    public class CBNReturnItemDto
    {
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Notes { get; set; }
    }

    public class NDICReturnsDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal InsuredDeposits { get; set; }
        public decimal PremiumPayable { get; set; }
        public IEnumerable<NDICReturnItemDto> ReturnItems { get; set; }
    }

    public class NDICReturnItemDto
    {
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
    }

    public class IFRSDisclosureDto
    {
        public string DisclosureType { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IDictionary<string, object> DisclosureData { get; set; }
    }

    #endregion
}