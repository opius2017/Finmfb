// Variance Analysis Types
export interface VarianceReport {
  budgetId: string;
  budgetName: string;
  period: Period;
  lines: VarianceReportLine[];
  summary: VarianceSummary;
  generatedAt: Date;
}

export interface Period {
  type: 'month' | 'quarter' | 'year' | 'ytd' | 'custom';
  startDate: Date;
  endDate: Date;
  label: string;
}

export interface VarianceReportLine {
  accountId: string;
  accountCode: string;
  accountName: string;
  category: string;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  varianceType: VarianceType;
  explanation?: string;
  trend: TrendIndicator;
}

export type VarianceType = 'favorable' | 'unfavorable' | 'neutral';

export type TrendIndicator = 'improving' | 'declining' | 'stable';

export interface VarianceSummary {
  totalBudget: number;
  totalActual: number;
  totalVariance: number;
  variancePercentage: number;
  favorableVariances: number;
  unfavorableVariances: number;
  significantVariances: number;
  variancesByCategory: CategoryVariance[];
}

export interface CategoryVariance {
  category: string;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  accountCount: number;
}

export interface VarianceAlert {
  id: string;
  budgetId: string;
  accountId: string;
  accountName: string;
  variance: number;
  variancePercentage: number;
  threshold: number;
  severity: 'low' | 'medium' | 'high' | 'critical';
  status: 'open' | 'acknowledged' | 'resolved';
  createdAt: Date;
  acknowledgedBy?: string;
  acknowledgedAt?: Date;
  resolution?: string;
  resolvedAt?: Date;
}

export interface VarianceExplanation {
  id: string;
  budgetId: string;
  lineId: string;
  accountName: string;
  variance: number;
  explanation: string;
  actionPlan?: string;
  submittedBy: string;
  submittedAt: Date;
  approvedBy?: string;
  approvedAt?: Date;
  status: 'pending' | 'approved' | 'rejected';
}

export interface VarianceTrend {
  accountId: string;
  accountName: string;
  periods: PeriodVariance[];
  trendDirection: 'up' | 'down' | 'stable';
  averageVariance: number;
  volatility: number;
}

export interface PeriodVariance {
  period: string;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
}

export interface VarianceThreshold {
  id: string;
  name: string;
  accountCategory?: string;
  accountId?: string;
  thresholdType: 'percentage' | 'absolute';
  thresholdValue: number;
  alertLevel: 'low' | 'medium' | 'high' | 'critical';
  notifyUsers: string[];
  enabled: boolean;
}

export interface VarianceFilter {
  budgetId?: string;
  period?: Period;
  category?: string;
  varianceType?: VarianceType;
  minVariance?: number;
  maxVariance?: number;
  showOnlySignificant?: boolean;
}

export interface VarianceChart {
  type: 'bar' | 'line' | 'waterfall' | 'heatmap';
  data: ChartData[];
  labels: string[];
  title: string;
}

export interface ChartData {
  label: string;
  budget: number;
  actual: number;
  variance: number;
  color?: string;
}
