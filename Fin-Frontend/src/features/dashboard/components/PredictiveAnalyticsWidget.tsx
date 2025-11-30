import React, { useEffect, useState } from 'react';
import { clsx } from 'clsx';
import { TrendingUp, TrendingDown, AlertTriangle, CheckCircle } from 'lucide-react';
import { LineChart, Line, Area, AreaChart, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { analyticsService, type CashFlowForecast, type RevenueTrend, type RiskIndicators } from '../services/analyticsService';

export interface CashFlowForecastWidgetProps {
  historicalData: Array<{ date: Date; amount: number }>;
  days?: number;
}

export const CashFlowForecastWidget: React.FC<CashFlowForecastWidgetProps> = ({
  historicalData,
  days = 90,
}) => {
  const [forecast, setForecast] = useState<CashFlowForecast | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const generateForecast = async () => {
      setLoading(true);
      try {
        const result = await analyticsService.generateCashFlowForecast(historicalData, days);
        setForecast(result);
      } catch (error) {
        console.error('Error generating forecast:', error);
      } finally {
        setLoading(false);
      }
    };

    generateForecast();
  }, [historicalData, days]);

  if (loading || !forecast) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    );
  }

  const chartData = forecast.dates.map((date, index) => ({
    date: date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
    predicted: forecast.predicted[index],
    lower: forecast.confidence.lower[index],
    upper: forecast.confidence.upper[index],
  }));

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-sm font-medium text-neutral-700 dark:text-neutral-300">
            {days}-Day Cash Flow Forecast
          </h3>
          <p className="text-xs text-neutral-500 dark:text-neutral-400">
            Accuracy: {(forecast.accuracy * 100).toFixed(0)}%
          </p>
        </div>
      </div>

      <ResponsiveContainer width="100%" height={250}>
        <AreaChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" className="stroke-neutral-200 dark:stroke-neutral-700" />
          <XAxis
            dataKey="date"
            className="text-xs text-neutral-600 dark:text-neutral-400"
          />
          <YAxis className="text-xs text-neutral-600 dark:text-neutral-400" />
          <Tooltip
            contentStyle={{
              backgroundColor: 'var(--tooltip-bg)',
              border: '1px solid var(--tooltip-border)',
              borderRadius: '0.5rem',
            }}
          />
          <Legend />
          <Area
            type="monotone"
            dataKey="upper"
            stackId="1"
            stroke="none"
            fill="#0ea5e9"
            fillOpacity={0.1}
            name="Upper Bound"
          />
          <Area
            type="monotone"
            dataKey="predicted"
            stackId="2"
            stroke="#0ea5e9"
            fill="#0ea5e9"
            fillOpacity={0.3}
            name="Predicted"
          />
          <Area
            type="monotone"
            dataKey="lower"
            stackId="3"
            stroke="none"
            fill="#0ea5e9"
            fillOpacity={0.1}
            name="Lower Bound"
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};

export interface RevenueTrendWidgetProps {
  historicalData: Array<{ date: Date; value: number }>;
  forecastPeriods?: number;
}

export const RevenueTrendWidget: React.FC<RevenueTrendWidgetProps> = ({
  historicalData,
  forecastPeriods = 12,
}) => {
  const [trend, setTrend] = useState<RevenueTrend | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const analyzeTrend = async () => {
      setLoading(true);
      try {
        const result = await analyticsService.analyzeRevenueTrend(historicalData, forecastPeriods);
        setTrend(result);
      } catch (error) {
        console.error('Error analyzing trend:', error);
      } finally {
        setLoading(false);
      }
    };

    analyzeTrend();
  }, [historicalData, forecastPeriods]);

  if (loading || !trend) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    );
  }

  const TrendIcon = trend.trend === 'increasing' ? TrendingUp : TrendingDown;
  const trendColor = trend.trend === 'increasing' ? 'text-success-600' : 'text-error-600';

  const chartData = [
    ...trend.historical.map(d => ({
      date: d.date.toLocaleDateString('en-US', { month: 'short' }),
      actual: d.value,
      predicted: null,
    })),
    ...trend.predicted.map(d => ({
      date: d.date.toLocaleDateString('en-US', { month: 'short' }),
      actual: null,
      predicted: d.value,
    })),
  ];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-sm font-medium text-neutral-700 dark:text-neutral-300">
            Revenue Trend Analysis
          </h3>
          <div className="flex items-center gap-2 mt-1">
            <TrendIcon className={clsx('h-4 w-4', trendColor)} />
            <span className={clsx('text-sm font-medium', trendColor)}>
              {trend.growthRate > 0 ? '+' : ''}{trend.growthRate.toFixed(1)}% growth
            </span>
          </div>
        </div>
      </div>

      <ResponsiveContainer width="100%" height={250}>
        <LineChart data={chartData}>
          <CartesianGrid strokeDasharray="3 3" className="stroke-neutral-200 dark:stroke-neutral-700" />
          <XAxis
            dataKey="date"
            className="text-xs text-neutral-600 dark:text-neutral-400"
          />
          <YAxis className="text-xs text-neutral-600 dark:text-neutral-400" />
          <Tooltip
            contentStyle={{
              backgroundColor: 'var(--tooltip-bg)',
              border: '1px solid var(--tooltip-border)',
              borderRadius: '0.5rem',
            }}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="actual"
            stroke="#0ea5e9"
            strokeWidth={2}
            dot={{ fill: '#0ea5e9', r: 4 }}
            name="Actual"
          />
          <Line
            type="monotone"
            dataKey="predicted"
            stroke="#a855f7"
            strokeWidth={2}
            strokeDasharray="5 5"
            dot={{ fill: '#a855f7', r: 4 }}
            name="Predicted"
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};

export interface RiskIndicatorsWidgetProps {
  data: {
    overdueInvoices: number;
    totalInvoices: number;
    currentRatio: number;
    debtToEquity: number;
    operatingCashFlow: number;
    failedTransactions: number;
    totalTransactions: number;
  };
}

export const RiskIndicatorsWidget: React.FC<RiskIndicatorsWidgetProps> = ({ data }) => {
  const [indicators, setIndicators] = useState<RiskIndicators | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const calculateRisk = async () => {
      setLoading(true);
      try {
        const result = await analyticsService.calculateRiskIndicators(data);
        setIndicators(result);
      } catch (error) {
        console.error('Error calculating risk:', error);
      } finally {
        setLoading(false);
      }
    };

    calculateRisk();
  }, [data]);

  if (loading || !indicators) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    );
  }

  const getRiskColor = (level: string) => {
    switch (level) {
      case 'low': return 'text-success-600 bg-success-100 dark:bg-success-900/20';
      case 'medium': return 'text-warning-600 bg-warning-100 dark:bg-warning-900/20';
      case 'high': return 'text-error-600 bg-error-100 dark:bg-error-900/20';
      case 'critical': return 'text-error-700 bg-error-200 dark:bg-error-900/40';
      default: return 'text-neutral-600 bg-neutral-100';
    }
  };

  const RiskIcon = indicators.riskLevel === 'low' ? CheckCircle : AlertTriangle;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-neutral-700 dark:text-neutral-300">
          Risk Assessment
        </h3>
        <div className={clsx('flex items-center gap-2 px-3 py-1 rounded-full', getRiskColor(indicators.riskLevel))}>
          <RiskIcon className="h-4 w-4" />
          <span className="text-xs font-medium uppercase">{indicators.riskLevel}</span>
        </div>
      </div>

      <div className="space-y-3">
        <RiskBar label="Credit Risk" value={indicators.creditRisk} />
        <RiskBar label="Liquidity Risk" value={indicators.liquidityRisk} />
        <RiskBar label="Operational Risk" value={indicators.operationalRisk} />
        <div className="pt-2 border-t border-neutral-200 dark:border-neutral-700">
          <RiskBar label="Overall Risk" value={indicators.overallRisk} highlight />
        </div>
      </div>
    </div>
  );
};

const RiskBar: React.FC<{ label: string; value: number; highlight?: boolean }> = ({
  label,
  value,
  highlight = false,
}) => {
  const getColor = () => {
    if (value < 25) return 'bg-success-500';
    if (value < 50) return 'bg-warning-500';
    if (value < 75) return 'bg-error-500';
    return 'bg-error-700';
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-1">
        <span className={clsx(
          'text-sm',
          highlight ? 'font-semibold text-neutral-900 dark:text-neutral-100' : 'text-neutral-700 dark:text-neutral-300'
        )}>
          {label}
        </span>
        <span className={clsx(
          'text-sm font-medium',
          highlight ? 'text-neutral-900 dark:text-neutral-100' : 'text-neutral-600 dark:text-neutral-400'
        )}>
          {value}%
        </span>
      </div>
      <div className="w-full bg-neutral-200 dark:bg-neutral-700 rounded-full h-2">
        <div
          className={clsx('h-2 rounded-full transition-all duration-500', getColor())}
          style={{ width: `${value}%` }}
        />
      </div>
    </div>
  );
};
