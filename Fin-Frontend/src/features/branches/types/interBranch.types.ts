// Inter-Branch Transaction Types
export interface InterBranchTransfer {
  id: string;
  transferNumber: string;
  fromBranchId: string;
  fromBranchName: string;
  toBranchId: string;
  toBranchName: string;
  amount: number;
  currency: string;
  description: string;
  reference?: string;
  status: TransferStatus;
  type: TransferType;
  accountMapping: TransferAccountMapping;
  approvals: TransferApproval[];
  createdBy: string;
  createdAt: Date;
  approvedAt?: Date;
  executedAt?: Date;
  reconciledAt?: Date;
}

export type TransferStatus = 
  | 'draft' 
  | 'pending_approval' 
  | 'approved' 
  | 'executed' 
  | 'reconciled' 
  | 'rejected' 
  | 'cancelled';

export type TransferType = 
  | 'cash' 
  | 'inventory' 
  | 'asset' 
  | 'expense_allocation' 
  | 'revenue_allocation';

export interface TransferAccountMapping {
  fromBranchDebitAccount: string;
  fromBranchCreditAccount: string;
  toBranchDebitAccount: string;
  toBranchCreditAccount: string;
  interBranchAccount: string;
}

export interface TransferApproval {
  id: string;
  approverId: string;
  approverName: string;
  approverRole: string;
  decision: 'approved' | 'rejected';
  comment?: string;
  timestamp: Date;
}

export interface InterBranchReconciliation {
  id: string;
  period: DateRange;
  fromBranchId: string;
  toBranchId: string;
  transfers: InterBranchTransfer[];
  fromBranchBalance: number;
  toBranchBalance: number;
  difference: number;
  status: ReconciliationStatus;
  adjustments: ReconciliationAdjustment[];
  reconciledBy?: string;
  reconciledAt?: Date;
}

export type ReconciliationStatus = 'pending' | 'in_progress' | 'reconciled' | 'disputed';

export interface ReconciliationAdjustment {
  id: string;
  type: 'timing_difference' | 'error_correction' | 'exchange_rate' | 'other';
  amount: number;
  description: string;
  journalEntryId?: string;
  createdBy: string;
  createdAt: Date;
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface InterBranchReport {
  period: DateRange;
  branches: BranchTransferSummary[];
  totalTransfers: number;
  totalAmount: number;
  pendingReconciliation: number;
  unreconciledAmount: number;
}

export interface BranchTransferSummary {
  branchId: string;
  branchName: string;
  transfersOut: number;
  transfersIn: number;
  amountOut: number;
  amountIn: number;
  netPosition: number;
}

export interface TransferJournalEntry {
  transferId: string;
  branchId: string;
  branchName: string;
  entries: JournalEntryLine[];
  totalDebit: number;
  totalCredit: number;
  createdAt: Date;
}

export interface JournalEntryLine {
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  description: string;
}

export interface TransferApprovalWorkflow {
  transferId: string;
  requiredApprovers: ApproverConfig[];
  currentApprovals: TransferApproval[];
  status: 'pending' | 'approved' | 'rejected';
  nextApprover?: string;
}

export interface ApproverConfig {
  role: string;
  minAmount?: number;
  maxAmount?: number;
  required: boolean;
}

export interface TransferNotification {
  id: string;
  transferId: string;
  recipientId: string;
  type: NotificationType;
  message: string;
  read: boolean;
  createdAt: Date;
}

export type NotificationType = 
  | 'transfer_created' 
  | 'approval_required' 
  | 'transfer_approved' 
  | 'transfer_rejected' 
  | 'transfer_executed' 
  | 'reconciliation_required';
