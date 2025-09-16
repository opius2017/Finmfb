using System;
using System.Collections.Generic;
using FinTech.Domain.Common;
using FinTech.Domain.Events.Loans;

namespace FinTech.Domain.Entities.Loans
{
    public class Loan : BaseEntity
    {
        public string LoanNumber { get; private set; }
        public int CustomerId { get; private set; }
        public decimal PrincipalAmount { get; private set; }
        public decimal OutstandingPrincipal { get; private set; }
        public decimal OutstandingInterest { get; private set; }
        public decimal InterestRate { get; private set; }
        public int LoanTermMonths { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime MaturityDate { get; private set; }
        public string LoanType { get; private set; }
        public string LoanStatus { get; private set; }
        public DateTime? DisbursementDate { get; private set; }
        public string RepaymentFrequency { get; private set; }
        public decimal MonthlyPayment { get; private set; }
        public string Currency { get; private set; }
        public virtual ICollection<LoanTransaction> Transactions { get; private set; } = new List<LoanTransaction>();

        private Loan() { } // For EF Core

        public Loan(
            string loanNumber,
            int customerId,
            decimal principalAmount,
            decimal interestRate,
            int loanTermMonths,
            DateTime startDate,
            string loanType,
            string repaymentFrequency,
            string currency)
        {
            LoanNumber = loanNumber;
            CustomerId = customerId;
            PrincipalAmount = principalAmount;
            OutstandingPrincipal = 0; // Not yet disbursed
            OutstandingInterest = 0;
            InterestRate = interestRate;
            LoanTermMonths = loanTermMonths;
            StartDate = startDate;
            MaturityDate = startDate.AddMonths(loanTermMonths);
            LoanType = loanType;
            LoanStatus = "APPROVED";
            RepaymentFrequency = repaymentFrequency;
            Currency = currency;
            
            // Calculate monthly payment based on loan term and interest rate
            // This is a simplified calculation for demo purposes
            decimal monthlyRate = interestRate / 12 / 100;
            MonthlyPayment = principalAmount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths) 
                / ((decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths) - 1);
        }

        public void Disburse(string reference, string description)
        {
            if (LoanStatus != "APPROVED")
                throw new InvalidOperationException("Loan must be in APPROVED status to disburse");

            OutstandingPrincipal = PrincipalAmount;
            DisbursementDate = DateTime.UtcNow;
            LoanStatus = "ACTIVE";

            var transaction = new LoanTransaction(
                this.Id,
                "DISBURSEMENT",
                PrincipalAmount,
                0,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new LoanDisbursedEvent(
                this.Id,
                PrincipalAmount,
                reference,
                description));
        }

        public void RecordRepayment(decimal principalAmount, decimal interestAmount, string reference, string description)
        {
            if (LoanStatus != "ACTIVE")
                throw new InvalidOperationException("Loan must be in ACTIVE status to record repayment");

            if (principalAmount < 0 || interestAmount < 0)
                throw new ArgumentException("Repayment amounts must be positive");

            if (principalAmount > OutstandingPrincipal)
                throw new ArgumentException("Principal repayment cannot exceed outstanding principal");

            if (interestAmount > OutstandingInterest)
                throw new ArgumentException("Interest repayment cannot exceed outstanding interest");

            OutstandingPrincipal -= principalAmount;
            OutstandingInterest -= interestAmount;

            var transaction = new LoanTransaction(
                this.Id,
                "REPAYMENT",
                principalAmount,
                interestAmount,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Check if loan is fully repaid
            if (OutstandingPrincipal == 0 && OutstandingInterest == 0)
            {
                LoanStatus = "CLOSED";
            }

            // Raise domain event
            AddDomainEvent(new LoanRepaymentReceivedEvent(
                this.Id,
                principalAmount,
                interestAmount,
                reference,
                description));
        }

        public void AccrueInterest(decimal amount, string reference, string description)
        {
            if (LoanStatus != "ACTIVE")
                throw new InvalidOperationException("Loan must be in ACTIVE status to accrue interest");

            if (amount <= 0)
                throw new ArgumentException("Interest amount must be positive", nameof(amount));

            OutstandingInterest += amount;

            var transaction = new LoanTransaction(
                this.Id,
                "INTEREST_ACCRUAL",
                0,
                amount,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new LoanInterestAccruedEvent(
                this.Id,
                amount,
                reference,
                description));
        }

        public void ChargeFee(decimal amount, string feeType, string reference, string description)
        {
            if (LoanStatus != "ACTIVE" && LoanStatus != "APPROVED")
                throw new InvalidOperationException("Loan must be in ACTIVE or APPROVED status to charge fee");

            if (amount <= 0)
                throw new ArgumentException("Fee amount must be positive", nameof(amount));

            var transaction = new LoanTransaction(
                this.Id,
                "FEE",
                0,
                amount,
                reference,
                description);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new LoanFeeChargedEvent(
                this.Id,
                amount,
                feeType,
                reference,
                description));
        }

        public void WriteOff(string reference, string description)
        {
            if (LoanStatus != "ACTIVE")
                throw new InvalidOperationException("Loan must be in ACTIVE status to be written off");

            decimal writeOffAmount = OutstandingPrincipal + OutstandingInterest;

            var transaction = new LoanTransaction(
                this.Id,
                "WRITE_OFF",
                OutstandingPrincipal,
                OutstandingInterest,
                reference,
                description);
            
            Transactions.Add(transaction);

            OutstandingPrincipal = 0;
            OutstandingInterest = 0;
            LoanStatus = "WRITTEN_OFF";

            // Raise domain event
            AddDomainEvent(new LoanWrittenOffEvent(
                this.Id,
                writeOffAmount,
                reference,
                description));
        }
    }
}