import { SecurityAlert, SecurityMetrics, IPWhitelist } from '../types/monitoring.types';

export class SecurityMonitoringService {
  private apiEndpoint = '/api/security/monitoring';

  async getAlerts(status?: string): Promise<SecurityAlert[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    const response = await fetch(`${this.apiEndpoint}/alerts?${params}`);
    if (!response.ok) throw new Error('Failed to fetch alerts');
    return response.json();
  }

  async getAlert(alertId: string): Promise<SecurityAlert> {
    const response = await fetch(`${this.apiEndpoint}/alerts/${alertId}`);
    if (!response.ok) throw new Error('Failed to fetch alert');
    return response.json();
  }

  async resolveAlert(alertId: string, resolution: string): Promise<SecurityAlert> {
    const response = await fetch(`${this.apiEndpoint}/alerts/${alertId}/resolve`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ resolution }),
    });
    if (!response.ok) throw new Error('Failed to resolve alert');
    return response.json();
  }

  async getMetrics(fromDate: Date, toDate: Date): Promise<SecurityMetrics> {
    const params = new URLSearchParams({
      fromDate: fromDate.toISOString(),
      toDate: toDate.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/metrics?${params}`);
    if (!response.ok) throw new Error('Failed to fetch metrics');
    return response.json();
  }

  async getIPWhitelist(): Promise<IPWhitelist[]> {
    const response = await fetch(`${this.apiEndpoint}/ip-whitelist`);
    if (!response.ok) throw new Error('Failed to fetch IP whitelist');
    return response.json();
  }

  async addToWhitelist(ipAddress: string, description: string): Promise<IPWhitelist> {
    const response = await fetch(`${this.apiEndpoint}/ip-whitelist`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ipAddress, description }),
    });
    if (!response.ok) throw new Error('Failed to add to whitelist');
    return response.json();
  }

  async removeFromWhitelist(id: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/ip-whitelist/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to remove from whitelist');
  }

  async reportSuspiciousActivity(details: any): Promise<void> {
    await fetch(`${this.apiEndpoint}/report`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(details),
    });
  }
}

export const securityMonitoringService = new SecurityMonitoringService();
