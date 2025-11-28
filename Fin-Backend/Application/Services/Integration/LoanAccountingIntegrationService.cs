using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// using FinTech.Core.Application.Services.Accounting; (removed to avoid ambiguous IFinancialPeriodService)
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Application.Interfaces.Accounting;
using FinTech.Core.Application.Interfaces.Services;

namespace FinTech.Core.Application.Services.Integration
{
    // Interface declared in Interfaces/Services/Integration - implementation class below

    public class LoanAccountingIntegrationService : ILoanAccountingIntegrationService
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly FinTech.Core.Application.Interfaces.Accounting.IFinancialPeriodService _financialPeriodService;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;

        public LoanAccountingIntegrationService(
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService,
            FinTech.Core.Application.Interfaces.Accounting.IFinancialPeriodService financialPeriodService,
            IChartOfAccountRepository chartOfAccountRepository)
        {
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _chartOfAccountService = chartOfAccountService ?? throw new ArgumentNullException(nameof(chartOfAccountService));
            _financialPeriodService = financialPeriodService ?? throw new ArgumentNullException(nameof(financialPeriodService));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
        }

        public async Task<string> ProcessLoanDisbursementAsync(
            LoanAccount loan, 
            LoanTransaction disbursementTransaction, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));
                
            if (disbursementTransaction == null)
                throw new ArgumentNullException(nameof(disbursementTransaction));

            if (disbursementTransaction.Amount <= 0)
                throw new ArgumentException("Disbursement amount must be greater than zero", nameof(disbursementTransaction));

            // Get the relevant GL accounts
            var loansReceivableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1200", cancellationToken);  // Loans receivable
            var cashAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Cash account

            if (loansReceivableAccount == null || cashAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Loan disbursement for account {loan.LoanAccountNumber} - {disbursementTransaction.TransactionReference}",
                EntryDate = disbursementTransaction.TransactionDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "LoanIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Loans Receivable
                    new JournalEntryLine
                    {
                        AccountId = loansReceivableAccount.Id,
                        Description = $"Loan disbursement for account {loan.LoanAccountNumber}",
                        DebitAmount = disbursementTransaction.Amount,
                        CreditAmount = 0,
                        CreatedBy = "LoanIntegration"
                    },
                    // Credit Cash
                    new JournalEntryLine
                    {
                        AccountId = cashAccount.Id,
                        Description = $"Loan disbursement for account {loan.LoanAccountNumber}",
                        DebitAmount = 0,
                        CreditAmount = disbursementTransaction.Amount,
                        CreatedBy = "LoanIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);

            return journalEntryId;
        }

        // Adapter stubs to satisfy ILoanAccountingIntegrationService
        public Task ProcessLoanDisbursementAsync(int loanId, decimal amount, string reference, string description)
        {
            throw new System.NotImplementedException();
        }

        public Task ProcessLoanRepaymentAsync(int loanId, decimal principalAmount, decimal interestAmount, string reference, string description)
        {
            throw new System.NotImplementedException();
        }

        public Task ProcessLoanWriteOffAsync(int loanId, decimal amount, string reference, string description)
        {
            throw new System.NotImplementedException();
        }

        public Task ProcessLoanInterestAccrualAsync(int loanId, decimal amount, string reference, string description)
        {
            throw new System.NotImplementedException();
        }

        public Task ProcessLoanFeeChargeAsync(int loanId, decimal amount, string feeType, string reference, string description)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> ProcessLoanRepaymentAsync(
            LoanAccount loan, 
            LoanTransaction repaymentTransaction, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));
                
            if (repaymentTransaction == null)
                throw new ArgumentNullException(nameof(repaymentTransaction));

            if (repaymentTransaction.Amount <= 0)
                throw new ArgumentException("Repayment amount must be greater than zero", nameof(repaymentTransaction));

            // Get the relevant GL accounts
            var loansReceivableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1200", cancellationToken);  // Loans receivable
            var interestIncomeAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("4010", cancellationToken);  // Interest income
            var cashAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Cash account

            if (loansReceivableAccount == null || interestIncomeAccount == null || cashAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Split the repayment between principal and interest
            decimal principalAmount = repaymentTransaction.PrincipalAmount;
            decimal interestAmount = repaymentTransaction.InterestAmount;
            decimal totalAmount = principalAmount + interestAmount;

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Loan repayment for account {loan.LoanAccountNumber} - {repaymentTransaction.TransactionReference}",
                EntryDate = repaymentTransaction.TransactionDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "LoanIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Cash
                    new JournalEntryLine
                    {
                        AccountId = cashAccount.Id,
                        Description = $"Loan repayment for account {loan.LoanAccountNumber}",
                        DebitAmount = totalAmount,
                        CreditAmount = 0,
                        CreatedBy = "LoanIntegration"
                    }
                }
            };

            // Add credit entries based on principal and interest components
            if (principalAmount > 0)
            {
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = loansReceivableAccount.Id,
                    Description = $"Principal repayment for account {loan.LoanAccountNumber}",
                    DebitAmount = 0,
                    CreditAmount = principalAmount,
                    CreatedBy = "LoanIntegration"
                });
            }

            if (interestAmount > 0)
            {
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = interestIncomeAccount.Id,
                    Description = $"Interest payment for account {loan.LoanAccountNumber}",
                    DebitAmount = 0,
                    CreditAmount = interestAmount,
                    CreatedBy = "LoanIntegration"
                });
            }

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessInterestAccrualAsync(
            IEnumerable<LoanAccount> loans, 
            DateTime accrualDate,
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (loans == null || !loans.Any())
                throw new ArgumentException("Loans cannot be null or empty", nameof(loans));

            // Get the relevant GL accounts
            var interestReceivableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1210", cancellationToken);  // Interest receivable
            var interestIncomeAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("4010", cancellationToken);  // Interest income

            if (interestReceivableAccount == null || interestIncomeAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Calculate total accrued interest
            decimal totalInterest = 0;
            
            // In a real implementation, this would calculate interest based on loan balances and rates
            // For simplicity, we're assuming the interest amount is already calculated
            foreach (var loan in loans)
            {
                decimal loanInterest = loan.InterestRate * loan.PrincipalBalance / 365 * 30; // Monthly accrual
                totalInterest += loanInterest;
            }

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Interest accrual for loan accounts - {accrualDate:yyyy-MM-dd}",
                EntryDate = accrualDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "LoanIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Interest Receivable
                    new JournalEntryLine
                    {
                        AccountId = interestReceivableAccount.Id,
                        Description = $"Interest accrual for loan accounts - {accrualDate:yyyy-MM-dd}",
                        DebitAmount = totalInterest,
                        CreditAmount = 0,
                        CreatedBy = "LoanIntegration"
                    },
                    // Credit Interest Income
                    new JournalEntryLine
                    {
                        AccountId = interestIncomeAccount.Id,
                        Description = $"Interest accrual for loan accounts - {accrualDate:yyyy-MM-dd}",
                        DebitAmount = 0,
                        CreditAmount = totalInterest,
                        CreatedBy = "LoanIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessLoanWriteOffAsync(
            LoanAccount loan, 
            decimal writeOffAmount, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));
                
            if (writeOffAmount <= 0)
                throw new ArgumentException("Write-off amount must be greater than zero", nameof(writeOffAmount));

            // Get the relevant GL accounts
            var loansReceivableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1200", cancellationToken);  // Loans receivable
            var allowanceForLoanLossesAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1290", cancellationToken);  // Allowance for loan losses

            if (loansReceivableAccount == null || allowanceForLoanLossesAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Loan write-off for account {loan.LoanAccountNumber}",
                EntryDate = DateTime.UtcNow.Date,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "LoanIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Allowance for Loan Losses
                    new JournalEntryLine
                    {
                        AccountId = allowanceForLoanLossesAccount.Id,
                        Description = $"Loan write-off for account {loan.LoanAccountNumber}",
                        DebitAmount = writeOffAmount,
                        CreditAmount = 0,
                        CreatedBy = "LoanIntegration"
                    },
                    // Credit Loans Receivable
                    new JournalEntryLine
                    {
                        AccountId = loansReceivableAccount.Id,
                        Description = $"Loan write-off for account {loan.LoanAccountNumber}",
                        DebitAmount = 0,
                        CreditAmount = writeOffAmount,
                        CreatedBy = "LoanIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "LoanIntegration", cancellationToken);

            return journalEntryId;
        }
    }
}
