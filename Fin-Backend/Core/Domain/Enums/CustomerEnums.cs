namespace FinTech.Core.Domain.Enums;

public enum CustomerType
{
    Individual,
    Business,
    Group
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Dormant,
    Closed,
    Blacklisted,
    Deceased
}

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

public enum AccountStatus
{
    Active,
    Dormant,
    Closed,
    Frozen,
    Pending
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    InterestPosting,
    FeeDeduction,
    Adjustment,
    Reversal
}

public enum EntryType
{
    Debit,
    Credit
}

public enum LoanStatus
{
    Draft,
    Pending,
    Approved,
    Rejected,
    Disbursed,
    Active,
    Restructured,
    Closed,
    WrittenOff
}

public enum LoanClassification
{
    Performing,
    SpecialMention,
    Substandard,
    Doubtful,
    Lost
}

public enum LoanTransactionType
{
    Application,
    Approval,
    Disbursement,
    Repayment,
    PenaltyAssessment,
    WriteOff,
    Restructure
}

public enum RepaymentStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Overdue,
    Deferred
}

public enum RepaymentFrequency
{
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Quarterly,
    SemiAnnually,
    Annually
}

public enum JournalStatus
{
    Draft,
    PendingApproval,
    Approved,
    Posted,
    Rejected
}

public enum TransactionStatus
{
    Pending,
    Authorized,
    Posted,
    Failed,
    Reversed
}

public enum MakerCheckerStatus
{
    PendingApproval,
    Approved,
    Rejected,
    Executed,
    Failed,
    Expired
}

public enum InventoryTransactionType
{
    Purchase,
    Sale,
    Return,
    Adjustment,
    Transfer
}

public enum PayrollFrequency
{
    Weekly,
    BiWeekly,
    Monthly
}

public enum PaymentMethod
{
    Cash,
    BankTransfer,
    Cheque,
    Card,
    DigitalWallet
}

public enum TaxReportingType
{
    VAT,
    WHT,
    PAYE,
    CIT
}
