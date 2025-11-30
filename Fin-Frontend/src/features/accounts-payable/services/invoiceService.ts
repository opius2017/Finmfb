import { VendorInvoice, DuplicateCheckResult, InvoiceValidationResult } from '../types/invoice.types';

export class InvoiceService {
  private apiEndpoint = '/api/accounts-payable/invoices';

  /**
   * Create new invoice
   */
  async createInvoice(invoice: Partial<VendorInvoice>): Promise<VendorInvoice> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(invoice),
    });

    if (!response.ok) {
      throw new Error('Failed to create invoice');
    }

    return response.json();
  }

  /**
   * Update existing invoice
   */
  async updateInvoice(id: string, invoice: Partial<VendorInvoice>): Promise<VendorInvoice> {
    const response = await fetch(`${this.apiEndpoint}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(invoice),
    });

    if (!response.ok) {
      throw new Error('Failed to update invoice');
    }

    return response.json();
  }

  /**
   * Get invoice by ID
   */
  async getInvoice(id: string): Promise<VendorInvoice> {
    const response = await fetch(`${this.apiEndpoint}/${id}`);

    if (!response.ok) {
      throw new Error('Failed to fetch invoice');
    }

    return response.json();
  }

  /**
   * Check for duplicate invoices
   */
  async checkDuplicate(
    vendorId: string,
    invoiceNumber: string,
    amount?: number
  ): Promise<DuplicateCheckResult> {
    const params = new URLSearchParams({
      vendorId,
      invoiceNumber,
      ...(amount && { amount: amount.toString() }),
    });

    const response = await fetch(`${this.apiEndpoint}/check-duplicate?${params}`);

    if (!response.ok) {
      throw new Error('Failed to check for duplicates');
    }

    return response.json();
  }

  /**
   * Validate invoice data
   */
  async validateInvoice(invoice: Partial<VendorInvoice>): Promise<InvoiceValidationResult> {
    const response = await fetch(`${this.apiEndpoint}/validate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(invoice),
    });

    if (!response.ok) {
      throw new Error('Failed to validate invoice');
    }

    return response.json();
  }

  /**
   * Match vendor by name (fuzzy matching)
   */
  async matchVendor(vendorName: string): Promise<{ id: string; name: string; confidence: number }[]> {
    const response = await fetch(`/api/vendors/match?name=${encodeURIComponent(vendorName)}`);

    if (!response.ok) {
      throw new Error('Failed to match vendor');
    }

    return response.json();
  }

  /**
   * Upload invoice attachment
   */
  async uploadAttachment(invoiceId: string, file: File): Promise<{ id: string; url: string }> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${this.apiEndpoint}/${invoiceId}/attachments`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error('Failed to upload attachment');
    }

    return response.json();
  }

  /**
   * Delete invoice attachment
   */
  async deleteAttachment(invoiceId: string, attachmentId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${invoiceId}/attachments/${attachmentId}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error('Failed to delete attachment');
    }
  }

  /**
   * Submit invoice for approval
   */
  async submitForApproval(invoiceId: string): Promise<VendorInvoice> {
    const response = await fetch(`${this.apiEndpoint}/${invoiceId}/submit`, {
      method: 'POST',
    });

    if (!response.ok) {
      throw new Error('Failed to submit invoice for approval');
    }

    return response.json();
  }

  /**
   * Get invoices list with filters
   */
  async getInvoices(filters?: {
    status?: string;
    vendorId?: string;
    fromDate?: Date;
    toDate?: Date;
    page?: number;
    pageSize?: number;
  }): Promise<{ invoices: VendorInvoice[]; total: number }> {
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
      throw new Error('Failed to fetch invoices');
    }

    return response.json();
  }
}

export const invoiceService = new InvoiceService();
