import {
  ConsolidationReport,
  ConsolidationWorksheet,
  EliminationEntry,
  ConsolidationAdjustment,
  ConsolidationRule,
  MinorityInterest,
} from '../types/consolidation.types';

export class ConsolidationService {
  private apiEndpoint = '/api/consolidation';

  async getReports(): Promise<ConsolidationReport[]> {
    const response = await fetch(`${this.apiEndpoint}/reports`);
    if (!response.ok) throw new Error('Failed to fetch consolidation reports');
    return response.json();
  }

  async getReport(reportId: string): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}`);
    if (!response.ok) throw new Error('Failed to fetch consolidation report');
    return response.json();
  }

  async createReport(
    name: string,
    branches: string[],
    from: Date,
    to: Date
  ): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, branches, from, to }),
    });
    if (!response.ok) throw new Error('Failed to create consolidation report');
    return response.json();
  }

  async generateConsolidation(reportId: string): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/generate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to generate consolidation');
    return response.json();
  }

  async getWorksheet(reportId: string): Promise<ConsolidationWorksheet> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/worksheet`);
    if (!response.ok) throw new Error('Failed to fetch consolidation worksheet');
    return response.json();
  }

  async addElimination(
    reportId: string,
    elimination: Partial<EliminationEntry>
  ): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/eliminations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(elimination),
    });
    if (!response.ok) throw new Error('Failed to add elimination');
    return response.json();
  }

  async removeElimination(reportId: string, eliminationId: string): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/eliminations/${eliminationId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to remove elimination');
    return response.json();
  }

  async addAdjustment(
    reportId: string,
    adjustment: Partial<ConsolidationAdjustment>
  ): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/adjustments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(adjustment),
    });
    if (!response.ok) throw new Error('Failed to add adjustment');
    return response.json();
  }

  async removeAdjustment(reportId: string, adjustmentId: string): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/adjustments/${adjustmentId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to remove adjustment');
    return response.json();
  }

  async approveReport(reportId: string): Promise<ConsolidationReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/approve`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to approve report');
    return response.json();
  }

  async getRules(): Promise<ConsolidationRule[]> {
    const response = await fetch(`${this.apiEndpoint}/rules`);
    if (!response.ok) throw new Error('Failed to fetch consolidation rules');
    return response.json();
  }

  async createRule(rule: Partial<ConsolidationRule>): Promise<ConsolidationRule> {
    const response = await fetch(`${this.apiEndpoint}/rules`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(rule),
    });
    if (!response.ok) throw new Error('Failed to create rule');
    return response.json();
  }

  async updateRule(ruleId: string, rule: Partial<ConsolidationRule>): Promise<ConsolidationRule> {
    const response = await fetch(`${this.apiEndpoint}/rules/${ruleId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(rule),
    });
    if (!response.ok) throw new Error('Failed to update rule');
    return response.json();
  }

  async deleteRule(ruleId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/rules/${ruleId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete rule');
  }

  async getMinorityInterests(reportId: string): Promise<MinorityInterest[]> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/minority-interests`);
    if (!response.ok) throw new Error('Failed to fetch minority interests');
    return response.json();
  }

  async exportReport(reportId: string, format: 'pdf' | 'excel'): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/reports/${reportId}/export?format=${format}`);
    if (!response.ok) throw new Error('Failed to export report');
    return response.blob();
  }

  // Utility methods
  calculateMinorityInterest(netAssets: number, minorityPercentage: number): number {
    return netAssets * (minorityPercentage / 100);
  }

  calculateConsolidatedAmount(
    branchAmounts: number[],
    eliminations: number,
    adjustments: number
  ): number {
    const total = branchAmounts.reduce((sum, amount) => sum + amount, 0);
    return total - eliminations + adjustments;
  }

  validateElimination(elimination: Partial<EliminationEntry>): { valid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!elimination.type) errors.push('Elimination type is required');
    if (!elimination.description) errors.push('Description is required');
    if (!elimination.debitAccount) errors.push('Debit account is required');
    if (!elimination.creditAccount) errors.push('Credit account is required');
    if (!elimination.amount || elimination.amount <= 0) {
      errors.push('Amount must be greater than zero');
    }
    if (!elimination.affectedBranches || elimination.affectedBranches.length === 0) {
      errors.push('At least one affected branch is required');
    }

    return {
      valid: errors.length === 0,
      errors,
    };
  }

  getEliminationTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      inter_branch_transfer: 'Inter-Branch Transfer',
      inter_branch_sale: 'Inter-Branch Sale',
      inter_branch_loan: 'Inter-Branch Loan',
      unrealized_profit: 'Unrealized Profit',
      dividend: 'Dividend',
      management_fee: 'Management Fee',
    };
    return labels[type] || type;
  }

  getAdjustmentTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      reclassification: 'Reclassification',
      accrual: 'Accrual',
      provision: 'Provision',
      fair_value: 'Fair Value Adjustment',
      currency_translation: 'Currency Translation',
      other: 'Other',
    };
    return labels[type] || type;
  }

  formatCurrency(amount: number): string {
    return `₦${amount.toLocaleString()}`;
  }

  calculateVariance(actual: number, expected: number): number {
    return actual - expected;
  }

  calculateVariancePercentage(actual: number, expected: number): number {
    return expected === 0 ? 0 : ((actual - expected) / expected) * 100;
  }
}

export const consolidationService = new ConsolidationService();
