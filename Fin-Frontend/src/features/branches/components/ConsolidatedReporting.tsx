import React, { useState, useEffect } from 'react';
import { FileText, Plus, Download, CheckCircle, TrendingUp } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { ConsolidationReport, LineItem } from '../types/consolidation.types';
import { consolidationService } from '../services/consolidationService';

export const ConsolidatedReporting: React.FC = () => {
  const [reports, setReports] = useState<ConsolidationReport[]>([]);
  const [selectedReport, setSelectedReport] = useState<ConsolidationReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [view, setView] = useState<'list' | 'detail'>('list');

  useEffect(() => {
    loadReports();
  }, []);

  const loadReports = async () => {
    setLoading(true);
    try {
      const data = await consolidationService.getReports();
      setReports(data);
    } catch (error) {
      console.error('Failed to load reports:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleViewReport = async (reportId: string) => {
    try {
      const report = await consolidationService.getReport(reportId);
      setSelectedReport(report);
      setView('detail');
    } catch (error) {
      console.error('Failed to load report:', error);
    }
  };

  const handleExport = async (reportId: string, format: 'pdf' | 'excel') => {
    try {
      const blob = await consolidationService.exportReport(reportId, format);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `consolidated-report.${format}`;
      a.click();
    } catch (error) {
      console.error('Failed to export report:', error);
    }
  };

  const renderLineItems = (items: LineItem[], title: string) => (
    <div className="mb-6">
      <h3 className="font-semibold text-lg mb-3">{title}</h3>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b-2 border-neutral-300">
              <th className="text-left py-2 px-2">Account</th>
              {items[0]?.branchAmounts.map((ba) => (
                <th key={ba.branchId} className="text-right py-2 px-2">
                  {ba.branchName}
                </th>
              ))}
              <th className="text-right py-2 px-2">Eliminations</th>
              <th className="text-right py-2 px-2">Adjustments</th>
              <th className="text-right py-2 px-2 font-bold">Consolidated</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item, idx) => (
              <tr key={idx} className="border-b border-neutral-100">
                <td className="py-2 px-2">{item.accountName}</td>
                {item.branchAmounts.map((ba) => (
                  <td key={ba.branchId} className="text-right py-2 px-2">
                    {consolidationService.formatCurrency(ba.amount)}
                  </td>
                ))}
                <td className="text-right py-2 px-2 text-error-600">
                  {item.eliminations !== 0 && consolidationService.formatCurrency(item.eliminations)}
                </td>
                <td className="text-right py-2 px-2 text-warning-600">
                  {item.adjustments !== 0 && consolidationService.formatCurrency(item.adjustments)}
                </td>
                <td className="text-right py-2 px-2 font-bold">
                  {consolidationService.formatCurrency(item.consolidated)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  if (view === 'detail' && selectedReport) {
    const { financialStatements } = selectedReport;

    return (
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <Button variant="ghost" onClick={() => setView('list')}>
              ← Back to Reports
            </Button>
            <h1 className="text-2xl font-bold mt-2">{selectedReport.name}</h1>
            <p className="text-sm text-neutral-600">
              {new Date(selectedReport.period.from).toLocaleDateString()} -{' '}
              {new Date(selectedReport.period.to).toLocaleDateString()}
            </p>
          </div>
          <div className="flex space-x-3">
            <Button variant="outline" onClick={() => handleExport(selectedReport.id, 'excel')}>
              <Download className="w-4 h-4 mr-2" />
              Excel
            </Button>
            <Button variant="outline" onClick={() => handleExport(selectedReport.id, 'pdf')}>
              <Download className="w-4 h-4 mr-2" />
              PDF
            </Button>
          </div>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Assets</div>
            <div className="text-2xl font-bold">
              {consolidationService.formatCurrency(financialStatements.balanceSheet.totalAssets)}
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Liabilities</div>
            <div className="text-2xl font-bold">
              {consolidationService.formatCurrency(financialStatements.balanceSheet.totalLiabilities)}
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Equity</div>
            <div className="text-2xl font-bold text-primary-600">
              {consolidationService.formatCurrency(financialStatements.balanceSheet.totalEquity)}
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Net Income</div>
            <div className="text-2xl font-bold text-success-600">
              {consolidationService.formatCurrency(financialStatements.incomeStatement.netIncome)}
            </div>
          </Card>
        </div>

        {/* Balance Sheet */}
        <Card className="p-6 mb-6">
          <h2 className="text-xl font-bold mb-4">Consolidated Balance Sheet</h2>
          
          {renderLineItems(
            financialStatements.balanceSheet.assets.currentAssets,
            'Current Assets'
          )}
          
          {renderLineItems(
            financialStatements.balanceSheet.assets.nonCurrentAssets,
            'Non-Current Assets'
          )}

          {renderLineItems(
            financialStatements.balanceSheet.liabilities.currentLiabilities,
            'Current Liabilities'
          )}

          {renderLineItems(
            financialStatements.balanceSheet.liabilities.nonCurrentLiabilities,
            'Non-Current Liabilities'
          )}

          {renderLineItems(
            financialStatements.balanceSheet.equity.items,
            'Equity'
          )}
        </Card>

        {/* Income Statement */}
        <Card className="p-6 mb-6">
          <h2 className="text-xl font-bold mb-4">Consolidated Income Statement</h2>
          
          {renderLineItems(financialStatements.incomeStatement.revenue, 'Revenue')}
          {renderLineItems(financialStatements.incomeStatement.costOfSales, 'Cost of Sales')}
          {renderLineItems(financialStatements.incomeStatement.operatingExpenses, 'Operating Expenses')}

          <div className="mt-6 p-4 bg-neutral-50 rounded-lg">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-sm text-neutral-600">Gross Profit</div>
                <div className="text-lg font-bold">
                  {consolidationService.formatCurrency(financialStatements.incomeStatement.grossProfit)}
                </div>
              </div>
              <div>
                <div className="text-sm text-neutral-600">Operating Income</div>
                <div className="text-lg font-bold">
                  {consolidationService.formatCurrency(financialStatements.incomeStatement.operatingIncome)}
                </div>
              </div>
              <div>
                <div className="text-sm text-neutral-600">Net Income</div>
                <div className="text-xl font-bold text-success-600">
                  {consolidationService.formatCurrency(financialStatements.incomeStatement.netIncome)}
                </div>
              </div>
              <div>
                <div className="text-sm text-neutral-600">Attributable to Parent</div>
                <div className="text-xl font-bold text-primary-600">
                  {consolidationService.formatCurrency(
                    financialStatements.incomeStatement.netIncomeAttributableToParent
                  )}
                </div>
              </div>
            </div>
          </div>
        </Card>

        {/* Eliminations & Adjustments */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card className="p-6">
            <h3 className="font-semibold text-lg mb-4">Elimination Entries</h3>
            <div className="space-y-3">
              {selectedReport.eliminations.map((elim) => (
                <div key={elim.id} className="p-3 bg-error-50 border border-error-200 rounded-lg">
                  <div className="font-medium text-sm mb-1">
                    {consolidationService.getEliminationTypeLabel(elim.type)}
                  </div>
                  <div className="text-xs text-neutral-600 mb-2">{elim.description}</div>
                  <div className="text-sm font-bold text-error-700">
                    {consolidationService.formatCurrency(elim.amount)}
                  </div>
                </div>
              ))}
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="font-semibold text-lg mb-4">Consolidation Adjustments</h3>
            <div className="space-y-3">
              {selectedReport.adjustments.map((adj) => (
                <div key={adj.id} className="p-3 bg-warning-50 border border-warning-200 rounded-lg">
                  <div className="font-medium text-sm mb-1">
                    {consolidationService.getAdjustmentTypeLabel(adj.type)}
                  </div>
                  <div className="text-xs text-neutral-600 mb-2">{adj.description}</div>
                  <div className="text-sm font-bold text-warning-700">
                    {consolidationService.formatCurrency(adj.amount)}
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Consolidated Reporting</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Generate and view consolidated financial statements
          </p>
        </div>
        <Button variant="primary">
          <Plus className="w-4 h-4 mr-2" />
          New Consolidation
        </Button>
      </div>

      {/* Reports List */}
      <div className="space-y-4">
        {loading ? (
          <Card className="p-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          </Card>
        ) : reports.length === 0 ? (
          <Card className="p-8">
            <div className="text-center text-neutral-600">
              <FileText className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No consolidation reports found</p>
            </div>
          </Card>
        ) : (
          reports.map((report) => (
            <Card key={report.id} className="p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <FileText className="w-5 h-5 text-primary-600" />
                    <div>
                      <h3 className="text-lg font-semibold">{report.name}</h3>
                      <p className="text-sm text-neutral-600">
                        {new Date(report.period.from).toLocaleDateString()} -{' '}
                        {new Date(report.period.to).toLocaleDateString()}
                      </p>
                    </div>
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                      report.status === 'approved' ? 'bg-success-100 text-success-800' :
                      report.status === 'completed' ? 'bg-primary-100 text-primary-800' :
                      'bg-warning-100 text-warning-800'
                    }`}>
                      {report.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4 text-sm">
                    <div>
                      <div className="text-neutral-600">Branches</div>
                      <div className="font-medium">{report.branches.length}</div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Eliminations</div>
                      <div className="font-medium">{report.eliminations.length}</div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Adjustments</div>
                      <div className="font-medium">{report.adjustments.length}</div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Created</div>
                      <div className="font-medium">
                        {new Date(report.createdAt).toLocaleDateString()}
                      </div>
                    </div>
                  </div>
                </div>

                <div className="flex items-center space-x-2 ml-4">
                  <Button
                    variant="primary"
                    size="sm"
                    onClick={() => handleViewReport(report.id)}
                  >
                    <TrendingUp className="w-4 h-4 mr-2" />
                    View Report
                  </Button>
                  {report.status === 'approved' && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleExport(report.id, 'pdf')}
                    >
                      <Download className="w-4 h-4" />
                    </Button>
                  )}
                </div>
              </div>
            </Card>
          ))
        )}
      </div>
    </div>
  );
};
