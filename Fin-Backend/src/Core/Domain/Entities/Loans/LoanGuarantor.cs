using System.ComponentModel.DataAnnotations;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Entities.Customers;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Loans;

public class LoanGuarantor : BaseEntity
{
    [Required]
    public Guid LoanAccountId { get; set; }
    public virtual LoanAccount LoanAccount { get; set; } = null!;
    
    public Guid? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(200)]
    public string? Occupation { get; set; }
    
    [StringLength(200)]
    public string? EmployerName { get; set; }
    
    [StringLength(20)]
    public string? BVN { get; set; }
    
    [StringLength(20)]
    public string? NIN { get; set; }
    
    [StringLength(100)]
    public string? Relationship { get; set; }
    
    [Required]
    public GuarantorStatus Status { get; set; } = GuarantorStatus.Active;
    
    public DateTime? ConsentDate { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}