using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.GeneralLedger;
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
}