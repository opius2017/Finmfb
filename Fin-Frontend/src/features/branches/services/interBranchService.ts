import {
  InterBranchTransfer,
  InterBranchReconciliation,
  InterBranchReport,
  TransferJournalEntry,
  TransferApprovalWorkflow,
} from '../types/interBranch.types';

export class InterBranchService {
  private apiEndpoint = '/api/inter-branch';

  async getTransfers(status?: string): Promise<InterBranchTransfer[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    const response = await fetch(`${this.apiEndpoint}/transfers?${params}`);
    if (!response.ok) throw new Error('Failed to fetch transfers');
    return response.json();
  }

  async getTransfer(transferId: string): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}`);
    if (!response.ok) throw new Error('Failed to fetch transfer');
    return response.json();
  }

  async createTransfer(transfer: Partial<InterBranchTransfer>): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(transfer),
    });
    if (!response.ok) throw new Error('Failed to create transfer');
    return response.json();
  }

  async updateTransfer(transferId: string, transfer: Partial<InterBranchTransfer>): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(transfer),
    });
    if (!response.ok) throw new Error('Failed to update transfer');
    return response.json();
  }

  async submitForApproval(transferId: string): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/submit`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to submit transfer');
    return response.json();
  }

  async approveTransfer(transferId: string, comment?: string): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/approve`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ comment }),
    });
    if (!response.ok) throw new Error('Failed to approve transfer');
    return response.json();
  }

  async rejectTransfer(transferId: string, reason: string): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/reject`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });
    if (!response.ok) throw new Error('Failed to reject transfer');
    return response.json();
  }

  async executeTransfer(transferId: string): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/execute`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to execute transfer');
    return response.json();
  }

  async cancelTransfer(transferId: string, reason: string): Promise<InterBranchTransfer> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/cancel`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ reason }),
    });
    if (!response.ok) throw new Error('Failed to cancel transfer');
    return response.json();
  }

  async getJournalEntries(transferId: string): Promise<TransferJournalEntry[]> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/journal-entries`);
    if (!response.ok) throw new Error('Failed to fetch journal entries');
    return response.json();
  }

  async getReconciliations(branchId?: string): Promise<InterBranchReconciliation[]> {
    const params = new URLSearchParams();
    if (branchId) params.append('branchId', branchId);

    const response = await fetch(`${this.apiEndpoint}/reconciliations?${params}`);
    if (!response.ok) throw new Error('Failed to fetch reconciliations');
    return response.json();
  }

  async getReconciliation(reconciliationId: string): Promise<InterBranchReconciliation> {
    const response = await fetch(`${this.apiEndpoint}/reconciliations/${reconciliationId}`);
    if (!response.ok) throw new Error('Failed to fetch reconciliation');
    return response.json();
  }

  async createReconciliation(
    fromBranchId: string,
    toBranchId: string,
    from: Date,
    to: Date
  ): Promise<InterBranchReconciliation> {
    const response = await fetch(`${this.apiEndpoint}/reconciliations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ fromBranchId, toBranchId, from, to }),
    });
    if (!response.ok) throw new Error('Failed to create reconciliation');
    return response.json();
  }

  async addAdjustment(
    reconciliationId: string,
    adjustment: any
  ): Promise<InterBranchReconciliation> {
    const response = await fetch(`${this.apiEndpoint}/reconciliations/${reconciliationId}/adjustments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(adjustment),
    });
    if (!response.ok) throw new Error('Failed to add adjustment');
    return response.json();
  }

  async completeReconciliation(reconciliationId: string): Promise<InterBranchReconciliation> {
    const response = await fetch(`${this.apiEndpoint}/reconciliations/${reconciliationId}/complete`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to complete reconciliation');
    return response.json();
  }

  async getReport(from: Date, to: Date): Promise<InterBranchReport> {
    const params = new URLSearchParams({
      from: from.toISOString(),
      to: to.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/reports?${params}`);
    if (!response.ok) throw new Error('Failed to fetch report');
    return response.json();
  }

  async getApprovalWorkflow(transferId: string): Promise<TransferApprovalWorkflow> {
    const response = await fetch(`${this.apiEndpoint}/transfers/${transferId}/workflow`);
    if (!response.ok) throw new Error('Failed to fetch approval workflow');
    return response.json();
  }

  // Utility methods
  calculateNetPosition(transfersOut: number, transfersIn: number): number {
    return transfersIn - transfersOut;
  }

  isReconciled(transfer: InterBranchTransfer): boolean {
    return transfer.status === 'reconciled';
  }

  requiresApproval(transfer: InterBranchTransfer): boolean {
    return transfer.status === 'pending_approval';
  }

  canApprove(transfer: InterBranchTransfer, userId: string): boolean {
    // Check if user hasn't already approved
    const hasApproved = transfer.approvals.some(a => a.approverId === userId);
    return !hasApproved && transfer.status === 'pending_approval';
  }

  getTransferStatusColor(status: string): string {
    switch (status) {
      case 'draft': return 'bg-neutral-100 text-neutral-800';
      case 'pending_approval': return 'bg-warning-100 text-warning-800';
      case 'approved': return 'bg-primary-100 text-primary-800';
      case 'executed': return 'bg-success-100 text-success-800';
      case 'reconciled': return 'bg-success-100 text-success-800';
      case 'rejected': return 'bg-error-100 text-error-800';
      case 'cancelled': return 'bg-neutral-100 text-neutral-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  formatTransferNumber(transfer: InterBranchTransfer): string {
    return transfer.transferNumber || `IBT-${transfer.id.substring(0, 8)}`;
  }

  validateTransfer(transfer: Partial<InterBranchTransfer>): { valid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!transfer.fromBranchId) errors.push('Source branch is required');
    if (!transfer.toBranchId) errors.push('Destination branch is required');
    if (transfer.fromBranchId === transfer.toBranchId) {
      errors.push('Source and destination branches must be different');
    }
    if (!transfer.amount || transfer.amount <= 0) {
      errors.push('Amount must be greater than zero');
    }
    if (!transfer.description) errors.push('Description is required');

    return {
      valid: errors.length === 0,
      errors,
    };
  }
}

export const interBranchService = new InterBranchService();
