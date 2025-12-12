using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a cooperative member
/// </summary>
public class Member : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string MemberNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(10)]
    public string Gender { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; } = "Nigeria";

    [StringLength(50)]
    public string? EmployeeId { get; set; }

    [StringLength(200)]
    public string? Employer { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [StringLength(100)]
    public string? JobTitle { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyIncome { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, INACTIVE, SUSPENDED, TERMINATED

    [StringLength(50)]
    public string? BankName { get; set; }

    [StringLength(50)]
    public string? AccountNumber { get; set; }

    [StringLength(50)]
    public string? AccountName { get; set; }

    [StringLength(20)]
    public string? BVN { get; set; }

    [StringLength(20)]
    public string? NIN { get; set; }

    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSavings { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FreeEquity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LockedEquity { get; set; }

    public int TotalLoans { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingLoanBalance { get; set; }

    // Navigation properties
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public virtual ICollection<Guarantor> GuarantorsProvided { get; set; } = new List<Guarantor>();
    public virtual ICollection<Guarantor> GuarantorsReceived { get; set; } = new List<Guarantor>();
}
