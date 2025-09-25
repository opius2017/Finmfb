using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Core.Application.Services.Accounting
{
    public interface IJournalEntryService
    {
        Task<JournalEntry> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<JournalEntry> GetByJournalNumberAsync(string journalNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByFinancialPeriodAsync(string financialPeriodId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
        Task<string> CreateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);
        Task UpdateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);
        Task<string> SubmitForApprovalAsync(string id, string submittedBy, CancellationToken cancellationToken = default);
        Task<string> ApproveJournalEntryAsync(string id, string approvedBy, CancellationToken cancellationToken = default);
        Task<string> RejectJournalEntryAsync(string id, string rejectedBy, string rejectionReason, CancellationToken cancellationToken = default);
        Task<string> PostJournalEntryAsync(string id, string postedBy, CancellationToken cancellationToken = default);
        Task<string> ReverseJournalEntryAsync(string id, string reversedBy, string reversalReason, CancellationToken cancellationToken = default);
        Task<string> GenerateJournalNumberAsync(JournalEntryType entryType, CancellationToken cancellationToken = default);
    }

    public class JournalEntryService : IJournalEntryService
    {
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly IUnitOfWork _unitOfWork;

        public JournalEntryService(
            IJournalEntryRepository journalEntryRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IGeneralLedgerService generalLedgerService,
            IUnitOfWork unitOfWork)
        {
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _generalLedgerService = generalLedgerService ?? throw new ArgumentNullException(nameof(generalLedgerService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<JournalEntry> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<JournalEntry> GetByJournalNumberAsync(string journalNumber, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetByJournalNumberAsync(journalNumber, cancellationToken);
        }

        public async Task<IReadOnlyList<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetByStatusAsync(status, cancellationToken);
        }

        public async Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        public async Task<IReadOnlyList<JournalEntry>> GetByFinancialPeriodAsync(string financialPeriodId, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetByFinancialPeriodAsync(financialPeriodId, cancellationToken);
        }

        public async Task<IReadOnlyList<JournalEntry>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetByAccountIdAsync(accountId, cancellationToken);
        }

        public async Task<IReadOnlyList<JournalEntry>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GetPendingApprovalsAsync(cancellationToken);
        }

        public async Task<string> CreateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
        {
            // Validate journal entry
            await ValidateJournalEntryAsync(journalEntry, cancellationToken);

            // Set default values if not provided
            if (string.IsNullOrEmpty(journalEntry.Id))
            {
                journalEntry.Id = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(journalEntry.JournalEntryNumber))
            {
                journalEntry.JournalEntryNumber = await GenerateJournalNumberAsync(journalEntry.EntryType, cancellationToken);
            }

            // Set the creation metadata
            journalEntry.CreatedAt = DateTime.UtcNow;
            journalEntry.Status = JournalEntryStatus.Draft;

            // Set line item IDs and link to journal entry
            if (journalEntry.JournalEntryLines != null)
            {
                for (int i = 0; i < journalEntry.JournalEntryLines.Count; i++)
                {
                    var line = journalEntry.JournalEntryLines[i];
                    line.Id = Guid.NewGuid().ToString();
                    line.JournalEntryId = journalEntry.Id;
                    line.LineNumber = i + 1;
                    line.CreatedBy = journalEntry.CreatedBy;
                    line.CreatedAt = journalEntry.CreatedAt;
                }
            }

            // Add to repository
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return journalEntry.Id;
        }

        public async Task UpdateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
        {
            // Get the existing journal entry
            var existingJournalEntry = await _journalEntryRepository.GetByIdAsync(journalEntry.Id, cancellationToken);
            if (existingJournalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {journalEntry.Id} not found");
            }

            // Only allow updates to draft journal entries
            if (existingJournalEntry.Status != JournalEntryStatus.Draft && 
                existingJournalEntry.Status != JournalEntryStatus.Rejected)
            {
                throw new InvalidOperationException($"Cannot update journal entry with status {existingJournalEntry.Status}");
            }

            // Validate journal entry
            await ValidateJournalEntryAsync(journalEntry, cancellationToken);

            // Update the metadata
            journalEntry.LastModifiedAt = DateTime.UtcNow;

            // Manage line items
            if (journalEntry.JournalEntryLines != null)
            {
                for (int i = 0; i < journalEntry.JournalEntryLines.Count; i++)
                {
                    var line = journalEntry.JournalEntryLines[i];
                    line.JournalEntryId = journalEntry.Id;
                    line.LineNumber = i + 1;
                    
                    if (string.IsNullOrEmpty(line.Id))
                    {
                        line.Id = Guid.NewGuid().ToString();
                        line.CreatedBy = journalEntry.LastModifiedBy;
                        line.CreatedAt = journalEntry.LastModifiedAt.Value;
                    }
                    else
                    {
                        line.LastModifiedBy = journalEntry.LastModifiedBy;
                        line.LastModifiedAt = journalEntry.LastModifiedAt;
                    }
                }
            }

            // Update in repository
            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> SubmitForApprovalAsync(string id, string submittedBy, CancellationToken cancellationToken = default)
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (journalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {id} not found");
            }

            if (journalEntry.Status != JournalEntryStatus.Draft && journalEntry.Status != JournalEntryStatus.Rejected)
            {
                throw new InvalidOperationException($"Cannot submit journal entry with status {journalEntry.Status} for approval");
            }

            // Validate the journal entry
            await ValidateJournalEntryAsync(journalEntry, cancellationToken);

            // Update status
            journalEntry.Status = JournalEntryStatus.Pending;
            journalEntry.LastModifiedBy = submittedBy;
            journalEntry.LastModifiedAt = DateTime.UtcNow;

            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return journalEntry.Id;
        }

        public async Task<string> ApproveJournalEntryAsync(string id, string approvedBy, CancellationToken cancellationToken = default)
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (journalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {id} not found");
            }

            if (journalEntry.Status != JournalEntryStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot approve journal entry with status {journalEntry.Status}");
            }

            // Update status
            journalEntry.Status = JournalEntryStatus.Approved;
            journalEntry.ApprovedBy = approvedBy;
            journalEntry.ApprovedAt = DateTime.UtcNow;

            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return journalEntry.Id;
        }

        public async Task<string> RejectJournalEntryAsync(string id, string rejectedBy, string rejectionReason, CancellationToken cancellationToken = default)
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (journalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {id} not found");
            }

            if (journalEntry.Status != JournalEntryStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot reject journal entry with status {journalEntry.Status}");
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                throw new ArgumentException("Rejection reason is required");
            }

            // Update status
            journalEntry.Status = JournalEntryStatus.Rejected;
            journalEntry.RejectedBy = rejectedBy;
            journalEntry.RejectedAt = DateTime.UtcNow;
            journalEntry.RejectionReason = rejectionReason;

            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return journalEntry.Id;
        }

        public async Task<string> PostJournalEntryAsync(string id, string postedBy, CancellationToken cancellationToken = default)
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (journalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {id} not found");
            }

            if (journalEntry.Status != JournalEntryStatus.Approved)
            {
                throw new InvalidOperationException($"Cannot post journal entry with status {journalEntry.Status}");
            }

            // Verify that the financial period is open
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(journalEntry.FinancialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {journalEntry.FinancialPeriodId} not found");
            }

            if (financialPeriod.Status != FinancialPeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot post to a financial period with status {financialPeriod.Status}");
            }

            // Update status
            journalEntry.Status = JournalEntryStatus.Posted;
            journalEntry.PostedBy = postedBy;
            journalEntry.PostedAt = DateTime.UtcNow;

            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            
            // Update account balances using the GeneralLedgerService
            await _generalLedgerService.UpdateAccountBalancesAsync(journalEntry, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return journalEntry.Id;
        }

        public async Task<string> ReverseJournalEntryAsync(string id, string reversedBy, string reversalReason, CancellationToken cancellationToken = default)
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (journalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {id} not found");
            }

            if (journalEntry.Status != JournalEntryStatus.Posted)
            {
                throw new InvalidOperationException($"Cannot reverse journal entry with status {journalEntry.Status}");
            }

            if (string.IsNullOrWhiteSpace(reversalReason))
            {
                throw new ArgumentException("Reversal reason is required");
            }

            // Create a reversal journal entry
            var reversalEntry = new JournalEntry
            {
                Id = Guid.NewGuid().ToString(),
                Description = $"Reversal of {journalEntry.JournalEntryNumber}: {journalEntry.Description}",
                EntryDate = DateTime.UtcNow.Date,
                EntryType = JournalEntryType.Reversal,
                Status = JournalEntryStatus.Draft,
                FinancialPeriodId = journalEntry.FinancialPeriodId,
                CreatedBy = reversedBy,
                CreatedAt = DateTime.UtcNow,
                IsSystemGenerated = true,
                OriginalJournalEntryId = journalEntry.Id
            };

            // Generate journal entry number
            reversalEntry.JournalEntryNumber = await GenerateJournalNumberAsync(JournalEntryType.Reversal, cancellationToken);

            // Create reversed line items (with debits and credits swapped)
            reversalEntry.JournalEntryLines = new List<JournalEntryLine>();
            foreach (var line in journalEntry.JournalEntryLines)
            {
                reversalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    Id = Guid.NewGuid().ToString(),
                    JournalEntryId = reversalEntry.Id,
                    AccountId = line.AccountId,
                    Description = $"Reversal of {line.Description}",
                    DebitAmount = line.CreditAmount,
                    CreditAmount = line.DebitAmount,
                    CreatedBy = reversedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Add the reversal entry
            await _journalEntryRepository.AddAsync(reversalEntry, cancellationToken);

            // Update the original entry
            journalEntry.ReversalJournalEntryId = reversalEntry.Id;
            journalEntry.ReversalReason = reversalReason;
            journalEntry.ReversedBy = reversedBy;
            journalEntry.ReversedAt = DateTime.UtcNow;

            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Automatically submit, approve and post the reversal entry
            await SubmitForApprovalAsync(reversalEntry.Id, reversedBy, cancellationToken);
            await ApproveJournalEntryAsync(reversalEntry.Id, reversedBy, cancellationToken);
            await PostJournalEntryAsync(reversalEntry.Id, reversedBy, cancellationToken);

            return reversalEntry.Id;
        }

        public async Task<string> GenerateJournalNumberAsync(JournalEntryType entryType, CancellationToken cancellationToken = default)
        {
            return await _journalEntryRepository.GenerateJournalNumberAsync(entryType, cancellationToken);
        }

        private async Task ValidateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(journalEntry.Description))
            {
                throw new ArgumentException("Journal entry description is required");
            }

            if (journalEntry.JournalEntryLines == null || !journalEntry.JournalEntryLines.Any())
            {
                throw new ArgumentException("Journal entry must have at least one line item");
            }

            // Validate financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(journalEntry.FinancialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {journalEntry.FinancialPeriodId} not found");
            }

            if (journalEntry.EntryDate < financialPeriod.StartDate || journalEntry.EntryDate > financialPeriod.EndDate)
            {
                throw new InvalidOperationException($"Journal entry date {journalEntry.EntryDate:yyyy-MM-dd} is outside the financial period {financialPeriod.StartDate:yyyy-MM-dd} to {financialPeriod.EndDate:yyyy-MM-dd}");
            }

            // Validate accounts
            foreach (var line in journalEntry.JournalEntryLines)
            {
                var account = await _chartOfAccountRepository.GetByIdAsync(line.AccountId, cancellationToken);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account with ID {line.AccountId} not found");
                }

                if (account.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException($"Account {account.AccountNumber} - {account.AccountName} is not active");
                }

                if (!account.AllowManualEntry)
                {
                    throw new InvalidOperationException($"Account {account.AccountNumber} - {account.AccountName} does not allow manual entries");
                }
            }

            // Ensure double-entry balance
            decimal totalDebits = journalEntry.JournalEntryLines.Sum(line => line.DebitAmount);
            decimal totalCredits = journalEntry.JournalEntryLines.Sum(line => line.CreditAmount);

            if (totalDebits != totalCredits)
            {
                throw new InvalidOperationException($"Journal entry does not balance. Total debits: {totalDebits}, Total credits: {totalCredits}");
            }
        }
    }
}
