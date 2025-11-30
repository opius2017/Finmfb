import React, { useState, useEffect } from 'react';
import { ArrowRightLeft, Plus, CheckCircle, XCircle, Clock, FileText } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { InterBranchTransfer } from '../types/interBranch.types';
import { interBranchService } from '../services/interBranchService';

export const InterBranchTransfers: React.FC = () => {
  const [transfers, setTransfers] = useState<InterBranchTransfer[]>([]);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<string>('all');

  useEffect(() => {
    loadTransfers();
  }, [filter]);

  const loadTransfers = async () => {
    setLoading(true);
    try {
      const status = filter === 'all' ? undefined : filter;
      const data = await interBranchService.getTransfers(status);
      setTransfers(data);
    } catch (error) {
      console.error('Failed to load transfers:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (transferId: string) => {
    const comment = prompt('Add approval comment (optional):');
    try {
      await interBranchService.approveTransfer(transferId, comment || undefined);
      await loadTransfers();
    } catch (error) {
      console.error('Failed to approve transfer:', error);
    }
  };

  const handleReject = async (transferId: string) => {
    const reason = prompt('Please provide a reason for rejection:');
    if (!reason) return;

    try {
      await interBranchService.rejectTransfer(transferId, reason);
      await loadTransfers();
    } catch (error) {
      console.error('Failed to reject transfer:', error);
    }
  };

  const handleExecute = async (transferId: string) => {
    if (!confirm('Execute this transfer?')) return;

    try {
      await interBranchService.executeTransfer(transferId);
      await loadTransfers();
    } catch (error) {
      console.error('Failed to execute transfer:', error);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'executed':
      case 'reconciled':
        return <CheckCircle className="w-5 h-5 text-success-600" />;
      case 'rejected':
      case 'cancelled':
        return <XCircle className="w-5 h-5 text-error-600" />;
      case 'pending_approval':
        return <Clock className="w-5 h-5 text-warning-600" />;
      default:
        return <FileText className="w-5 h-5 text-neutral-600" />;
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Inter-Branch Transfers</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Manage transfers between branches
          </p>
        </div>
        <Button variant="primary">
          <Plus className="w-4 h-4 mr-2" />
          New Transfer
        </Button>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-2 mb-6">
        {['all', 'pending_approval', 'approved', 'executed', 'reconciled'].map((status) => (
          <button
            key={status}
            onClick={() => setFilter(status)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              filter === status
                ? 'bg-primary-600 text-white'
                : 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200'
            }`}
          >
            {status.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ')}
          </button>
        ))}
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Pending Approval</div>
          <div className="text-2xl font-bold text-warning-600">
            {transfers.filter(t => t.status === 'pending_approval').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Executed</div>
          <div className="text-2xl font-bold text-success-600">
            {transfers.filter(t => t.status === 'executed').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Reconciled</div>
          <div className="text-2xl font-bold text-primary-600">
            {transfers.filter(t => t.status === 'reconciled').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Total Amount</div>
          <div className="text-2xl font-bold">
            ₦{transfers
              .filter(t => t.status === 'executed' || t.status === 'reconciled')
              .reduce((sum, t) => sum + t.amount, 0)
              .toLocaleString()}
          </div>
        </Card>
      </div>

      {/* Transfers List */}
      <div className="space-y-4">
        {loading ? (
          <Card className="p-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          </Card>
        ) : transfers.length === 0 ? (
          <Card className="p-8">
            <div className="text-center text-neutral-600">
              <ArrowRightLeft className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No transfers found</p>
            </div>
          </Card>
        ) : (
          transfers.map((transfer) => (
            <Card key={transfer.id} className="p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    {getStatusIcon(transfer.status)}
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="text-lg font-semibold">
                          {interBranchService.formatTransferNumber(transfer)}
                        </h3>
                        <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                          interBranchService.getTransferStatusColor(transfer.status)
                        }`}>
                          {transfer.status.replace('_', ' ')}
                        </span>
                      </div>
                      <p className="text-sm text-neutral-600">{transfer.description}</p>
                    </div>
                  </div>

                  <div className="flex items-center space-x-4 my-3">
                    <div className="flex items-center space-x-2">
                      <div className="px-3 py-2 bg-error-50 rounded-lg">
                        <div className="text-xs text-error-600">From</div>
                        <div className="font-medium">{transfer.fromBranchName}</div>
                      </div>
                      <ArrowRightLeft className="w-5 h-5 text-neutral-400" />
                      <div className="px-3 py-2 bg-success-50 rounded-lg">
                        <div className="text-xs text-success-600">To</div>
                        <div className="font-medium">{transfer.toBranchName}</div>
                      </div>
                    </div>
                    <div className="px-4 py-2 bg-primary-50 rounded-lg">
                      <div className="text-xs text-primary-600">Amount</div>
                      <div className="text-lg font-bold text-primary-700">
                        ₦{transfer.amount.toLocaleString()}
                      </div>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                    <div>
                      <div className="text-neutral-600">Type</div>
                      <div className="font-medium capitalize">{transfer.type.replace('_', ' ')}</div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Created By</div>
                      <div className="font-medium">{transfer.createdBy}</div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Created</div>
                      <div className="font-medium">
                        {new Date(transfer.createdAt).toLocaleDateString()}
                      </div>
                    </div>
                    {transfer.executedAt && (
                      <div>
                        <div className="text-neutral-600">Executed</div>
                        <div className="font-medium">
                          {new Date(transfer.executedAt).toLocaleDateString()}
                        </div>
                      </div>
                    )}
                  </div>

                  {transfer.approvals.length > 0 && (
                    <div className="mt-3 pt-3 border-t border-neutral-200">
                      <div className="text-xs text-neutral-600 mb-2">Approvals:</div>
                      <div className="flex flex-wrap gap-2">
                        {transfer.approvals.map((approval, idx) => (
                          <div
                            key={idx}
                            className={`px-2 py-1 rounded text-xs ${
                              approval.decision === 'approved'
                                ? 'bg-success-100 text-success-800'
                                : 'bg-error-100 text-error-800'
                            }`}
                          >
                            {approval.approverName} - {approval.decision}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>

                <div className="flex flex-col space-y-2 ml-4">
                  {transfer.status === 'pending_approval' && (
                    <>
                      <Button
                        variant="primary"
                        size="sm"
                        onClick={() => handleApprove(transfer.id)}
                      >
                        <CheckCircle className="w-4 h-4 mr-2" />
                        Approve
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleReject(transfer.id)}
                      >
                        <XCircle className="w-4 h-4 mr-2" />
                        Reject
                      </Button>
                    </>
                  )}
                  {transfer.status === 'approved' && (
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={() => handleExecute(transfer.id)}
                    >
                      Execute
                    </Button>
                  )}
                  <Button variant="ghost" size="sm">
                    View Details
                  </Button>
                </div>
              </div>
            </Card>
          ))
        )}
      </div>
    </div>
  );
};
