import React, { useState, useEffect } from 'react';
import { Shield, Plus, Edit, Trash2, Clock, AlertTriangle } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { RetentionPolicy, RetentionReport } from '../types/retention.types';
import { retentionService } from '../services/retentionService';

export const RetentionPolicies: React.FC = () => {
  const [policies, setPolicies] = useState<RetentionPolicy[]>([]);
  const [report, setReport] = useState<RetentionReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [policiesData, reportData] = await Promise.all([
        retentionService.getPolicies(),
        retentionService.getRetentionReport(),
      ]);
      setPolicies(policiesData);
      setReport(reportData);
    } catch (error) {
      console.error('Failed to load retention data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDeletePolicy = async (policyId: string) => {
    if (!confirm('Are you sure you want to delete this retention policy?')) return;
    
    try {
      await retentionService.deletePolicy(policyId);
      await loadData();
    } catch (error) {
      console.error('Failed to delete policy:', error);
    }
  };

  const handleExecuteActions = async () => {
    if (!confirm('This will execute all pending retention actions. Continue?')) return;
    
    setLoading(true);
    try {
      const result = await retentionService.executeRetentionActions();
      alert(`Completed: ${result.archived} archived, ${result.deleted} deleted, ${result.errors} errors`);
      await loadData();
    } catch (error) {
      console.error('Failed to execute retention actions:', error);
    } finally {
      setLoading(false);
    }
  };

  const getActionColor = (action: string) => {
    switch (action) {
      case 'delete': return 'text-error-600 bg-error-50';
      case 'archive': return 'text-warning-600 bg-warning-50';
      case 'review': return 'text-primary-600 bg-primary-50';
      default: return 'text-neutral-600 bg-neutral-50';
    }
  };

  const formatRetentionPeriod = (period: number, unit: string) => {
    return `${period} ${unit}`;
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Document Retention Policies</h1>
        <div className="flex space-x-3">
          <Button variant="outline" onClick={handleExecuteActions} disabled={loading}>
            <Clock className="w-4 h-4 mr-2" />
            Execute Actions
          </Button>
          <Button variant="primary" onClick={() => setShowCreateModal(true)}>
            <Plus className="w-4 h-4 mr-2" />
            Create Policy
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      {report && (
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Documents</div>
            <div className="text-2xl font-bold">{report.totalDocuments.toLocaleString()}</div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Active Retention</div>
            <div className="text-2xl font-bold text-success-600">{report.activeRetention.toLocaleString()}</div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Pending Archive</div>
            <div className="text-2xl font-bold text-warning-600">{report.pendingArchive.toLocaleString()}</div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Pending Deletion</div>
            <div className="text-2xl font-bold text-error-600">{report.pendingDeletion.toLocaleString()}</div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Legal Hold</div>
            <div className="text-2xl font-bold text-primary-600">{report.onLegalHold.toLocaleString()}</div>
          </Card>
        </div>
      )}

      {/* Policies List */}
      <Card className="p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4">Retention Policies</h2>
        
        {loading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
          </div>
        ) : policies.length === 0 ? (
          <div className="text-center py-8 text-neutral-600">
            <Shield className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
            <p>No retention policies configured</p>
          </div>
        ) : (
          <div className="space-y-3">
            {policies.map((policy) => (
              <div
                key={policy.id}
                className="flex items-center justify-between p-4 border border-neutral-200 rounded-lg hover:bg-neutral-50"
              >
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <h3 className="font-semibold">{policy.name}</h3>
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getActionColor(policy.action)}`}>
                      {policy.action}
                    </span>
                    {!policy.isActive && (
                      <span className="px-2 py-1 text-xs bg-neutral-200 text-neutral-700 rounded-full">
                        Inactive
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-neutral-600 mb-2">{policy.description}</p>
                  <div className="flex items-center space-x-4 text-xs text-neutral-500">
                    <span>Document Type: {policy.documentType}</span>
                    <span>•</span>
                    <span>Retention: {formatRetentionPeriod(policy.retentionPeriod, policy.retentionUnit)}</span>
                    <span>•</span>
                    <span>Priority: {policy.priority}</span>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <Button variant="ghost" size="sm">
                    <Edit className="w-4 h-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDeletePolicy(policy.id)}
                  >
                    <Trash2 className="w-4 h-4 text-error-600" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>

      {/* Upcoming Actions */}
      {report && report.upcomingActions.length > 0 && (
        <Card className="p-6">
          <div className="flex items-center space-x-3 mb-4">
            <AlertTriangle className="w-6 h-6 text-warning-600" />
            <h2 className="text-lg font-semibold">Upcoming Retention Actions</h2>
          </div>

          <div className="space-y-2">
            {report.upcomingActions.slice(0, 10).map((action, index) => (
              <div
                key={index}
                className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
              >
                <div className="flex-1">
                  <div className="font-medium text-sm">{action.documentName}</div>
                  <div className="text-xs text-neutral-600">
                    Policy: {action.policyName}
                  </div>
                </div>
                <div className="text-right">
                  <div className={`text-sm font-medium ${
                    action.daysUntil <= 7 ? 'text-error-600' :
                    action.daysUntil <= 30 ? 'text-warning-600' :
                    'text-neutral-600'
                  }`}>
                    {action.daysUntil} days
                  </div>
                  <div className="text-xs text-neutral-500">
                    {action.action}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
};
