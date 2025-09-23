using System;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Events.Loans
{
    public class LoanApplicationSubmittedEvent : DomainEvent
    {
        public string LoanApplicationId { get; }

        public LoanApplicationSubmittedEvent(string loanApplicationId)
        {
            LoanApplicationId = loanApplicationId;
        }
    }

    public class LoanApplicationApprovedEvent : DomainEvent
    {
        public string LoanApplicationId { get; }

        public LoanApplicationApprovedEvent(string loanApplicationId)
        {
            LoanApplicationId = loanApplicationId;
        }
    }

    public class LoanApplicationRejectedEvent : DomainEvent
    {
        public string LoanApplicationId { get; }
        public string RejectionReason { get; }

        public LoanApplicationRejectedEvent(string loanApplicationId, string rejectionReason)
        {
            LoanApplicationId = loanApplicationId;
            RejectionReason = rejectionReason;
        }
    }

    public class LoanCreatedFromApplicationEvent : DomainEvent
    {
        public string LoanApplicationId { get; }
        public string LoanId { get; }

        public LoanCreatedFromApplicationEvent(string loanApplicationId, string loanId)
        {
            LoanApplicationId = loanApplicationId;
            LoanId = loanId;
        }
    }

    public class LoanStatusChangedEvent : DomainEvent
    {
        public string LoanId { get; }
        public string PreviousStatus { get; }
        public string NewStatus { get; }
        public DateTime StatusChangeDate { get; }

        public LoanStatusChangedEvent(string loanId, string previousStatus, string newStatus, DateTime statusChangeDate)
        {
            LoanId = loanId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            StatusChangeDate = statusChangeDate;
        }
    }

    public class LoanPastDueEvent : DomainEvent
    {
        public string LoanId { get; }
        public int DaysOverdue { get; }
        public decimal OverdueAmount { get; }

        public LoanPastDueEvent(string loanId, int daysOverdue, decimal overdueAmount)
        {
            LoanId = loanId;
            DaysOverdue = daysOverdue;
            OverdueAmount = overdueAmount;
        }
    }

    public class LoanRescheduledEvent : DomainEvent
    {
        public string LoanId { get; }
        public DateTime OriginalEndDate { get; }
        public DateTime NewEndDate { get; }
        public string ReschedulingReason { get; }
        public string ApprovedBy { get; }

        public LoanRescheduledEvent(
            string loanId, 
            DateTime originalEndDate, 
            DateTime newEndDate, 
            string reschedulingReason, 
            string approvedBy)
        {
            LoanId = loanId;
            OriginalEndDate = originalEndDate;
            NewEndDate = newEndDate;
            ReschedulingReason = reschedulingReason;
            ApprovedBy = approvedBy;
        }
    }

    public class LoanCollateralAddedEvent : DomainEvent
    {
        public string LoanId { get; }
        public string CollateralId { get; }
        public string CollateralType { get; }
        public decimal ValuationAmount { get; }

        public LoanCollateralAddedEvent(
            string loanId, 
            string collateralId, 
            string collateralType, 
            decimal valuationAmount)
        {
            LoanId = loanId;
            CollateralId = collateralId;
            CollateralType = collateralType;
            ValuationAmount = valuationAmount;
        }
    }

    public class LoanGuarantorAddedEvent : DomainEvent
    {
        public string LoanId { get; }
        public string GuarantorId { get; }
        public string GuarantorName { get; }

        public LoanGuarantorAddedEvent(string loanId, string guarantorId, string guarantorName)
        {
            LoanId = loanId;
            GuarantorId = guarantorId;
            GuarantorName = guarantorName;
        }
    }
}