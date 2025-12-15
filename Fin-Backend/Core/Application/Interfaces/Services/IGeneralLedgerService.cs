using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.ValueObjects;
using FinTech.Core.Application.DTOs.GeneralLedger.Account;
using FinTech.Core.Application.DTOs.GeneralLedger.Journal;
using FinTech.Core.Application.DTOs.GeneralLedger.Financial;
using FinTech.Core.Application.DTOs.GeneralLedger.Period;
using FinTech.Core.Application.DTOs.GeneralLedger.Regulatory;

namespace FinTech.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Interface for General Ledger operations supporting double-entry accounting,
    /// financial reporting, and regulatory compliance for Nigerian Microfinance Banks.
    /// </summary>
    public interface IGeneralLedgerService
    {
        // Account Management
        Task<bool> UpdateAccountBalanceAsync(string accountId, Money amount, bool isDebit);
        Task<Money> GetAccountBalanceAsync(string accountId);
        Task<AccountBalanceDto> GetAccountWithBalanceAsync(string accountId);
        Task<IEnumerable<AccountBalanceDto>> GetAccountBalancesAsync(IEnumerable<string> accountIds, DateTime asOfDate, System.Threading.CancellationToken cancellationToken = default);
        Task<IEnumerable<JournalEntryLineDto>> GetAccountLedgerEntriesAsync(string accountId, DateTime fromDate, DateTime toDate);
        
        // Added for Period Closing and Fixed Assets
        Task<Money> GetAccountBalanceAsync(string accountId, DateTime fromDate, DateTime toDate);
        Task<string> GenerateJournalNumberAsync();
        Task<bool> CreateJournalEntryAsync(FinTech.Core.Domain.Entities.Accounting.JournalEntry journalEntry);
        Task UpdateAccountBalancesAsync(FinTech.Core.Domain.Entities.Accounting.JournalEntry journalEntry, System.Threading.CancellationToken cancellationToken = default);
        
        // Financial Period Management
        Task<bool> IsAccountingPeriodOpenAsync(DateTime transactionDate);
        Task<bool> ClosePeriodAsync(string fiscalPeriodId, string userId);
        Task<bool> ReopenPeriodAsync(string fiscalPeriodId, string userId);
        Task<bool> CreateNewFiscalYearAsync(FiscalYearDto fiscalYear, string userId);
        Task<bool> CloseFiscalYearAsync(string fiscalYearId, string userId);
        Task<FinancialPeriodDto> GetCurrentPeriodAsync();
        Task<FiscalYearDto> GetCurrentFiscalYearAsync();
        
        // Financial Reporting
        Task<TrialBalanceDto> GenerateTrialBalanceAsync(DateTime asOfDate, string? fiscalPeriodId = null);
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
