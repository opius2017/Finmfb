// Consolidation Types
export interface ConsolidationReport {
  id: string;
  name: string;
  period: DateRange;
  branches: string[];
  status: ConsolidationStatus;
  financialStatements: ConsolidatedFinancials;
  eliminations: EliminationEntry[];
  adjustments: ConsolidationAdjustment[];
  createdBy: string;
  createdAt: Date;
  approvedBy?: string;
  approvedAt?: Date;
}

export type ConsolidationStatus = 'draft' | 'in_progress' | 'completed' | 'approved';

export interface DateRange {
  from: Date;
  to: Date;
}

export interface ConsolidatedFinancials {
  balanceSheet: ConsolidatedBalanceSheet;
  incomeStatement: ConsolidatedIncomeStatement;
  cashFlowStatement: ConsolidatedCashFlow;
}

export interface ConsolidatedBalanceSheet {
  assets: AssetSection;
  liabilities: LiabilitySection;
  equity: EquitySection;
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
}

export interface AssetSection {
  currentAssets: LineItem[];
  nonCurrentAssets: LineItem[];
  totalCurrentAssets: number;
  totalNonCurrentAssets: number;
}

export interface LiabilitySection {
  currentLiabilities: LineItem[];
  nonCurrentLiabilities: LineItem[];
  totalCurrentLiabilities: number;
  totalNonCurrentLiabilities: number;
}

export interface EquitySection {
  items: LineItem[];
  minorityInterest: number;
  totalEquity: number;
}

export interface LineItem {
  accountCode: string;
  accountName: string;
  branchAmounts: BranchAmount[];
  eliminations: number;
  adjustments: number;
  consolidated: number;
}

export interface BranchAmount {
  branchId: string;
  branchName: string;
  amount: number;
}

export interface ConsolidatedIncomeStatement {
  revenue: LineItem[];
  costOfSales: LineItem[];
  operatingExpenses: LineItem[];
  otherIncome: LineItem[];
  otherExpenses: LineItem[];
  totalRevenue: number;
  grossProfit: number;
  operatingIncome: number;
  netIncome: number;
  minorityInterestShare: number;
  netIncomeAttributableToParent: number;
}

export interface ConsolidatedCashFlow {
  operatingActivities: LineItem[];
  investingActivities: LineItem[];
  financingActivities: LineItem[];
  netOperatingCashFlow: number;
  netInvestingCashFlow: number;
  netFinancingCashFlow: number;
  netCashFlow: number;
  openingCash: number;
  closingCash: number;
}

export interface EliminationEntry {
  id: string;
  type: EliminationType;
  description: string;
  debitAccount: string;
  creditAccount: string;
  amount: number;
  affectedBranches: string[];
  reference?: string;
  createdBy: string;
  createdAt: Date;
}

export type EliminationType = 
  | 'inter_branch_transfer' 
  | 'inter_branch_sale' 
  | 'inter_branch_loan' 
  | 'unrealized_profit' 
  | 'dividend' 
  | 'management_fee';

export interface ConsolidationAdjustment {
  id: string;
  type: AdjustmentType;
  description: string;
  accountCode: string;
  accountName: string;
  amount: number;
  branchId?: string;
  reason: string;
  createdBy: string;
  createdAt: Date;
}

export type AdjustmentType = 
  | 'reclassification' 
  | 'accrual' 
  | 'provision' 
  | 'fair_value' 
  | 'currency_translation' 
  | 'other';

export interface MinorityInterest {
  branchId: string;
  branchName: string;
  ownershipPercentage: number;
  minorityPercentage: number;
  netAssets: number;
  minorityShare: number;
  profitShare: number;
}

export interface ConsolidationWorksheet {
  period: DateRange;
  branches: BranchFinancialData[];
  eliminations: EliminationEntry[];
  adjustments: ConsolidationAdjustment[];
  consolidated: ConsolidatedFinancials;
}

export interface BranchFinancialData {
  branchId: string;
  branchName: string;
  balanceSheet: any;
  incomeStatement: any;
  cashFlow: any;
}

export interface ConsolidationRule {
  id: string;
  name: string;
  type: EliminationType;
  description: string;
  autoApply: boolean;
  conditions: RuleCondition[];
  accountMappings: AccountMapping[];
  isActive: boolean;
}

export interface RuleCondition {
  field: string;
  operator: 'equals' | 'contains' | 'greater_than' | 'less_than';
  value: any;
}

export interface AccountMapping {
  sourceAccount: string;
  targetAccount: string;
  percentage?: number;
}

export interface ConsolidationAuditTrail {
  id: string;
  consolidationId: string;
  action: string;
  performedBy: string;
  timestamp: Date;
  changes: Record<string, any>;
  comment?: string;
}
