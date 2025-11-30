import React, { useState } from 'react';
import { clsx } from 'clsx';
import { AlertCircle, CheckCircle, TrendingUp } from 'lucide-react';
import { Button, Card, Input, toastService } from '@/design-system';
import { arService } from '../services/arService';
import type { CreditLimit } from '../types/ar.types';

export const CreditManagement: React.FC<{ customerId: string; customerName: string }> = ({
  customerId,
  customerName,
}) => {
  const [creditLimit, setCreditLimit] = useState<CreditLimit | null>(null);
  const [checkAmount, setCheckAmount] = useState('');
  const [checkResult, setCheckResult] = useState<{ approved: boolean; reason?: string } | null>(null);

  React.useEffect(() => {
    loadCreditLimit();
  }, [customerId]);

  const loadCreditLimit = async () => {
    const limit = await arService['getCreditLimit'](customerId);
    setCreditLimit(limit);
  };

  const handleCheckCredit = async () => {
    const amount = parseFloat(checkAmount);
    if (isNaN(amount) || amount <= 0) {
      toastService.error('Please enter a valid amount');
      return;
    }

    const result = await arService.checkCreditLimit(customerId, amount);
    setCheckResult(result);
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
    }).format(amount);
  };

  if (!creditLimit) {
    return <div>Loading...</div>;
  }

  const utilizationPercent = (creditLimit.utilized / creditLimit.limit) * 100;

  return (
    <div className="space-y-6">
      <Card title={`Credit Management - ${customerName}`}>
        <div className="grid grid-cols-3 gap-6 mb-6">
          <div>
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Credit Limit</p>
            <p className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
              {formatAmount(creditLimit.limit)}
            </p>
          </div>
          <div>
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Utilized</p>
            <p className="text-2xl font-bold text-warning-600 dark:text-warning-400">
              {formatAmount(creditLimit.utilized)}
            </p>
          </div>
          <div>
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Available</p>
            <p className="text-2xl font-bold text-success-600 dark:text-success-400">
              {formatAmount(creditLimit.available)}
            </p>
          </div>
        </div>

        {/* Utilization Bar */}
        <div className="mb-6">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-neutral-700 dark:text-neutral-300">
              Credit Utilization
            </span>
            <span className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
              {utilizationPercent.toFixed(1)}%
            </span>
          </div>
          <div className="w-full bg-neutral-200 dark:bg-neutral-700 rounded-full h-3">
            <div
              className={clsx(
                'h-3 rounded-full transition-all duration-500',
                utilizationPercent > 90
                  ? 'bg-error-500'
                  : utilizationPercent > 75
                  ? 'bg-warning-500'
                  : 'bg-success-500'
              )}
              style={{ width: `${Math.min(utilizationPercent, 100)}%` }}
            />
          </div>
        </div>

        {/* Credit Check */}
        <div className="border-t border-neutral-200 dark:border-neutral-700 pt-6">
          <h3 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100 mb-4">
            Check Credit Availability
          </h3>
          <div className="flex gap-4">
            <Input
              type="number"
              placeholder="Enter amount"
              value={checkAmount}
              onChange={(e) => setCheckAmount(e.target.value)}
              className="flex-1"
            />
            <Button onClick={handleCheckCredit}>Check</Button>
          </div>

          {checkResult && (
            <div className={clsx(
              'mt-4 p-4 rounded-lg flex items-start gap-3',
              checkResult.approved
                ? 'bg-success-50 dark:bg-success-900/20'
                : 'bg-error-50 dark:bg-error-900/20'
            )}>
              {checkResult.approved ? (
                <CheckCircle className="h-5 w-5 text-success-600 dark:text-success-400 flex-shrink-0" />
              ) : (
                <AlertCircle className="h-5 w-5 text-error-600 dark:text-error-400 flex-shrink-0" />
              )}
              <div>
                <p className={clsx(
                  'font-medium',
                  checkResult.approved
                    ? 'text-success-700 dark:text-success-400'
                    : 'text-error-700 dark:text-error-400'
                )}>
                  {checkResult.approved ? 'Credit Approved' : 'Credit Denied'}
                </p>
                {checkResult.reason && (
                  <p className="text-sm text-neutral-600 dark:text-neutral-400 mt-1">
                    {checkResult.reason}
                  </p>
                )}
              </div>
            </div>
          )}
        </div>
      </Card>
    </div>
  );
};
