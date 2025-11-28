using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Events.Loans;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class Loan : AggregateRoot
    {
        public string LoanNumber { get; private set; }
        // Use string IDs across the domain to match BaseEntity.Id
        public string CustomerId { get; private set; }
        public string LoanProductId { get; private set; }
        public string LoanApplicationId { get; private set; }

        // Keep principal terminology used elsewhere while also exposing LoanAmount for configuration compatibility
        public decimal PrincipalAmount { get; private set; }
        public decimal LoanAmount => PrincipalAmount;

        public decimal OutstandingPrincipal { get; private set; }
        public decimal OutstandingInterest { get; private set; }
        public decimal InterestRate { get; private set; }
        // Loan term in months - expose both names used in codebases
        public int LoanTermMonths { get; private set; }
        public int LoanTerm => LoanTermMonths;

        public DateTime StartDate { get; private set; }
        public DateTime MaturityDate { get; private set; }
        public string LoanType { get; private set; }
        // Status string to align with configuration
        public string Status { get; private set; }
        public DateTime? DisbursementDate { get; private set; }
        public string RepaymentFrequency { get; private set; }
        public decimal MonthlyPayment { get; private set; }
        public string InterestType { get; private set; }
        public string Currency { get; private set; }
        public string AccountNumber { get; private set; }
        public string Purpose { get; private set; }

        // Navigation properties for mapping/queries
        public virtual Customer Customer { get; set; }
        public virtual LoanProduct LoanProduct { get; set; }
        public virtual ICollection<LoanTransaction> Transactions { get; private set; } = new List<LoanTransaction>();

        private Loan() { } // For EF Core

        public Loan(
            string loanNumber,
            string customerId,
            string loanProductId,
            decimal principalAmount,
            decimal interestRate,
            int loanTermMonths,
            DateTime startDate,
            string loanType,
            string repaymentFrequency,
            string currency,
            string? loanApplicationId = null,
            string? purpose = null)
        {
            LoanNumber = loanNumber;
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
            LoanProductId = loanProductId ?? string.Empty;
            LoanApplicationId = loanApplicationId ?? string.Empty;
            PrincipalAmount = principalAmount;
            OutstandingPrincipal = 0; // Not yet disbursed
            OutstandingInterest = 0;
            InterestRate = interestRate;
            LoanTermMonths = loanTermMonths;
            StartDate = startDate;
            MaturityDate = startDate.AddMonths(loanTermMonths);
            LoanType = loanType ?? string.Empty;
            Status = "PENDING";
            RepaymentFrequency = repaymentFrequency ?? string.Empty;
            Currency = currency ?? string.Empty;
            Purpose = purpose ?? string.Empty;

            // Calculate monthly payment based on loan term and interest rate
            decimal monthlyRate = interestRate / 12 / 100;
            MonthlyPayment = monthlyRate == 0 ? Math.Round(PrincipalAmount / loanTermMonths, 2)
                : PrincipalAmount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths)
                    / ((decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths) - 1);
        }

        public void Disburse(string reference, string description)
        {
            if (Status != "APPROVED")
                throw new InvalidOperationException("Loan must be in APPROVED status to disburse");

            OutstandingPrincipal = PrincipalAmount;
            DisbursementDate = DateTime.UtcNow;
            Status = "ACTIVE";

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
            if (Status != "ACTIVE")
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
                Status = "CLOSED";
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
            if (Status != "ACTIVE")
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
            if (Status != "ACTIVE" && Status != "APPROVED")
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
            if (Status != "ACTIVE")
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
            Status = "WRITTEN_OFF";

            // Raise domain event
            AddDomainEvent(new LoanWrittenOffEvent(
                this.Id,
                writeOffAmount,
                reference,
                description));
        }

        // Backwards-compatible constructor used by older call sites that pass int IDs
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
            : this(
                  loanNumber: loanNumber,
                  customerId: customerId.ToString(),
                  loanProductId: string.Empty,
                  principalAmount: principalAmount,
                  interestRate: interestRate,
                  loanTermMonths: loanTermMonths,
                  startDate: startDate,
                  loanType: loanType,
                  repaymentFrequency: repaymentFrequency,
                  currency: currency,
                  loanApplicationId: null,
                  purpose: null)
        {
            // Older constructor semantics set approved status by default
            Status = "APPROVED";
        }
    }
}
