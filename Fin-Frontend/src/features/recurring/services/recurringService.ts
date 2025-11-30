import {
  RecurringTemplate,
  RecurringExecution,
  TemplateHistory,
  RecurringDashboard,
  TemplatePreview,
  ApprovalWorkflow,
} from '../types/recurring.types';

export class RecurringService {
  private apiEndpoint = '/api/recurring';

  async getTemplates(status?: string): Promise<RecurringTemplate[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    const response = await fetch(`${this.apiEndpoint}/templates?${params}`);
    if (!response.ok) throw new Error('Failed to fetch recurring templates');
    return response.json();
  }

  async getTemplate(templateId: string): Promise<RecurringTemplate> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}`);
    if (!response.ok) throw new Error('Failed to fetch recurring template');
    return response.json();
  }

  async createTemplate(template: Partial<RecurringTemplate>): Promise<RecurringTemplate> {
    const response = await fetch(`${this.apiEndpoint}/templates`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(template),
    });
    if (!response.ok) throw new Error('Failed to create recurring template');
    return response.json();
  }

  async updateTemplate(templateId: string, template: Partial<RecurringTemplate>): Promise<RecurringTemplate> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(template),
    });
    if (!response.ok) throw new Error('Failed to update recurring template');
    return response.json();
  }

  async deleteTemplate(templateId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete recurring template');
  }

  async activateTemplate(templateId: string): Promise<RecurringTemplate> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}/activate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to activate template');
    return response.json();
  }

  async pauseTemplate(templateId: string): Promise<RecurringTemplate> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}/pause`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to pause template');
    return response.json();
  }

  async getExecutions(templateId?: string): Promise<RecurringExecution[]> {
    const params = new URLSearchParams();
    if (templateId) params.append('templateId', templateId);

    const response = await fetch(`${this.apiEndpoint}/executions?${params}`);
    if (!response.ok) throw new Error('Failed to fetch executions');
    return response.json();
  }

  async executeNow(templateId: string): Promise<RecurringExecution> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}/execute`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to execute template');
    return response.json();
  }

  async approveExecution(executionId: string, comment?: string): Promise<RecurringExecution> {
    const response = await fetch(`${this.apiEndpoint}/executions/${executionId}/approve`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ comment }),
    });
    if (!response.ok) throw new Error('Failed to approve execution');
    return response.json();
  }

  async rejectExecution(executionId: string, reason: string): Promise<RecurringExecution> {
    const response = await fetch(`${this.apiEndpoint}/executions/${executionId}/reject`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });
    if (!response.ok) throw new Error('Failed to reject execution');
    return response.json();
  }

  async getTemplateHistory(templateId: string): Promise<TemplateHistory[]> {
    const response = await fetch(`${this.apiEndpoint}/templates/${templateId}/history`);
    if (!response.ok) throw new Error('Failed to fetch template history');
    return response.json();
  }

  async getDashboard(): Promise<RecurringDashboard> {
    const response = await fetch(`${this.apiEndpoint}/dashboard`);
    if (!response.ok) throw new Error('Failed to fetch dashboard');
    return response.json();
  }

  async previewTemplate(template: Partial<RecurringTemplate>): Promise<TemplatePreview> {
    const response = await fetch(`${this.apiEndpoint}/templates/preview`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(template),
    });
    if (!response.ok) throw new Error('Failed to preview template');
    return response.json();
  }

  async testFormula(formula: string, variables: Record<string, number>): Promise<number> {
    const response = await fetch(`${this.apiEndpoint}/formulas/test`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ formula, variables }),
    });
    if (!response.ok) throw new Error('Failed to test formula');
    const result = await response.json();
    return result.value;
  }

  // Utility methods
  calculateNextRunDate(startDate: Date, frequency: any, lastRun?: Date): Date {
    const baseDate = lastRun || startDate;
    const next = new Date(baseDate);

    switch (frequency.type) {
      case 'daily':
        next.setDate(next.getDate() + frequency.interval);
        break;
      case 'weekly':
        next.setDate(next.getDate() + (7 * frequency.interval));
        break;
      case 'monthly':
        next.setMonth(next.getMonth() + frequency.interval);
        if (frequency.dayOfMonth) {
          next.setDate(frequency.dayOfMonth);
        }
        break;
      case 'quarterly':
        next.setMonth(next.getMonth() + (3 * frequency.interval));
        break;
      case 'yearly':
        next.setFullYear(next.getFullYear() + frequency.interval);
        if (frequency.monthOfYear) {
          next.setMonth(frequency.monthOfYear - 1);
        }
        break;
    }

    return next;
  }

  generateSchedule(template: RecurringTemplate, months: number = 12): Date[] {
    const schedule: Date[] = [];
    let currentDate = new Date(template.nextRunDate);
    const endDate = template.endDate || new Date();
    endDate.setMonth(endDate.getMonth() + months);

    while (currentDate <= endDate) {
      schedule.push(new Date(currentDate));
      currentDate = this.calculateNextRunDate(template.startDate, template.frequency, currentDate);
    }

    return schedule;
  }

  validateFormula(formula: string): { valid: boolean; error?: string } {
    try {
      // Basic validation - check for dangerous operations
      const dangerous = ['eval', 'Function', 'require', 'import'];
      for (const keyword of dangerous) {
        if (formula.includes(keyword)) {
          return { valid: false, error: `Forbidden keyword: ${keyword}` };
        }
      }

      // Check for balanced parentheses
      let balance = 0;
      for (const char of formula) {
        if (char === '(') balance++;
        if (char === ')') balance--;
        if (balance < 0) return { valid: false, error: 'Unbalanced parentheses' };
      }
      if (balance !== 0) return { valid: false, error: 'Unbalanced parentheses' };

      return { valid: true };
    } catch (error) {
      return { valid: false, error: 'Invalid formula syntax' };
    }
  }

  formatFrequency(frequency: any): string {
    const { type, interval } = frequency;
    
    if (interval === 1) {
      return type.charAt(0).toUpperCase() + type.slice(1);
    }
    
    return `Every ${interval} ${type}`;
  }

  estimateMonthlyAmount(template: RecurringTemplate): number {
    const schedule = this.generateSchedule(template, 1);
    const amount = template.amount.lastCalculatedAmount || template.amount.fixedAmount || 0;
    return schedule.length * amount;
  }
}

export const recurringService = new RecurringService();
