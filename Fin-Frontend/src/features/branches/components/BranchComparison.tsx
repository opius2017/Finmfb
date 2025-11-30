import React, { useState, useEffect } from 'react';
import { BarChart3, TrendingUp, Award, Target } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { Branch, BranchComparison as ComparisonData } from '../types/branch.types';
import { branchService } from '../services/branchService';

export const BranchComparison: React.FC = () => {
  const [branches, setBranches] = useState<Branch[]>([]);
  const [selectedBranches, setSelectedBranches] = useState<string[]>([]);
  const [comparison, setComparison] = useState<ComparisonData | null>(null);
  const [loading, setLoading] = useState(false);
  const [dateRange, setDateRange] = useState({
    from: new Date(new Date().getFullYear(), 0, 1),
    to: new Date(),
  });

  useEffect(() => {
    loadBranches();
  }, []);

  const loadBranches = async () => {
    try {
      const data = await branchService.getBranches('active');
      setBranches(data);
      if (data.length > 0) {
        setSelectedBranches(data.slice(0, 3).map(b => b.id));
      }
    } catch (error) {
      console.error('Failed to load branches:', error);
    }
  };

  const loadComparison = async () => {
    if (selectedBranches.length === 0) return;

    setLoading(true);
    try {
      const data = await branchService.compareBranches(
        selectedBranches,
        dateRange.from,
        dateRange.to
      );
      setComparison(data);
    } catch (error) {
      console.error('Failed to load comparison:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (selectedBranches.length > 0) {
      loadComparison();
    }
  }, [selectedBranches, dateRange]);

  const toggleBranch = (branchId: string) => {
    setSelectedBranches(prev =>
      prev.includes(branchId)
        ? prev.filter(id => id !== branchId)
        : [...prev, branchId]
    );
  };

  const getRankColor = (rank: number) => {
    switch (rank) {
      case 1: return 'text-yellow-600 bg-yellow-50';
      case 2: return 'text-neutral-600 bg-neutral-100';
      case 3: return 'text-orange-600 bg-orange-50';
      default: return 'text-neutral-600 bg-neutral-50';
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Branch Comparison</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Compare performance across branches
          </p>
        </div>
        <Button variant="outline" onClick={loadComparison}>
          Refresh
        </Button>
      </div>

      {/* Branch Selection */}
      <Card className="p-6 mb-6">
        <h3 className="text-lg font-semibold mb-4">Select Branches to Compare</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          {branches.map((branch) => (
            <button
              key={branch.id}
              onClick={() => toggleBranch(branch.id)}
              className={`p-3 rounded-lg border-2 transition-all ${
                selectedBranches.includes(branch.id)
                  ? 'border-primary-600 bg-primary-50'
                  : 'border-neutral-200 hover:border-neutral-300'
              }`}
            >
              <div className="font-medium text-sm">{branch.name}</div>
              <div className="text-xs text-neutral-600">{branch.code}</div>
            </button>
          ))}
        </div>
      </Card>

      {loading ? (
        <Card className="p-8">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
          </div>
        </Card>
      ) : comparison ? (
        <>
          {/* Rankings */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            {['revenue', 'profit', 'profitMargin'].map((metric) => {
              const rankings = comparison.rankings.filter(r => r.metric === metric);
              const topBranch = rankings[0];

              return (
                <Card key={metric} className="p-4">
                  <div className="flex items-center space-x-2 mb-3">
                    <Award className="w-5 h-5 text-primary-600" />
                    <h3 className="font-semibold capitalize">
                      {metric.replace(/([A-Z])/g, ' $1').trim()}
                    </h3>
                  </div>
                  {topBranch && (
                    <div>
                      <div className="text-lg font-bold">{topBranch.branchName}</div>
                      <div className="text-sm text-neutral-600">
                        {metric === 'profitMargin'
                          ? `${topBranch.value.toFixed(1)}%`
                          : `₦${topBranch.value.toLocaleString()}`}
                      </div>
                    </div>
                  )}
                </Card>
              );
            })}
          </div>

          {/* Performance Comparison Table */}
          <Card className="p-6 mb-6">
            <div className="flex items-center space-x-3 mb-4">
              <BarChart3 className="w-6 h-6 text-primary-600" />
              <h2 className="text-lg font-semibold">Performance Metrics</h2>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200">
                    <th className="text-left py-3 px-4 font-semibold">Branch</th>
                    <th className="text-right py-3 px-4 font-semibold">Revenue</th>
                    <th className="text-right py-3 px-4 font-semibold">Expenses</th>
                    <th className="text-right py-3 px-4 font-semibold">Profit</th>
                    <th className="text-right py-3 px-4 font-semibold">Margin</th>
                    <th className="text-right py-3 px-4 font-semibold">Growth</th>
                  </tr>
                </thead>
                <tbody>
                  {comparison.branches.map((branch) => (
                    <tr key={branch.branchId} className="border-b border-neutral-100">
                      <td className="py-3 px-4 font-medium">{branch.branchName}</td>
                      <td className="text-right py-3 px-4">
                        ₦{branch.revenue.toLocaleString()}
                      </td>
                      <td className="text-right py-3 px-4">
                        ₦{branch.expenses.toLocaleString()}
                      </td>
                      <td className="text-right py-3 px-4 font-medium">
                        ₦{branch.profit.toLocaleString()}
                      </td>
                      <td className="text-right py-3 px-4">
                        {branch.profitMargin.toFixed(1)}%
                      </td>
                      <td className={`text-right py-3 px-4 font-medium ${
                        branch.growth >= 0 ? 'text-success-600' : 'text-error-600'
                      }`}>
                        {branch.growth >= 0 && '+'}{branch.growth.toFixed(1)}%
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>

          {/* Rankings by Metric */}
          <Card className="p-6 mb-6">
            <div className="flex items-center space-x-3 mb-4">
              <Target className="w-6 h-6 text-primary-600" />
              <h2 className="text-lg font-semibold">Rankings</h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              {['revenue', 'profit', 'profitMargin'].map((metric) => {
                const rankings = comparison.rankings
                  .filter(r => r.metric === metric)
                  .sort((a, b) => a.rank - b.rank);

                return (
                  <div key={metric}>
                    <h3 className="font-semibold mb-3 capitalize">
                      {metric.replace(/([A-Z])/g, ' $1').trim()}
                    </h3>
                    <div className="space-y-2">
                      {rankings.map((ranking) => (
                        <div
                          key={ranking.branchId}
                          className="flex items-center justify-between p-2 rounded-lg bg-neutral-50"
                        >
                          <div className="flex items-center space-x-2">
                            <span className={`w-6 h-6 flex items-center justify-center rounded-full text-xs font-bold ${getRankColor(ranking.rank)}`}>
                              {ranking.rank}
                            </span>
                            <span className="text-sm font-medium">
                              {ranking.branchName}
                            </span>
                          </div>
                          <span className="text-sm text-neutral-600">
                            {metric === 'profitMargin'
                              ? `${ranking.value.toFixed(1)}%`
                              : `₦${ranking.value.toLocaleString()}`}
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                );
              })}
            </div>
          </Card>

          {/* Insights */}
          {comparison.insights.length > 0 && (
            <Card className="p-6">
              <div className="flex items-center space-x-3 mb-4">
                <TrendingUp className="w-6 h-6 text-primary-600" />
                <h2 className="text-lg font-semibold">Key Insights</h2>
              </div>

              <div className="space-y-2">
                {comparison.insights.map((insight, index) => (
                  <div
                    key={index}
                    className="p-3 bg-primary-50 border border-primary-200 rounded-lg text-sm"
                  >
                    {insight}
                  </div>
                ))}
              </div>
            </Card>
          )}
        </>
      ) : (
        <Card className="p-8">
          <div className="text-center text-neutral-600">
            <BarChart3 className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
            <p>Select branches to compare their performance</p>
          </div>
        </Card>
      )}
    </div>
  );
};
