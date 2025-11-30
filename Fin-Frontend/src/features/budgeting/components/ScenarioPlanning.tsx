import React, { useState, useEffect } from 'react';
import { Plus, GitCompare, TrendingUp, AlertCircle } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Scenario, ScenarioComparison } from '../types/scenario.types';
import { scenarioService } from '../services/scenarioService';

interface ScenarioPlanningProps {
  budgetId: string;
}

export const ScenarioPlanning: React.FC<ScenarioPlanningProps> = ({ budgetId }) => {
  const [scenarios, setScenarios] = useState<Scenario[]>([]);
  const [comparison, setComparison] = useState<ScenarioComparison | null>(null);
  const [selectedScenarios, setSelectedScenarios] = useState<Set<string>>(new Set());

  useEffect(() => {
    loadScenarios();
  }, [budgetId]);

  const loadScenarios = async () => {
    try {
      const list = await scenarioService.getScenarios(budgetId);
      setScenarios(list);
    } catch (error) {
      console.error('Failed to load scenarios:', error);
    }
  };

  const handleCompare = async () => {
    if (selectedScenarios.size < 2) return;
    try {
      const result = await scenarioService.compareScenarios(Array.from(selectedScenarios));
      setComparison(result);
    } catch (error) {
      console.error('Failed to compare scenarios:', error);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold">Scenario Planning</h2>
        <div className="flex space-x-3">
          <Button
            variant="outline"
            onClick={handleCompare}
            disabled={selectedScenarios.size < 2}
          >
            <GitCompare className="w-4 h-4 mr-2" />
            Compare ({selectedScenarios.size})
          </Button>
          <Button variant="primary">
            <Plus className="w-4 h-4 mr-2" />
            New Scenario
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {scenarios.map((scenario) => (
          <Card key={scenario.id} className="p-4">
            <div className="flex items-start justify-between mb-3">
              <div>
                <h3 className="font-semibold">{scenario.name}</h3>
                <p className="text-sm text-neutral-600 mt-1">{scenario.description}</p>
              </div>
              <input
                type="checkbox"
                checked={selectedScenarios.has(scenario.id)}
                onChange={(e) => {
                  const newSet = new Set(selectedScenarios);
                  if (e.target.checked) {
                    newSet.add(scenario.id);
                  } else {
                    newSet.delete(scenario.id);
                  }
                  setSelectedScenarios(newSet);
                }}
                className="rounded"
              />
            </div>
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-neutral-600">Revenue:</span>
                <span className="font-semibold">
                  ₦{(scenario.results.totalRevenue / 1000000).toFixed(1)}M
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-neutral-600">Net Income:</span>
                <span className="font-semibold">
                  ₦{(scenario.results.netIncome / 1000000).toFixed(1)}M
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-neutral-600">Profit Margin:</span>
                <span className="font-semibold">
                  {scenario.results.profitMargin.toFixed(1)}%
                </span>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {comparison && (
        <Card className="p-6">
          <h3 className="font-semibold mb-4">Scenario Comparison</h3>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-3 px-4">Metric</th>
                  {comparison.scenarios.map((s) => (
                    <th key={s.id} className="text-right py-3 px-4">{s.name}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {comparison.comparisonMetrics.map((metric) => (
                  <tr key={metric.metricName} className="border-b border-neutral-100">
                    <td className="py-3 px-4 font-medium">{metric.metricName}</td>
                    {comparison.scenarios.map((s) => (
                      <td key={s.id} className="py-3 px-4 text-right">
                        {metric.values[s.id]?.toLocaleString()} {metric.unit}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  );
};
