import React from 'react';
import { ChevronRight, Home } from 'lucide-react';
import { clsx } from 'clsx';

export interface BreadcrumbItem {
  label: string;
  href?: string;
  onClick?: () => void;
}

export interface BreadcrumbProps {
  items: BreadcrumbItem[];
  showHome?: boolean;
  className?: string;
}

export const Breadcrumb: React.FC<BreadcrumbProps> = ({
  items,
  showHome = true,
  className,
}) => {
  return (
    <nav
      aria-label="Breadcrumb"
      className={clsx('flex items-center space-x-2 text-sm', className)}
    >
      {showHome && (
        <>
          <button
            onClick={() => window.location.href = '/'}
            className={clsx(
              'text-neutral-500 dark:text-neutral-400',
              'hover:text-neutral-700 dark:hover:text-neutral-200',
              'focus:outline-none focus:ring-2 focus:ring-primary-500 rounded',
              'transition-colors duration-150'
            )}
            aria-label="Home"
          >
            <Home className="h-4 w-4" />
          </button>
          {items.length > 0 && (
            <ChevronRight className="h-4 w-4 text-neutral-400 dark:text-neutral-600" />
          )}
        </>
      )}

      {items.map((item, index) => {
        const isLast = index === items.length - 1;

        return (
          <React.Fragment key={index}>
            {item.href || item.onClick ? (
              <button
                onClick={item.onClick || (() => window.location.href = item.href!)}
                className={clsx(
                  'font-medium transition-colors duration-150',
                  'focus:outline-none focus:ring-2 focus:ring-primary-500 rounded px-1',
                  isLast
                    ? 'text-neutral-900 dark:text-neutral-100 cursor-default'
                    : 'text-neutral-500 dark:text-neutral-400 hover:text-neutral-700 dark:hover:text-neutral-200'
                )}
                aria-current={isLast ? 'page' : undefined}
              >
                {item.label}
              </button>
            ) : (
              <span
                className={clsx(
                  'font-medium px-1',
                  isLast
                    ? 'text-neutral-900 dark:text-neutral-100'
                    : 'text-neutral-500 dark:text-neutral-400'
                )}
                aria-current={isLast ? 'page' : undefined}
              >
                {item.label}
              </span>
            )}

            {!isLast && (
              <ChevronRight className="h-4 w-4 text-neutral-400 dark:text-neutral-600" />
            )}
          </React.Fragment>
        );
      })}
    </nav>
  );
};
