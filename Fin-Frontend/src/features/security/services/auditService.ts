import { AuditLog, AuditFilter, AuditReport } from '../types/audit.types';

export class AuditService {
  private apiEndpoint = '/api/security/audit';

  async getLogs(filter?: AuditFilter): Promise<{ logs: AuditLog[]; total: number }> {
    const params = new URLSearchParams();
    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}/logs?${params}`);
    if (!response.ok) throw new Error('Failed to fetch audit logs');
    return response.json();
  }

  async getLog(logId: string): Promise<AuditLog> {
    const response = await fetch(`${this.apiEndpoint}/logs/${logId}`);
    if (!response.ok) throw new Error('Failed to fetch audit log');
    return response.json();
  }

  async getReport(fromDate: Date, toDate: Date): Promise<AuditReport> {
    const params = new URLSearchParams({
      fromDate: fromDate.toISOString(),
      toDate: toDate.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/report?${params}`);
    if (!response.ok) throw new Error('Failed to fetch audit report');
    return response.json();
  }

  async exportLogs(filter?: AuditFilter, format: 'csv' | 'excel' | 'pdf' = 'csv'): Promise<Blob> {
    const params = new URLSearchParams({ format });
    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}/export?${params}`);
    if (!response.ok) throw new Error('Failed to export audit logs');
    return response.blob();
  }

  async logEvent(action: string, resource: string, resourceId?: string, changes?: any[]): Promise<void> {
    await fetch(`${this.apiEndpoint}/log`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ action, resource, resourceId, changes }),
    });
  }
}

export const auditService = new AuditService();
