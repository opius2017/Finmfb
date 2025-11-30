import {
  Budget,
  BudgetLine,
  BudgetTemplate,
  BudgetCopyOptions,
  BudgetComparison,
  BudgetFilter,
  BudgetStatistics,
  BudgetVersion,
} from '../types/budget.types';

export class BudgetService {
  private apiEndpoint = '/api/budgets';

  /**
   * Get all budgets with filters
   */
  async getBudgets(filters?: BudgetFilter): Promise<{ budgets: Budget[]; total: number }> {
    const params = new URLSearchParams();
    
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch budgets');
    }

    return response.json();
  }

  /**
   * Get budget by ID
   */
  async getBudget(budgetId: string): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch budget');
    }

    return response.json();
  }

  /**
   * Create new budget
   */
  async createBudget(budget: Partial<Budget>): Promise<Budget> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(budget),
    });

    if (!response.ok) {
      throw new Error('Failed to create budget');
    }

    return response.json();
  }

  /**
   * Update budget
   */
  async updateBudget(budgetId: string, budget: Partial<Budget>): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(budget),
    });

    if (!response.ok) {
      throw new Error('Failed to update budget');
    }

    return response.json();
  }

  /**
   * Delete budget
   */
  async deleteBudget(budgetId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error('Failed to delete budget');
    }
  }

  /**
   * Get budget templates
   */
  async getTemplates(industry?: string): Promise<BudgetTemplate[]> {
    const params = new URLSearchParams();
    if (industry) params.append('industry', industry);

    const response = await fetch(`${this.apiEndpoint}/templates?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch templates');
    }

    return response.json();
  }

  /**
   * Create budget from template
   */
  async createFromTemplate(
    templateId: string,
    fiscalYear: number,
    name: string
  ): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/from-template`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ templateId, fiscalYear, name }),
    });

    if (!response.ok) {
      throw new Error('Failed to create budget from template');
    }

    return response.json();
  }

  /**
   * Copy budget from prior year
   */
  async copyFromPriorYear(options: BudgetCopyOptions): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/copy`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(options),
    });

    if (!response.ok) {
      throw new Error('Failed to copy budget');
    }

    return response.json();
  }

  /**
   * Add budget line
   */
  async addBudgetLine(budgetId: string, line: Partial<BudgetLine>): Promise<BudgetLine> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/lines`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(line),
    });

    if (!response.ok) {
      throw new Error('Failed to add budget line');
    }

    return response.json();
  }

  /**
   * Update budget line
   */
  async updateBudgetLine(
    budgetId: string,
    lineId: string,
    line: Partial<BudgetLine>
  ): Promise<BudgetLine> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/lines/${lineId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(line),
    });

    if (!response.ok) {
      throw new Error('Failed to update budget line');
    }

    return response.json();
  }

  /**
   * Delete budget line
   */
  async deleteBudgetLine(budgetId: string, lineId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/lines/${lineId}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error('Failed to delete budget line');
    }
  }

  /**
   * Submit budget for approval
   */
  async submitForApproval(budgetId: string): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/submit`, {
      method: 'POST',
    });

    if (!response.ok) {
      throw new Error('Failed to submit budget for approval');
    }

    return response.json();
  }

  /**
   * Approve budget
   */
  async approveBudget(budgetId: string, comments?: string): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/approve`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ comments }),
    });

    if (!response.ok) {
      throw new Error('Failed to approve budget');
    }

    return response.json();
  }

  /**
   * Reject budget
   */
  async rejectBudget(budgetId: string, reason: string): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/reject`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });

    if (!response.ok) {
      throw new Error('Failed to reject budget');
    }

    return response.json();
  }

  /**
   * Compare two budgets
   */
  async compareBudgets(budget1Id: string, budget2Id: string): Promise<BudgetComparison> {
    const response = await fetch(
      `${this.apiEndpoint}/compare?budget1=${budget1Id}&budget2=${budget2Id}`
    );

    if (!response.ok) {
      throw new Error('Failed to compare budgets');
    }

    return response.json();
  }

  /**
   * Get budget statistics
   */
  async getStatistics(fiscalYear?: number): Promise<BudgetStatistics> {
    const params = new URLSearchParams();
    if (fiscalYear) params.append('fiscalYear', fiscalYear.toString());

    const response = await fetch(`${this.apiEndpoint}/statistics?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch statistics');
    }

    return response.json();
  }

  /**
   * Get budget versions
   */
  async getVersions(budgetId: string): Promise<BudgetVersion[]> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/versions`);

    if (!response.ok) {
      throw new Error('Failed to fetch versions');
    }

    return response.json();
  }

  /**
   * Restore budget version
   */
  async restoreVersion(budgetId: string, version: number): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/restore/${version}`, {
      method: 'POST',
    });

    if (!response.ok) {
      throw new Error('Failed to restore version');
    }

    return response.json();
  }

  /**
   * Lock budget period
   */
  async lockPeriod(budgetId: string, month: number, year: number): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/lock-period`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ month, year }),
    });

    if (!response.ok) {
      throw new Error('Failed to lock period');
    }

    return response.json();
  }

  /**
   * Unlock budget period
   */
  async unlockPeriod(budgetId: string, month: number, year: number): Promise<Budget> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/unlock-period`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ month, year }),
    });

    if (!response.ok) {
      throw new Error('Failed to unlock period');
    }

    return response.json();
  }

  /**
   * Export budget to Excel
   */
  async exportToExcel(budgetId: string): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/export/excel`);

    if (!response.ok) {
      throw new Error('Failed to export budget');
    }

    return response.blob();
  }

  /**
   * Import budget from Excel
   */
  async importFromExcel(budgetId: string, file: File): Promise<Budget> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${this.apiEndpoint}/${budgetId}/import`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error('Failed to import budget');
    }

    return response.json();
  }

  /**
   * Calculate period allocation
   */
  calculatePeriodAllocation(
    totalAmount: number,
    method: 'equal' | 'weighted' | 'custom',
    periods: number,
    weights?: number[]
  ): number[] {
    if (method === 'equal') {
      const amountPerPeriod = totalAmount / periods;
      return Array(periods).fill(amountPerPeriod);
    }

    if (method === 'weighted' && weights) {
      const totalWeight = weights.reduce((sum, w) => sum + w, 0);
      return weights.map(w => (totalAmount * w) / totalWeight);
    }

    return Array(periods).fill(0);
  }

  /**
   * Validate budget
   */
  async validateBudget(budgetId: string): Promise<{
    isValid: boolean;
    errors: string[];
    warnings: string[];
  }> {
    const response = await fetch(`${this.apiEndpoint}/${budgetId}/validate`);

    if (!response.ok) {
      throw new Error('Failed to validate budget');
    }

    return response.json();
  }
}

export const budgetService = new BudgetService();
