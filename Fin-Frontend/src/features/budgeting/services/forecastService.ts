import { Forecast, SeasonalityPattern, ForecastComparison } from '../types/forecast.types';

export class ForecastService {
  private apiEndpoint = '/api/budgets/forecasts';

  async getForecasts(budgetId: string): Promise<Forecast[]> {
    const response = await fetch(`${this.apiEndpoint}?budgetId=${budgetId}`);
    if (!response.ok) throw new Error('Failed to fetch forecasts');
    return response.json();
  }

  async createForecast(forecast: Partial<Forecast>): Promise<Forecast> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(forecast),
    });
    if (!response.ok) throw new Error('Failed to create forecast');
    return response.json();
  }

  async updateForecast(forecastId: string): Promise<Forecast> {
    const response = await fetch(`${this.apiEndpoint}/${forecastId}/update`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to update forecast');
    return response.json();
  }

  async detectSeasonality(budgetId: string): Promise<SeasonalityPattern[]> {
    const response = await fetch(`${this.apiEndpoint}/seasonality?budgetId=${budgetId}`);
    if (!response.ok) throw new Error('Failed to detect seasonality');
    return response.json();
  }

  async compareForecastVsActual(forecastId: string): Promise<ForecastComparison> {
    const response = await fetch(`${this.apiEndpoint}/${forecastId}/compare`);
    if (!response.ok) throw new Error('Failed to compare forecast');
    return response.json();
  }
}

export const forecastService = new ForecastService();
