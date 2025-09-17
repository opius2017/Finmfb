using System;
using System.Collections.Generic;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a delinquent loan that needs collection actions
    /// </summary>
    public class LoanCollection : AuditableEntity
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public DateTime OverdueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public CollectionStatus Status { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string ResolutionNotes { get; set; }
        public CollectionPriority Priority { get; set; }
        public string ReasonForDelinquency { get; set; }
        public string Notes { get; set; }
        public string ActionPlan { get; set; }
        
        // Navigation properties
        public virtual Loan Loan { get; set; }
        public virtual ICollection<LoanCollectionAction> CollectionActions { get; set; } = new List<LoanCollectionAction>();
        
        // Business logic methods
        public void AssignCollector(string collectorId)
        {
            AssignedTo = collectorId;
        }
        
        public void UpdateStatus(CollectionStatus newStatus, string notes = null)
        {
            Status = newStatus;
            
            if (!string.IsNullOrEmpty(notes))
            {
                Notes = string.IsNullOrEmpty(Notes) 
                    ? notes 
                    : $"{Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {notes}";
            }
            
            if (newStatus == CollectionStatus.Resolved || 
                newStatus == CollectionStatus.Restructured ||
                newStatus == CollectionStatus.WrittenOff)
            {
                ResolutionDate = DateTime.UtcNow;
                ResolutionNotes = notes;
            }
        }
        
        public void AddCollectionAction(LoanCollectionAction action)
        {
            CollectionActions.Add(action);
        }
    }
    
    public enum CollectionStatus
    {
        New,
        InProgress,
        OnHold,
        PartialPaymentReceived,
        Escalated,
        LegalAction,
        Resolved,
        Restructured,
        WrittenOff
    }
    
    public enum CollectionPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents an action taken to collect a delinquent loan
    /// </summary>
    public class LoanCollectionAction : AuditableEntity
    {
        public string Id { get; set; }
        public string CollectionId { get; set; }
        public ActionType ActionType { get; set; }
        public DateTime ActionDate { get; set; }
        public string PerformedBy { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public bool RequiresFollowUp { get; set; }
        public string ContactPerson { get; set; }
        public string ContactDetails { get; set; }
        
        // Navigation property
        public virtual LoanCollection Collection { get; set; }
    }
    
    public enum ActionType
    {
        PhoneCall,
        Email,
        SMS,
        VisitToCustomer,
        VisitFromCustomer,
        PaymentPromise,
        PaymentReceived,
        LegalNotice,
        CollateralSeizure,
        Restructuring,
        Other
    }
}