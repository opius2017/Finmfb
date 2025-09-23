using System;
using System.Collections.Generic;
using System.Linq;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting
{
    public enum JournalEntryStatus
    {
        Draft = 1,
        Pending = 2,
        Approved = 3,
        Posted = 4,
        Rejected = 5,
        Reversed = 6
    }
    
    public enum JournalEntryType
    {
        Standard = 1,
        Recurring = 2,
        Reversal = 3,
        SystemGenerated = 4,
        YearEndClosing = 5
    }
    
    /// <summary>
    /// Represents a journal entry in the general ledger
    /// </summary>
    public class JournalEntry : AggregateRoot
    {
        public string JournalEntryNumber { get; private set; }
        public DateTime EntryDate { get; private set; }
        public string Description { get; private set; }
        public JournalEntryStatus Status { get; private set; }
        public JournalEntryType EntryType { get; private set; }
        public string Reference { get; private set; }
        public string SourceDocument { get; private set; }
        public string ApprovedBy { get; private set; }
        public DateTime? ApprovalDate { get; private set; }
        public string PostedBy { get; private set; }
        public DateTime? PostedDate { get; private set; }
        public string ReversalReason { get; private set; }
        public string ReversalJournalEntryId { get; private set; }
        public JournalEntry ReversalJournalEntry { get; private set; }
        public string FinancialPeriodId { get; private set; }
        public string ModuleSource { get; private set; }
        public bool IsRecurring { get; private set; }
        public string RecurrencePattern { get; private set; }
        public string Notes { get; private set; }
        
        private List<JournalEntryLine> _journalEntryLines = new List<JournalEntryLine>();
        public IReadOnlyCollection<JournalEntryLine> JournalEntryLines => _journalEntryLines.AsReadOnly();
        
        // Required by EF Core
        private JournalEntry() { }
        
        public JournalEntry(
            string journalEntryNumber,
            DateTime entryDate,
            string description,
            JournalEntryType entryType,
            string reference = null,
            string sourceDocument = null,
            string financialPeriodId = null,
            string moduleSource = null,
            bool isRecurring = false,
            string recurrencePattern = null,
            string notes = null)
        {
            JournalEntryNumber = journalEntryNumber;
            EntryDate = entryDate;
            Description = description;
            Status = JournalEntryStatus.Draft;
            EntryType = entryType;
            Reference = reference;
            SourceDocument = sourceDocument;
            FinancialPeriodId = financialPeriodId;
            ModuleSource = moduleSource;
            IsRecurring = isRecurring;
            RecurrencePattern = recurrencePattern;
            Notes = notes;
            
            AddDomainEvent(new JournalEntryCreatedEvent(Id, journalEntryNumber));
        }
        
        public void AddJournalLine(
            string accountId,
            Money amount,
            bool isDebit,
            string description = null,
            string reference = null)
        {
            var line = new JournalEntryLine(
                this.Id,
                accountId,
                amount,
                isDebit,
                description,
                reference);
                
            _journalEntryLines.Add(line);
            
            // Validate balance after adding line
            ValidateBalance();
        }
        
        public void RemoveJournalLine(string lineId)
        {
            var line = _journalEntryLines.FirstOrDefault(l => l.Id == lineId);
            
            if (line == null)
                throw new InvalidOperationException($"Journal line with ID {lineId} not found");
                
            _journalEntryLines.Remove(line);
            
            // Validate balance after removing line
            ValidateBalance();
        }
        
        public void SubmitForApproval()
        {
            if (Status != JournalEntryStatus.Draft)
                throw new InvalidOperationException($"Cannot submit journal entry in {Status} status for approval");
                
            // Validate journal entry
            if (_journalEntryLines.Count < 2)
                throw new InvalidOperationException("Journal entry must have at least two lines");
                
            ValidateBalance();
            
            Status = JournalEntryStatus.Pending;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new JournalEntrySubmittedEvent(Id, JournalEntryNumber));
        }
        
        public void Approve(string approvedBy)
        {
            if (Status != JournalEntryStatus.Pending)
                throw new InvalidOperationException($"Cannot approve journal entry in {Status} status");
                
            Status = JournalEntryStatus.Approved;
            ApprovedBy = approvedBy;
            ApprovalDate = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new JournalEntryApprovedEvent(Id, JournalEntryNumber, approvedBy));
        }
        
        public void Reject(string rejectedBy, string reason)
        {
            if (Status != JournalEntryStatus.Pending)
                throw new InvalidOperationException($"Cannot reject journal entry in {Status} status");
                
            Status = JournalEntryStatus.Rejected;
            Notes = reason;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new JournalEntryRejectedEvent(Id, JournalEntryNumber, rejectedBy, reason));
        }
        
        public void Post(string postedBy)
        {
            if (Status != JournalEntryStatus.Approved)
                throw new InvalidOperationException($"Cannot post journal entry in {Status} status");
                
            Status = JournalEntryStatus.Posted;
            PostedBy = postedBy;
            PostedDate = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new JournalEntryPostedEvent(Id, JournalEntryNumber, postedBy));
        }
        
        public JournalEntry CreateReversal(
            string journalEntryNumber,
            string reversalReason,
            string postedBy)
        {
            if (Status != JournalEntryStatus.Posted)
                throw new InvalidOperationException($"Cannot reverse journal entry in {Status} status");
                
            var reversalEntry = new JournalEntry(
                journalEntryNumber,
                DateTime.UtcNow,
                $"Reversal of {JournalEntryNumber}: {Description}",
                JournalEntryType.Reversal,
                Reference,
                SourceDocument,
                FinancialPeriodId,
                ModuleSource);
                
            // Add reversed lines (debit becomes credit and vice versa)
            foreach (var line in _journalEntryLines)
            {
                reversalEntry.AddJournalLine(
                    line.AccountId,
                    line.Amount,
                    !line.IsDebit,
                    $"Reversal of {line.Description}",
                    line.Reference);
            }
            
            reversalEntry.ReversalReason = reversalReason;
            
            // Auto-approve and post the reversal
            reversalEntry.Status = JournalEntryStatus.Approved;
            reversalEntry.ApprovedBy = postedBy;
            reversalEntry.ApprovalDate = DateTime.UtcNow;
            reversalEntry.Post(postedBy);
            
            // Link reversal to original
            reversalEntry.ReversalJournalEntryId = this.Id;
            
            // Update original entry
            this.Status = JournalEntryStatus.Reversed;
            this.LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new JournalEntryReversedEvent(Id, JournalEntryNumber, reversalEntry.Id, reversalEntry.JournalEntryNumber, postedBy));
            
            return reversalEntry;
        }
        
        private void ValidateBalance()
        {
            // Group by currency first, then validate each currency group is balanced
            var currencyGroups = _journalEntryLines
                .GroupBy(l => l.Amount.Currency)
                .ToDictionary(g => g.Key, g => g.ToList());
                
            foreach (var currencyGroup in currencyGroups)
            {
                var currency = currencyGroup.Key;
                var lines = currencyGroup.Value;
                
                var totalDebits = lines
                    .Where(l => l.IsDebit)
                    .Sum(l => l.Amount.Amount);
                    
                var totalCredits = lines
                    .Where(l => !l.IsDebit)
                    .Sum(l => l.Amount.Amount);
                    
                // Journal should be balanced: Total Debits = Total Credits
                if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow small rounding differences
                {
                    throw new InvalidOperationException(
                        $"Journal entry is not balanced for currency {currency}. " +
                        $"Total Debits: {totalDebits}, Total Credits: {totalCredits}");
                }
            }
        }
    }
    
    /// <summary>
    /// Represents a line item in a journal entry
    /// </summary>
    public class JournalEntryLine : BaseEntity
    {
        public string JournalEntryId { get; private set; }
        public string AccountId { get; private set; }
        public ChartOfAccount Account { get; private set; }
        public Money Amount { get; private set; }
        public bool IsDebit { get; private set; }
        public string Description { get; private set; }
        public string Reference { get; private set; }
        
        // Required by EF Core
        private JournalEntryLine() { }
        
        public JournalEntryLine(
            string journalEntryId,
            string accountId,
            Money amount,
            bool isDebit,
            string description = null,
            string reference = null)
        {
            JournalEntryId = journalEntryId;
            AccountId = accountId;
            Amount = amount;
            IsDebit = isDebit;
            Description = description;
            Reference = reference;
        }
    }
    
    // Domain Events
    public class JournalEntryCreatedEvent : DomainEvent
    {
        public string JournalEntryId { get; }
        public string JournalEntryNumber { get; }
        
        public JournalEntryCreatedEvent(string journalEntryId, string journalEntryNumber)
        {
            JournalEntryId = journalEntryId;
            JournalEntryNumber = journalEntryNumber;
        }
    }
    
    public class JournalEntrySubmittedEvent : DomainEvent
    {
        public string JournalEntryId { get; }
        public string JournalEntryNumber { get; }
        
        public JournalEntrySubmittedEvent(string journalEntryId, string journalEntryNumber)
        {
            JournalEntryId = journalEntryId;
            JournalEntryNumber = journalEntryNumber;
        }
    }
    
    public class JournalEntryApprovedEvent : DomainEvent
    {
        public string JournalEntryId { get; }
        public string JournalEntryNumber { get; }
        public string ApprovedBy { get; }
        
        public JournalEntryApprovedEvent(string journalEntryId, string journalEntryNumber, string approvedBy)
        {
            JournalEntryId = journalEntryId;
            JournalEntryNumber = journalEntryNumber;
            ApprovedBy = approvedBy;
        }
    }
    
    public class JournalEntryRejectedEvent : DomainEvent
    {
        public string JournalEntryId { get; }
        public string JournalEntryNumber { get; }
        public string RejectedBy { get; }
        public string Reason { get; }
        
        public JournalEntryRejectedEvent(string journalEntryId, string journalEntryNumber, string rejectedBy, string reason)
        {
            JournalEntryId = journalEntryId;
            JournalEntryNumber = journalEntryNumber;
            RejectedBy = rejectedBy;
            Reason = reason;
        }
    }
    
    public class JournalEntryPostedEvent : DomainEvent
    {
        public string JournalEntryId { get; }
        public string JournalEntryNumber { get; }
        public string PostedBy { get; }
        
        public JournalEntryPostedEvent(string journalEntryId, string journalEntryNumber, string postedBy)
        {
            JournalEntryId = journalEntryId;
            JournalEntryNumber = journalEntryNumber;
            PostedBy = postedBy;
        }
    }
    
    public class JournalEntryReversedEvent : DomainEvent
    {
        public string JournalEntryId { get; }
        public string JournalEntryNumber { get; }
        public string ReversalJournalEntryId { get; }
        public string ReversalJournalEntryNumber { get; }
        public string ReversedBy { get; }
        
        public JournalEntryReversedEvent(
            string journalEntryId, 
            string journalEntryNumber, 
            string reversalJournalEntryId, 
            string reversalJournalEntryNumber, 
            string reversedBy)
        {
            JournalEntryId = journalEntryId;
            JournalEntryNumber = journalEntryNumber;
            ReversalJournalEntryId = reversalJournalEntryId;
            ReversalJournalEntryNumber = reversalJournalEntryNumber;
            ReversedBy = reversedBy;
        }
    }
}