import {
  PaymentBatch,
  BatchPayment,
  PaymentFilter,
  PaymentFile,
  PaymentConfirmation,
  PaymentRegister,
  PaymentStatistics,
} from '../types/payment.types';

export class PaymentService {
  private apiEndpoint = '/api/accounts-payable/payments';

  /**
   * Create new payment batch
   */
  async createBatch(batch: Partial<PaymentBatch>): Promise<PaymentBatch> {
    const response = await fetch(`${this.apiEndpoint}/batches`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(batch),
    });

    if (!response.ok) {
      throw new Error('Failed to create payment batch');
    }

    return response.json();
  }

  /**
   * Get payment batch by ID
   */
  async getBatch(batchId: string): Promise<PaymentBatch> {
    const response = await fetch(`${this.apiEndpoint}/batches/${batchId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch payment batch');
    }

    return response.json();
  }

  /**
   * Get all payment batches with filters
   */
  async getBatches(filters?: {
    status?: string;
    fromDate?: Date;
    toDate?: Date;
    page?: number;
    pageSize?: number;
  }): Promise<{ batches: PaymentBatch[]; total: number }> {
    const params = new URLSearchParams();
    
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}/batches?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch payment batches');
    }

    return response.json();
  }

  /**
   * Get eligible invoices for payment
   */
  async getEligibleInvoices(filters?: PaymentFilter): Promise<BatchPayment[]> {
    const params = new URLSearchParams();
    
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}/eligible-invoices?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch eligible invoices');
    }

    return response.json();
  }

  /**
   * Add payments to batch
   */
  async addPaymentsToBatch(batchId: string, paymentIds: string[]): Promise<PaymentBatch> {
    const response = await fetch(`${this.apiEndpoint}/batches/${batchId}/payments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ paymentIds }),
    });

    if (!response.ok) {
      throw new Error('Failed to add payments to batch');
    }

    return response.json();
  }

  /**
   * Remove payment from batch
   */
  async removePaymentFromBatch(batchId: string, paymentId: string): Promise<PaymentBatch> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/payments/${paymentId}`,
      { method: 'DELETE' }
    );

    if (!response.ok) {
      throw new Error('Failed to remove payment from batch');
    }

    return response.json();
  }

  /**
   * Generate payment file
   */
  async generatePaymentFile(
    batchId: string,
    format: string
  ): Promise<PaymentFile> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/generate-file`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ format }),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to generate payment file');
    }

    return response.json();
  }

  /**
   * Download payment file
   */
  async downloadPaymentFile(batchId: string): Promise<Blob> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/download`
    );

    if (!response.ok) {
      throw new Error('Failed to download payment file');
    }

    return response.blob();
  }

  /**
   * Submit batch for approval
   */
  async submitForApproval(batchId: string): Promise<PaymentBatch> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/submit`,
      { method: 'POST' }
    );

    if (!response.ok) {
      throw new Error('Failed to submit batch for approval');
    }

    return response.json();
  }

  /**
   * Approve payment batch
   */
  async approveBatch(batchId: string, comments?: string): Promise<PaymentBatch> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/approve`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ comments }),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to approve batch');
    }

    return response.json();
  }

  /**
   * Reject payment batch
   */
  async rejectBatch(batchId: string, reason: string): Promise<PaymentBatch> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/reject`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reason }),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to reject batch');
    }

    return response.json();
  }

  /**
   * Process payment batch
   */
  async processBatch(batchId: string): Promise<PaymentBatch> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/process`,
      { method: 'POST' }
    );

    if (!response.ok) {
      throw new Error('Failed to process batch');
    }

    return response.json();
  }

  /**
   * Import payment confirmations
   */
  async importConfirmations(
    batchId: string,
    file: File
  ): Promise<PaymentConfirmation[]> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/confirmations`,
      {
        method: 'POST',
        body: formData,
      }
    );

    if (!response.ok) {
      throw new Error('Failed to import confirmations');
    }

    return response.json();
  }

  /**
   * Update payment status
   */
  async updatePaymentStatus(
    paymentId: string,
    status: string,
    confirmationNumber?: string
  ): Promise<BatchPayment> {
    const response = await fetch(`${this.apiEndpoint}/payments/${paymentId}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status, confirmationNumber }),
    });

    if (!response.ok) {
      throw new Error('Failed to update payment status');
    }

    return response.json();
  }

  /**
   * Get payment register
   */
  async getPaymentRegister(
    fromDate: Date,
    toDate: Date
  ): Promise<PaymentRegister> {
    const params = new URLSearchParams({
      fromDate: fromDate.toISOString(),
      toDate: toDate.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/register?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch payment register');
    }

    return response.json();
  }

  /**
   * Get payment statistics
   */
  async getStatistics(period?: { from: Date; to: Date }): Promise<PaymentStatistics> {
    const params = new URLSearchParams();
    
    if (period) {
      params.append('from', period.from.toISOString());
      params.append('to', period.to.toISOString());
    }

    const response = await fetch(`${this.apiEndpoint}/statistics?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch payment statistics');
    }

    return response.json();
  }

  /**
   * Calculate early payment discount
   */
  calculateEarlyPaymentDiscount(
    amount: number,
    dueDate: Date,
    paymentDate: Date,
    discountRate: number
  ): number {
    const daysEarly = Math.floor(
      (dueDate.getTime() - paymentDate.getTime()) / (1000 * 60 * 60 * 24)
    );

    if (daysEarly > 0) {
      return amount * (discountRate / 100);
    }

    return 0;
  }

  /**
   * Validate batch before processing
   */
  async validateBatch(batchId: string): Promise<{
    isValid: boolean;
    errors: string[];
    warnings: string[];
  }> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/validate`
    );

    if (!response.ok) {
      throw new Error('Failed to validate batch');
    }

    return response.json();
  }

  /**
   * Cancel payment batch
   */
  async cancelBatch(batchId: string, reason: string): Promise<PaymentBatch> {
    const response = await fetch(
      `${this.apiEndpoint}/batches/${batchId}/cancel`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reason }),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to cancel batch');
    }

    return response.json();
  }
}

export const paymentService = new PaymentService();
