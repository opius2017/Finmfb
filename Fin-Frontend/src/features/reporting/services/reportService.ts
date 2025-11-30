import { Report, ReportData, ReportExecution, ReportTemplate } from '../types/report.types';

export class ReportService {
  private apiEndpoint = '/api/reports';

  async getReports(category?: string): Promise<Report[]> {
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    
    const response = await fetch(`${this.apiEndpoint}?${params}`);
    if (!response.ok) throw new Error('Failed to fetch reports');
    return response.json();
  }

  async getReport(reportId: string): Promise<Report> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}`);
    if (!response.ok) throw new Error('Failed to fetch report');
    return response.json();
  }

  async createReport(report: Partial<Report>): Promise<Report> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(report),
    });
    if (!response.ok) throw new Error('Failed to create report');
    return response.json();
  }

  async updateReport(reportId: string, report: Partial<Report>): Promise<Report> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(report),
    });
    if (!response.ok) throw new Error('Failed to update report');
    return response.json();
  }

  async deleteReport(reportId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete report');
  }

  async executeReport(reportId: string, parameters?: Record<string, any>): Promise<ReportData> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}/execute`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ parameters }),
    });
    if (!response.ok) throw new Error('Failed to execute report');
    return response.json();
  }

  async exportReport(
    reportId: string,
    format: 'pdf' | 'excel' | 'csv',
    parameters?: Record<string, any>
  ): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}/export`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ format, parameters }),
    });
    if (!response.ok) throw new Error('Failed to export report');
    return response.blob();
  }

  async getExecutionHistory(reportId: string): Promise<ReportExecution[]> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}/executions`);
    if (!response.ok) throw new Error('Failed to fetch execution history');
    return response.json();
  }

  async getTemplates(): Promise<ReportTemplate[]> {
    const response = await fetch(`${this.apiEndpoint}/templates`);
    if (!response.ok) throw new Error('Failed to fetch templates');
    return response.json();
  }

  async createFromTemplate(templateId: string, name: string): Promise<Report> {
    const response = await fetch(`${this.apiEndpoint}/from-template`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ templateId, name }),
    });
    if (!response.ok) throw new Error('Failed to create from template');
    return response.json();
  }

  async getDataSources(): Promise<any[]> {
    const response = await fetch(`${this.apiEndpoint}/data-sources`);
    if (!response.ok) throw new Error('Failed to fetch data sources');
    return response.json();
  }

  async getFields(dataSourceId: string): Promise<any[]> {
    const response = await fetch(`${this.apiEndpoint}/data-sources/${dataSourceId}/fields`);
    if (!response.ok) throw new Error('Failed to fetch fields');
    return response.json();
  }

  async validateFormula(formula: string): Promise<{ valid: boolean; error?: string }> {
    const response = await fetch(`${this.apiEndpoint}/validate-formula`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ formula }),
    });
    if (!response.ok) throw new Error('Failed to validate formula');
    return response.json();
  }

  async scheduleReport(reportId: string, schedule: any): Promise<Report> {
    const response = await fetch(`${this.apiEndpoint}/${reportId}/schedule`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(schedule),
    });
    if (!response.ok) throw new Error('Failed to schedule report');
    return response.json();
  }
}

export const reportService = new ReportService();
