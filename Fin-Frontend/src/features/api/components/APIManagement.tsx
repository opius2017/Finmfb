import React, { useState, useEffect } from 'react';
import { Key, Plus, Copy, RefreshCw, Trash2, Activity, FileCode } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { APIKey, APIUsageStats } from '../types/api.types';
import { apiManagementService } from '../services/apiService';

export const APIManagement: React.FC = () => {
  const [apiKeys, setApiKeys] = useState<APIKey[]>([]);
  const [usageStats, setUsageStats] = useState<APIUsageStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [selectedKey, setSelectedKey] = useState<string | null>(null);

  useEffect(() => {
    loadAPIKeys();
    loadUsageStats();
  }, []);

  const loadAPIKeys = async () => {
    setLoading(true);
    try {
      const data = await apiManagementService.getAPIKeys();
      setApiKeys(data);
    } catch (error) {
      console.error('Failed to load API keys:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadUsageStats = async () => {
    try {
      const data = await apiManagementService.getUsageStats();
      setUsageStats(data);
    } catch (error) {
      console.error('Failed to load usage stats:', error);
    }
  };

  const handleCopyKey = (key: string) => {
    navigator.clipboard.writeText(key);
    alert('API key copied to clipboard');
  };

  const handleRegenerateKey = async (keyId: string) => {
    if (!confirm('Regenerate this API key? The old key will stop working immediately.')) return;

    try {
      await apiManagementService.regenerateAPIKey(keyId);
      await loadAPIKeys();
    } catch (error) {
      console.error('Failed to regenerate key:', error);
    }
  };

  const handleRevokeKey = async (keyId: string) => {
    if (!confirm('Revoke this API key? This action cannot be undone.')) return;

    try {
      await apiManagementService.revokeAPIKey(keyId);
      await loadAPIKeys();
    } catch (error) {
      console.error('Failed to revoke key:', error);
    }
  };

  const handleDownloadSpec = async (format: 'json' | 'yaml') => {
    try {
      const blob = await apiManagementService.downloadOpenAPISpec(undefined, format);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `openapi-spec.${format}`;
      a.click();
    } catch (error) {
      console.error('Failed to download spec:', error);
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">API Management</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Manage API keys, monitor usage, and access documentation
          </p>
        </div>
        <div className="flex space-x-3">
          <Button variant="outline" onClick={() => handleDownloadSpec('json')}>
            <FileCode className="w-4 h-4 mr-2" />
            OpenAPI Spec
          </Button>
          <Button variant="primary">
            <Plus className="w-4 h-4 mr-2" />
            Create API Key
          </Button>
        </div>
      </div>

      {/* Usage Stats */}
      {usageStats && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Total Requests</div>
              <Activity className="w-5 h-5 text-primary-600" />
            </div>
            <div className="text-2xl font-bold">{usageStats.totalRequests.toLocaleString()}</div>
            <div className="text-xs text-neutral-500 mt-1">
              {usageStats.successfulRequests.toLocaleString()} successful
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Success Rate</div>
              <Activity className="w-5 h-5 text-success-600" />
            </div>
            <div className="text-2xl font-bold text-success-600">
              {((usageStats.successfulRequests / usageStats.totalRequests) * 100).toFixed(1)}%
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Avg Response Time</div>
              <Activity className="w-5 h-5 text-warning-600" />
            </div>
            <div className="text-2xl font-bold">
              {apiManagementService.formatResponseTime(usageStats.averageResponseTime)}
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Failed Requests</div>
              <Activity className="w-5 h-5 text-error-600" />
            </div>
            <div className="text-2xl font-bold text-error-600">
              {usageStats.failedRequests.toLocaleString()}
            </div>
          </Card>
        </div>
      )}

      {/* API Keys List */}
      <Card className="p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4">API Keys</h2>

        <div className="space-y-4">
          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          ) : apiKeys.length === 0 ? (
            <div className="text-center py-8 text-neutral-600">
              <Key className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No API keys found</p>
            </div>
          ) : (
            apiKeys.map((key) => (
              <div
                key={key.id}
                className="p-4 border border-neutral-200 rounded-lg hover:bg-neutral-50"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-2">
                      <Key className="w-5 h-5 text-primary-600" />
                      <div>
                        <h3 className="font-semibold">{key.name}</h3>
                        <div className="flex items-center space-x-2 mt-1">
                          <code className="text-sm bg-neutral-100 px-2 py-1 rounded font-mono">
                            {apiManagementService.maskAPIKey(key.key)}
                          </code>
                          <button
                            onClick={() => handleCopyKey(key.key)}
                            className="text-primary-600 hover:text-primary-700"
                          >
                            <Copy className="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        apiManagementService.getStatusColor(key.status)
                      }`}>
                        {key.status}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-3 text-sm">
                      <div>
                        <div className="text-neutral-600">Rate Limit</div>
                        <div className="font-medium">
                          {key.rateLimit.requestsPerMinute}/min
                        </div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Permissions</div>
                        <div className="font-medium">{key.permissions.length} resources</div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Last Used</div>
                        <div className="font-medium">
                          {key.lastUsedAt
                            ? new Date(key.lastUsedAt).toLocaleDateString()
                            : 'Never'}
                        </div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Created</div>
                        <div className="font-medium">
                          {new Date(key.createdAt).toLocaleDateString()}
                        </div>
                      </div>
                    </div>

                    {key.ipWhitelist.length > 0 && (
                      <div className="mt-3 text-xs text-neutral-600">
                        IP Whitelist: {key.ipWhitelist.join(', ')}
                      </div>
                    )}
                  </div>

                  <div className="flex items-center space-x-2 ml-4">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleRegenerateKey(key.id)}
                      title="Regenerate"
                    >
                      <RefreshCw className="w-4 h-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleRevokeKey(key.id)}
                      title="Revoke"
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

      {/* Top Endpoints */}
      {usageStats && usageStats.topEndpoints.length > 0 && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Top Endpoints</h2>
          <div className="space-y-3">
            {usageStats.topEndpoints.map((endpoint, index) => (
              <div
                key={index}
                className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
              >
                <div className="flex-1">
                  <div className="flex items-center space-x-2">
                    <span className="px-2 py-1 text-xs font-mono bg-primary-100 text-primary-700 rounded">
                      {endpoint.method}
                    </span>
                    <span className="font-mono text-sm">{endpoint.endpoint}</span>
                  </div>
                </div>
                <div className="flex items-center space-x-6 text-sm">
                  <div>
                    <div className="text-neutral-600">Requests</div>
                    <div className="font-medium">{endpoint.count.toLocaleString()}</div>
                  </div>
                  <div>
                    <div className="text-neutral-600">Avg Time</div>
                    <div className="font-medium">
                      {apiManagementService.formatResponseTime(endpoint.averageResponseTime)}
                    </div>
                  </div>
                  <div>
                    <div className="text-neutral-600">Error Rate</div>
                    <div className={`font-medium ${
                      endpoint.errorRate > 5 ? 'text-error-600' : 'text-success-600'
                    }`}>
                      {endpoint.errorRate.toFixed(1)}%
                    </div>
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
