using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.GeneralLedger;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Domain.Enums;
using FinTech.Infrastructure.Data;

namespace FinTech.Core.Application.Services;

public interface IGeneralLedgerService
{
    Task<bool> PostJournalEntryAsync(CreateJournalEntryRequest request);
    Task<bool> PostLoanDisbursementAsync(LoanAccount loanAccount, decimal amount, string postedBy);
    Task<bool> PostLoanRepaymentAsync(LoanAccount loanAccount, decimal principal, decimal interest, string postedBy);
    Task<bool> PostDepositTransactionAsync(DepositAccount account, decimal amount, TransactionType type, string postedBy);
    Task<bool> PostInterestAccrualAsync(Guid accountId, decimal interestAmount, string postedBy);
    Task<List<GeneralLedgerEntry>> GetTrialBalanceAsync(Guid tenantId, DateTime asOfDate);
    Task<bool> CloseFinancialPeriodAsync(Guid tenantId, DateTime periodEnd, string closedBy);
}

public class GeneralLedgerService : IGeneralLedgerService
{
    private readonly ApplicationDbContext _context;

    public GeneralLedgerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> PostJournalEntryAsync(CreateJournalEntryRequest request)
    {
        // Validate that debits equal credits
        var totalDebits = request.Details.Where(d => d.EntryType == EntryType.Debit).Sum(d => d.Amount);
        var totalCredits = request.Details.Where(d => d.EntryType == EntryType.Credit).Sum(d => d.Amount);

        if (totalDebits != totalCredits)
            throw new InvalidOperationException("Debits must equal credits");

        var journalEntry = new JournalEntry
        {
            JournalNumber = await GenerateJournalNumberAsync(),
            TransactionDate = request.TransactionDate,
            Description = request.Description,
            TotalDebit = totalDebits,
            TotalCredit = totalCredits,
            Status = JournalStatus.Draft,
            PreparedBy = request.PreparedBy,
            PreparedDate = DateTime.UtcNow,
            TenantId = request.TenantId
        };

        _context.JournalEntries.Add(journalEntry);
        await _context.SaveChangesAsync();

        // Add journal entry details
        foreach (var detail in request.Details)
        {
            var journalDetail = new JournalEntryDetail
            {
                JournalEntryId = journalEntry.Id,
                AccountId = detail.AccountId,
                EntryType = detail.EntryType,
                Amount = detail.Amount,
                Description = detail.Description,
                Reference = detail.Reference,
                TenantId = request.TenantId
            };

            _context.JournalEntryDetails.Add(journalDetail);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PostLoanDisbursementAsync(LoanAccount loanAccount, decimal amount, string postedBy)
    {
        var transactionRef = $"LOAN-DISB-{loanAccount.AccountNumber}";

        // Get GL accounts for loan disbursement
        var loanReceivableAccount = await GetAccountByCodeAsync("1200", loanAccount.TenantId); // Loans Receivable
        var cashAccount = await GetAccountByCodeAsync("1000", loanAccount.TenantId); // Cash

        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit: Loans Receivable
            new GeneralLedgerEntry
            {
                AccountId = loanReceivableAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Debit,
                Amount = amount,
                BaseAmount = amount,
                Description = $"Loan disbursement - {loanAccount.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = loanAccount.TenantId
            },
            // Credit: Cash
            new GeneralLedgerEntry
            {
                AccountId = cashAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Credit,
                Amount = amount,
                BaseAmount = amount,
                Description = $"Loan disbursement - {loanAccount.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = loanAccount.TenantId
            }
        };

        _context.GeneralLedgerEntries.AddRange(glEntries);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PostLoanRepaymentAsync(LoanAccount loanAccount, decimal principal, decimal interest, string postedBy)
    {
        var transactionRef = $"LOAN-REPAY-{loanAccount.AccountNumber}";

        // Get GL accounts
        var cashAccount = await GetAccountByCodeAsync("1000", loanAccount.TenantId);
        var loanReceivableAccount = await GetAccountByCodeAsync("1200", loanAccount.TenantId);
        var interestIncomeAccount = await GetAccountByCodeAsync("4100", loanAccount.TenantId);

        var glEntries = new List<GeneralLedgerEntry>
        {
            // Debit: Cash (Total payment)
            new GeneralLedgerEntry
            {
                AccountId = cashAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Debit,
                Amount = principal + interest,
                BaseAmount = principal + interest,
                Description = $"Loan repayment - {loanAccount.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = loanAccount.TenantId
            }
        };

        // Credit: Loans Receivable (Principal portion)
        if (principal > 0)
        {
            glEntries.Add(new GeneralLedgerEntry
            {
                AccountId = loanReceivableAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Credit,
                Amount = principal,
                BaseAmount = principal,
                Description = $"Principal repayment - {loanAccount.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = loanAccount.TenantId
            });
        }

        // Credit: Interest Income (Interest portion)
        if (interest > 0)
        {
            glEntries.Add(new GeneralLedgerEntry
            {
                AccountId = interestIncomeAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Credit,
                Amount = interest,
                BaseAmount = interest,
                Description = $"Interest income - {loanAccount.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = loanAccount.TenantId
            });
        }

        _context.GeneralLedgerEntries.AddRange(glEntries);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PostDepositTransactionAsync(DepositAccount account, decimal amount, TransactionType type, string postedBy)
    {
        var transactionRef = $"DEP-{type}-{account.AccountNumber}";

        // Get GL accounts
        var cashAccount = await GetAccountByCodeAsync("1000", account.TenantId);
        var customerDepositAccount = await GetAccountByCodeAsync("2100", account.TenantId);

        GeneralLedgerEntry debitEntry, creditEntry;

        if (type == TransactionType.Deposit)
        {
            // Debit: Cash, Credit: Customer Deposits
            debitEntry = new GeneralLedgerEntry
            {
                AccountId = cashAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Debit,
                Amount = amount,
                BaseAmount = amount,
                Description = $"Customer deposit - {account.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = account.TenantId
            };

            creditEntry = new GeneralLedgerEntry
            {
                AccountId = customerDepositAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Credit,
                Amount = amount,
                BaseAmount = amount,
                Description = $"Customer deposit - {account.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = account.TenantId
            };
        }
        else // Withdrawal
        {
            // Debit: Customer Deposits, Credit: Cash
            debitEntry = new GeneralLedgerEntry
            {
                AccountId = customerDepositAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Debit,
                Amount = amount,
                BaseAmount = amount,
                Description = $"Customer withdrawal - {account.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = account.TenantId
            };

            creditEntry = new GeneralLedgerEntry
            {
                AccountId = cashAccount.Id,
                TransactionReference = transactionRef,
                TransactionDate = DateTime.UtcNow,
                EntryType = EntryType.Credit,
                Amount = amount,
                BaseAmount = amount,
                Description = $"Customer withdrawal - {account.AccountNumber}",
                Status = TransactionStatus.Posted,
                PostedBy = postedBy,
                PostedDate = DateTime.UtcNow,
                TenantId = account.TenantId
            };
        }

        _context.GeneralLedgerEntries.AddRange(new[] { debitEntry, creditEntry });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PostInterestAccrualAsync(Guid accountId, decimal interestAmount, string postedBy)
    {
        // Implementation for interest accrual posting
        // This would be called by a background service for daily interest calculations
        return true;
    }

    public async Task<List<GeneralLedgerEntry>> GetTrialBalanceAsync(Guid tenantId, DateTime asOfDate)
    {
        return await _context.GeneralLedgerEntries
            .Include(g => g.Account)
            .Where(g => g.TenantId == tenantId && 
                       g.TransactionDate <= asOfDate && 
                       g.Status == TransactionStatus.Posted)
            .OrderBy(g => g.Account.AccountCode)
            .ToListAsync();
    }

    public async Task<bool> CloseFinancialPeriodAsync(Guid tenantId, DateTime periodEnd, string closedBy)
    {
        // Implementation for period closing
        // This would involve closing revenue and expense accounts to retained earnings
        return true;
    }

    private async Task<ChartOfAccounts> GetAccountByCodeAsync(string accountCode, Guid tenantId)
    {
        var account = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(a => a.AccountCode == accountCode && a.TenantId == tenantId);
        
        if (account == null)
        {
            throw new InvalidOperationException($"Account with code {accountCode} not found");
        }

        return account;
    }

    private async Task<string> GenerateJournalNumberAsync()
    {
        var lastJournal = await _context.JournalEntries
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastJournal != null && lastJournal.JournalNumber.StartsWith("JE"))
        {
            if (int.TryParse(lastJournal.JournalNumber.Substring(2), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"JE{nextNumber:D6}";
    }
}

public class CreateJournalEntryRequest
{
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PreparedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public List<JournalEntryDetailRequest> Details { get; set; } = new();
}

public class JournalEntryDetailRequest
{
    public Guid AccountId { get; set; }
    public EntryType EntryType { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
}
