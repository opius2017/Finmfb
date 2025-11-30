using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents an entry in the official loan register
    /// Immutable once created for audit compliance
    /// </summary>
    public class LoanRegister : AuditableEntity
    {
        public string SerialNumber { get; private set; }
        public Guid LoanId { get; private set; }
        public Guid ApplicationId { get; private set; }
        public Guid MemberId { get; private set; }
        public string MemberNumber { get; private set; }
        public string MemberName { get; private set; }
        public decimal PrincipalAmount { get; private set; }
        public decimal InterestRate { get; private set; }
        public int TenorMonths { get; private set; }
        public decimal MonthlyEMI { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        public DateTime DisbursementDate { get; private set; }
        public DateTime MaturityDate { get; private set; }
        public int RegistrationYear { get; private set; }
        public int RegistrationMonth { get; private set; }
        public int SequenceNumber { get; private set; }
        public string LoanType { get; private set; }
        public string RegisteredBy { get; private set; }
        public string Status { get; private set; }
        public string Notes { get; private set; }
        
        // Navigation properties
        public virtual Loan Loan { get; private set; }
        public virtual LoanApplication Application { get; private set; }
        public virtual Member Member { get; private set; }
        
        private LoanRegister() { } // For EF Core
        
        public LoanRegister(
            string serialNumber,
            Guid loanId,
            Guid applicationId,
            Guid memberId,
            string memberNumber,
            string memberName,
            decimal principalAmount,
            decimal interestRate,
            int tenorMonths,
            decimal monthlyEMI,
            DateTime disbursementDate,
            DateTime maturityDate,
            string loanType,
            string registeredBy,
            int year,
            int month,
            int sequenceNumber)
        {
            SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
            LoanId = loanId;
            ApplicationId = applicationId;
            MemberId = memberId;
            MemberNumber = memberNumber ?? throw new ArgumentNullException(nameof(memberNumber));
            MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
            PrincipalAmount = principalAmount;
            InterestRate = interestRate;
            TenorMonths = tenorMonths;
            MonthlyEMI = monthlyEMI;
            RegistrationDate = DateTime.UtcNow;
            DisbursementDate = disbursementDate;
            MaturityDate = maturityDate;
            RegistrationYear = year;
            RegistrationMonth = month;
            SequenceNumber = sequenceNumber;
            LoanType = loanType ?? "NORMAL";
            RegisteredBy = registeredBy ?? throw new ArgumentNullException(nameof(registeredBy));
            Status = "REGISTERED";
        }
        
        /// <summary>
        /// Update status (limited updates allowed for audit compliance)
        /// </summary>
        public void UpdateStatus(string newStatus, string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty", nameof(newStatus));
            
            Status = newStatus;
            if (!string.IsNullOrWhiteSpace(notes))
                Notes = notes;
        }
        
        /// <summary>
        /// Check if this is a read-only register entry
        /// </summary>
        public bool IsReadOnly()
        {
            // Register entries are read-only after 30 days
            return (DateTime.UtcNow - RegistrationDate).TotalDays > 30;
        }
    }
}
