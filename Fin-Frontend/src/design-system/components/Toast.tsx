import React from 'react';
import toast, { Toaster, Toast as HotToast } from 'react-hot-toast';
import { CheckCircle, XCircle, AlertCircle, Info, X } from 'lucide-react';
import { clsx } from 'clsx';

export interface ToastOptions {
  duration?: number;
  position?: 'top-left' | 'top-center' | 'top-right' | 'bottom-left' | 'bottom-center' | 'bottom-right';
}

const iconMap = {
  success: CheckCircle,
  error: XCircle,
  warning: AlertCircle,
  info: Info,
};

const colorMap = {
  success: 'text-success-600',
  error: 'text-error-600',
  warning: 'text-warning-600',
  info: 'text-primary-600',
};

interface CustomToastProps {
  t: HotToast;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
}

const CustomToast: React.FC<CustomToastProps> = ({ t, type, message }) => {
  const Icon = iconMap[type];

  return (
    <div
      className={clsx(
        'flex items-start gap-3 p-4 rounded-lg shadow-lg bg-white border',
        'max-w-md w-full',
        'transition-all duration-300',
        t.visible ? 'animate-enter' : 'animate-leave'
      )}
    >
      <Icon className={clsx('h-5 w-5 flex-shrink-0 mt-0.5', colorMap[type])} />
      <p className="flex-1 text-sm text-neutral-900">{message}</p>
      <button
        onClick={() => toast.dismiss(t.id)}
        className="flex-shrink-0 text-neutral-400 hover:text-neutral-600 focus:outline-none focus:ring-2 focus:ring-primary-500 rounded p-0.5"
        aria-label="Dismiss notification"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  );
};

export const toastService = {
  success: (message: string, options?: ToastOptions) => {
    return toast.custom(
      (t) => <CustomToast t={t} type="success" message={message} />,
      { duration: options?.duration || 4000, position: options?.position || 'top-right' }
    );
  },

  error: (message: string, options?: ToastOptions) => {
    return toast.custom(
      (t) => <CustomToast t={t} type="error" message={message} />,
      { duration: options?.duration || 5000, position: options?.position || 'top-right' }
    );
  },

  warning: (message: string, options?: ToastOptions) => {
    return toast.custom(
      (t) => <CustomToast t={t} type="warning" message={message} />,
      { duration: options?.duration || 4000, position: options?.position || 'top-right' }
    );
  },

  info: (message: string, options?: ToastOptions) => {
    return toast.custom(
      (t) => <CustomToast t={t} type="info" message={message} />,
      { duration: options?.duration || 4000, position: options?.position || 'top-right' }
    );
  },

  dismiss: (toastId?: string) => {
    toast.dismiss(toastId);
  },

  promise: <T,>(
    promise: Promise<T>,
    messages: {
      loading: string;
      success: string | ((data: T) => string);
      error: string | ((error: any) => string);
    },
    options?: ToastOptions
  ) => {
    return toast.promise(
      promise,
      {
        loading: messages.loading,
        success: messages.success,
        error: messages.error,
      },
      { position: options?.position || 'top-right' }
    );
  },
};

export const ToastProvider: React.FC = () => {
  return (
    <Toaster
      position="top-right"
      toastOptions={{
        className: '',
        style: {
          background: 'transparent',
          boxShadow: 'none',
          padding: 0,
        },
      }}
    />
  );
};
