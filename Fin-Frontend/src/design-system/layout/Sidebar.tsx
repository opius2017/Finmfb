import React, { useState } from 'react';
import { clsx } from 'clsx';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

export interface SidebarProps {
  children: React.ReactNode;
  collapsible?: boolean;
  defaultCollapsed?: boolean;
  className?: string;
}

export const Sidebar: React.FC<SidebarProps> = ({
  children,
  collapsible = true,
  defaultCollapsed = false,
  className,
}) => {
  const [isCollapsed, setIsCollapsed] = useState(defaultCollapsed);

  return (
    <motion.aside
      initial={false}
      animate={{ width: isCollapsed ? '4rem' : '16rem' }}
      transition={{ duration: 0.3, ease: 'easeInOut' }}
      className={clsx(
        'hidden lg:flex flex-col',
        'bg-white dark:bg-neutral-900',
        'border-r border-neutral-200 dark:border-neutral-800',
        'relative',
        className
      )}
    >
      <div className="flex-1 overflow-y-auto overflow-x-hidden">
        {children}
      </div>

      {collapsible && (
        <button
          onClick={() => setIsCollapsed(!isCollapsed)}
          className={clsx(
            'absolute -right-3 top-6 z-10',
            'flex items-center justify-center',
            'w-6 h-6 rounded-full',
            'bg-white dark:bg-neutral-800',
            'border border-neutral-200 dark:border-neutral-700',
            'text-neutral-600 dark:text-neutral-400',
            'hover:text-neutral-900 dark:hover:text-neutral-100',
            'hover:border-neutral-300 dark:hover:border-neutral-600',
            'focus:outline-none focus:ring-2 focus:ring-primary-500',
            'transition-all duration-150'
          )}
          aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {isCollapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </button>
      )}
    </motion.aside>
  );
};

export interface SidebarItemProps {
  icon?: React.ReactNode;
  label: string;
  active?: boolean;
  onClick?: () => void;
  className?: string;
}

export const SidebarItem: React.FC<SidebarItemProps> = ({
  icon,
  label,
  active = false,
  onClick,
  className,
}) => {
  return (
    <button
      onClick={onClick}
      className={clsx(
        'flex items-center gap-3 px-4 py-3 w-full',
        'text-sm font-medium',
        'transition-all duration-150',
        'focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500',
        active
          ? 'bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400'
          : 'text-neutral-700 dark:text-neutral-300 hover:bg-neutral-50 dark:hover:bg-neutral-800',
        className
      )}
    >
      {icon && (
        <span className="flex-shrink-0 w-5 h-5">
          {icon}
        </span>
      )}
      <span className="truncate">{label}</span>
    </button>
  );
};

export interface SidebarSectionProps {
  title?: string;
  children: React.ReactNode;
  className?: string;
}

export const SidebarSection: React.FC<SidebarSectionProps> = ({
  title,
  children,
  className,
}) => {
  return (
    <div className={clsx('py-4', className)}>
      {title && (
        <h3 className="px-4 text-xs font-semibold text-neutral-500 dark:text-neutral-400 uppercase tracking-wider mb-2">
          {title}
        </h3>
      )}
      <nav className="space-y-1">
        {children}
      </nav>
    </div>
  );
};
