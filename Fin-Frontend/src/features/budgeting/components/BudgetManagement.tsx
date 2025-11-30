import React, { useState, useEffect } from 'react';
import { Plus, TrendingUp, TrendingDown, BarChart, Download, Edit } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Budget, BudgetStatistics } from '../types/budget.types';
import { budgetService } from '../services/budgetService';
import { BudgetWizard } from './BudgetWizard';

type ViewMode = 'list' | 'create' | 'edit' | 'variance' | 'scenario';

export const BudgetManagement: React.FC = () => {
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [budgets, setBudgets] = useState<Budget[]>([]);
  const [statistics, setStatistics] = useState<BudgetStatistics | null>(null);
  const [selectedBudget, setSelectedBudget] = useState<Budget | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadBudgets();
    loadStatistics();
  }, []);

  const loadBudgets = async () => {
    setLoading(true);
    try {
      const result = await budgetService.getBudgets();
      setBudgets(result.budgets);
    } catch (error) {
      console.error('Failed to load budgets:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadStatistics = async () => {
    try {
      const stats = await budgetService.getStatistics();
      setStatistics(stats);
    } catch (error) {
      console.error('Failed to load statistics:', error);
    }
  };

  const handleBudgetCreated = (budget: Budget) => {
    setBudgets([budget, ...budgets]);
    setViewMode('list');
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'approved':
      case 'active':
        return 'bg-success-100 text-success-800';
      case 'submitted':
      case 'under-review':
        return 'bg-warning-100 text-warning-800';
      case 'draft':
        return 'bg-neutral-100 text-neutral-800';
      case 'rejected':
        return 'bg-error-100 text-error-800';
      default:
        return 'bg-neutral-100 text-neutral-800';
    }
  };

  const getVarianceColor = (variance: number) => {
    if (variance > 0) return 'text-error-600';
    if (variance < 0) return 'text-success-600';
    return 'text-neutral-600';
  };

  if (viewMode === 'create') {
    return (
      <div className="p-6">
        <BudgetWizard
          onComplete={handleBudgetCreated}
          onCancel={() => setViewMode('list')}
        />
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Budget Management</h1>
          <p className="text-neutral-600 mt-1">
            Create and manage budgets with variance analysis
          </p>
        </div>
        <Button variant="primary" onClick={() => setViewMode('create')}>
          <Plus className="w-4 h-4 mr-2" />
          Create Budget
        </Button>
      </div>

      {/* Statistics Cards */}
      {statistics && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Budgets</div>
            <div className="text-2xl font-bold">{statistics.totalBudgets}</div>
            <div className="text-xs text-success-600 mt-1">
              {statistics.activeBudgets} active
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Budget</div>
            <div className="text-2xl font-bold">
              ₦{(statistics.totalBudgetAmount / 1000000).toFixed(1)}M
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Actual</div>
            <div className="text-2xl font-bold">
              ₦{(statistics.totalActualAmount / 1000000).toFixed(1)}M
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Overall Variance</div>
            <div className={`text-2xl font-bold ${getVarianceColor(statistics.overallVariance)}`}>
              {statistics.overallVariancePercentage > 0 && '+'}
              {statistics.overallVariancePercentage.toFixed(1)}%
            </div>
            <div className="flex items-center mt-1">
              {statistics.overallVariance > 0 ? (
                <TrendingUp className="w-4 h-4 text-error-600 mr-1" />
              ) : (
                <TrendingDown className="w-4 h-4 text-success-600 mr-1" />
              )}
              <span className="text-xs">
                ₦{Math.abs(statistics.overallVariance / 1000000).toFixed(1)}M
              </span>
            </div>
          </Card>
        </div>
      )}

      {/* Budgets List */}
      <Card className="p-6">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-neutral-200">
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Budget Name
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Fiscal Year
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Type
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Budget Amount
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Actual Amount
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Variance
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Status
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {budgets.map((budget) => (
                <tr key={budget.id} className="border-b border-neutral-100 hover:bg-neutral-50">
                  <td className="py-3 px-4">
                    <div className="font-medium">{budget.name}</div>
                    <div className="text-xs text-neutral-600">
                      Version {budget.version}
                    </div>
                  </td>
                  <td className="py-3 px-4">{budget.fiscalYear}</td>
                  <td className="py-3 px-4 capitalize">{budget.type}</td>
                  <td className="py-3 px-4 text-right font-semibold">
                    ₦{budget.totalBudget.toLocaleString()}
                  </td>
                  <td className="py-3 px-4 text-right">
                    ₦{budget.totalActual.toLocaleString()}
                  </td>
                  <td className="py-3 px-4 text-right">
                    <div className={`font-semibold ${getVarianceColor(budget.totalVariance)}`}>
                      {budget.variancePercentage > 0 && '+'}
                      {budget.variancePercentage.toFixed(1)}%
                    </div>
                    <div className="text-xs text-neutral-600">
                      ₦{Math.abs(budget.totalVariance).toLocaleString()}
                    </div>
                  </td>
                  <td className="py-3 px-4">
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(budget.status)}`}>
                      {budget.status}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-right">
                    <div className="flex justify-end space-x-2">
                      <Button variant="ghost" size="sm">
                        <Edit className="w-4 h-4" />
                      </Button>
                      <Button variant="ghost" size="sm">
                        <BarChart className="w-4 h-4" />
                      </Button>
                      <Button variant="ghost" size="sm">
                        <Download className="w-4 h-4" />
                      </Button>
                    </div>
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
