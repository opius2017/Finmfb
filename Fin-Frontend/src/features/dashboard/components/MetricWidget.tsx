import React from 'react';
import { clsx } from 'clsx';
import { TrendingUp, TrendingDown } from 'lucide-react';
import { motion } from 'framer-motion';
import type { MetricConfig } from '../types/widget.types';

export interface MetricWidgetProps {
  config: MetricConfig;
}

export const MetricWidget: React.FC<MetricWidgetProps> = ({ config }) => {
  const formatValue = (value: number | string): string => {
    if (typeof value === 'string') return value;

    switch (config.format) {
      case 'currency':
        return new Intl.NumberFormat('en-NG', {
          style: 'currency',
          currency: 'NGN',
          minimumFractionDigits: 0,
          maximumFractionDigits: 0,
        }).format(value);
      case 'percentage':
        return `${value.toFixed(1)}%`;
      case 'number':
      default:
        return new Intl.NumberFormat('en-NG').format(value);
    }
  };

  const getTrendColor = () => {
    if (!config.trend) return '';
    
    const isPositive = config.trend.isPositive ?? config.trend.direction === 'up';
    return isPositive ? 'text-success-600 dark:text-success-400' : 'text-error-600 dark:text-error-400';
  };

  const TrendIcon = config.trend?.direction === 'up' ? TrendingUp : TrendingDown;

  return (
    <div className="flex flex-col h-full justify-center p-6">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-neutral-600 dark:text-neutral-400 mb-2">
            {config.label}
          </p>
          
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            <p className={clsx(
              'text-3xl font-bold',
              config.color || 'text-neutral-900 dark:text-neutral-100'
            )}>
              {formatValue(config.value)}
            </p>
          </motion.div>

          {config.trend && (
            <motion.div
              initial={{ opacity: 0, x: -10 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.3, delay: 0.1 }}
              className={clsx('flex items-center gap-1 mt-2', getTrendColor())}
            >
              <TrendIcon className="h-4 w-4" />
              <span className="text-sm font-medium">
                {Math.abs(config.trend.value)}%
              </span>
              <span className="text-xs text-neutral-500 dark:text-neutral-400">
                vs last period
              </span>
            </motion.div>
          )}
        </div>

        {config.icon && (
          <div className={clsx(
            'flex items-center justify-center',
            'w-12 h-12 rounded-lg',
            'bg-primary-100 dark:bg-primary-900/20',
            'text-primary-600 dark:text-primary-400'
          )}>
            <span className="text-2xl">{config.icon}</span>
          </div>
        )}
      </div>
    </div>
  );
};
