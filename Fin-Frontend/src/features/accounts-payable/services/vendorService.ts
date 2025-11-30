import {
  Vendor,
  VendorAgingReport,
  VendorStatement,
  VendorPerformance,
  VendorCommunication,
  VendorEvaluation,
  VendorStatistics,
} from '../types/vendor.types';

export class VendorService {
  private apiEndpoint = '/api/vendors';

  /**
   * Get all vendors with filters
   */
  async getVendors(filters?: {
    status?: string;
    category?: string;
    search?: string;
    page?: number;
    pageSize?: number;
  }): Promise<{ vendors: Vendor[]; total: number }> {
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
      throw new Error('Failed to fetch vendors');
    }

    return response.json();
  }

  /**
   * Get vendor by ID
   */
  async getVendor(vendorId: string): Promise<Vendor> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch vendor');
    }

    return response.json();
  }

  /**
   * Create new vendor
   */
  async createVendor(vendor: Partial<Vendor>): Promise<Vendor> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(vendor),
    });

    if (!response.ok) {
      throw new Error('Failed to create vendor');
    }

    return response.json();
  }

  /**
   * Update vendor
   */
  async updateVendor(vendorId: string, vendor: Partial<Vendor>): Promise<Vendor> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(vendor),
    });

    if (!response.ok) {
      throw new Error('Failed to update vendor');
    }

    return response.json();
  }

  /**
   * Get vendor aging report
   */
  async getAgingReport(asOfDate?: Date): Promise<VendorAgingReport> {
    const params = new URLSearchParams();
    if (asOfDate) {
      params.append('asOfDate', asOfDate.toISOString());
    }

    const response = await fetch(`${this.apiEndpoint}/aging-report?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch aging report');
    }

    return response.json();
  }

  /**
   * Get vendor statement
   */
  async getVendorStatement(
    vendorId: string,
    periodFrom: Date,
    periodTo: Date
  ): Promise<VendorStatement> {
    const params = new URLSearchParams({
      periodFrom: periodFrom.toISOString(),
      periodTo: periodTo.toISOString(),
    });

    const response = await fetch(
      `${this.apiEndpoint}/${vendorId}/statement?${params}`
    );

    if (!response.ok) {
      throw new Error('Failed to fetch vendor statement');
    }

    return response.json();
  }

  /**
   * Send vendor statement via email
   */
  async sendStatement(vendorId: string, statement: VendorStatement): Promise<void> {
    const response = await fetch(
      `${this.apiEndpoint}/${vendorId}/send-statement`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(statement),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to send statement');
    }
  }

  /**
   * Get vendor performance metrics
   */
  async getPerformance(vendorId: string): Promise<VendorPerformance> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}/performance`);

    if (!response.ok) {
      throw new Error('Failed to fetch vendor performance');
    }

    return response.json();
  }

  /**
   * Update vendor rating
   */
  async updateRating(
    vendorId: string,
    ratings: {
      quality: number;
      delivery: number;
      pricing: number;
      service: number;
    }
  ): Promise<Vendor> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}/rating`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(ratings),
    });

    if (!response.ok) {
      throw new Error('Failed to update rating');
    }

    return response.json();
  }

  /**
   * Get vendor communication history
   */
  async getCommunicationHistory(vendorId: string): Promise<VendorCommunication[]> {
    const response = await fetch(
      `${this.apiEndpoint}/${vendorId}/communications`
    );

    if (!response.ok) {
      throw new Error('Failed to fetch communication history');
    }

    return response.json();
  }

  /**
   * Send communication to vendor
   */
  async sendCommunication(
    vendorId: string,
    communication: Partial<VendorCommunication>
  ): Promise<VendorCommunication> {
    const response = await fetch(
      `${this.apiEndpoint}/${vendorId}/communications`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(communication),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to send communication');
    }

    return response.json();
  }

  /**
   * Create vendor evaluation
   */
  async createEvaluation(
    vendorId: string,
    evaluation: Partial<VendorEvaluation>
  ): Promise<VendorEvaluation> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}/evaluations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(evaluation),
    });

    if (!response.ok) {
      throw new Error('Failed to create evaluation');
    }

    return response.json();
  }

  /**
   * Get vendor evaluations
   */
  async getEvaluations(vendorId: string): Promise<VendorEvaluation[]> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}/evaluations`);

    if (!response.ok) {
      throw new Error('Failed to fetch evaluations');
    }

    return response.json();
  }

  /**
   * Enable/disable vendor portal access
   */
  async togglePortalAccess(
    vendorId: string,
    enabled: boolean
  ): Promise<Vendor> {
    const response = await fetch(
      `${this.apiEndpoint}/${vendorId}/portal-access`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ enabled }),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to toggle portal access');
    }

    return response.json();
  }

  /**
   * Upload vendor document
   */
  async uploadDocument(
    vendorId: string,
    file: File,
    documentType: string
  ): Promise<{ id: string; url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);

    const response = await fetch(`${this.apiEndpoint}/${vendorId}/documents`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error('Failed to upload document');
    }

    return response.json();
  }

  /**
   * Delete vendor document
   */
  async deleteDocument(vendorId: string, documentId: string): Promise<void> {
    const response = await fetch(
      `${this.apiEndpoint}/${vendorId}/documents/${documentId}`,
      { method: 'DELETE' }
    );

    if (!response.ok) {
      throw new Error('Failed to delete document');
    }
  }

  /**
   * Get vendor statistics
   */
  async getStatistics(period?: { from: Date; to: Date }): Promise<VendorStatistics> {
    const params = new URLSearchParams();
    
    if (period) {
      params.append('from', period.from.toISOString());
      params.append('to', period.to.toISOString());
    }

    const response = await fetch(`${this.apiEndpoint}/statistics?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch statistics');
    }

    return response.json();
  }

  /**
   * Suspend vendor
   */
  async suspendVendor(vendorId: string, reason: string): Promise<Vendor> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}/suspend`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });

    if (!response.ok) {
      throw new Error('Failed to suspend vendor');
    }

    return response.json();
  }

  /**
   * Reactivate vendor
   */
  async reactivateVendor(vendorId: string): Promise<Vendor> {
    const response = await fetch(`${this.apiEndpoint}/${vendorId}/reactivate`, {
      method: 'POST',
    });

    if (!response.ok) {
      throw new Error('Failed to reactivate vendor');
    }

    return response.json();
  }

  /**
   * Export vendor list
   */
  async exportVendors(format: 'excel' | 'csv' | 'pdf'): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/export?format=${format}`);

    if (!response.ok) {
      throw new Error('Failed to export vendors');
    }

    return response.blob();
  }
}

export const vendorService = new VendorService();
