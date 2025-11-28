namespace FinTech.Core.Domain.Enums;

public enum ReconciliationStatus
{
    Draft = 0,
    InProgress = 1,
    Completed = 2,
    Approved = 3,
    Rejected = 4
}

public enum ReconciliationItemType
{
    DepositInTransit = 0,
    OutstandingCheck = 1,
    BankCharge = 2,
    BankInterest = 3,
    BankError = 4,
    BookError = 5,
    Adjustment = 6,
    Matched = 7
}

public enum ReconciliationItemStatus
{
    Unmatched = 0,
    Matched = 1,
    Adjusted = 2,
    Ignored = 3
}

public enum BankStatementStatus
{
    Imported = 0,
    Processing = 1,
    Reconciled = 2,
    PartiallyReconciled = 3,
    Failed = 4
}
