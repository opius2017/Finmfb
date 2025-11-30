// Budget Types
export interface Budget {
  id: string;
  name: string;
  fiscalYear: number;
  startDate: Date;
  endDate: Date;
  status: BudgetStatus;
  type: BudgetType;
  lines: BudgetLine[];
  totalBudget: number;
  totalActual: number;
  totalVariance: number;
  variancePercentage: number;
  version: number;
  parentBudgetId?: string;
  approvalWorkflow: ApprovalWorkflow;
  createdBy: string;
  createdAt: Date;
  updatedBy: string;
  updatedAt: Date;
  approvedBy?: string;
  approvedAt?: Date;
}

export type BudgetStatus = 
  | 'draft' 
  | 'submitted' 
  | 'under-review' 
  | 'approved' 
  | 'active' 
  | 'closed' 
  | 'rejected';

export type BudgetType = 
  | 'operating' 
  | 'capital' 
  | 'cash-flow' 
  | 'project' 
  | 'departmental';

export interface BudgetLine {
  id: string;
  budgetId: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  departmentId?: string;
  departmentName?: string;
  costCenterId?: string;
  costCenterName?: string;
  periods: BudgetPeriod[];
  total: number;
  actual: number;
  variance: number;
  variancePercentage: number;
  notes?: string;
}

export interface BudgetPeriod {
  month: number;
  year: number;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  locked: boolean;
}

export interface ApprovalWorkflow {
  levels: ApprovalLevel[];
  currentLevel: number;
  status: 'pending' | 'approved' | 'rejected';
  requiresApproval: boolean;
}

export interface ApprovalLevel {
  level: number;
  approverRole: string;
  approverName?: string;
  status: 'pending' | 'approved' | 'rejected';
  comments?: string;
  approvedAt?: Date;
  required: boolean;
}

export interface BudgetTemplate {
  id: string;
  name: string;
  description: string;
  industry: string;
  type: BudgetType;
  accounts: TemplateAccount[];
  isDefault: boolean;
  createdBy: string;
  createdAt: Date;
}

export interface TemplateAccount {
  accountCode: string;
  accountName: string;
  category: string;
  defaultAmount?: number;
  calculationFormula?: string;
  notes?: string;
}

export interface BudgetCopyOptions {
  sourceBudgetId: string;
  targetYear: number;
  adjustmentType: 'percentage' | 'fixed' | 'none';
  adjustmentValue: number;
  copyActuals: boolean;
  copyNotes: boolean;
}

export interface BudgetComparison {
  budget1: Budget;
  budget2: Budget;
  differences: BudgetDifference[];
  summary: ComparisonSummary;
}

export interface BudgetDifference {
  accountCode: string;
  accountName: string;
  budget1Amount: number;
  budget2Amount: number;
  difference: number;
  differencePercentage: number;
}

export interface ComparisonSummary {
  totalDifference: number;
  differencePercentage: number;
  accountsChanged: number;
  accountsAdded: number;
  accountsRemoved: number;
}

export interface BudgetAllocation {
  budgetId: string;
  accountId: string;
  totalAmount: number;
  allocationMethod: 'equal' | 'weighted' | 'custom';
  periods: PeriodAllocation[];
}

export interface PeriodAllocation {
  month: number;
  year: number;
  amount: number;
  percentage: number;
}

export interface BudgetFilter {
  fiscalYear?: number;
  status?: BudgetStatus;
  type?: BudgetType;
  departmentId?: string;
  createdBy?: string;
  search?: string;
}

export interface BudgetStatistics {
  totalBudgets: number;
  activeBudgets: number;
  totalBudgetAmount: number;
  totalActualAmount: number;
  overallVariance: number;
  overallVariancePercentage: number;
  budgetsByStatus: StatusCount[];
  budgetsByType: TypeCount[];
}

export interface StatusCount {
  status: BudgetStatus;
  count: number;
  totalAmount: number;
}

export interface TypeCount {
  type: BudgetType;
  count: number;
  totalAmount: number;
}

export interface BudgetVersion {
  version: number;
  budgetId: string;
  createdBy: string;
  createdAt: Date;
  changes: string;
  snapshot: Budget;
}

export interface BudgetNote {
  id: string;
  budgetId: string;
  lineId?: string;
  note: string;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}
