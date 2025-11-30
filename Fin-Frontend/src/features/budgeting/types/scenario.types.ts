// Scenario Planning Types
export interface Scenario {
  id: string;
  name: string;
  description: string;
  baseBudgetId: string;
  baseScenarioId?: string;
  type: ScenarioType;
  assumptions: Assumption[];
  adjustments: ScenarioAdjustment[];
  results: ScenarioResults;
  status: 'draft' | 'active' | 'archived';
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export type ScenarioType = 'best-case' | 'worst-case' | 'most-likely' | 'custom';

export interface Assumption {
  id: string;
  name: string;
  description: string;
  value: number;
  unit: 'percentage' | 'amount' | 'multiplier';
  category: string;
}

export interface ScenarioAdjustment {
  id: string;
  accountId: string;
  accountName: string;
  adjustmentType: 'percentage' | 'fixed' | 'formula';
  adjustmentValue: number;
  formula?: string;
  appliedAmount: number;
  reason?: string;
}

export interface ScenarioResults {
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
  cashFlow: number;
  profitMargin: number;
  breakEvenPoint?: number;
  roi?: number;
}

export interface ScenarioComparison {
  scenarios: Scenario[];
  comparisonMetrics: ComparisonMetric[];
  recommendations: string[];
  bestScenario?: string;
  worstScenario?: string;
}

export interface ComparisonMetric {
  metricName: string;
  values: { [scenarioId: string]: number };
  unit: string;
  bestValue: number;
  worstValue: number;
}

export interface WhatIfAnalysis {
  variable: string;
  baseValue: number;
  testValues: number[];
  results: WhatIfResult[];
  sensitivity: number;
}

export interface WhatIfResult {
  inputValue: number;
  outputMetrics: { [metric: string]: number };
  percentageChange: number;
}

export interface ImpactAnalysis {
  scenarioId: string;
  changes: Change[];
  totalImpact: number;
  impactByCategory: CategoryImpact[];
  riskLevel: 'low' | 'medium' | 'high';
  confidence: number;
}

export interface Change {
  description: string;
  affectedAccounts: string[];
  estimatedImpact: number;
  probability: number;
}

export interface CategoryImpact {
  category: string;
  impact: number;
  impactPercentage: number;
}
