using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Customers;

public class CustomerInquiry : BaseEntity
{
    public Guid CustomerId { get; set; }
    public CommunicationChannel Channel { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime InquiryDate { get; set; }
    public InquiryStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string? Response { get; set; }
    public string? RespondedBy { get; set; }
    public DateTime? RespondedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; }
}

public class CustomerComplaint : BaseEntity
{
    public string ComplaintNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public CommunicationChannel Channel { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ComplaintDate { get; set; }
    public ComplaintStatus Status { get; set; }
    public Priority Priority { get; set; }
    public DateTime DueDate { get; set; }
    public string? Resolution { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; }
}

public class CustomerCommunicationLog : BaseEntity
{
    public Guid CustomerId { get; set; }
    public CommunicationType CommunicationType { get; set; }
    public CommunicationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CommunicationDate { get; set; }
    public string ContactedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; }
}
