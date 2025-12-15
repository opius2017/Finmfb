namespace FinTech.Core.Domain.Enums;

public enum AccountType
{
    Assets = 1,
    Liabilities = 2,
    Equity = 3,
    Revenue = 4,
    Expenses = 5
}

public enum AccountCategory
{
    CurrentAssets = 1,
    NonCurrentAssets = 2,
    CurrentLiabilities = 3,
    NonCurrentLiabilities = 4,
    ShareCapital = 5,
    RetainedEarnings = 6,
    OperatingRevenue = 7,
    NonOperatingRevenue = 8,
    OperatingExpenses = 9,
    NonOperatingExpenses = 10
}

public enum EntryType
{
    Debit = 1,
    Credit = 2
}

public enum TransactionStatus
{
    Pending = 1,
    Posted = 2,
    Reversed = 3,
    Cancelled = 4
}

public enum JournalStatus
{
    Draft = 1,
    PendingReview = 2,
    PendingApproval = 3,
    Approved = 4,
    Posted = 5,
    Rejected = 6
}

public enum CustomerType
{
    Individual = 1,
    Corporate = 2
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Closed = 4
}

public enum RiskRating
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum OrganizationType
{
    MicrofinanceBank = 1,
    SmallMediumEnterprise = 2,
    CorporateEntity = 3
}

public enum SystemModule
{
    GeneralLedger = 1,
    CustomerManagement = 2,
    DepositManagement = 3,
    LoanManagement = 4,
    AccountsPayable = 5,
    AccountsReceivable = 6,
    InventoryManagement = 7,
    PayrollManagement = 8,
    ReportsAndAnalytics = 9,
    SystemAdministration = 10
}

public enum DepositProductType
{
    SavingsAccount = 1,
    CurrentAccount = 2,
    FixedDeposit = 3,
    TargetSavings = 4,
    KiddiesSavings = 5
}

public enum InterestCalculationMethod
{
    Simple = 1,
    Compound = 2,
    ReducingBalance = 3
}

public enum InterestPostingFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6
}

public enum AccountStatus
{
    Undefined = 0,
    Active = 1,
    Dormant = 2,
    Inactive = 2,
    Frozen = 3,
    Closed = 4,
    PendingClosure = 5,
    Suspended = 6,
    PendingApproval = 7
}

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    InterestCredit = 4,
    ChargeDebit = 5,
    Reversal = 6,
    Opening = 7,
    Closing = 8,
    Credit = 9,
    Debit = 10
}

// Loan Management Enums
public enum LoanProductType
{
    IndividualLoan = 1,
    GroupLoan = 2,
    SMELoan = 3,
    AssetFinancing = 4,
    WorkingCapital = 5,
    MortgageLoan = 6,
    PersonalLoan = 7,
    BusinessLoan = 8
}

public enum RepaymentFrequency
{
    Daily = 1,
    Weekly = 2,
    BiWeekly = 3,
    Monthly = 4,
    Quarterly = 5,
    SemiAnnually = 6,
    Annually = 7
}

public enum LoanStatus
{
    Applied = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Disbursed = 5,
    Active = 6,
    Closed = 7,
    WrittenOff = 8,
    Restructured = 9
}

public enum LoanClassification
{
    Performing = 1,
    SpecialMention = 2,
    Substandard = 3,
    Doubtful = 4,
    Lost = 5
}

public enum LoanTransactionType
{
    Disbursement = 1,
    Repayment = 2,
    InterestCharge = 3,
    FeeCharge = 4,
    PenaltyCharge = 5,
    Waiver = 6,
    WriteOff = 7,
    Reversal = 8
}

public enum RepaymentStatus
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Waived = 5,
    WrittenOff = 6
}

public enum CollateralStatus
{
    Active = 1,
    Released = 2,
    Liquidated = 3,
    Impaired = 4
}

public enum GuarantorStatus
{
    Active = 1,
    Released = 2,
    Defaulted = 3
}

// Accounts Payable Enums
public enum VendorType
{
    Individual = 1,
    Company = 2,
    Government = 3,
    NonProfit = 4
}

public enum VendorStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Blacklisted = 4
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Sent = 4,
    PartiallyReceived = 5,
    Received = 6,
    Closed = 7,
    Cancelled = 8
}

public enum BillStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    PartiallyPaid = 4,
    Paid = 5,
    Overdue = 6,
    Cancelled = 7
}

public enum PaymentMethod
{
    Cash = 1,
    Cheque = 2,
    BankTransfer = 3,
    OnlineTransfer = 4,
    MobilePayment = 5,
    CreditCard = 6,
    DebitCard = 7
}

public enum PaymentStatus
{
    Pending = 1,
    Processed = 2,
    Cleared = 3,
    Failed = 4,
    Cancelled = 5,
    Reversed = 6
}

// Accounts Receivable Enums
public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Viewed = 3,
    PartiallyPaid = 4,
    Paid = 5,
    Overdue = 6,
    Cancelled = 7,
    Refunded = 8
}

// Inventory Management Enums
public enum ItemType
{
    Product = 1,
    Service = 2,
    Asset = 3,
    Consumable = 4
}

public enum ItemCategory
{
    RawMaterials = 1,
    WorkInProgress = 2,
    FinishedGoods = 3,
    ConsumableItems = 4,
    FixedAssets = 5,
    Services = 6
}

public enum ValuationMethod
{
    FIFO = 1,
    LIFO = 2,
    WeightedAverage = 3,
    SpecificIdentification = 4
}

public enum StockAdjustmentType
{
    StockIn = 1,
    StockOut = 2,
    Damage = 3,
    Expiry = 4,
    Theft = 5,
    Return = 6
}

public enum InventoryTransactionType
{
    Purchase = 1,
    Sale = 2,
    Return = 3,
    Adjustment = 4,
    Transfer = 5,
    Disposal = 6,
    Opening = 7,
    Damage = 8,
    Theft = 9
}

public enum AdjustmentStatus
{
    Draft = 1,
    Pending = 2,
    PendingApproval = 3,
    Approved = 4,
    Posted = 5,
    Rejected = 6
}

// Human Resource and Payroll Enums
public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Suspended = 3,
    Terminated = 4,
    Resigned = 5,
    Retired = 6
}

public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Temporary = 4,
    Intern = 5
}

public enum PayrollStatus
{
    Draft = 1,
    Calculated = 2,
    PendingApproval = 3,
    Approved = 4,
    Processing = 5,
    Completed = 6,
    Failed = 7
}

// Security and Audit Enums
public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Read = 4,
    Login = 5,
    Logout = 6,
    Export = 7,
    Import = 8
}

public enum MakerCheckerStatus
{
    Pending = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

// Reporting Enums
public enum FinancialStatementType
{
    IncomeStatement = 1,
    BalanceSheet = 2,
    CashFlowStatement = 3,
    ChangeInEquityStatement = 4,
    TrialBalance = 5
}

public enum FinancialStatementStatus
{
    Draft = 1,
    Generated = 2,
    Approved = 3,
    Published = 4,
    Archived = 5
}

public enum RegulatoryAuthority
{
    CentralBank = 1,
    SecuritiesCommission = 2,
    TaxAuthority = 3,
    DepositInsurance = 4
}

public enum ReportingFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6
}

public enum RegulatoryReportStatus
{
    Draft = 1,
    PendingReview = 2,
    Approved = 3,
    Submitted = 4,
    Rejected = 5,
    Revised = 6
}

// Multicurrency Enums
public enum RevaluationType
{
    GainLoss = 1,
    Translation = 2,
    Remeasurement = 3
}

public enum RevaluationStatus
{
    Pending = 1,
    Processed = 2,
    Posted = 3,
    Reversed = 4
}

public enum ExchangeRateSource
{
    Manual = 1,
    APIFeed = 2,
    CentralBank = 3,
    MarketRate = 4
}

// System Role Enums
public enum RoleLevel
{
    System = 1,
    Organization = 2,
    Branch = 3,
    Department = 4
}

public enum PermissionAction
{
    View = 1,
    Create = 2,
    Edit = 3,
    Delete = 4,
    Approve = 5,
    Export = 6
}

public enum DashboardType
{
    Executive = 1,
    Financial = 2,
    Operational = 3,
    RiskManagement = 4,
    Custom = 5
}

public enum FixedAssetStatus
{
    Active = 1,
    Inactive = 2,
    Disposed = 3,
    UnderMaintenance = 4,
    Lost = 5,
    Stolen = 6,
    Depreciated = 7
}
