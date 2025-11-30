import {
  RetentionPolicy,
  DocumentRetentionStatus,
  LegalHold,
  RetentionAuditLog,
  RetentionReport,
  RetentionConfiguration,
} from '../types/retention.types';

export class RetentionService {
  private apiEndpoint = '/api/documents/retention';

  async getPolicies(): Promise<RetentionPolicy[]> {
    const response = await fetch(`${this.apiEndpoint}/policies`);
    if (!response.ok) throw new Error('Failed to fetch retention policies');
    return response.json();
  }

  async getPolicy(policyId: string): Promise<RetentionPolicy> {
    const response = await fetch(`${this.apiEndpoint}/policies/${policyId}`);
    if (!response.ok) throw new Error('Failed to fetch retention policy');
    return response.json();
  }

  async createPolicy(policy: Partial<RetentionPolicy>): Promise<RetentionPolicy> {
    const response = await fetch(`${this.apiEndpoint}/policies`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(policy),
    });
    if (!response.ok) throw new Error('Failed to create retention policy');
    return response.json();
  }

  async updatePolicy(policyId: string, policy: Partial<RetentionPolicy>): Promise<RetentionPolicy> {
    const response = await fetch(`${this.apiEndpoint}/policies/${policyId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(policy),
    });
    if (!response.ok) throw new Error('Failed to update retention policy');
    return response.json();
  }

  async deletePolicy(policyId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/policies/${policyId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete retention policy');
  }

  async getDocumentRetentionStatus(documentId: string): Promise<DocumentRetentionStatus> {
    const response = await fetch(`${this.apiEndpoint}/documents/${documentId}/status`);
    if (!response.ok) throw new Error('Failed to fetch document retention status');
    return response.json();
  }

  async applyPolicyToDocument(documentId: string, policyId: string): Promise<DocumentRetentionStatus> {
    const response = await fetch(`${this.apiEndpoint}/documents/${documentId}/apply-policy`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ policyId }),
    });
    if (!response.ok) throw new Error('Failed to apply retention policy');
    return response.json();
  }

  async getLegalHolds(): Promise<LegalHold[]> {
    const response = await fetch(`${this.apiEndpoint}/legal-holds`);
    if (!response.ok) throw new Error('Failed to fetch legal holds');
    return response.json();
  }

  async createLegalHold(hold: Partial<LegalHold>): Promise<LegalHold> {
    const response = await fetch(`${this.apiEndpoint}/legal-holds`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(hold),
    });
    if (!response.ok) throw new Error('Failed to create legal hold');
    return response.json();
  }

  async releaseLegalHold(holdId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/legal-holds/${holdId}/release`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to release legal hold');
  }

  async addDocumentsToLegalHold(holdId: string, documentIds: string[]): Promise<LegalHold> {
    const response = await fetch(`${this.apiEndpoint}/legal-holds/${holdId}/documents`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ documentIds }),
    });
    if (!response.ok) throw new Error('Failed to add documents to legal hold');
    return response.json();
  }

  async getAuditLog(documentId?: string): Promise<RetentionAuditLog[]> {
    const params = new URLSearchParams();
    if (documentId) params.append('documentId', documentId);

    const response = await fetch(`${this.apiEndpoint}/audit-log?${params}`);
    if (!response.ok) throw new Error('Failed to fetch audit log');
    return response.json();
  }

  async getRetentionReport(): Promise<RetentionReport> {
    const response = await fetch(`${this.apiEndpoint}/reports/summary`);
    if (!response.ok) throw new Error('Failed to fetch retention report');
    return response.json();
  }

  async getConfiguration(): Promise<RetentionConfiguration> {
    const response = await fetch(`${this.apiEndpoint}/configuration`);
    if (!response.ok) throw new Error('Failed to fetch retention configuration');
    return response.json();
  }

  async updateConfiguration(config: Partial<RetentionConfiguration>): Promise<RetentionConfiguration> {
    const response = await fetch(`${this.apiEndpoint}/configuration`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(config),
    });
    if (!response.ok) throw new Error('Failed to update retention configuration');
    return response.json();
  }

  async archiveDocument(documentId: string, reason?: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/documents/${documentId}/archive`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });
    if (!response.ok) throw new Error('Failed to archive document');
  }

  async deleteDocument(documentId: string, reason: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/documents/${documentId}/delete`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });
    if (!response.ok) throw new Error('Failed to delete document');
  }

  async executeRetentionActions(): Promise<{ archived: number; deleted: number; errors: number }> {
    const response = await fetch(`${this.apiEndpoint}/execute-actions`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to execute retention actions');
    return response.json();
  }

  // Utility methods
  calculateRetentionDate(createdDate: Date, period: number, unit: string): Date {
    const date = new Date(createdDate);
    switch (unit) {
      case 'days':
        date.setDate(date.getDate() + period);
        break;
      case 'months':
        date.setMonth(date.getMonth() + period);
        break;
      case 'years':
        date.setFullYear(date.getFullYear() + period);
        break;
    }
    return date;
  }

  calculateDaysRemaining(retentionDate: Date): number {
    const now = new Date();
    const diff = retentionDate.getTime() - now.getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  isRetentionDue(retentionDate: Date): boolean {
    return this.calculateDaysRemaining(retentionDate) <= 0;
  }

  getRetentionStatus(daysRemaining: number): 'active' | 'pending_action' | 'overdue' {
    if (daysRemaining > 30) return 'active';
    if (daysRemaining > 0) return 'pending_action';
    return 'overdue';
  }
}

export const retentionService = new RetentionService();
