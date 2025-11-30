import React, { useState } from 'react';
import { FileText, Download, Calendar, TrendingUp } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { TrialBalance, ProfitAndLoss, BalanceSheet } from '../types/financial.types';
import { financialReportService } from '../services/financialReportService';

type ReportType = 'trial-balance' | 'profit-loss' | 'balance-sheet' | 'cash-flow' | 'general-ledger';

export const FinancialReports: React.FC = () => {
  const [selectedReport, setSelectedReport] = useState<ReportType>('trial-balance');
  const [trialBalance, setTrialBalance] = useState<TrialBalance | null>(null);
  const [profitLoss, setProfitLoss] = useState<ProfitAndLoss | null>(null);
  const [balanceSheet, setBalanceSheet] = useState<BalanceSheet | null>(null);
  const [loading, setLoading] = useState(false);

  const reports = [
    { id: 'trial-balance', name: 'Trial Balance', icon: FileText },
    { id: 'profit-loss', name: 'Profit & Loss', icon: TrendingUp },
    { id: 'balance-sheet', name: 'Balance Sheet', icon: FileText },
    { id: 'cash-flow', name: 'Cash Flow Statement', icon: TrendingUp },
    { id: 'general-ledger', name: 'General Ledger', icon: FileText },
  ];

  const loadTrialBalance = async () => {
    setLoading(true);
    try {
      const data = await financialReportService.getTrialBalance(new Date());
      setTrialBalance(data);
    } catch (error) {
      console.error('Failed to load trial balance:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadProfitLoss = async () => {
    setLoading(true);
    try {
      const data = await financialReportService.getProfitAndLoss({
        from: new Date(new Date().getFullYear(), 0, 1),
        to: new Date(),
      });
      setProfitLoss(data);
    } catch (error) {
      console.error('Failed to load P&L:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadBalanceSheet = async () => {
    setLoading(true);
    try {
      const data = await financialReportService.getBalanceSheet(new Date());
      setBalanceSheet(data);
    } catch (error) {
      console.error('Failed to load balance sheet:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleReportSelect = (reportId: ReportType) => {
    setSelectedReport(reportId);
    if (reportId === 'trial-balance') loadTrialBalance();
    else if (reportId === 'profit-loss') loadProfitLoss();
    else if (reportId === 'balance-sheet') loadBalanceSheet();
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Financial Reports</h1>
        <Button variant="outline">
          <Download className="w-4 h-4 mr-2" />
          Export
        </Button>
      </div>

      <div className="grid grid-cols-5 gap-4 mb-6">
        {reports.map((report) => (
          <button
            key={report.id}
            onClick={() => handleReportSelect(report.id as ReportType)}
            className={`p-4 rounded-lg border-2 transition-colors ${
              selectedReport === report.id
                ? 'border-primary-500 bg-primary-50'
                : 'border-neutral-200 hover:border-primary-300'
            }`}
          >
            <report.icon className="w-8 h-8 mx-auto mb-2 text-primary-600" />
            <div className="text-sm font-medium text-center">{report.name}</div>
          </button>
        ))}
      </div>

      {selectedReport === 'trial-balance' && trialBalance && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">
            Trial Balance - {new Date(trialBalance.asOfDate).toLocaleDateString()}
          </h2>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-3 px-4">Account Code</th>
                  <th className="text-left py-3 px-4">Account Name</th>
                  <th className="text-right py-3 px-4">Debit</th>
                  <th className="text-right py-3 px-4">Credit</th>
                </tr>
              </thead>
              <tbody>
                {trialBalance.accounts.map((account) => (
                  <tr key={account.accountCode} className="border-b border-neutral-100">
                    <td className="py-3 px-4">{account.accountCode}</td>
                    <td className="py-3 px-4">{account.accountName}</td>
                    <td className="py-3 px-4 text-right">
                      {account.debit > 0 ? `₦${account.debit.toLocaleString()}` : '-'}
                    </td>
                    <td className="py-3 px-4 text-right">
                      {account.credit > 0 ? `₦${account.credit.toLocaleString()}` : '-'}
                    </td>
                  </tr>
                ))}
                <tr className="border-t-2 border-neutral-300 font-semibold">
                  <td colSpan={2} className="py-3 px-4">Total</td>
                  <td className="py-3 px-4 text-right">
                    ₦{trialBalance.totals.totalDebits.toLocaleString()}
                  </td>
                  <td className="py-3 px-4 text-right">
                    ₦{trialBalance.totals.totalCredits.toLocaleString()}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {selectedReport === 'profit-loss' && profitLoss && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Profit & Loss Statement</h2>
          <div className="space-y-4">
            <div>
              <h3 className="font-semibold mb-2">Revenue</h3>
              {profitLoss.revenue.items.map((item) => (
                <div key={item.accountCode} className="flex justify-between py-1 px-4">
                  <span>{item.accountName}</span>
                  <span>₦{item.amount.toLocaleString()}</span>
                </div>
              ))}
              <div className="flex justify-between py-2 px-4 font-semibold border-t">
                <span>Total Revenue</span>
                <span>₦{profitLoss.revenue.total.toLocaleString()}</span>
              </div>
            </div>

            <div>
              <h3 className="font-semibold mb-2">Cost of Sales</h3>
              {profitLoss.costOfSales.items.map((item) => (
                <div key={item.accountCode} className="flex justify-between py-1 px-4">
                  <span>{item.accountName}</span>
                  <span>₦{item.amount.toLocaleString()}</span>
                </div>
              ))}
              <div className="flex justify-between py-2 px-4 font-semibold border-t">
                <span>Total Cost of Sales</span>
                <span>₦{profitLoss.costOfSales.total.toLocaleString()}</span>
              </div>
            </div>

            <div className="flex justify-between py-2 px-4 font-bold text-lg border-t-2">
              <span>Gross Profit</span>
              <span>₦{profitLoss.grossProfit.toLocaleString()}</span>
            </div>

            <div>
              <h3 className="font-semibold mb-2">Operating Expenses</h3>
              {profitLoss.operatingExpenses.items.map((item) => (
                <div key={item.accountCode} className="flex justify-between py-1 px-4">
                  <span>{item.accountName}</span>
                  <span>₦{item.amount.toLocaleString()}</span>
                </div>
              ))}
              <div className="flex justify-between py-2 px-4 font-semibold border-t">
                <span>Total Operating Expenses</span>
                <span>₦{profitLoss.operatingExpenses.total.toLocaleString()}</span>
              </div>
            </div>

            <div className="flex justify-between py-3 px-4 font-bold text-xl border-t-2 bg-primary-50">
              <span>Net Income</span>
              <span className={profitLoss.netIncome >= 0 ? 'text-success-600' : 'text-error-600'}>
                ₦{profitLoss.netIncome.toLocaleString()}
              </span>
            </div>
          </div>
        </Card>
      )}

      {selectedReport === 'balance-sheet' && balanceSheet && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">
            Balance Sheet - {new Date(balanceSheet.asOfDate).toLocaleDateString()}
          </h2>
          <div className="grid grid-cols-2 gap-6">
            <div>
              <h3 className="font-semibold mb-3">Assets</h3>
              {balanceSheet.assets.subsections.map((subsection) => (
                <div key={subsection.name} className="mb-4">
                  <div className="font-medium mb-2">{subsection.name}</div>
                  {subsection.items.map((item) => (
                    <div key={item.accountCode} className="flex justify-between py-1 px-4 text-sm">
                      <span>{item.accountName}</span>
                      <span>₦{item.amount.toLocaleString()}</span>
                    </div>
                  ))}
                  <div className="flex justify-between py-1 px-4 font-semibold border-t">
                    <span>Subtotal</span>
                    <span>₦{subsection.total.toLocaleString()}</span>
                  </div>
                </div>
              ))}
              <div className="flex justify-between py-2 px-4 font-bold text-lg border-t-2">
                <span>Total Assets</span>
                <span>₦{balanceSheet.totalAssets.toLocaleString()}</span>
              </div>
            </div>

            <div>
              <h3 className="font-semibold mb-3">Liabilities & Equity</h3>
              {balanceSheet.liabilities.subsections.map((subsection) => (
                <div key={subsection.name} className="mb-4">
                  <div className="font-medium mb-2">{subsection.name}</div>
                  {subsection.items.map((item) => (
                    <div key={item.accountCode} className="flex justify-between py-1 px-4 text-sm">
                      <span>{item.accountName}</span>
                      <span>₦{item.amount.toLocaleString()}</span>
                    </div>
                  ))}
                  <div className="flex justify-between py-1 px-4 font-semibold border-t">
                    <span>Subtotal</span>
                    <span>₦{subsection.total.toLocaleString()}</span>
                  </div>
                </div>
              ))}
              <div className="flex justify-between py-2 px-4 font-bold border-t-2">
                <span>Total Liabilities</span>
                <span>₦{balanceSheet.totalLiabilities.toLocaleString()}</span>
              </div>

              <div className="mt-4">
                <div className="font-medium mb-2">Equity</div>
                {balanceSheet.equity.subsections.map((subsection) => (
                  <div key={subsection.name}>
                    {subsection.items.map((item) => (
                      <div key={item.accountCode} className="flex justify-between py-1 px-4 text-sm">
                        <span>{item.accountName}</span>
                        <span>₦{item.amount.toLocaleString()}</span>
                      </div>
                    ))}
                  </div>
                ))}
                <div className="flex justify-between py-2 px-4 font-bold border-t-2">
                  <span>Total Equity</span>
                  <span>₦{balanceSheet.totalEquity.toLocaleString()}</span>
                </div>
              </div>

              <div className="flex justify-between py-2 px-4 font-bold text-lg border-t-2 mt-4">
                <span>Total Liabilities & Equity</span>
                <span>₦{(balanceSheet.totalLiabilities + balanceSheet.totalEquity).toLocaleString()}</span>
              </div>
            </div>
          </div>
        </Card>
      )}
    </div>
  );
};
