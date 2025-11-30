import React, { useState, useEffect } from 'react';
import { Webhook as WebhookIcon, Plus, Play, Pause, Trash2, RefreshCw, Activity } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { Webhook, WebhookDelivery, WebhookStats } from '../types/webhook.types';
import { webhookService } from '../services/webhookService';

export const WebhookManagement: React.FC = () => {
  const [webhooks, setWebhooks] = useState<Webhook[]>([]);
  const [deliveries, setDeliveries] = useState<WebhookDelivery[]>([]);
  const [stats, setStats] = useState<WebhookStats | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [webhooksData, deliveriesData, statsData] = await Promise.all([
        webhookService.getWebhooks(),
        webhookService.getDeliveries(undefined, 50),
        webhookService.getStats(),
      ]);
      setWebhooks(webhooksData);
      setDeliveries(deliveriesData);
      setStats(statsData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleActivate = async (webhookId: string) => {
    try {
      await webhookService.activateWebhook(webhookId);
      await loadData();
    } catch (error) {
      console.error('Failed to activate webhook:', error);
    }
  };

  const handleDeactivate = async (webhookId: string) => {
    try {
      await webhookService.deactivateWebhook(webhookId);
      await loadData();
    } catch (error) {
      console.error('Failed to deactivate webhook:', error);
    }
  };

  const handleTest = async (webhookId: string) => {
    try {
      const result = await webhookService.testWebhook(webhookId, 'test.event', { test: true });
      alert(result.success ? 'Test successful!' : `Test failed: ${result.error}`);
    } catch (error) {
      console.error('Failed to test webhook:', error);
    }
  };

  const handleDelete = async (webhookId: string) => {
    if (!confirm('Delete this webhook?')) return;

    try {
      await webhookService.deleteWebhook(webhookId);
      await loadData();
    } catch (error) {
      console.error('Failed to delete webhook:', error);
    }
  };

  const handleRetry = async (deliveryId: string) => {
    try {
      await webhookService.retryDelivery(deliveryId);
      await loadData();
    } catch (error) {
      console.error('Failed to retry delivery:', error);
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Webhook Management</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Configure and monitor webhook endpoints
          </p>
        </div>
        <Button variant="primary">
          <Plus className="w-4 h-4 mr-2" />
          Create Webhook
        </Button>
      </div>

      {/* Stats */}
      {stats && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Total Deliveries</div>
              <Activity className="w-5 h-5 text-primary-600" />
            </div>
            <div className="text-2xl font-bold">{stats.totalDeliveries.toLocaleString()}</div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Success Rate</div>
              <Activity className="w-5 h-5 text-success-600" />
            </div>
            <div className="text-2xl font-bold text-success-600">
              {stats.successRate.toFixed(1)}%
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Avg Response Time</div>
              <Activity className="w-5 h-5 text-warning-600" />
            </div>
            <div className="text-2xl font-bold">
              {stats.averageResponseTime.toFixed(0)}ms
            </div>
          </Card>

          <Card className="p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="text-sm text-neutral-600">Failed</div>
              <Activity className="w-5 h-5 text-error-600" />
            </div>
            <div className="text-2xl font-bold text-error-600">
              {stats.failedDeliveries.toLocaleString()}
            </div>
          </Card>
        </div>
      )}

      {/* Webhooks List */}
      <Card className="p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4">Webhooks</h2>

        <div className="space-y-4">
          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          ) : webhooks.length === 0 ? (
            <div className="text-center py-8 text-neutral-600">
              <WebhookIcon className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No webhooks configured</p>
            </div>
          ) : (
            webhooks.map((webhook) => (
              <div
                key={webhook.id}
                className="p-4 border border-neutral-200 rounded-lg hover:bg-neutral-50"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-2">
                      <WebhookIcon className="w-5 h-5 text-primary-600" />
                      <div>
                        <h3 className="font-semibold">{webhook.name}</h3>
                        <code className="text-sm text-neutral-600 font-mono">{webhook.url}</code>
                      </div>
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                        webhookService.getStatusColor(webhook.status)
                      }`}>
                        {webhook.status}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-3 text-sm">
                      <div>
                        <div className="text-neutral-600">Events</div>
                        <div className="font-medium">{webhook.events.length} subscribed</div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Retry Policy</div>
                        <div className="font-medium">
                          {webhook.retryPolicy.maxAttempts} attempts
                        </div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Last Triggered</div>
                        <div className="font-medium">
                          {webhook.lastTriggeredAt
                            ? new Date(webhook.lastTriggeredAt).toLocaleDateString()
                            : 'Never'}
                        </div>
                      </div>
                      <div>
                        <div className="text-neutral-600">Secret</div>
                        <code className="text-xs font-mono">
                          {webhookService.maskSecret(webhook.secret)}
                        </code>
                      </div>
                    </div>

                    <div className="mt-3 flex flex-wrap gap-1">
                      {webhook.events.slice(0, 5).map((event) => (
                        <span
                          key={event}
                          className="px-2 py-1 text-xs bg-primary-50 text-primary-700 rounded"
                        >
                          {webhookService.formatEventName(event)}
                        </span>
                      ))}
                      {webhook.events.length > 5 && (
                        <span className="px-2 py-1 text-xs bg-neutral-100 text-neutral-700 rounded">
                          +{webhook.events.length - 5} more
                        </span>
                      )}
                    </div>
                  </div>

                  <div className="flex items-center space-x-2 ml-4">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleTest(webhook.id)}
                      title="Test"
                    >
                      <Play className="w-4 h-4" />
                    </Button>
                    {webhook.status === 'active' ? (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeactivate(webhook.id)}
                        title="Deactivate"
                      >
                        <Pause className="w-4 h-4" />
                      </Button>
                    ) : (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleActivate(webhook.id)}
                        title="Activate"
                      >
                        <Play className="w-4 h-4 text-success-600" />
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleDelete(webhook.id)}
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

      {/* Recent Deliveries */}
      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Recent Deliveries</h2>

        <div className="space-y-3">
          {deliveries.slice(0, 10).map((delivery) => (
            <div
              key={delivery.id}
              className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
            >
              <div className="flex-1">
                <div className="flex items-center space-x-2 mb-1">
                  <span className="font-medium text-sm">{delivery.webhookName}</span>
                  <span className="px-2 py-1 text-xs bg-primary-100 text-primary-700 rounded">
                    {webhookService.formatEventName(delivery.event)}
                  </span>
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                    webhookService.getDeliveryStatusColor(delivery.status)
                  }`}>
                    {delivery.status}
                  </span>
                </div>
                <div className="text-xs text-neutral-600">
                  {new Date(delivery.createdAt).toLocaleString()} • {delivery.attempts.length} attempts
                  {delivery.statusCode && ` • HTTP ${delivery.statusCode}`}
                </div>
              </div>

              {delivery.status === 'failed' && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleRetry(delivery.id)}
                >
                  <RefreshCw className="w-4 h-4 mr-2" />
                  Retry
                </Button>
              )}
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
};
