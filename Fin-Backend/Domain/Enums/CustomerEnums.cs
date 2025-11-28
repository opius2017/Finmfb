namespace FinTech.Core.Domain.Enums;

public enum CustomerRiskCategory
{
    Low,
    Medium,
    High
}

public enum InquiryStatus
{
    Pending,
    InProgress,
    Resolved,
    Closed
}

public enum ComplaintStatus
{
    Received,
    InProgress,
    Escalated,
    Resolved,
    Closed,
    Rejected
}

public enum CommunicationType
{
    Inquiry,
    InquiryResponse,
    Complaint,
    ComplaintResolution,
    Call,
    Email,
    SMS,
    Visit,
    Marketing,
    Other
}

public enum CommunicationChannel
{
    InPerson,
    Phone,
    Email,
    SMS,
    Web,
    Mobile,
    SocialMedia,
    System,
    Other
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

public enum PayrollFrequency
{
    Weekly,
    BiWeekly,
    Monthly
}

public enum TaxReportingType
{
    VAT,
    WHT,
    PAYE,
    CIT
}
