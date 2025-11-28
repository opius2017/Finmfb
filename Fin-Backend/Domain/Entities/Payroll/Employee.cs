using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Payroll;

public class Employee : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    
    public Gender? Gender { get; set; }
    
    [StringLength(20)]
    public string? MaritalStatus { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? BVN { get; set; }
    
    [StringLength(20)]
    public string? NIN { get; set; }
    
    [StringLength(20)]
    public string? TaxID { get; set; }
    
    [StringLength(20)]
    public string? PensionPIN { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Grade { get; set; }
    
    [Required]
    public DateTime HireDate { get; set; }
    
    public DateTime? TerminationDate { get; set; }
    
    [Required]
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    
    [Required]
    public EmploymentType EmploymentType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal HousingAllowance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TransportAllowance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MedicalAllowance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherAllowances { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossSalary { get; set; }
    
    [StringLength(100)]
    public string? BankName { get; set; }
    
    [StringLength(20)]
    public string? AccountNumber { get; set; }
    
    [StringLength(100)]
    public string? AccountName { get; set; }
    
    [StringLength(100)]
    public string? NextOfKinName { get; set; }
    
    [StringLength(20)]
    public string? NextOfKinPhone { get; set; }
    
    [StringLength(500)]
    public string? NextOfKinAddress { get; set; }
    
    [StringLength(100)]
    public string? NextOfKinRelationship { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<PayrollEntry> PayrollEntries { get; set; } = [];
    
    public string FullName => $"{FirstName} {LastName}";
}
