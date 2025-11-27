using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Banking;

public class BankReconciliation : AuditableEntity
{
    public string BankAccountId { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public DateTime ReconciliationDate { get; set; }
    public DateTime StatementStartDate { get; set; }
    public DateTime StatementEndDate { get; set; }
    
    public decimal StatementOpeningBalance { get; set; }
    public decimal StatementClosingBalance { get; set; }
    public decimal BookOpeningBalance { get; set; }
    public decimal BookClosingBalance { get; set; }
    
    public decimal TotalDepositsInTransit { get; set; }
    public decimal TotalOutstandingChecks { get; set; }
    public decimal TotalBankCharges { get; set; }
    public decimal TotalBankInterest { get; set; }
    public decimal TotalAdjustments { get; set; }
    
    public decimal ReconciledBalance { get; set; }
    public decimal Variance { get; set; }
    
    public ReconciliationStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? ReconciliationReference { get; set; }
    
    public string ReconciledBy { get; set; } = string.Empty;
    public DateTime? ReconciledDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    public ICollection<BankReconciliationItem> ReconciliationItems { get; set; } = new List<BankReconciliationItem>();
}

public class BankReconciliationItem : AuditableEntity
{
    public string BankReconciliationId { get; set; } = string.Empty;
    public BankReconciliation BankReconciliation { get; set; } = null!;
    
    public string? TransactionId { get; set; }
    public string? StatementLineId { get; set; }
    
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? CheckNumber { get; set; }
    
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Amount { get; set; }
    
    public ReconciliationItemType ItemType { get; set; }
    public ReconciliationItemStatus ItemStatus { get; set; }
    
    public bool IsMatched { get; set; }
    public string? MatchedTransactionId { get; set; }
    public DateTime? MatchedDate { get; set; }
    public string? MatchedBy { get; set; }
    
    public string? Notes { get; set; }
}

public class BankStatement : AuditableEntity
{
    public string BankAccountId { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    
    public DateTime StatementDate { get; set; }
    public DateTime StatementStartDate { get; set; }
    public DateTime StatementEndDate { get; set; }
    
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    
    public string? StatementReference { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    
    public BankStatementStatus Status { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public DateTime ImportedDate { get; set; }
    
    public ICollection<BankStatementLine> StatementLines { get; set; } = new List<BankStatementLine>();
}

public class BankStatementLine : AuditableEntity
{
    public string BankStatementId { get; set; } = string.Empty;
    public BankStatement BankStatement { get; set; } = null!;
    
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? CheckNumber { get; set; }
    
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }
    
    public bool IsReconciled { get; set; }
    public string? ReconciledTransactionId { get; set; }
    public DateTime? ReconciledDate { get; set; }
    public string? ReconciledBy { get; set; }
    
    public string? Notes { get; set; }
}
