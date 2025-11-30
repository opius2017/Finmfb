// Rolling Forecast Types
export interface Forecast {
  id: string;
  budgetId: string;
  name: string;
  forecastType: 'rolling' | 'static';
  horizonMonths: number;
  updateFrequency: 'monthly' | 'quarterly';
  lastUpdated: Date;
  nextUpdate: Date;
  periods: ForecastPeriod[];
  accuracy: ForecastAccuracy;
  status: 'active' | 'paused' | 'archived';
}

export interface ForecastPeriod {
  month: number;
  year: number;
  forecastAmount: number;
  actualAmount?: number;
  variance?: number;
  confidence: number;
  adjustments: ForecastAdjustment[];
  locked: boolean;
}

export interface ForecastAdjustment {
  type: 'seasonality' | 'trend' | 'manual' | 'event';
  amount: number;
  reason: string;
  appliedAt: Date;
  appliedBy: string;
}

export interface ForecastAccuracy {
  overallAccuracy: number;
  mape: number; // Mean Absolute Percentage Error
  rmse: number; // Root Mean Square Error
  bias: number;
  lastCalculated: Date;
  periodAccuracies: PeriodAccuracy[];
}

export interface PeriodAccuracy {
  period: string;
  forecast: number;
  actual: number;
  error: number;
  errorPercentage: number;
}

export interface SeasonalityPattern {
  accountId: string;
  accountName: string;
  pattern: 'monthly' | 'quarterly' | 'yearly';
  factors: number[];
  strength: number;
  detected: boolean;
}

export interface ForecastComparison {
  forecastId: string;
  periods: ComparisonPeriod[];
  summary: {
    totalForecast: number;
    totalActual: number;
    totalVariance: number;
    accuracyRate: number;
  };
}

export interface ComparisonPeriod {
  period: string;
  forecast: number;
  actual: number;
  variance: number;
  variancePercentage: number;
}
