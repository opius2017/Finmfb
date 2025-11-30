// Financial Report Types
export interface TrialBalance {
  asOfDate: Date;
  accounts: TrialBalanceAccount[];
  totals: {
    totalDebits: number;
    totalCredits: number;
    difference: number;
  };
}

export interface TrialBalanceAccount {
  accountCode: string;
  accountName: string;
  accountType: string;
  debit: number;
  credit: number;
  balance: number;
}

export interface GeneralLedger {
  accountId: string;
  accountCode: string;
  accountName: string;
  period: DateRange;
  openingBalance: number;
  transactions: GLTransaction[];
  closingBalance: number;
}

export interface GLTransaction {
  date: Date;
  reference: string;
  description: string;
  debit: number;
  credit: number;
  balance: number;
  entryType: string;
}

export interface ProfitAndLoss {
  period: DateRange;
  comparative?: DateRange;
  revenue: PLSection;
  costOfSales: PLSection;
  grossProfit: number;
  operatingExpenses: PLSection;
  operatingIncome: number;
  otherIncome: PLSection;
  otherExpenses: PLSection;
  netIncome: number;
  percentages?: PLPercentages;
}

export interface PLSection {
  total: number;
  items: PLItem[];
}

export interface PLItem {
  accountCode: string;
  accountName: string;
  amount: number;
  comparativeAmount?: number;
  variance?: number;
  variancePercentage?: number;
}

export interface PLPercentages {
  grossProfitMargin: number;
  operatingMargin: number;
  netProfitMargin: number;
}

export interface BalanceSheet {
  asOfDate: Date;
  comparative?: Date;
  assets: BSSection;
  liabilities: BSSection;
  equity: BSSection;
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
}

export interface BSSection {
  total: number;
  subsections: BSSubsection[];
}

export interface BSSubsection {
  name: string;
  total: number;
  items: BSItem[];
}

export interface BSItem {
  accountCode: string;
  accountName: string;
  amount: number;
  comparativeAmount?: number;
  variance?: number;
}

export interface CashFlowStatement {
  period: DateRange;
  method: 'direct' | 'indirect';
  operatingActivities: CFSection;
  investingActivities: CFSection;
  financingActivities: CFSection;
  netCashFlow: number;
  openingCash: number;
  closingCash: number;
}

export interface CFSection {
  total: number;
  items: CFItem[];
}

export interface CFItem {
  description: string;
  amount: number;
}

export interface DateRange {
  from: Date;
  to: Date;
}
