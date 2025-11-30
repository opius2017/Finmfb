import React, { useState, useEffect } from 'react';
import { RefreshCw, TrendingUp, Target, Calendar } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Forecast, ForecastComparison } from '../types/forecast.types';
import { forecastService } from '../services/forecastService';

interface RollingForecastProps {
  budgetId: string;
}

export const RollingForecast: React.FC<RollingForecastProps> = ({ budgetId }) => {
  const [forecasts, setForecasts] = useState<Forecast[]>([]);
  const [selectedForecast, setSelectedForecast] = useState<Forecast | null>(null);
  const [comparison, setComparison] = useState<ForecastComparison | null>(null);

  useEffect(() => {
    loadForecasts();
  }, [budgetId]);

  const loadForecasts = async () => {
    try {
      const list = await forecastService.getForecasts(budgetId);
      setForecasts(list);
      if (list.length > 0) {
        setSelectedForecast(list[0]);
        loadComparison(list[0].id);
      }
    } catch (error) {
      console.error('Failed to load forecasts:', error);
    }
  };

  const loadComparison = async (forecastId: string) => {
    try {
      const comp = await forecastService.compareForecastVsActual(forecastId);
      setComparison(comp);
    } catch (error) {
      console.error('Failed to load comparison:', error);
    }
  };

  const handleUpdateForecast = async () => {
    if (!selectedForecast) return;
    try {
      const updated = await forecastService.updateForecast(selectedForecast.id);
      setSelectedForecast(updated);
      loadComparison(updated.id);
    } catch (error) {
      console.error('Failed to update forecast:', error);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold">Rolling Forecast</h2>
        <Button variant="primary" onClick={handleUpdateForecast}>
          <RefreshCw className="w-4 h-4 mr-2" />
          Update Forecast
        </Button>
      </div>

      {selectedForecast && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card className="p-4">
              <div className="text-sm text-neutral-600 mb-1">Forecast Horizon</div>
              <div className="text-2xl font-bold">{selectedForecast.horizonMonths} months</div>
            </Card>
            <Card className="p-4">
              <div className="text-sm text-neutral-600 mb-1">Accuracy</div>
              <div className="text-2xl font-bold">
                {selectedForecast.accuracy.overallAccuracy.toFixed(1)}%
              </div>
            </Card>
            <Card className="p-4">
              <div className="text-sm text-neutral-600 mb-1">Last Updated</div>
              <div className="text-lg font-semibold">
                {new Date(selectedForecast.lastUpdated).toLocaleDateString()}
              </div>
            </Card>
            <Card className="p-4">
              <div className="text-sm text-neutral-600 mb-1">Next Update</div>
              <div className="text-lg font-semibold">
                {new Date(selectedForecast.nextUpdate).toLocaleDateString()}
              </div>
            </Card>
          </div>

          {comparison && (
            <Card className="p-6">
              <h3 className="font-semibold mb-4">Forecast vs Actual</h3>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-neutral-200">
                      <th className="text-left py-3 px-4">Period</th>
                      <th className="text-right py-3 px-4">Forecast</th>
                      <th className="text-right py-3 px-4">Actual</th>
                      <th className="text-right py-3 px-4">Variance</th>
                      <th className="text-right py-3 px-4">Accuracy</th>
                    </tr>
                  </thead>
                  <tbody>
                    {comparison.periods.map((period) => (
                      <tr key={period.period} className="border-b border-neutral-100">
                        <td className="py-3 px-4">{period.period}</td>
                        <td className="py-3 px-4 text-right">
                          ₦{period.forecast.toLocaleString()}
                        </td>
                        <td className="py-3 px-4 text-right">
                          ₦{period.actual.toLocaleString()}
                        </td>
                        <td className="py-3 px-4 text-right">
                          <span className={period.variance > 0 ? 'text-error-600' : 'text-success-600'}>
                            {period.variancePercentage > 0 && '+'}
                            {period.variancePercentage.toFixed(1)}%
                          </span>
                        </td>
                        <td className="py-3 px-4 text-right">
                          {(100 - Math.abs(period.variancePercentage)).toFixed(1)}%
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          )}
        </>
      )}
    </div>
  );
};
