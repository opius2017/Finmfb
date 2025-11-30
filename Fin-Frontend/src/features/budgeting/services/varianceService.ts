import {
  VarianceReport,
  VarianceAlert,
  VarianceExplanation,
  VarianceTrend,
  VarianceThreshold,
  VarianceFilter,
  Period,
} from '../types/variance.types';

export class VarianceService {
  private apiEndpoint = '/api/budgets/variance';

  /**
   * Generate variance report
   */
  async generateReport(budgetId: string, period: Period): Promise<VarianceReport> {
    const response = await fetch(`${this.apiEndpoint}/report`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ budgetId, period }),
    });

    if (!response.ok) {
      throw new Error('Failed to generate variance report');
    }

    return response.json();
  }

  /**
   * Get variance alerts
   */
  async getAlerts(budgetId: string, status?: string): Promise<VarianceAlert[]> {
    const params = new URLSearchParams({ budgetId });
    if (status) params.append('status', status);

    const response = await fetch(`${this.apiEndpoint}/alerts?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch variance alerts');
    }

    return response.json();
  }

  /**
   * Acknowledge variance alert
   */
  async acknowledgeAlert(alertId: string): Promise<VarianceAlert> {
    const response = await fetch(`${this.apiEndpoint}/alerts/${alertId}/acknowledge`, {
      method: 'POST',
    });

    if (!response.ok) {
      throw new Error('Failed to acknowledge alert');
    }

    return response.json();
  }

  /**
   * Resolve variance alert
   */
  async resolveAlert(alertId: string, resolution: string): Promise<VarianceAlert> {
    const response = await fetch(`${this.apiEndpoint}/alerts/${alertId}/resolve`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ resolution }),
    });

    if (!response.ok) {
      throw new Error('Failed to resolve alert');
    }

    return response.json();
  }

  /**
   * Submit variance explanation
   */
  async submitExplanation(
    budgetId: string,
    lineId: string,
    explanation: string,
    actionPlan?: string
  ): Promise<VarianceExplanation> {
    const response = await fetch(`${this.apiEndpoint}/explanations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ budgetId, lineId, explanation, actionPlan }),
    });

    if (!response.ok) {
      throw new Error('Failed to submit explanation');
    }

    return response.json();
  }

  /**
   * Get variance explanations
   */
  async getExplanations(budgetId: string): Promise<VarianceExplanation[]> {
    const response = await fetch(`${this.apiEndpoint}/explanations?budgetId=${budgetId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch explanations');
    }

    return response.json();
  }

  /**
   * Get variance trends
   */
  async getTrends(budgetId: string, accountIds?: string[]): Promise<VarianceTrend[]> {
    const params = new URLSearchParams({ budgetId });
    if (accountIds) {
      accountIds.forEach(id => params.append('accountIds', id));
    }

    const response = await fetch(`${this.apiEndpoint}/trends?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch variance trends');
    }

    return response.json();
  }

  /**
   * Get variance thresholds
   */
  async getThresholds(): Promise<VarianceThreshold[]> {
    const response = await fetch(`${this.apiEndpoint}/thresholds`);

    if (!response.ok) {
      throw new Error('Failed to fetch thresholds');
    }

    return response.json();
  }

  /**
   * Create variance threshold
   */
  async createThreshold(threshold: Partial<VarianceThreshold>): Promise<VarianceThreshold> {
    const response = await fetch(`${this.apiEndpoint}/thresholds`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(threshold),
    });

    if (!response.ok) {
      throw new Error('Failed to create threshold');
    }

    return response.json();
  }

  /**
   * Update variance threshold
   */
  async updateThreshold(
    thresholdId: string,
    threshold: Partial<VarianceThreshold>
  ): Promise<VarianceThreshold> {
    const response = await fetch(`${this.apiEndpoint}/thresholds/${thresholdId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(threshold),
    });

    if (!response.ok) {
      throw new Error('Failed to update threshold');
    }

    return response.json();
  }

  /**
   * Delete variance threshold
   */
  async deleteThreshold(thresholdId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/thresholds/${thresholdId}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error('Failed to delete threshold');
    }
  }

  /**
   * Export variance report
   */
  async exportReport(budgetId: string, period: Period, format: 'excel' | 'pdf'): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/export`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ budgetId, period, format }),
    });

    if (!response.ok) {
      throw new Error('Failed to export report');
    }

    return response.blob();
  }

  /**
   * Calculate variance type
   */
  calculateVarianceType(
    accountCategory: string,
    variance: number
  ): 'favorable' | 'unfavorable' | 'neutral' {
    // Revenue and income accounts: positive variance is favorable
    if (accountCategory.toLowerCase().includes('revenue') || 
        accountCategory.toLowerCase().includes('income')) {
      if (variance > 0) return 'favorable';
      if (variance < 0) return 'unfavorable';
    }
    
    // Expense accounts: negative variance is favorable
    if (accountCategory.toLowerCase().includes('expense') || 
        accountCategory.toLowerCase().includes('cost')) {
      if (variance < 0) return 'favorable';
      if (variance > 0) return 'unfavorable';
    }

    return 'neutral';
  }

  /**
   * Determine if variance is significant
   */
  isSignificantVariance(variance: number, threshold: number): boolean {
    return Math.abs(variance) >= threshold;
  }

  /**
   * Calculate trend direction
   */
  calculateTrend(variances: number[]): 'up' | 'down' | 'stable' {
    if (variances.length < 2) return 'stable';

    const recentVariances = variances.slice(-3);
    const average = recentVariances.reduce((sum, v) => sum + v, 0) / recentVariances.length;

    if (average > 5) return 'up';
    if (average < -5) return 'down';
    return 'stable';
  }
}

export const varianceService = new VarianceService();
