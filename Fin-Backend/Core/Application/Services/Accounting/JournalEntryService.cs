using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using IJournalEntryService = FinTech.Core.Application.Interfaces.Services.Accounting.IJournalEntryService;
using FinTech.Core.Application.Interfaces.Services.Accounting;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Domain.ValueObjects;

using System.Text.Json;
using FinTech.Core.Application.DTOs.GeneralLedger.Journal;
namespace FinTech.Core.Application.Services.Accounting
{
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
                // We cannot set JournalEntryNumber on existing instance as it is private set.
                // We must create a NEW instance.
                var number = await GenerateJournalNumberAsync(journalEntry.EntryType, cancellationToken);
                
                // Re-create the entity properly
                var newEntry = new JournalEntry(
                    number,
                    journalEntry.EntryDate,
                    journalEntry.Description,
                    journalEntry.EntryType,
                    journalEntry.Reference,
                    journalEntry.SourceDocument,
                    journalEntry.FinancialPeriodId,
                    journalEntry.ModuleSource,
                    journalEntry.IsRecurring,
                    journalEntry.RecurrencePattern,
                    journalEntry.Notes
                );
                
                // Copy lines
                foreach(var line in journalEntry.JournalEntryLines)
                {
                    newEntry.AddJournalLine(line.AccountId, line.Amount, line.IsDebit, line.Description, line.Reference);
                }
                
                // Use the new entry from here
                journalEntry = newEntry;
            }

            // Add to repository
            await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return journalEntry.Id;
        }

        public async Task<string> CreateJournalEntryAsync(JournalEntryDto journalEntryDto, string tenantId = null, CancellationToken cancellationToken = default)
        {
            var journalEntry = new JournalEntry(
                string.Empty,
                journalEntryDto.EntryDate,
                journalEntryDto.Description,
                (JournalEntryType)journalEntryDto.EntryType,
                journalEntryDto.Reference,
                journalEntryDto.SourceDocument,
                journalEntryDto.FinancialPeriodId,
                journalEntryDto.ModuleSource,
                journalEntryDto.IsRecurring,
                journalEntryDto.RecurrencePattern,
                journalEntryDto.Notes
            );

            if (journalEntryDto.JournalEntryLines != null)
            {
                foreach (var line in journalEntryDto.JournalEntryLines)
                {
                    journalEntry.AddJournalLine(line.AccountId, Money.Create(line.Amount, line.CurrencyCode ?? "NGN"), line.IsDebit, line.Description, line.Reference);
                }
            }

            return await CreateJournalEntryAsync(journalEntry, cancellationToken);
        }

        public async Task<string> CreateJournalEntryAsync(object journalEntry, string tenantId, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(journalEntry);
            var dto = JsonSerializer.Deserialize<JournalEntryDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (dto == null) throw new ArgumentException("Invalid journal entry object", nameof(journalEntry));
            
            return await CreateJournalEntryAsync(dto, tenantId, cancellationToken);
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

            // Update header
            existingJournalEntry.Update(
                journalEntry.Description,
                journalEntry.EntryDate,
                journalEntry.Reference,
                journalEntry.Notes,
                journalEntry.FinancialPeriodId
            );

            // Sync lines: Clear and re-add is simplest given no ID matching logic exposed easily
            // Note: This is destructive for line IDs but safe for data integrity if IDs aren't referenced elsewhere strongly yet
            // Ideally we should diff, but JournalEntry doesn't expose ClearLines or similar public method, only RemoveJournalLine.
            // We can iterate copy of lines to remove.
            var existingLines = existingJournalEntry.JournalEntryLines.ToList();
            foreach(var line in existingLines)
            {
                existingJournalEntry.RemoveJournalLine(line.Id);
            }
            
            foreach(var line in journalEntry.JournalEntryLines)
            {
                existingJournalEntry.AddJournalLine(line.AccountId, line.Amount, line.IsDebit, line.Description, line.Reference);
            }

            // Update in repository
            await _journalEntryRepository.UpdateAsync(existingJournalEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> SubmitForApprovalAsync(string id, string submittedBy, CancellationToken cancellationToken = default)
        {
            var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);
            if (journalEntry == null)
            {
                throw new InvalidOperationException($"Journal entry with ID {id} not found");
            }

            journalEntry.SubmitForApproval();
            // Note: SubmitForApproval logic inside entity should handle LastModifiedBy? It doesn't take 'submittedBy'.
            // The entity method: LastModifiedDate = DateTime.UtcNow; AddDomainEvent...
            // It does NOT set LastModifiedBy. We can't set it (private set). 
            // We might need to accept losing 'submittedBy' in pure entity field, or trust Domain Event to handle auditing.
            // However, previous code set LastModifiedBy. 'BaseEntity' LastModifiedBy is public set?
            // BaseEntity definition: public string? LastModifiedBy { get; set; }
            // So we CAN set it on BaseEntity!
            journalEntry.LastModifiedBy = submittedBy;

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

            journalEntry.Approve(approvedBy);
            journalEntry.LastModifiedBy = approvedBy;

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

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                throw new ArgumentException("Rejection reason is required");
            }

            journalEntry.MarkAsRejected(rejectedBy, rejectionReason);
            journalEntry.LastModifiedBy = rejectedBy;

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

            journalEntry.Post(postedBy);
            journalEntry.LastModifiedBy = postedBy;

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

            if (string.IsNullOrWhiteSpace(reversalReason))
            {
                throw new ArgumentException("Reversal reason is required");
            }

            // Generate journal entry number for reversal
            var reversalNumber = await GenerateJournalNumberAsync(JournalEntryType.Reversal, cancellationToken);
            
            // Use entity method
            var reversalEntry = journalEntry.CreateReversal(reversalNumber, reversalReason, reversedBy);
            // Reversal entry returned is already Approved/Posted and linked.
            // BUT CreateReversal sets Status=Reversed on original? YES.
            
            // We need to set IsSystemGenerated = true for reversal?
            // CreateReversal constructor call sets ModuleSource etc. 
            // We might need to manually set IsSystemGenerated? usage of internal/protected logic?
            // The Property IsSystemGenerated is readonly => EntryType == SystemGenerated.
            // But Reversal causes EntryType = Reversal. 
            // So IsSystemGenerated might be false (EntryType != SystemGenerated).
            // That's fine if logic expects it.
           
            // Add the reversal entry
            await _journalEntryRepository.AddAsync(reversalEntry, cancellationToken);

            // Update the original entry (Status=Reversed was set by CreateReversal)
            journalEntry.LastModifiedBy = reversedBy;
            await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);
            
            // Need to update account balances for the reversal?
            // Yes, because reversal is Posted.
            await _generalLedgerService.UpdateAccountBalancesAsync(reversalEntry, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

                if (!account.AllowManualEntries)
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
