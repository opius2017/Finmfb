import { analyticsService } from '../services/analyticsService';

describe('Analytics Service', () => {
  describe('Cash Flow Forecast', () => {
    it('should generate cash flow forecast', async () => {
      const historicalData = [
        { date: new Date('2024-01-01'), amount: 10000 },
        { date: new Date('2024-01-02'), amount: 12000 },
        { date: new Date('2024-01-03'), amount: 11000 },
        { date: new Date('2024-01-04'), amount: 13000 },
        { date: new Date('2024-01-05'), amount: 14000 },
      ];

      const forecast = await analyticsService.generateCashFlowForecast(historicalData, 30);

      expect(forecast).toBeDefined();
      expect(forecast.dates).toHaveLength(30);
      expect(forecast.predicted).toHaveLength(30);
      expect(forecast.confidence.lower).toHaveLength(30);
      expect(forecast.confidence.upper).toHaveLength(30);
      expect(forecast.accuracy).toBeGreaterThanOrEqual(0);
      expect(forecast.accuracy).toBeLessThanOrEqual(1);
    });

    it('should handle empty historical data', async () => {
      const forecast = await analyticsService.generateCashFlowForecast([], 30);

      expect(forecast).toBeDefined();
      expect(forecast.dates).toHaveLength(30);
    });

    it('should generate positive predictions', async () => {
      const historicalData = [
        { date: new Date('2024-01-01'), amount: 10000 },
        { date: new Date('2024-01-02'), amount: 12000 },
        { date: new Date('2024-01-03'), amount: 14000 },
      ];

      const forecast = await analyticsService.generateCashFlowForecast(historicalData, 10);

      // All predictions should be non-negative
      forecast.predicted.forEach(value => {
        expect(value).toBeGreaterThanOrEqual(0);
      });
    });
  });

  describe('Revenue Trend Analysis', () => {
    it('should analyze revenue trend', async () => {
      const historicalData = [
        { date: new Date('2024-01-01'), value: 10000 },
        { date: new Date('2024-02-01'), value: 12000 },
        { date: new Date('2024-03-01'), value: 14000 },
        { date: new Date('2024-04-01'), value: 16000 },
      ];

      const trend = await analyticsService.analyzeRevenueTrend(historicalData, 6);

      expect(trend).toBeDefined();
      expect(trend.historical).toEqual(historicalData);
      expect(trend.predicted).toHaveLength(6);
      expect(trend.seasonality).toBeDefined();
      expect(trend.trend).toMatch(/increasing|decreasing|stable/);
      expect(typeof trend.growthRate).toBe('number');
    });

    it('should detect increasing trend', async () => {
      const historicalData = [
        { date: new Date('2024-01-01'), value: 10000 },
        { date: new Date('2024-02-01'), value: 15000 },
        { date: new Date('2024-03-01'), value: 20000 },
      ];

      const trend = await analyticsService.analyzeRevenueTrend(historicalData, 3);

      expect(trend.trend).toBe('increasing');
      expect(trend.growthRate).toBeGreaterThan(0);
    });

    it('should detect decreasing trend', async () => {
      const historicalData = [
        { date: new Date('2024-01-01'), value: 20000 },
        { date: new Date('2024-02-01'), value: 15000 },
        { date: new Date('2024-03-01'), value: 10000 },
      ];

      const trend = await analyticsService.analyzeRevenueTrend(historicalData, 3);

      expect(trend.trend).toBe('decreasing');
      expect(trend.growthRate).toBeLessThan(0);
    });
  });

  describe('Risk Indicators', () => {
    it('should calculate risk indicators', async () => {
      const data = {
        overdueInvoices: 10,
        totalInvoices: 100,
        currentRatio: 1.5,
        debtToEquity: 0.5,
        operatingCashFlow: 50000,
        failedTransactions: 5,
        totalTransactions: 1000,
      };

      const indicators = await analyticsService.calculateRiskIndicators(data);

      expect(indicators).toBeDefined();
      expect(indicators.creditRisk).toBeGreaterThanOrEqual(0);
      expect(indicators.creditRisk).toBeLessThanOrEqual(100);
      expect(indicators.liquidityRisk).toBeGreaterThanOrEqual(0);
      expect(indicators.liquidityRisk).toBeLessThanOrEqual(100);
      expect(indicators.operationalRisk).toBeGreaterThanOrEqual(0);
      expect(indicators.operationalRisk).toBeLessThanOrEqual(100);
      expect(indicators.overallRisk).toBeGreaterThanOrEqual(0);
      expect(indicators.overallRisk).toBeLessThanOrEqual(100);
      expect(indicators.riskLevel).toMatch(/low|medium|high|critical/);
    });

    it('should identify low risk', async () => {
      const data = {
        overdueInvoices: 1,
        totalInvoices: 100,
        currentRatio: 2.0,
        debtToEquity: 0.3,
        operatingCashFlow: 100000,
        failedTransactions: 1,
        totalTransactions: 1000,
      };

      const indicators = await analyticsService.calculateRiskIndicators(data);

      expect(indicators.riskLevel).toBe('low');
      expect(indicators.overallRisk).toBeLessThan(25);
    });

    it('should identify high risk', async () => {
      const data = {
        overdueInvoices: 50,
        totalInvoices: 100,
        currentRatio: 0.5,
        debtToEquity: 2.0,
        operatingCashFlow: -10000,
        failedTransactions: 100,
        totalTransactions: 1000,
      };

      const indicators = await analyticsService.calculateRiskIndicators(data);

      expect(indicators.riskLevel).toMatch(/high|critical/);
      expect(indicators.overallRisk).toBeGreaterThan(50);
    });

    it('should calculate credit risk correctly', async () => {
      const data = {
        overdueInvoices: 25,
        totalInvoices: 100,
        currentRatio: 1.5,
        debtToEquity: 0.5,
        operatingCashFlow: 50000,
        failedTransactions: 5,
        totalTransactions: 1000,
      };

      const indicators = await analyticsService.calculateRiskIndicators(data);

      expect(indicators.creditRisk).toBe(25);
    });
  });
});
