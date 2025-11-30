import React from 'react';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  AreaChart,
  Area,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import type { ChartConfig } from '../types/widget.types';

export interface ChartWidgetProps {
  config: ChartConfig;
}

const DEFAULT_COLORS = [
  '#0ea5e9', // primary-500
  '#a855f7', // secondary-500
  '#22c55e', // success-500
  '#f59e0b', // warning-500
  '#ef4444', // error-500
];

export const ChartWidget: React.FC<ChartWidgetProps> = ({ config }) => {
  const colors = config.colors || DEFAULT_COLORS;

  const renderChart = () => {
    const commonProps = {
      data: config.data,
      margin: { top: 5, right: 20, left: 0, bottom: 5 },
    };

    switch (config.type) {
      case 'line':
        return (
          <LineChart {...commonProps}>
            {config.showGrid !== false && (
              <CartesianGrid strokeDasharray="3 3" className="stroke-neutral-200 dark:stroke-neutral-700" />
            )}
            <XAxis
              dataKey={config.xKey || 'name'}
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
            {config.showLegend !== false && <Legend />}
            <Line
              type="monotone"
              dataKey={config.yKey || 'value'}
              stroke={colors[0]}
              strokeWidth={2}
              dot={{ fill: colors[0], r: 4 }}
              activeDot={{ r: 6 }}
            />
          </LineChart>
        );

      case 'bar':
        return (
          <BarChart {...commonProps}>
            {config.showGrid !== false && (
              <CartesianGrid strokeDasharray="3 3" className="stroke-neutral-200 dark:stroke-neutral-700" />
            )}
            <XAxis
              dataKey={config.xKey || 'name'}
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
            {config.showLegend !== false && <Legend />}
            <Bar
              dataKey={config.yKey || 'value'}
              fill={colors[0]}
              radius={[4, 4, 0, 0]}
            />
          </BarChart>
        );

      case 'area':
        return (
          <AreaChart {...commonProps}>
            {config.showGrid !== false && (
              <CartesianGrid strokeDasharray="3 3" className="stroke-neutral-200 dark:stroke-neutral-700" />
            )}
            <XAxis
              dataKey={config.xKey || 'name'}
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
            {config.showLegend !== false && <Legend />}
            <Area
              type="monotone"
              dataKey={config.yKey || 'value'}
              stroke={colors[0]}
              fill={colors[0]}
              fillOpacity={0.3}
            />
          </AreaChart>
        );

      case 'pie':
      case 'donut':
        return (
          <PieChart>
            <Pie
              data={config.data}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
              outerRadius={config.type === 'donut' ? 80 : 100}
              innerRadius={config.type === 'donut' ? 50 : 0}
              fill="#8884d8"
              dataKey={config.yKey || 'value'}
            >
              {config.data.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={colors[index % colors.length]} />
              ))}
            </Pie>
            <Tooltip
              contentStyle={{
                backgroundColor: 'var(--tooltip-bg)',
                border: '1px solid var(--tooltip-border)',
                borderRadius: '0.5rem',
              }}
            />
            {config.showLegend !== false && <Legend />}
          </PieChart>
        );

      default:
        return <div>Unsupported chart type</div>;
    }
  };

  return (
    <div className="w-full h-full min-h-[200px] p-4">
      <ResponsiveContainer width="100%" height="100%">
        {renderChart()}
      </ResponsiveContainer>
    </div>
  );
};
