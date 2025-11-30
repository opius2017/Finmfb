import {
  APIKey,
  APIVersion,
  APIUsageStats,
  APILog,
  OpenAPISpec,
  APISandbox,
  APITestRequest,
  APITestResponse,
} from '../types/api.types';

export class APIManagementService {
  private apiEndpoint = '/api/management';

  async getAPIKeys(): Promise<APIKey[]> {
    const response = await fetch(`${this.apiEndpoint}/keys`);
    if (!response.ok) throw new Error('Failed to fetch API keys');
    return response.json();
  }

  async getAPIKey(keyId: string): Promise<APIKey> {
    const response = await fetch(`${this.apiEndpoint}/keys/${keyId}`);
    if (!response.ok) throw new Error('Failed to fetch API key');
    return response.json();
  }

  async createAPIKey(key: Partial<APIKey>): Promise<APIKey> {
    const response = await fetch(`${this.apiEndpoint}/keys`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(key),
    });
    if (!response.ok) throw new Error('Failed to create API key');
    return response.json();
  }

  async updateAPIKey(keyId: string, key: Partial<APIKey>): Promise<APIKey> {
    const response = await fetch(`${this.apiEndpoint}/keys/${keyId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(key),
    });
    if (!response.ok) throw new Error('Failed to update API key');
    return response.json();
  }

  async revokeAPIKey(keyId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/keys/${keyId}/revoke`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to revoke API key');
  }

  async regenerateAPIKey(keyId: string): Promise<APIKey> {
    const response = await fetch(`${this.apiEndpoint}/keys/${keyId}/regenerate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to regenerate API key');
    return response.json();
  }

  async getVersions(): Promise<APIVersion[]> {
    const response = await fetch(`${this.apiEndpoint}/versions`);
    if (!response.ok) throw new Error('Failed to fetch API versions');
    return response.json();
  }

  async getVersion(version: string): Promise<APIVersion> {
    const response = await fetch(`${this.apiEndpoint}/versions/${version}`);
    if (!response.ok) throw new Error('Failed to fetch API version');
    return response.json();
  }

  async getUsageStats(keyId?: string, from?: Date, to?: Date): Promise<APIUsageStats> {
    const params = new URLSearchParams();
    if (keyId) params.append('keyId', keyId);
    if (from) params.append('from', from.toISOString());
    if (to) params.append('to', to.toISOString());

    const response = await fetch(`${this.apiEndpoint}/usage?${params}`);
    if (!response.ok) throw new Error('Failed to fetch usage stats');
    return response.json();
  }

  async getLogs(keyId?: string, limit: number = 100): Promise<APILog[]> {
    const params = new URLSearchParams({ limit: limit.toString() });
    if (keyId) params.append('keyId', keyId);

    const response = await fetch(`${this.apiEndpoint}/logs?${params}`);
    if (!response.ok) throw new Error('Failed to fetch API logs');
    return response.json();
  }

  async getOpenAPISpec(version?: string): Promise<OpenAPISpec> {
    const params = version ? `?version=${version}` : '';
    const response = await fetch(`${this.apiEndpoint}/openapi${params}`);
    if (!response.ok) throw new Error('Failed to fetch OpenAPI spec');
    return response.json();
  }

  async downloadOpenAPISpec(version?: string, format: 'json' | 'yaml' = 'json'): Promise<Blob> {
    const params = new URLSearchParams({ format });
    if (version) params.append('version', version);

    const response = await fetch(`${this.apiEndpoint}/openapi/download?${params}`);
    if (!response.ok) throw new Error('Failed to download OpenAPI spec');
    return response.blob();
  }

  async getSandboxes(): Promise<APISandbox[]> {
    const response = await fetch(`${this.apiEndpoint}/sandboxes`);
    if (!response.ok) throw new Error('Failed to fetch sandboxes');
    return response.json();
  }

  async createSandbox(sandbox: Partial<APISandbox>): Promise<APISandbox> {
    const response = await fetch(`${this.apiEndpoint}/sandboxes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(sandbox),
    });
    if (!response.ok) throw new Error('Failed to create sandbox');
    return response.json();
  }

  async resetSandbox(sandboxId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/sandboxes/${sandboxId}/reset`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to reset sandbox');
  }

  async testEndpoint(request: APITestRequest): Promise<APITestResponse> {
    const response = await fetch(`${this.apiEndpoint}/test`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    });
    if (!response.ok) throw new Error('Failed to test endpoint');
    return response.json();
  }

  // Utility methods
  maskAPIKey(key: string): string {
    if (key.length <= 8) return key;
    return `${key.substring(0, 4)}...${key.substring(key.length - 4)}`;
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'active': return 'bg-success-100 text-success-800';
      case 'inactive': return 'bg-neutral-100 text-neutral-800';
      case 'revoked': return 'bg-error-100 text-error-800';
      case 'expired': return 'bg-warning-100 text-warning-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  getVersionStatusColor(status: string): string {
    switch (status) {
      case 'current': return 'bg-success-100 text-success-800';
      case 'supported': return 'bg-primary-100 text-primary-800';
      case 'deprecated': return 'bg-warning-100 text-warning-800';
      case 'retired': return 'bg-error-100 text-error-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  calculateErrorRate(total: number, failed: number): number {
    return total === 0 ? 0 : (failed / total) * 100;
  }

  formatResponseTime(ms: number): string {
    if (ms < 1000) return `${ms.toFixed(0)}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
  }

  isRateLimitExceeded(current: number, limit: number): boolean {
    return current >= limit;
  }

  validateIPAddress(ip: string): boolean {
    const ipv4Regex = /^(\d{1,3}\.){3}\d{1,3}$/;
    const ipv6Regex = /^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$/;
    return ipv4Regex.test(ip) || ipv6Regex.test(ip);
  }

  generateCurlCommand(request: APITestRequest, apiKey: string): string {
    let curl = `curl -X ${request.method} "${request.endpoint}"`;
    
    curl += ` -H "Authorization: Bearer ${apiKey}"`;
    
    Object.entries(request.headers).forEach(([key, value]) => {
      curl += ` -H "${key}: ${value}"`;
    });

    if (request.queryParams) {
      const params = new URLSearchParams(request.queryParams);
      curl += `?${params.toString()}`;
    }

    if (request.body) {
      curl += ` -d '${JSON.stringify(request.body)}'`;
    }

    return curl;
  }
}

export const apiManagementService = new APIManagementService();
