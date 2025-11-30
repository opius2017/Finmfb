import React, { useState, useEffect } from 'react';
import { TrendingUp, TrendingDown, AlertTriangle, MessageSquare, Download, Filter } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { VarianceReport, VarianceAlert, Period } from '../types/variance.types';
import { varianceService } from '../services/varianceService';

interface VarianceAnalysisProps {
  budgetId: string;
  budgetName: string;
}

export const VarianceAnalysis: React.FC<VarianceAnalysisProps> = ({ budgetId, budgetName }) => {
  const [report, setReport] = useState<VarianceReport | null>(null);
  const [alerts, setAlerts] = useState<VarianceAlert[]>([]);
  const [selectedPeriod, setSelectedPeriod] = useState<Period>({
    type: 'ytd',
    startDate: new Date(new Date().getFullYear(), 0, 1),
    endDate: new Date(),
    label: 'Year to Date',
  });
  const [loading, setLoading] = useState(false);
  const [showExplanationModal, setShowExplanationModal] = useState(false);
  const [selectedLine, setSelectedLine] = useState<any>(null);

  useEffect(() => {
    loadReport();
    loadAlerts();
  }, [budgetId, selectedPeriod]);

  const loadReport = async () => {
    setLoading(true);
    try {
      const varianceReport = await varianceService.generateReport(budgetId, selectedPeriod);
      setReport(varianceReport);
    } catch (error) {
      console.error('Failed to load variance report:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadAlerts = async () => {
    try {
      const alertList = await varianceService.getAlerts(budgetId, 'open');
      setAlerts(alertList);
    } catch (error) {
      console.error('Failed to load alerts:', error);
    }
  };

  const getVarianceColor = (varianceType: string) => {
    switch (varianceType) {
      case 'favorable':
        return 'text-success-600 bg-success-50';
      case 'unfavorable':
        return 'text-error-600 bg-error-50';
      default:
        return 'text-neutral-600 bg-neutral-50';
    }
  };

  const getVarianceIcon = (varianceType: string) => {
    if (varianceType === 'favorable') return <TrendingUp className="w-4 h-4" />;
    if (varianceType === 'unfavorable') return <TrendingDown className="w-4 h-4" />;
    return null;
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical':
        return 'bg-error-100 text-error-800';
      case 'high':
        return 'bg-warning-100 text-warning-800';
      case 'medium':
        return 'bg-primary-100 text-primary-800';
      default:
        return 'bg-neutral-100 text-neutral-800';
    }
  };

  const handleExportReport = async (format: 'excel' | 'pdf') => {
    try {
      const blob = await varianceService.exportReport(budgetId, selectedPeriod, format);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `variance-report-${budgetName}.${format}`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export report:', error);
    }
  };

  if (!report) {
    return <div>Loading...</div>;
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold">Variance Analysis</h2>
          <p className="text-sm text-neutral-600 mt-1">{budgetName} - {selectedPeriod.label}</p>
        </div>
        <div className="flex space-x-3">
          <Button variant="outline" size="sm">
            <Filter className="w-4 h-4 mr-2" />
            Filter
          </Button>
          <Button variant="outline" size="sm" onClick={() => handleExportReport('excel')}>
            <Download className="w-4 h-4 mr-2" />
            Export
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Total Budget</div>
          <div className="text-2xl font-bold">
            ₦{(report.summary.totalBudget / 1000000).toFixed(1)}M
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Total Actual</div>
          <div className="text-2xl font-bold">
            ₦{(report.summary.totalActual / 1000000).toFixed(1)}M
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Total Variance</div>
          <div className={`text-2xl font-bold ${
            report.summary.totalVariance > 0 ? 'text-error-600' : 'text-success-600'
          }`}>
            {report.summary.variancePercentage > 0 && '+'}
            {report.summary.variancePercentage.toFixed(1)}%
          </div>
          <div className="text-xs text-neutral-600 mt-1">
            ₦{Math.abs(report.summary.totalVariance / 1000000).toFixed(1)}M
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Significant Variances</div>
          <div className="text-2xl font-bold">{report.summary.significantVariances}</div>
          <div className="text-xs text-neutral-600 mt-1">
            {report.summary.unfavorableVariances} unfavorable
          </div>
        </Card>
      </div>

      {/* Active Alerts */}
      {alerts.length > 0 && (
        <Card className="p-6">
          <h3 className="font-semibold mb-4 flex items-center">
            <AlertTriangle className="w-5 h-5 text-warning-600 mr-2" />
            Active Variance Alerts ({alerts.length})
          </h3>
          <div className="space-y-2">
            {alerts.slice(0, 5).map((alert) => (
              <div
                key={alert.id}
                className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
              >
                <div className="flex-1">
                  <div className="font-medium">{alert.accountName}</div>
                  <div className="text-sm text-neutral-600">
                    Variance: {alert.variancePercentage.toFixed(1)}% 
                    (₦{Math.abs(alert.variance).toLocaleString()})
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getSeverityColor(alert.severity)}`}>
                    {alert.severity}
                  </span>
                  <Button variant="ghost" size="sm">
                    Review
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Variance by Category */}
      <Card className="p-6">
        <h3 className="font-semibold mb-4">Variance by Category</h3>
        <div className="space-y-3">
          {report.summary.variancesByCategory.map((category) => (
            <div key={category.category} className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg">
              <div className="flex-1">
                <div className="font-medium">{category.category}</div>
                <div className="text-sm text-neutral-600">
                  {category.accountCount} accounts
                </div>
              </div>
              <div className="text-right">
                <div className="font-semibold">
                  ₦{category.budgetAmount.toLocaleString()}
                </div>
                <div className={`text-sm ${
                  category.variance > 0 ? 'text-error-600' : 'text-success-600'
                }`}>
                  {category.variancePercentage > 0 && '+'}
                  {category.variancePercentage.toFixed(1)}%
                </div>
              </div>
            </div>
          ))}
        </div>
      </Card>

      {/* Detailed Variance Report */}
      <Card className="p-6">
        <h3 className="font-semibold mb-4">Detailed Variance Report</h3>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-neutral-200">
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Account
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Budget
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Actual
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Variance
                </th>
                <th className="text-center py-3 px-4 font-semibold text-neutral-700">
                  Type
                </th>
                <th className="text-center py-3 px-4 font-semibold text-neutral-700">
                  Trend
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {report.lines.map((line) => (
                <tr key={line.accountId} className="border-b border-neutral-100 hover:bg-neutral-50">
                  <td className="py-3 px-4">
                    <div className="font-medium">{line.accountName}</div>
                    <div className="text-xs text-neutral-600">{line.accountCode}</div>
                  </td>
                  <td className="py-3 px-4 text-right">
                    ₦{line.budgetAmount.toLocaleString()}
                  </td>
                  <td className="py-3 px-4 text-right">
                    ₦{line.actualAmount.toLocaleString()}
                  </td>
                  <td className="py-3 px-4 text-right">
                    <div className="font-semibold">
                      {line.variancePercentage > 0 && '+'}
                      {line.variancePercentage.toFixed(1)}%
                    </div>
                    <div className="text-xs text-neutral-600">
                      ₦{Math.abs(line.variance).toLocaleString()}
                    </div>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className={`inline-flex items-center space-x-1 px-2 py-1 text-xs font-semibold rounded-full ${getVarianceColor(line.varianceType)}`}>
                      {getVarianceIcon(line.varianceType)}
                      <span className="capitalize">{line.varianceType}</span>
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="text-xs capitalize">{line.trend}</span>
                  </td>
                  <td className="py-3 px-4 text-right">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setSelectedLine(line);
                        setShowExplanationModal(true);
                      }}
                    >
                      <MessageSquare className="w-4 h-4" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};
