import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { clsx } from 'clsx';
import { MoreVertical, RefreshCw, X, Maximize2, Settings } from 'lucide-react';
import { Card } from '@/design-system';
import type { Widget as WidgetType } from '../types/widget.types';

export interface WidgetProps {
  widget: WidgetType;
  onRefresh?: (widgetId: string) => void;
  onRemove?: (widgetId: string) => void;
  onConfigure?: (widgetId: string) => void;
  onResize?: (widgetId: string, size: { w: number; h: number }) => void;
  children: React.ReactNode;
  isDragging?: boolean;
}

export const Widget: React.FC<WidgetProps> = ({
  widget,
  onRefresh,
  onRemove,
  onConfigure,
  children,
  isDragging = false,
}) => {
  const [showMenu, setShowMenu] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const handleRefresh = async () => {
    if (onRefresh && !isRefreshing) {
      setIsRefreshing(true);
      await onRefresh(widget.id);
      setTimeout(() => setIsRefreshing(false), 500);
    }
  };

  const actions = (
    <div className="relative">
      <button
        onClick={() => setShowMenu(!showMenu)}
        className={clsx(
          'p-1 rounded-md transition-colors',
          'text-neutral-500 hover:text-neutral-700 hover:bg-neutral-100',
          'dark:text-neutral-400 dark:hover:text-neutral-200 dark:hover:bg-neutral-800',
          'focus:outline-none focus:ring-2 focus:ring-primary-500'
        )}
        aria-label="Widget menu"
      >
        <MoreVertical className="h-4 w-4" />
      </button>

      {showMenu && (
        <>
          <div
            className="fixed inset-0 z-10"
            onClick={() => setShowMenu(false)}
          />
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: -10 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: -10 }}
            className={clsx(
              'absolute right-0 mt-2 w-48 rounded-lg shadow-lg z-20',
              'bg-white dark:bg-neutral-800',
              'border border-neutral-200 dark:border-neutral-700',
              'py-1'
            )}
          >
            {onRefresh && (
              <button
                onClick={() => {
                  handleRefresh();
                  setShowMenu(false);
                }}
                className={clsx(
                  'w-full px-4 py-2 text-sm text-left',
                  'text-neutral-700 dark:text-neutral-300',
                  'hover:bg-neutral-50 dark:hover:bg-neutral-700',
                  'flex items-center gap-2'
                )}
              >
                <RefreshCw className="h-4 w-4" />
                Refresh
              </button>
            )}
            {onConfigure && (
              <button
                onClick={() => {
                  onConfigure(widget.id);
                  setShowMenu(false);
                }}
                className={clsx(
                  'w-full px-4 py-2 text-sm text-left',
                  'text-neutral-700 dark:text-neutral-300',
                  'hover:bg-neutral-50 dark:hover:bg-neutral-700',
                  'flex items-center gap-2'
                )}
              >
                <Settings className="h-4 w-4" />
                Configure
              </button>
            )}
            <button
              className={clsx(
                'w-full px-4 py-2 text-sm text-left',
                'text-neutral-700 dark:text-neutral-300',
                'hover:bg-neutral-50 dark:hover:bg-neutral-700',
                'flex items-center gap-2'
              )}
            >
              <Maximize2 className="h-4 w-4" />
              Expand
            </button>
            {onRemove && (
              <>
                <div className="border-t border-neutral-200 dark:border-neutral-700 my-1" />
                <button
                  onClick={() => {
                    onRemove(widget.id);
                    setShowMenu(false);
                  }}
                  className={clsx(
                    'w-full px-4 py-2 text-sm text-left',
                    'text-error-600 dark:text-error-400',
                    'hover:bg-error-50 dark:hover:bg-error-900/20',
                    'flex items-center gap-2'
                  )}
                >
                  <X className="h-4 w-4" />
                  Remove
                </button>
              </>
            )}
          </motion.div>
        </>
      )}
    </div>
  );

  return (
    <motion.div
      layout
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      exit={{ opacity: 0, scale: 0.9 }}
      transition={{ duration: 0.2 }}
      className={clsx(
        'h-full',
        isDragging && 'opacity-50 cursor-grabbing'
      )}
    >
      <Card
        title={widget.title}
        subtitle={widget.description}
        actions={actions}
        loading={widget.isLoading}
        className="h-full flex flex-col"
      >
        <div className="flex-1 overflow-auto">
          {widget.error ? (
            <div className="flex items-center justify-center h-full">
              <div className="text-center">
                <p className="text-error-600 dark:text-error-400 text-sm">
                  {widget.error}
                </p>
                {onRefresh && (
                  <button
                    onClick={handleRefresh}
                    className="mt-2 text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400"
                  >
                    Try again
                  </button>
                )}
              </div>
            </div>
          ) : (
            children
          )}
        </div>

        {widget.lastUpdated && (
          <div className="mt-2 pt-2 border-t border-neutral-200 dark:border-neutral-700">
            <p className="text-xs text-neutral-500 dark:text-neutral-400">
              Last updated: {new Date(widget.lastUpdated).toLocaleTimeString()}
            </p>
          </div>
        )}
      </Card>
    </motion.div>
  );
};
