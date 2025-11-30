import {
  Integration,
  SyncLog,
  IntegrationStats,
  IntegrationTest,
  TestResult,
  PaymentTransaction,
  BankingConnection,
  BankTransaction,
} from '../types/integration.types';

export class IntegrationService {
  private apiEndpoint = '/api/integrations';

  async getIntegrations(): Promise<Integration[]> {
    const response = await fetch(this.apiEndpoint);
    if (!response.ok) throw new Error('Failed to fetch integrations');
    return response.json();
  }

  async getIntegration(integrationId: string): Promise<Integration> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}`);
    if (!response.ok) throw new Error('Failed to fetch integration');
    return response.json();
  }

  async createIntegration(integration: Partial<Integration>): Promise<Integration> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(integration),
    });
    if (!response.ok) throw new Error('Failed to create integration');
    return response.json();
  }

  async updateIntegration(integrationId: string, integration: Partial<Integration>): Promise<Integration> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(integration),
    });
    if (!response.ok) throw new Error('Failed to update integration');
    return response.json();
  }

  async deleteIntegration(integrationId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete integration');
  }

  async activateIntegration(integrationId: string): Promise<Integration> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}/activate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to activate integration');
    return response.json();
  }

  async deactivateIntegration(integrationId: string): Promise<Integration> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}/deactivate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to deactivate integration');
    return response.json();
  }

  async syncIntegration(integrationId: string): Promise<SyncLog> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}/sync`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to sync integration');
    return response.json();
  }

  async getSyncLogs(integrationId?: string, limit: number = 50): Promise<SyncLog[]> {
    const params = new URLSearchParams({ limit: limit.toString() });
    if (integrationId) params.append('integrationId', integrationId);

    const response = await fetch(`${this.apiEndpoint}/sync-logs?${params}`);
    if (!response.ok) throw new Error('Failed to fetch sync logs');
    return response.json();
  }

  async getStats(integrationId?: string, from?: Date, to?: Date): Promise<IntegrationStats> {
    const params = new URLSearchParams();
    if (integrationId) params.append('integrationId', integrationId);
    if (from) params.append('from', from.toISOString());
    if (to) params.append('to', to.toISOString());

    const response = await fetch(`${this.apiEndpoint}/stats?${params}`);
    if (!response.ok) throw new Error('Failed to fetch stats');
    return response.json();
  }

  async testIntegration(integrationId: string, testType: string): Promise<TestResult> {
    const response = await fetch(`${this.apiEndpoint}/${integrationId}/test`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ testType }),
    });
    if (!response.ok) throw new Error('Failed to test integration');
    return response.json();
  }

  // Payment Gateway Methods
  async initializePayment(amount: number, email: string, metadata?: any): Promise<PaymentTransaction> {
    const response = await fetch(`${this.apiEndpoint}/payments/initialize`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ amount, email, metadata }),
    });
    if (!response.ok) throw new Error('Failed to initialize payment');
    return response.json();
  }

  async verifyPayment(reference: string): Promise<PaymentTransaction> {
    const response = await fetch(`${this.apiEndpoint}/payments/verify/${reference}`);
    if (!response.ok) throw new Error('Failed to verify payment');
    return response.json();
  }

  async getPaymentTransactions(limit: number = 100): Promise<PaymentTransaction[]> {
    const response = await fetch(`${this.apiEndpoint}/payments/transactions?limit=${limit}`);
    if (!response.ok) throw new Error('Failed to fetch payment transactions');
    return response.json();
  }

  // Banking Methods
  async getBankingConnections(): Promise<BankingConnection[]> {
    const response = await fetch(`${this.apiEndpoint}/banking/connections`);
    if (!response.ok) throw new Error('Failed to fetch banking connections');
    return response.json();
  }

  async getBankTransactions(connectionId: string, from?: Date, to?: Date): Promise<BankTransaction[]> {
    const params = new URLSearchParams();
    if (from) params.append('from', from.toISOString());
    if (to) params.append('to', to.toISOString());

    const response = await fetch(`${this.apiEndpoint}/banking/connections/${connectionId}/transactions?${params}`);
    if (!response.ok) throw new Error('Failed to fetch bank transactions');
    return response.json();
  }

  async syncBankTransactions(connectionId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/banking/connections/${connectionId}/sync`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to sync bank transactions');
  }

  // Email Methods
  async sendEmail(to: string, templateId: string, variables: Record<string, any>): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/email/send`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ to, templateId, variables }),
    });
    if (!response.ok) throw new Error('Failed to send email');
  }

  // SMS Methods
  async sendSMS(to: string, message: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/sms/send`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ to, message }),
    });
    if (!response.ok) throw new Error('Failed to send SMS');
  }

  // Utility methods
  getProviderIcon(provider: string): string {
    const icons: Record<string, string> = {
      quickbooks: '📊',
      paystack: '💳',
      flutterwave: '🦋',
      'open-banking': '🏦',
      sendgrid: '📧',
      twilio: '📱',
    };
    return icons[provider] || '🔌';
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'active': return 'bg-success-100 text-success-800';
      case 'inactive': return 'bg-neutral-100 text-neutral-800';
      case 'error': return 'bg-error-100 text-error-800';
      case 'pending': return 'bg-warning-100 text-warning-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  getSyncStatusColor(status: string): string {
    switch (status) {
      case 'success': return 'bg-success-100 text-success-800';
      case 'partial': return 'bg-warning-100 text-warning-800';
      case 'failed': return 'bg-error-100 text-error-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  formatProviderName(provider: string): string {
    return provider.split('-').map(word => 
      word.charAt(0).toUpperCase() + word.slice(1)
    ).join(' ');
  }

  maskCredential(credential: string): string {
    if (credential.length <= 8) return credential;
    return `${credential.substring(0, 4)}...${credential.substring(credential.length - 4)}`;
  }

  calculateSyncSuccessRate(successful: number, total: number): number {
    return total === 0 ? 0 : (successful / total) * 100;
  }
}

export const integrationService = new IntegrationService();
