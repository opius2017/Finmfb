namespace FinTech.Domain.Enums;

public enum CustomerRiskCategory
{
    Low,
    Medium,
    High
}

public enum InquiryStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum ComplaintStatus
{
    Draft,
    Submitted,
    UnderInvestigation,
    Escalated,
    Resolved,
    Closed
}

public enum CommunicationType
{
    Email,
    Sms,
    PhoneCall,
    MailDocument,
    FaceToFace,
    WebPortal,
    MobileApp,
    SocialMedia,
    VideoCall,
    InstantMessage,
    Newsletter,
    PushNotification,
    Fax
}

public enum CommunicationChannel
{
    Email,
    Sms,
    MobileApp,
    WebPortal,
    PhoneCall,
    SocialMedia,
    InAppNotification,
    PushNotification,
    DirectMail,
    BranchVisit,
    AtmMessage,
    UssdService
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
    Monthly,
    Quarterly,
    Annually
}

public enum TaxReportingType
{
    Annual,
    Quarterly,
    Monthly,
    Weekly
}