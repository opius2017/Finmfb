/**
 * Bank Reconciliation Types
 * Type definitions for bank reconciliation system
 */

export type StatementFormat = 'CSV' | 'Excel' | 'OFX' | 'MT940' | 'PDF';

export type MatchType = 'exact' | 'fuzzy' | 'rule-based' | 'manual' | 'unmatched';

export type ReconciliationStatus = 'in-progress' | 'completed' | 'approved' | 'rejected';

export interface BankTransaction {
  id: string;
  date: Date;
  description: string;
  reference?: string;
  debit?: number;
  credit?: number;
  balance?: number;
  category?: string;
  metadata?: Record<string, any>;
}

export interface InternalTransaction {
  id: string;
  date: Date;
  description: string;
  reference?: string;
  amount: number;
  type: 'debit' | 'credit';
  accountId: string;
  accountName: string;
  category?: string;
  metadata?: Record<string, any>;
}

export interface ReconciliationMatch {
  id: string;
  bankTransaction: BankTransaction;
  internalTransaction: InternalTransaction;
  matchType: MatchType;
  confidence: number; // 0-100
  matchedBy: string;
  matchedAt: Date;
  notes?: string;
}

export interface MatchingRule {
  id: string;
  name: string;
  description?: string;
  conditions: MatchCondition[];
  priority: number;
  enabled: boolean;
  createdBy: string;
  createdAt: Date;
}

export interface MatchCondition {
  field: 'amount' | 'date' | 'description' | 'reference';
  operator: 'equals' | 'contains' | 'startsWith' | 'endsWith' | 'range';
  value: any;
  tolerance?: number; // For amount matching
}

export interface BankStatement {
  id: string;
  accountId: string;
  accountName: string;
  accountNumber: string;
  statementDate: Date;
  openingBalance: number;
  closingBalance: number;
  format: StatementFormat;
  fileName: string;
  uploadedBy: string;
  uploadedAt: Date;
  transactions: BankTransaction[];
}

export interface ReconciliationSession {
  id: string;
  accountId: string;
  accountName: string;
  statementId: string;
  statementDate: Date;
  openingBalance: number;
  closingBalance: number;
  bookBalance: number;
  difference: number;
  status: ReconciliationStatus;
  bankTransactions: BankTransaction[];
  internalTransactions: InternalTransaction[];
  matches: ReconciliationMatch[];
  unmatchedBank: BankTransaction[];
  unmatchedInternal: InternalTransaction[];
  adjustments: AdjustmentEntry[];
  reconciledBy: string;
  reconciledAt: Date;
  approvedBy?: string;
  approvedAt?: Date;
  notes?: string;
}

export interface AdjustmentEntry {
  id: string;
  type: 'bank-error' | 'book-error' | 'timing-difference' | 'other';
  description: string;
  amount: number;
  accountId: string;
  requiresApproval: boolean;
  approved: boolean;
  approvedBy?: string;
  approvedAt?: Date;
  createdBy: string;
  createdAt: Date;
}

export interface ImportResult {
  success: boolean;
  statement?: BankStatement;
  errors: ImportError[];
  warnings: ImportWarning[];
  transactionCount: number;
  duplicateCount: number;
}

export interface ImportError {
  row?: number;
  field?: string;
  message: string;
  severity: 'error' | 'warning';
}

export interface ImportWarning {
  row?: number;
  message: string;
}

export interface MatchSuggestion {
  bankTransaction: BankTransaction;
  internalTransaction: InternalTransaction;
  confidence: number;
  matchType: MatchType;
  reasons: string[];
}

export interface ReconciliationReport {
  sessionId: string;
  accountName: string;
  statementDate: Date;
  openingBalance: number;
  closingBalance: number;
  bookBalance: number;
  difference: number;
  matchedCount: number;
  unmatchedBankCount: number;
  unmatchedInternalCount: number;
  adjustmentCount: number;
  totalAdjustments: number;
  status: ReconciliationStatus;
  generatedAt: Date;
  generatedBy: string;
}
