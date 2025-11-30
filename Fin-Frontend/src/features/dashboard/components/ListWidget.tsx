import React from 'react';
import { clsx } from 'clsx';
import { motion } from 'framer-motion';
import type { ListConfig } from '../types/widget.types';

export interface ListWidgetProps {
  config: ListConfig;
}

export const ListWidget: React.FC<ListWidgetProps> = ({ config }) => {
  const maxItems = config.maxItems || config.items.length;
  const displayItems = config.items.slice(0, maxItems);

  return (
    <div className="divide-y divide-neutral-200 dark:divide-neutral-700">
      {displayItems.length === 0 ? (
        <div className="py-8 text-center text-sm text-neutral-500 dark:text-neutral-400">
          No items to display
        </div>
      ) : (
        displayItems.map((item, index) => (
          <motion.div
            key={item.id}
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.2, delay: index * 0.05 }}
            className={clsx(
              'flex items-center justify-between p-4',
              'hover:bg-neutral-50 dark:hover:bg-neutral-800',
              'transition-colors cursor-pointer'
            )}
          >
            <div className="flex items-center gap-3 flex-1 min-w-0">
              {item.icon && (
                <div className={clsx(
                  'flex-shrink-0 w-10 h-10 rounded-lg',
                  'bg-primary-100 dark:bg-primary-900/20',
                  'flex items-center justify-center',
                  'text-primary-600 dark:text-primary-400'
                )}>
                  <span className="text-lg">{item.icon}</span>
                </div>
              )}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100 truncate">
                  {item.title}
                </p>
                {item.subtitle && (
                  <p className="text-xs text-neutral-500 dark:text-neutral-400 truncate">
                    {item.subtitle}
                  </p>
                )}
              </div>
            </div>
            {item.value && (
              <div className="flex-shrink-0 ml-4">
                <span className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
                  {item.value}
                </span>
              </div>
            )}
          </motion.div>
        ))
      )}
      
      {config.items.length > maxItems && (
        <div className="p-4 text-center">
          <button className="text-sm text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300">
            View all ({config.items.length} items)
          </button>
        </div>
      )}
    </div>
  );
};
