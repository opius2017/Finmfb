// Multi-Branch Types
export interface Branch {
  id: string;
  code: string;
  name: string;
  description?: string;
  address: Address;
  contact: ContactInfo;
  status: BranchStatus;
  type: BranchType;
  parentBranchId?: string;
  manager: string;
  currency: string;
  timezone: string;
  settings: BranchSettings;
  createdAt: Date;
  updatedAt: Date;
}

export type BranchStatus = 'active' | 'inactive' | 'suspended';

export type BranchType = 'headquarters' | 'regional' | 'local' | 'warehouse' | 'retail';

export interface Address {
  street: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

export interface ContactInfo {
  phone: string;
  email: string;
  fax?: string;
}

export interface BranchSettings {
  allowInterBranchTransfers: boolean;
  requireApprovalForTransfers: boolean;
  autoConsolidate: boolean;
  fiscalYearStart: number; // Month (1-12)
  reportingCurrency: string;
}

export interface BranchPerformance {
  branchId: string;
  branchName: string;
  period: DateRange;
  revenue: number;
  expenses: number;
  profit: number;
  profitMargin: number;
  growth: number;
  metrics: PerformanceMetric[];
}

export interface PerformanceMetric {
  name: string;
  value: number;
  target?: number;
  unit: string;
  trend: 'up' | 'down' | 'stable';
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface BranchComparison {
  period: DateRange;
  branches: BranchPerformance[];
  rankings: BranchRanking[];
  insights: string[];
}

export interface BranchRanking {
  branchId: string;
  branchName: string;
  metric: string;
  value: number;
  rank: number;
}

export interface BranchFinancials {
  branchId: string;
  branchName: string;
  period: DateRange;
  balanceSheet: BalanceSheetData;
  incomeStatement: IncomeStatementData;
  cashFlow: CashFlowData;
}

export interface BalanceSheetData {
  assets: number;
  liabilities: number;
  equity: number;
  currentAssets: number;
  currentLiabilities: number;
  workingCapital: number;
}

export interface IncomeStatementData {
  revenue: number;
  costOfSales: number;
  grossProfit: number;
  operatingExpenses: number;
  operatingIncome: number;
  netIncome: number;
}

export interface CashFlowData {
  operatingCashFlow: number;
  investingCashFlow: number;
  financingCashFlow: number;
  netCashFlow: number;
  cashBalance: number;
}

export interface BranchDashboard {
  totalBranches: number;
  activeBranches: number;
  totalRevenue: number;
  totalProfit: number;
  topPerformers: BranchPerformance[];
  recentActivity: BranchActivity[];
}

export interface BranchActivity {
  id: string;
  branchId: string;
  branchName: string;
  type: ActivityType;
  description: string;
  amount?: number;
  timestamp: Date;
  user: string;
}

export type ActivityType = 
  | 'transfer' 
  | 'transaction' 
  | 'report' 
  | 'consolidation' 
  | 'adjustment';

export interface BranchUser {
  userId: string;
  userName: string;
  branchId: string;
  branchName: string;
  role: string;
  permissions: string[];
  assignedAt: Date;
}

export interface BranchAccess {
  userId: string;
  allowedBranches: string[];
  defaultBranch: string;
  canViewAll: boolean;
  canManageBranches: boolean;
}
