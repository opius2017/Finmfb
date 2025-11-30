import React, { useState, useEffect } from 'react';
import { clsx } from 'clsx';
import { Download, TrendingUp, AlertCircle } from 'lucide-react';
import { Button, Card } from '@/design-system';
import { arService } from '../services/arService';
import type { AgingReport, CustomerAging } from '../types/ar.types';

export const AgingReportView: React.FC = () => {
  const [report, setReport] = useState<AgingReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerAging | null>(null);

  useEffect(() => {
    loadReport();
  }, []);

  const loadReport = async () => {
    setLoading(true);
    try {
      const data = await arService.generateAgingReport();
      setReport(data);
    } finally {
      setLoading(false);
    }
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
    }).format(amount);
  };

  if (loading || !report) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
            AR Aging Report
          </h2>
          <p className="text-neutral-600 dark:text-neutral-400">
            As of {report.asOfDate.toLocaleDateString()}
          </p>
        </div>
        <Button variant="outline" icon={<Download className="h-4 w-4" />}>
          Export
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-5 gap-4">
        <Card>
          <div className="text-center">
            <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">Total Outstanding</p>
            <p className="text-2xl font-bold text-neutral-900 dark:text-neutral-100">
              {formatAmount(report.totalOutstanding)}
            </p>
          </div>
        </Card>

        {Object.entries(report.buckets).map(([bucket, data]) => (
          <Card key={bucket}>
            <div className="text-center">
              <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-1">{bucket} Days</p>
              <p className="text-xl font-bold text-neutral-900 dark:text-neutral-100">
                {formatAmount(data.amount)}
              </p>
              <p className="text-xs text-neutral-500 dark:text-neutral-400 mt-1">
                {data.count} invoices
              </p>
            </div>
          </Card>
        ))}
      </div>

      {/* Customer Aging Table */}
      <Card title="Customer Aging Details">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-neutral-200 dark:divide-neutral-700">
            <thead className="bg-neutral-50 dark:bg-neutral-800">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Customer
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Total
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  Current
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  31-60
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  61-90
                </th>
                <th className="px-4 py-3 text-right text-xs font-semibold text-neutral-700 dark:text-neutral-300 uppercase">
                  90+
                </th>
              </tr>
            </thead>
            <tbody className="bg-white dark:bg-neutral-900 divide-y divide-neutral-200 dark:divide-neutral-800">
              {report.customers.map(customer => (
                <tr
                  key={customer.customerId}
                  onClick={() => setSelectedCustomer(customer)}
                  className="hover:bg-neutral-50 dark:hover:bg-neutral-800 cursor-pointer"
                >
                  <td className="px-4 py-3 text-sm font-medium text-neutral-900 dark:text-neutral-100">
                    {customer.customerName}
                  </td>
                  <td className="px-4 py-3 text-sm text-right font-semibold text-neutral-900 dark:text-neutral-100">
                    {formatAmount(customer.totalOutstanding)}
                  </td>
                  <td className="px-4 py-3 text-sm text-right text-neutral-900 dark:text-neutral-100">
                    {formatAmount(customer.current)}
                  </td>
                  <td className="px-4 py-3 text-sm text-right text-neutral-900 dark:text-neutral-100">
                    {formatAmount(customer.days30)}
                  </td>
                  <td className="px-4 py-3 text-sm text-right text-neutral-900 dark:text-neutral-100">
                    {formatAmount(customer.days60)}
                  </td>
                  <td className="px-4 py-3 text-sm text-right text-error-600 dark:text-error-400 font-medium">
                    {formatAmount(customer.over90)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Customer Detail Modal */}
      {selectedCustomer && (
        <Card title={`${selectedCustomer.customerName} - Invoice Details`}>
          <div className="space-y-2">
            {selectedCustomer.invoices.map(invoice => (
              <div
                key={invoice.id}
                className="flex items-center justify-between p-3 rounded-lg border border-neutral-200 dark:border-neutral-700"
              >
                <div>
                  <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
                    {invoice.invoiceNumber}
                  </p>
                  <p className="text-xs text-neutral-600 dark:text-neutral-400">
                    Due: {invoice.dueDate.toLocaleDateString()} • {invoice.daysPastDue} days overdue
                  </p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">
                    {formatAmount(invoice.balance)}
                  </p>
                  <span className={clsx(
                    'text-xs px-2 py-1 rounded-full',
                    invoice.agingBucket === '90+'
                      ? 'bg-error-100 text-error-700 dark:bg-error-900/20 dark:text-error-400'
                      : 'bg-warning-100 text-warning-700 dark:bg-warning-900/20 dark:text-warning-400'
                  )}>
                    {invoice.agingBucket} days
                  </span>
                </div>
              </div>
            ))}
          </div>
          <div className="mt-4 flex justify-end">
            <Button variant="outline" onClick={() => setSelectedCustomer(null)}>
              Close
            </Button>
          </div>
        </Card>
      )}
    </div>
  );
};
