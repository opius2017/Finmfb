import React, { useState, useEffect } from 'react';
import { Zap, CheckCircle, XCircle, Clock, AlertCircle } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { RecurringExecution, ApprovalWorkflow } from '../types/recurring.types';
import { recurringService } from '../services/recurringService';

export const AutomatedGeneration: React.FC = () => {
  const [executions, setExecutions] = useState<RecurringExecution[]>([]);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<string>('pending');

  useEffect(() => {
    loadExecutions();
  }, [filter]);

  const loadExecutions = async () => {
    setLoading(true);
    try {
      const data = await recurringService.getExecutions();
      const filtered = filter === 'all' 
        ? data 
        : data.filter(e => e.status === filter);
      setExecutions(filtered);
    } catch (error) {
      console.error('Failed to load executions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (executionId: string) => {
    try {
      await recurringService.approveExecution(executionId);
      await loadExecutions();
    } catch (error) {
      console.error('Failed to approve execution:', error);
    }
  };

  const handleReject = async (executionId: string) => {
    const reason = prompt('Please provide a reason for rejection:');
    if (!reason) return;

    try {
      await recurringService.rejectExecution(executionId, reason);
      await loadExecutions();
    } catch (error) {
      console.error('Failed to reject execution:', error);
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'executed': return <CheckCircle className="w-5 h-5 text-success-600" />;
      case 'failed': return <XCircle className="w-5 h-5 text-error-600" />;
      case 'pending': return <Clock className="w-5 h-5 text-warning-600" />;
      case 'approved': return <CheckCircle className="w-5 h-5 text-primary-600" />;
      case 'cancelled': return <XCircle className="w-5 h-5 text-neutral-600" />;
      default: return <AlertCircle className="w-5 h-5 text-neutral-600" />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'executed': return 'bg-success-100 text-success-800';
      case 'failed': return 'bg-error-100 text-error-800';
      case 'pending': return 'bg-warning-100 text-warning-800';
      case 'approved': return 'bg-primary-100 text-primary-800';
      case 'cancelled': return 'bg-neutral-100 text-neutral-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Automated Transaction Generation</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Review and approve automatically generated transactions
          </p>
        </div>
        <Button variant="outline" onClick={loadExecutions}>
          Refresh
        </Button>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-2 mb-6">
        {['pending', 'approved', 'executed', 'failed', 'all'].map((status) => (
          <button
            key={status}
            onClick={() => setFilter(status)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              filter === status
                ? 'bg-primary-600 text-white'
                : 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200'
            }`}
          >
            {status.charAt(0).toUpperCase() + status.slice(1)}
          </button>
        ))}
      </div>

      {/* Executions List */}
      <div className="space-y-4">
        {loading ? (
          <Card className="p-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          </Card>
        ) : executions.length === 0 ? (
          <Card className="p-8">
            <div className="text-center text-neutral-600">
              <Zap className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No executions found</p>
            </div>
          </Card>
        ) : (
          executions.map((execution) => (
            <Card key={execution.id} className="p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    {getStatusIcon(execution.status)}
                    <h3 className="text-lg font-semibold">{execution.templateName}</h3>
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(execution.status)}`}>
                      {execution.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm mb-3">
                    <div>
                      <div className="text-neutral-600">Scheduled Date</div>
                      <div className="font-medium">
                        {new Date(execution.scheduledDate).toLocaleDateString()}
                      </div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Amount</div>
                      <div className="font-medium text-lg">
                        ₦{execution.amount.toLocaleString()}
                      </div>
                    </div>
                    {execution.executedDate && (
                      <div>
                        <div className="text-neutral-600">Executed Date</div>
                        <div className="font-medium">
                          {new Date(execution.executedDate).toLocaleDateString()}
                        </div>
                      </div>
                    )}
                    {execution.transactionId && (
                      <div>
                        <div className="text-neutral-600">Transaction ID</div>
                        <div className="font-medium font-mono text-xs">
                          {execution.transactionId}
                        </div>
                      </div>
                    )}
                  </div>

                  {execution.error && (
                    <div className="p-3 bg-error-50 border border-error-200 rounded-lg text-sm text-error-700 mb-3">
                      <div className="font-medium mb-1">Error:</div>
                      {execution.error}
                    </div>
                  )}

                  {execution.approvedBy && (
                    <div className="text-xs text-neutral-500">
                      Approved by {execution.approvedBy} on{' '}
                      {execution.approvedAt && new Date(execution.approvedAt).toLocaleString()}
                    </div>
                  )}
                </div>

                {execution.status === 'pending' && (
                  <div className="flex items-center space-x-2 ml-4">
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={() => handleApprove(execution.id)}
                    >
                      <CheckCircle className="w-4 h-4 mr-2" />
                      Approve
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleReject(execution.id)}
                    >
                      <XCircle className="w-4 h-4 mr-2" />
                      Reject
                    </Button>
                  </div>
                )}
              </div>
            </Card>
          ))
        )}
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Pending Approval</div>
          <div className="text-2xl font-bold text-warning-600">
            {executions.filter(e => e.status === 'pending').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Executed Today</div>
          <div className="text-2xl font-bold text-success-600">
            {executions.filter(e => 
              e.status === 'executed' && 
              e.executedDate &&
              new Date(e.executedDate).toDateString() === new Date().toDateString()
            ).length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Failed</div>
          <div className="text-2xl font-bold text-error-600">
            {executions.filter(e => e.status === 'failed').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Total Amount</div>
          <div className="text-2xl font-bold">
            ₦{executions
              .filter(e => e.status === 'executed')
              .reduce((sum, e) => sum + e.amount, 0)
              .toLocaleString()}
          </div>
        </Card>
      </div>
    </div>
  );
};
