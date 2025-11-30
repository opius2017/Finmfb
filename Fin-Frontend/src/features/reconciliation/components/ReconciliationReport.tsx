import React from 'react';
import { clsx } from 'clsx';
import { Download, Printer, CheckCircle, AlertCircle, TrendingUp, TrendingDown } from 'lucide-react';
import { Button, Card } from '@/design-system';
import type { ReconciliationSession, ReconciliationReport as ReportType } from '../types/reconciliation.types';

export interface ReconciliationReportProps {
  session: ReconciliationSession;
  report: ReportType;
  onExport?: () => void;
  onPrint?: () => void;
}

export const ReconciliationReport: React.FC<ReconciliationReportProps> = ({
  session,
  report,
  onExport,
  onPrint,
}) => {
  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      minimumFractionDigits: 2,
    }).format(amount);
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'approved':
        return 'text-success-700 bg-success-100 dark:bg-success-900/20 dark:text-success-400';
      case 'completed':
        return 'text-primary-700 bg-primary-100 dark:bg-primary-900/20 dark:text-primary-400';
      case 'in-progress':
        return 'text-warning-700 bg-warning-100 dark:bg-warning-900/20 dark:text-warning-400';
      default:
        return 'text-neutral-700 bg-neutral-100 dark:bg-neutral-800 dark:text-neutral-400';
    }
  };

  const matchRate = session.bankTransactions.length > 0
    ? (session.matches.length / session.bankTransactions.length) * 100
    : 0;

  const isReconciled = Math.abs(report.difference) < 0.01;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <h2 className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
            Reconciliation Report
          </h2>
          <p className="text-neutral-600 dark:text-neutral-400 mt-1">
            {report.accountName} • {formatDate(report.statementDate)}
          </p>
        </div>
        <div className="flex gap-2">
          {onPrint && (
            <Button
              variant="outline"
              size="sm"
              icon={<Printer className="h-4 w-4" />}
              onClick={onPrint}
            >
              Print
            </Button>
          )}
          {onExport && (
            <Button
              variant="outline"
              size="sm"
              icon={<Download className="h-4 w-4" />}
              onClick={onExport}
            >
              Export
            </Button>
          )}
        </div>
      </div>

      {/* Status Banner */}
      <Card>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            {isReconciled ? (
              <CheckCircle className="h-12 w-12 text-success-600 dark:text-success-400" />
            ) : (
              <AlertCircle className="h-12 w-12 text-warning-600 dark:text-warning-400" />
            )}
            <div>
              <h3 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">
                {isReconciled ? 'Reconciliation Complete' : 'Reconciliation In Progress'}
              </h3>
              <p className="text-sm text-neutral-600 dark:text-neutral-400">
                {isReconciled
                  ? 'All transactions have been reconciled successfully'
                  : `Difference of ${formatAmount(Math.abs(report.difference))} needs to be resolved`}
              </p>
            </div>
          </div>
          <span className={clsx('px-3 py-1 rounded-full text-sm font-medium uppercase', getStatusColor(report.status))}>
            {report.status}
          </span>
        </div>
      </Card>

      {/* Summary Statistics */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <div className="text-center">
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Opening Balance</p>
            <p className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
              {formatAmount(report.openingBalance)}
            </p>
          </div>
        </Card>

        <Card>
          <div className="text-center">
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Closing Balance</p>
            <p className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
              {formatAmount(report.closingBalance)}
            </p>
          </div>
        </Card>

        <Card>
          <div className="text-center">
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Book Balance</p>
            <p className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
              {formatAmount(report.bookBalance)}
            </p>
          </div>
        </Card>

        <Card>
          <div className="text-center">
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Difference</p>
            <p className={clsx(
              'text-2xl font-bold',
              isReconciled
                ? 'text-success-600 dark:text-success-400'
                : 'text-error-600 dark:text-error-400'
            )}>
              {formatAmount(report.difference)}
            </p>
          </div>
        </Card>
      </div>

      {/* Matching Statistics */}
      <Card title="Matching Statistics">
        <div className="grid grid-cols-3 gap-6">
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-neutral-600 dark:text-neutral-400">Matched Transactions</span>
              <span className="text-lg font-semibold text-success-600 dark:text-success-400">
                {report.matchedCount}
              </span>
            </div>
            <div className="w-full bg-neutral-200 dark:bg-neutral-700 rounded-full h-2">
              <div
                className="bg-success-500 h-2 rounded-full transition-all duration-500"
                style={{ width: `${matchRate}%` }}
              />
            </div>
            <p className="text-xs text-neutral-500 dark:text-neutral-400 mt-1">
              {matchRate.toFixed(1)}% match rate
            </p>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-neutral-600 dark:text-neutral-400">Unmatched Bank</span>
              <span className="text-lg font-semibold text-warning-600 dark:text-warning-400">
                {report.unmatchedBankCount}
              </span>
            </div>
            <div className="flex items-center gap-2 text-xs text-neutral-600 dark:text-neutral-400">
              <AlertCircle className="h-3 w-3" />
              <span>Requires attention</span>
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-neutral-600 dark:text-neutral-400">Unmatched Internal</span>
              <span className="text-lg font-semibold text-warning-600 dark:text-warning-400">
                {report.unmatchedInternalCount}
              </span>
            </div>
            <div className="flex items-center gap-2 text-xs text-neutral-600 dark:text-neutral-400">
              <AlertCircle className="h-3 w-3" />
              <span>Requires attention</span>
            </div>
          </div>
        </div>
      </Card>

      {/* Adjustments */}
      {session.adjustments.length > 0 && (
        <Card title="Adjustment Entries" subtitle={`${report.adjustmentCount} adjustments totaling ${formatAmount(report.totalAdjustments)}`}>
          <div className="space-y-2">
            {session.adjustments.map(adjustment => (
              <div
                key={adjustment.id}
                className="flex items-center justify-between p-3 rounded-lg border border-neutral-200 dark:border-neutral-700"
              >
                <div className="flex-1">
                  <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
                    {adjustment.description}
                  </p>
                  <div className="flex items-center gap-3 mt-1">
                    <span className="text-xs text-neutral-600 dark:text-neutral-400">
                      {adjustment.type.replace('-', ' ').toUpperCase()}
                    </span>
                    {adjustment.requiresApproval && (
                      <span className={clsx(
                        'text-xs px-2 py-0.5 rounded-full',
                        adjustment.approved
                          ? 'bg-success-100 text-success-700 dark:bg-success-900/20 dark:text-success-400'
                          : 'bg-warning-100 text-warning-700 dark:bg-warning-900/20 dark:text-warning-400'
                      )}>
                        {adjustment.approved ? 'Approved' : 'Pending Approval'}
                      </span>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {adjustment.amount >= 0 ? (
                    <TrendingUp className="h-4 w-4 text-success-600 dark:text-success-400" />
                  ) : (
                    <TrendingDown className="h-4 w-4 text-error-600 dark:text-error-400" />
                  )}
                  <span className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
                    {formatAmount(Math.abs(adjustment.amount))}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Matched Transactions Detail */}
      <Card title="Matched Transactions" subtitle={`${report.matchedCount} transactions matched`}>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-neutral-200 dark:divide-neutral-700">
            <thead className="bg-neutral-50 dark:bg-neutral-800">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Date
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Bank Description
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Internal Description
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Amount
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Match Type
                </th>
              </tr>
            </thead>
            <tbody className="bg-white dark:bg-neutral-900 divide-y divide-neutral-200 dark:divide-neutral-800">
              {session.matches.map(match => (
                <tr key={match.id} className="hover:bg-neutral-50 dark:hover:bg-neutral-800">
                  <td className="px-4 py-3 text-sm text-neutral-900 dark:text-neutral-100">
                    {formatDate(match.bankTransaction.date)}
                  </td>
                  <td className="px-4 py-3 text-sm text-neutral-900 dark:text-neutral-100">
                    {match.bankTransaction.description}
                  </td>
                  <td className="px-4 py-3 text-sm text-neutral-900 dark:text-neutral-100">
                    {match.internalTransaction.description}
                  </td>
                  <td className="px-4 py-3 text-sm text-right font-medium text-neutral-900 dark:text-neutral-100">
                    {formatAmount((match.bankTransaction.debit || 0) || (match.bankTransaction.credit || 0))}
                  </td>
                  <td className="px-4 py-3 text-center">
                    <span className={clsx(
                      'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium',
                      match.matchType === 'exact'
                        ? 'bg-success-100 text-success-700 dark:bg-success-900/20 dark:text-success-400'
                        : match.matchType === 'fuzzy'
                        ? 'bg-primary-100 text-primary-700 dark:bg-primary-900/20 dark:text-primary-400'
                        : match.matchType === 'rule-based'
                        ? 'bg-secondary-100 text-secondary-700 dark:bg-secondary-900/20 dark:text-secondary-400'
                        : 'bg-neutral-100 text-neutral-700 dark:bg-neutral-800 dark:text-neutral-400'
                    )}>
                      {match.matchType}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Report Footer */}
      <Card>
        <div className="text-sm text-neutral-600 dark:text-neutral-400 space-y-1">
          <p>Report generated on {formatDate(report.generatedAt)}</p>
          <p>Generated by {report.generatedBy}</p>
          {session.approvedBy && session.approvedAt && (
            <p>Approved by {session.approvedBy} on {formatDate(session.approvedAt)}</p>
          )}
        </div>
      </Card>
    </div>
  );
};
