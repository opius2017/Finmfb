/**
 * Predictive Analytics Service
 * Provides forecasting and predictive analytics capabilities
 */

export interface CashFlowForecast {
  dates: Date[];
  predicted: number[];
  confidence: {
    lower: number[];
    upper: number[];
  };
  accuracy: number;
}

export interface RevenueTrend {
  historical: Array<{ date: Date; value: number }>;
  predicted: Array<{ date: Date; value: number }>;
  seasonality: number[];
  trend: 'increasing' | 'decreasing' | 'stable';
  growthRate: number;
}

export interface RiskIndicators {
  creditRisk: number; // 0-100
  liquidityRisk: number; // 0-100
  operationalRisk: number; // 0-100
  overallRisk: number; // 0-100
  riskLevel: 'low' | 'medium' | 'high' | 'critical';
}

class AnalyticsService {
  /**
   * Generate cash flow forecast for the next N days
   */
  async generateCashFlowForecast(
    historicalData: Array<{ date: Date; amount: number }>,
    days: number = 90
  ): Promise<CashFlowForecast> {
    // Simple moving average with trend analysis
    const values = historicalData.map(d => d.amount);
    const dates: Date[] = [];
    const predicted: number[] = [];
    const lower: number[] = [];
    const upper: number[] = [];

    // Calculate trend
    const trend = this.calculateTrend(values);
    const volatility = this.calculateVolatility(values);
    const lastValue = values[values.length - 1];

    // Generate forecast
    for (let i = 0; i < days; i++) {
      const date = new Date();
      date.setDate(date.getDate() + i + 1);
      dates.push(date);

      // Simple linear projection with seasonality
      const seasonalFactor = this.getSeasonalFactor(date);
      const predictedValue = lastValue + (trend * (i + 1)) * seasonalFactor;
      
      predicted.push(Math.max(0, predictedValue));
      lower.push(Math.max(0, predictedValue - volatility * 1.96));
      upper.push(predictedValue + volatility * 1.96);
    }

    return {
      dates,
      predicted,
      confidence: { lower, upper },
      accuracy: this.calculateForecastAccuracy(historicalData),
    };
  }

  /**
   * Analyze revenue trends and generate predictions
   */
  async analyzeRevenueTrend(
    historicalData: Array<{ date: Date; value: number }>,
    forecastPeriods: number = 12
  ): Promise<RevenueTrend> {
    const values = historicalData.map(d => d.value);
    const trend = this.calculateTrend(values);
    const seasonality = this.detectSeasonality(values);

    // Generate predictions
    const lastValue = values[values.length - 1];
    const predicted: Array<{ date: Date; value: number }> = [];

    for (let i = 0; i < forecastPeriods; i++) {
      const date = new Date();
      date.setMonth(date.getMonth() + i + 1);
      
      const seasonalFactor = seasonality[i % seasonality.length];
      const predictedValue = lastValue + (trend * (i + 1)) * seasonalFactor;
      
      predicted.push({
        date,
        value: Math.max(0, predictedValue),
      });
    }

    // Determine trend direction
    let trendDirection: 'increasing' | 'decreasing' | 'stable';
    if (trend > 0.05) trendDirection = 'increasing';
    else if (trend < -0.05) trendDirection = 'decreasing';
    else trendDirection = 'stable';

    // Calculate growth rate
    const growthRate = this.calculateGrowthRate(values);

    return {
      historical: historicalData,
      predicted,
      seasonality,
      trend: trendDirection,
      growthRate,
    };
  }

  /**
   * Calculate risk indicators
   */
  async calculateRiskIndicators(data: {
    overdueInvoices: number;
    totalInvoices: number;
    currentRatio: number;
    debtToEquity: number;
    operatingCashFlow: number;
    failedTransactions: number;
    totalTransactions: number;
  }): Promise<RiskIndicators> {
    // Credit Risk (based on overdue invoices)
    const creditRisk = Math.min(100, (data.overdueInvoices / data.totalInvoices) * 100);

    // Liquidity Risk (based on current ratio)
    const liquidityRisk = data.currentRatio < 1 
      ? Math.min(100, (1 - data.currentRatio) * 100)
      : Math.max(0, (2 - data.currentRatio) * 50);

    // Operational Risk (based on failed transactions and cash flow)
    const transactionFailureRate = data.failedTransactions / data.totalTransactions;
    const cashFlowRisk = data.operatingCashFlow < 0 ? 50 : 0;
    const operationalRisk = Math.min(100, (transactionFailureRate * 100) + cashFlowRisk);

    // Overall Risk (weighted average)
    const overallRisk = (creditRisk * 0.4 + liquidityRisk * 0.3 + operationalRisk * 0.3);

    // Determine risk level
    let riskLevel: 'low' | 'medium' | 'high' | 'critical';
    if (overallRisk < 25) riskLevel = 'low';
    else if (overallRisk < 50) riskLevel = 'medium';
    else if (overallRisk < 75) riskLevel = 'high';
    else riskLevel = 'critical';

    return {
      creditRisk: Math.round(creditRisk),
      liquidityRisk: Math.round(liquidityRisk),
      operationalRisk: Math.round(operationalRisk),
      overallRisk: Math.round(overallRisk),
      riskLevel,
    };
  }

  /**
   * Calculate trend from historical data
   */
  private calculateTrend(values: number[]): number {
    if (values.length < 2) return 0;

    const n = values.length;
    const xMean = (n - 1) / 2;
    const yMean = values.reduce((a, b) => a + b, 0) / n;

    let numerator = 0;
    let denominator = 0;

    for (let i = 0; i < n; i++) {
      numerator += (i - xMean) * (values[i] - yMean);
      denominator += Math.pow(i - xMean, 2);
    }

    return denominator === 0 ? 0 : numerator / denominator;
  }

  /**
   * Calculate volatility (standard deviation)
   */
  private calculateVolatility(values: number[]): number {
    if (values.length < 2) return 0;

    const mean = values.reduce((a, b) => a + b, 0) / values.length;
    const squaredDiffs = values.map(v => Math.pow(v - mean, 2));
    const variance = squaredDiffs.reduce((a, b) => a + b, 0) / values.length;
    
    return Math.sqrt(variance);
  }

  /**
   * Detect seasonality patterns
   */
  private detectSeasonality(values: number[]): number[] {
    // Simple 12-month seasonality detection
    const seasonalFactors: number[] = [];
    const period = 12;

    if (values.length < period) {
      return Array(period).fill(1);
    }

    const mean = values.reduce((a, b) => a + b, 0) / values.length;

    for (let i = 0; i < period; i++) {
      const seasonalValues = values.filter((_, index) => index % period === i);
      const seasonalMean = seasonalValues.reduce((a, b) => a + b, 0) / seasonalValues.length;
      seasonalFactors.push(seasonalMean / mean);
    }

    return seasonalFactors;
  }

  /**
   * Get seasonal factor for a specific date
   */
  private getSeasonalFactor(date: Date): number {
    // Simple monthly seasonality
    const month = date.getMonth();
    const seasonalPattern = [0.9, 0.85, 0.95, 1.0, 1.05, 1.1, 1.15, 1.1, 1.05, 1.0, 0.95, 1.2];
    return seasonalPattern[month];
  }

  /**
   * Calculate forecast accuracy
   */
  private calculateForecastAccuracy(historicalData: Array<{ date: Date; amount: number }>): number {
    // Simple accuracy metric based on data consistency
    if (historicalData.length < 2) return 0.5;

    const values = historicalData.map(d => d.amount);
    const volatility = this.calculateVolatility(values);
    const mean = values.reduce((a, b) => a + b, 0) / values.length;
    
    const coefficientOfVariation = mean === 0 ? 1 : volatility / mean;
    const accuracy = Math.max(0, Math.min(1, 1 - coefficientOfVariation));
    
    return Math.round(accuracy * 100) / 100;
  }

  /**
   * Calculate growth rate
   */
  private calculateGrowthRate(values: number[]): number {
    if (values.length < 2) return 0;

    const firstValue = values[0];
    const lastValue = values[values.length - 1];
    
    if (firstValue === 0) return 0;
    
    const growthRate = ((lastValue - firstValue) / firstValue) * 100;
    return Math.round(growthRate * 100) / 100;
  }
}

// Export singleton instance
export const analyticsService = new AnalyticsService();
