import React, { useState, useEffect } from 'react';
import { Plug, Plus, Play, Pause, RefreshCw, Trash2, CheckCircle } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { Integration, SyncLog } from '../types/integration.types';
import { integrationService } from '../services/integrationService';

export const IntegrationManagement: React.FC = () => {
  const [integrations, setIntegrations] = useState<Integration[]>([]);
  const [syncLogs, setSyncLogs] = useState<SyncLog[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [integrationsData, logsData] = await Promise.all([
        integrationService.getIntegrations(),
        integrationService.getSyncLogs(undefined, 20),
      ]);
      setIntegrations(integrationsData);
      setSyncLogs(logsData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleActivate = async (integrationId: string) => {
    try {
      await integrationService.activateIntegration(integrationId);
      await loadData();
    } catch (error) {
      console.error('Failed to activate integration:', error);
    }
  };

  const handleDeactivate = async (integrationId: string) => {
    try {
      await integrationService.deactivateIntegration(integrationId);
      await loadData();
    } catch (error) {
      console.error('Failed to deactivate integration:', error);
    }
  };

  const handleSync = async (integrationId: string) => {
    try {
      await integrationService.syncIntegration(integrationId);
      await loadData();
      alert('Sync started successfully');
    } catch (error) {
      console.error('Failed to sync integration:', error);
    }
  };

  const handleTest = async (integrationId: string) => {
    try {
      const result = await integrationService.testIntegration(integrationId, 'connection');
      alert(result.success ? 'Connection test successful!' : `Test failed: ${result.message}`);
    } catch (error) {
      console.error('Failed to test integration:', error);
    }
  };

  const handleDelete = async (integrationId: string) => {
    if (!confirm('Delete this integration?')) return;

    try {
      await integrationService.deleteIntegration(integrationId);
      await loadData();
    } catch (error) {
      console.error('Failed to delete integration:', error);
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Integrations</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Connect with third-party services
          </p>
        </div>
        <Button variant="primary">
          <Plus className="w-4 h-4 mr-2" />
          Add Integration
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Total Integrations</div>
          <div className="text-2xl font-bold">{integrations.length}</div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Active</div>
          <div className="text-2xl font-bold text-success-600">
            {integrations.filter(i => i.status === 'active').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Recent Syncs</div>
          <div className="text-2xl font-bold">
            {syncLogs.filter(l => l.status === 'success').length}
          </div>
        </Card>
        <Card className="p-4">
          <div className="text-sm text-neutral-600 mb-1">Failed Syncs</div>
          <div className="text-2xl font-bold text-error-600">
            {syncLogs.filter(l => l.status === 'failed').length}
          </div>
        </Card>
      </div>

      {/* Integrations List */}
      <Card className="p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4">Connected Services</h2>

        <div className="space-y-4">
          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          ) : integrations.length === 0 ? (
            <div className="text-center py-8 text-neutral-600">
              <Plug className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No integrations configured</p>
            </div>
          ) : (
            integrations.map((integration) => (
              <div
                key={integration.id}
                className="p-4 border border-neutral-200 rounded-lg hover:bg-neutral-50"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-2">
                      <span className="text-2xl">
                        {integrationService.getProviderIcon(integration.provider)}
                      </span>
                      <div>
                        <h3 className="font-semibold">{integration.name}</h3>
                        <p className="text-sm text-neutral-600">
                          {integrationService.formatProviderName(integration.provider)} • {integration.type}
                        </p>
                      </div>
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        integrationService.getStatusColor(integration.status)
                      }`}>
                        {integration.status}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-3 text-sm">
                      <div>
                        <div className="text-neutral-600">Environment</div>
                        <div className="font-medium capitalize">{integration.config.environment}</div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Auto Sync</div>
                        <div className="font-medium">
                          {integration.syncSettings.autoSync ? 'Enabled' : 'Disabled'}
                        </div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Last Sync</div>
                        <div className="font-medium">
                          {integration.lastSyncAt
                            ? new Date(integration.lastSyncAt).toLocaleDateString()
                            : 'Never'}
                        </div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Sync Status</div>
                        <div className="flex items-center space-x-1">
                          {integration.syncSettings.lastSyncStatus === 'success' && (
                            <CheckCircle className="w-4 h-4 text-success-600" />
                          )}
                          <span className="font-medium">
                            {integration.syncSettings.lastSyncStatus || 'N/A'}
                          </span>
                        </div>
                      </div>
                    </div>

                    {integration.syncSettings.syncEntities.length > 0 && (
                      <div className="mt-3 flex flex-wrap gap-1">
                        {integration.syncSettings.syncEntities.map((entity) => (
                          <span
                            key={entity}
                            className="px-2 py-1 text-xs bg-primary-50 text-primary-700 rounded"
                          >
                            {entity}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>

                  <div className="flex items-center space-x-2 ml-4">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleSync(integration.id)}
                      title="Sync Now"
                    >
                      <RefreshCw className="w-4 h-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleTest(integration.id)}
                      title="Test Connection"
                    >
                      <Play className="w-4 h-4" />
                    </Button>
                    {integration.status === 'active' ? (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeactivate(integration.id)}
                        title="Deactivate"
                      >
                        <Pause className="w-4 h-4" />
                      </Button>
                    ) : (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleActivate(integration.id)}
                        title="Activate"
                      >
                        <Play className="w-4 h-4 text-success-600" />
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleDelete(integration.id)}
                      title="Delete"
                    >
                      <Trash2 className="w-4 h-4 text-error-600" />
                    </Button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </Card>

      {/* Recent Sync Logs */}
      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Recent Sync Activity</h2>

        <div className="space-y-3">
          {syncLogs.map((log) => (
            <div
              key={log.id}
              className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
            >
              <div className="flex-1">
                <div className="flex items-center space-x-2 mb-1">
                  <span className="font-medium text-sm">{log.integrationName}</span>
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                    integrationService.getSyncStatusColor(log.status)
                  }`}>
                    {log.status}
                  </span>
                </div>
                <div className="text-xs text-neutral-600">
                  {new Date(log.startedAt).toLocaleString()} • 
                  {log.summary.totalRecords} records • 
                  {log.summary.duration}s duration
                </div>
              </div>

              <div className="text-right text-sm">
                <div className="text-success-600 font-medium">
                  {log.summary.successfulRecords} successful
                </div>
                {log.summary.failedRecords > 0 && (
                  <div className="text-error-600">
                    {log.summary.failedRecords} failed
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
};
