import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import axios from 'axios';
import { format } from 'date-fns';

interface AgingSummary {
  totalOutstanding: number;
  current: number;
  days1To30: number;
  days31To60: number;
  days61To90: number;
  days91To120: number;
  over120Days: number;
  totalCustomers: number;
  customersOverdue: number;
}

interface CustomerAging {
  customerId: string;
  customerName: string;
  customerCode: string;
  totalOutstanding: number;
  current: number;
  days1To30: number;
  days31To60: number;
  days61To90: number;
  days91To120: number;
  over120Days: number;
  daysOverdue: number;
  riskLevel: string;
}

interface AgingReport {
  asOfDate: string;
  generatedDate: string;
  summary: AgingSummary;
  customerAging: CustomerAging[];
}

const ARAgingReport: React.FC = () => {
  const [asOfDate, setAsOfDate] = useState(format(new Date(), 'yyyy-MM-dd'));
  const [includeZeroBalances, setIncludeZeroBalances] = useState(false);
  const [expandedCustomers, setExpandedCustomers] = useState<Set<string>>(new Set());

  const { data: report, isLoading, refetch } = useQuery<AgingReport>({
    queryKey: ['arAgingReport', asOfDate, includeZeroBalances],
    queryFn: async () => {
      const response = await axios.get('/api/v1/accounts-receivable/aging-report', {
        params: { asOfDate, includeZeroBalances }
      });
      return response.data;
    }
  });

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      minimumFractionDigits: 2
    }).format(amount);
  };

  const formatPercentage = (part: number, total: number) => {
    if (total === 0) return '0%';
    return `${((part / total) * 100).toFixed(1)}%`;
  };

  const getRiskColor = (riskLevel: string) => {
    const colors: Record<string, string> = {
      Low: 'text-green-600 bg-green-50',
      Medium: 'text-yellow-600 bg-yellow-50',
      High: 'text-red-600 bg-red-50'
    };
    return colors[riskLevel] || 'text-gray-600 bg-gray-50';
  };

  const toggleCustomer = (customerId: string) => {
    const newExpanded = new Set(expandedCustomers);
    if (newExpanded.has(customerId)) {
      newExpanded.delete(customerId);
    } else {
      newExpanded.add(customerId);
    }
    setExpandedCustomers(newExpanded);
  };

  const exportToExcel = () => {
    // TODO: Implement Excel export
    console.log('Exporting to Excel...');
  };

  const exportToPDF = () => {
    // TODO: Implement PDF export
    console.log('Exporting to PDF...');
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Accounts Receivable Aging Report</h1>
        <div className="flex space-x-2">
          <button
            onClick={exportToExcel}
            className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
          >
            Export Excel
          </button>
          <button
            onClick={exportToPDF}
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
          >
            Export PDF
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              As of Date
            </label>
            <input
              type="date"
              value={asOfDate}
              onChange={(e) => setAsOfDate(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
            />
          </div>
          <div className="flex items-end">
            <label className="flex items-center space-x-2">
              <input
                type="checkbox"
                checked={includeZeroBalances}
                onChange={(e) => setIncludeZeroBalances(e.target.checked)}
                className="w-4 h-4 text-emerald-600 border-gray-300 rounded focus:ring-emerald-500"
              />
              <span className="text-sm text-gray-700">Include zero balances</span>
            </label>
          </div>
          <div className="flex items-end">
            <button
              onClick={() => refetch()}
              className="w-full px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors"
            >
              Generate Report
            </button>
          </div>
        </div>
      </div>

      {isLoading ? (
        <div className="bg-white rounded-lg shadow-md p-8 text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Generating report...</p>
        </div>
      ) : report ? (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Total Outstanding</h3>
              <p className="text-2xl font-bold text-gray-900">
                {formatCurrency(report.summary.totalOutstanding)}
              </p>
              <p className="text-sm text-gray-500 mt-1">
                {report.summary.totalCustomers} customers
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Current</h3>
              <p className="text-2xl font-bold text-green-600">
                {formatCurrency(report.summary.current)}
              </p>
              <p className="text-sm text-gray-500 mt-1">
                {formatPercentage(report.summary.current, report.summary.totalOutstanding)}
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Overdue</h3>
              <p className="text-2xl font-bold text-red-600">
                {formatCurrency(
                  report.summary.days1To30 +
                  report.summary.days31To60 +
                  report.summary.days61To90 +
                  report.summary.days91To120 +
                  report.summary.over120Days
                )}
              </p>
              <p className="text-sm text-gray-500 mt-1">
                {report.summary.customersOverdue} customers
              </p>
            </div>
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Over 90 Days</h3>
              <p className="text-2xl font-bold text-red-700">
                {formatCurrency(report.summary.days91To120 + report.summary.over120Days)}
              </p>
              <p className="text-sm text-gray-500 mt-1">
                {formatPercentage(
                  report.summary.days91To120 + report.summary.over120Days,
                  report.summary.totalOutstanding
                )}
              </p>
            </div>
          </div>

          {/* Aging Summary Bar */}
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h3 className="text-lg font-semibold mb-4">Aging Distribution</h3>
            <div className="flex h-8 rounded-lg overflow-hidden">
              {report.summary.current > 0 && (
                <div
                  className="bg-green-500 flex items-center justify-center text-white text-xs font-medium"
                  style={{ width: formatPercentage(report.summary.current, report.summary.totalOutstanding) }}
                  title={`Current: ${formatCurrency(report.summary.current)}`}
                >
                  {formatPercentage(report.summary.current, report.summary.totalOutstanding)}
                </div>
              )}
              {report.summary.days1To30 > 0 && (
                <div
                  className="bg-blue-500 flex items-center justify-center text-white text-xs font-medium"
                  style={{ width: formatPercentage(report.summary.days1To30, report.summary.totalOutstanding) }}
                  title={`1-30 Days: ${formatCurrency(report.summary.days1To30)}`}
                >
                  {formatPercentage(report.summary.days1To30, report.summary.totalOutstanding)}
                </div>
              )}
              {report.summary.days31To60 > 0 && (
                <div
                  className="bg-yellow-500 flex items-center justify-center text-white text-xs font-medium"
                  style={{ width: formatPercentage(report.summary.days31To60, report.summary.totalOutstanding) }}
                  title={`31-60 Days: ${formatCurrency(report.summary.days31To60)}`}
                >
                  {formatPercentage(report.summary.days31To60, report.summary.totalOutstanding)}
                </div>
              )}
              {report.summary.days61To90 > 0 && (
                <div
                  className="bg-orange-500 flex items-center justify-center text-white text-xs font-medium"
                  style={{ width: formatPercentage(report.summary.days61To90, report.summary.totalOutstanding) }}
                  title={`61-90 Days: ${formatCurrency(report.summary.days61To90)}`}
                >
                  {formatPercentage(report.summary.days61To90, report.summary.totalOutstanding)}
                </div>
              )}
              {(report.summary.days91To120 + report.summary.over120Days) > 0 && (
                <div
                  className="bg-red-600 flex items-center justify-center text-white text-xs font-medium"
                  style={{ 
                    width: formatPercentage(
                      report.summary.days91To120 + report.summary.over120Days,
                      report.summary.totalOutstanding
                    ) 
                  }}
                  title={`Over 90 Days: ${formatCurrency(report.summary.days91To120 + report.summary.over120Days)}`}
                >
                  {formatPercentage(
                    report.summary.days91To120 + report.summary.over120Days,
                    report.summary.totalOutstanding
                  )}
                </div>
              )}
            </div>
            <div className="flex justify-between mt-4 text-sm">
              <div className="flex items-center">
                <div className="w-4 h-4 bg-green-500 rounded mr-2"></div>
                <span>Current</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-blue-500 rounded mr-2"></div>
                <span>1-30 Days</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-yellow-500 rounded mr-2"></div>
                <span>31-60 Days</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-orange-500 rounded mr-2"></div>
                <span>61-90 Days</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-red-600 rounded mr-2"></div>
                <span>Over 90 Days</span>
              </div>
            </div>
          </div>

          {/* Customer Details Table */}
          <div className="bg-white rounded-lg shadow-md overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200">
              <h3 className="text-lg font-semibold">Customer Aging Details</h3>
            </div>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Customer
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Total
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Current
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      1-30
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      31-60
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      61-90
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Over 90
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Risk
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {report.customerAging.map((customer) => (
                    <tr key={customer.customerId} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">{customer.customerName}</div>
                        <div className="text-sm text-gray-500">{customer.customerCode}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-semibold text-gray-900">
                        {formatCurrency(customer.totalOutstanding)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(customer.current)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(customer.days1To30)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(customer.days31To60)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(customer.days61To90)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(customer.days91To120 + customer.over120Days)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <span className={`px-2 py-1 text-xs font-medium rounded-full ${getRiskColor(customer.riskLevel)}`}>
                          {customer.riskLevel}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50 font-semibold">
                  <tr>
                    <td className="px-6 py-4 text-sm text-gray-900">Total</td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      {formatCurrency(report.summary.totalOutstanding)}
                    </td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      {formatCurrency(report.summary.current)}
                    </td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      {formatCurrency(report.summary.days1To30)}
                    </td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      {formatCurrency(report.summary.days31To60)}
                    </td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      {formatCurrency(report.summary.days61To90)}
                    </td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      {formatCurrency(report.summary.days91To120 + report.summary.over120Days)}
                    </td>
                    <td className="px-6 py-4"></td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
};

export default ARAgingReport;
