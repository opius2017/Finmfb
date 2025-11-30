import React from 'react';
import { clsx } from 'clsx';
import { Wifi, WifiOff, Loader2 } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useRealtimeConnection } from '../services/realtimeService';

export const ConnectionStatus: React.FC = () => {
  const { status, connect } = useRealtimeConnection();
  const [showTooltip, setShowTooltip] = useState(false);

  const getStatusConfig = () => {
    switch (status) {
      case 'connected':
        return {
          icon: Wifi,
          color: 'text-success-600 dark:text-success-400',
          bgColor: 'bg-success-100 dark:bg-success-900/20',
          label: 'Connected',
          description: 'Real-time updates active',
        };
      case 'connecting':
        return {
          icon: Loader2,
          color: 'text-warning-600 dark:text-warning-400',
          bgColor: 'bg-warning-100 dark:bg-warning-900/20',
          label: 'Connecting',
          description: 'Establishing connection...',
          animate: true,
        };
      case 'disconnected':
      default:
        return {
          icon: WifiOff,
          color: 'text-error-600 dark:text-error-400',
          bgColor: 'bg-error-100 dark:bg-error-900/20',
          label: 'Disconnected',
          description: 'Real-time updates unavailable',
        };
    }
  };

  const config = getStatusConfig();
  const Icon = config.icon;

  return (
    <div className="relative">
      <button
        onMouseEnter={() => setShowTooltip(true)}
        onMouseLeave={() => setShowTooltip(false)}
        onClick={() => status === 'disconnected' && connect()}
        className={clsx(
          'flex items-center gap-2 px-3 py-1.5 rounded-lg transition-all',
          config.bgColor,
          config.color,
          'hover:opacity-80',
          status === 'disconnected' && 'cursor-pointer',
          'focus:outline-none focus:ring-2 focus:ring-primary-500'
        )}
        aria-label={`Connection status: ${config.label}`}
      >
        <Icon
          className={clsx(
            'h-4 w-4',
            config.animate && 'animate-spin'
          )}
        />
        <span className="text-xs font-medium hidden sm:inline">
          {config.label}
        </span>
      </button>

      <AnimatePresence>
        {showTooltip && (
          <motion.div
            initial={{ opacity: 0, y: 5 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 5 }}
            className={clsx(
              'absolute top-full mt-2 right-0 z-50',
              'px-3 py-2 rounded-lg shadow-lg',
              'bg-neutral-900 dark:bg-neutral-800',
              'text-white text-xs',
              'whitespace-nowrap'
            )}
          >
            {config.description}
            <div className="absolute -top-1 right-4 w-2 h-2 bg-neutral-900 dark:bg-neutral-800 transform rotate-45" />
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// Add React import for useState
import { useState } from 'react';
