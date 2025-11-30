import React from 'react';
import { clsx } from 'clsx';
import { motion } from 'framer-motion';

export interface CardProps {
  title?: string;
  subtitle?: string;
  actions?: React.ReactNode;
  footer?: React.ReactNode;
  loading?: boolean;
  hoverable?: boolean;
  bordered?: boolean;
  className?: string;
  children: React.ReactNode;
}

export const Card: React.FC<CardProps> = ({
  title,
  subtitle,
  actions,
  footer,
  loading = false,
  hoverable = false,
  bordered = true,
  className,
  children,
}) => {
  const CardWrapper = hoverable ? motion.div : 'div';
  const hoverProps = hoverable
    ? {
        whileHover: { y: -4, boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' },
        transition: { duration: 0.2 },
      }
    : {};

  return (
    <CardWrapper
      className={clsx(
        'bg-white rounded-lg overflow-hidden',
        'transition-all duration-200',
        bordered && 'border border-neutral-200',
        !bordered && 'shadow-md',
        hoverable && 'cursor-pointer',
        className
      )}
      {...hoverProps}
    >
      {(title || subtitle || actions) && (
        <div className="px-6 py-4 border-b border-neutral-200">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              {title && (
                <h3 className="text-lg font-semibold text-neutral-900">
                  {title}
                </h3>
              )}
              {subtitle && (
                <p className="mt-1 text-sm text-neutral-500">{subtitle}</p>
              )}
            </div>
            {actions && <div className="ml-4 flex-shrink-0">{actions}</div>}
          </div>
        </div>
      )}

      <div className="px-6 py-4">
        {loading ? (
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
          </div>
        ) : (
          children
        )}
      </div>

      {footer && (
        <div className="px-6 py-4 bg-neutral-50 border-t border-neutral-200">
          {footer}
        </div>
      )}
    </CardWrapper>
  );
};

Card.displayName = 'Card';
