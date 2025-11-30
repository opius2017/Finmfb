import {
  Webhook,
  WebhookDelivery,
  WebhookStats,
  WebhookTest,
  WebhookTestResult,
  WebhookLog,
} from '../types/webhook.types';

export class WebhookService {
  private apiEndpoint = '/api/webhooks';

  async getWebhooks(): Promise<Webhook[]> {
    const response = await fetch(this.apiEndpoint);
    if (!response.ok) throw new Error('Failed to fetch webhooks');
    return response.json();
  }

  async getWebhook(webhookId: string): Promise<Webhook> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}`);
    if (!response.ok) throw new Error('Failed to fetch webhook');
    return response.json();
  }

  async createWebhook(webhook: Partial<Webhook>): Promise<Webhook> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(webhook),
    });
    if (!response.ok) throw new Error('Failed to create webhook');
    return response.json();
  }

  async updateWebhook(webhookId: string, webhook: Partial<Webhook>): Promise<Webhook> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(webhook),
    });
    if (!response.ok) throw new Error('Failed to update webhook');
    return response.json();
  }

  async deleteWebhook(webhookId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete webhook');
  }

  async activateWebhook(webhookId: string): Promise<Webhook> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}/activate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to activate webhook');
    return response.json();
  }

  async deactivateWebhook(webhookId: string): Promise<Webhook> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}/deactivate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to deactivate webhook');
    return response.json();
  }

  async getDeliveries(webhookId?: string, limit: number = 100): Promise<WebhookDelivery[]> {
    const params = new URLSearchParams({ limit: limit.toString() });
    if (webhookId) params.append('webhookId', webhookId);

    const response = await fetch(`${this.apiEndpoint}/deliveries?${params}`);
    if (!response.ok) throw new Error('Failed to fetch deliveries');
    return response.json();
  }

  async getDelivery(deliveryId: string): Promise<WebhookDelivery> {
    const response = await fetch(`${this.apiEndpoint}/deliveries/${deliveryId}`);
    if (!response.ok) throw new Error('Failed to fetch delivery');
    return response.json();
  }

  async retryDelivery(deliveryId: string): Promise<WebhookDelivery> {
    const response = await fetch(`${this.apiEndpoint}/deliveries/${deliveryId}/retry`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to retry delivery');
    return response.json();
  }

  async getStats(webhookId?: string, from?: Date, to?: Date): Promise<WebhookStats> {
    const params = new URLSearchParams();
    if (webhookId) params.append('webhookId', webhookId);
    if (from) params.append('from', from.toISOString());
    if (to) params.append('to', to.toISOString());

    const response = await fetch(`${this.apiEndpoint}/stats?${params}`);
    if (!response.ok) throw new Error('Failed to fetch stats');
    return response.json();
  }

  async testWebhook(webhookId: string, event: string, payload?: any): Promise<WebhookTestResult> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}/test`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ event, payload }),
    });
    if (!response.ok) throw new Error('Failed to test webhook');
    return response.json();
  }

  async getLogs(webhookId?: string, limit: number = 100): Promise<WebhookLog[]> {
    const params = new URLSearchParams({ limit: limit.toString() });
    if (webhookId) params.append('webhookId', webhookId);

    const response = await fetch(`${this.apiEndpoint}/logs?${params}`);
    if (!response.ok) throw new Error('Failed to fetch logs');
    return response.json();
  }

  async regenerateSecret(webhookId: string): Promise<Webhook> {
    const response = await fetch(`${this.apiEndpoint}/${webhookId}/regenerate-secret`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to regenerate secret');
    return response.json();
  }

  // Utility methods
  generateSignature(payload: string, secret: string): string {
    // This would use crypto in a real implementation
    return `sha256=${secret}`;
  }

  verifySignature(payload: string, signature: string, secret: string): boolean {
    const expectedSignature = this.generateSignature(payload, secret);
    return signature === expectedSignature;
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'active': return 'bg-success-100 text-success-800';
      case 'inactive': return 'bg-neutral-100 text-neutral-800';
      case 'failed': return 'bg-error-100 text-error-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  getDeliveryStatusColor(status: string): string {
    switch (status) {
      case 'delivered': return 'bg-success-100 text-success-800';
      case 'pending': return 'bg-warning-100 text-warning-800';
      case 'retrying': return 'bg-primary-100 text-primary-800';
      case 'failed': return 'bg-error-100 text-error-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  formatEventName(event: string): string {
    return event.split('.').map(word => 
      word.charAt(0).toUpperCase() + word.slice(1)
    ).join(' ');
  }

  calculateNextRetryDelay(attempt: number, policy: any): number {
    const delay = policy.initialDelay * Math.pow(policy.backoffMultiplier, attempt - 1);
    return Math.min(delay, policy.maxDelay);
  }

  maskSecret(secret: string): string {
    if (secret.length <= 8) return secret;
    return `${secret.substring(0, 4)}...${secret.substring(secret.length - 4)}`;
  }

  validateURL(url: string): boolean {
    try {
      new URL(url);
      return true;
    } catch {
      return false;
    }
  }

  generateCurlCommand(webhook: Webhook, payload: any): string {
    let curl = `curl -X POST "${webhook.url}"`;
    
    curl += ` -H "Content-Type: application/json"`;
    curl += ` -H "X-Webhook-Signature: ${this.generateSignature(JSON.stringify(payload), webhook.secret)}"`;
    
    Object.entries(webhook.headers).forEach(([key, value]) => {
      curl += ` -H "${key}: ${value}"`;
    });

    curl += ` -d '${JSON.stringify(payload, null, 2)}'`;

    return curl;
  }
}

export const webhookService = new WebhookService();
