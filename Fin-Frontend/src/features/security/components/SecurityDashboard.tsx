import React, { useState, useEffect } from 'react';
import { Shield, AlertTriangle, Lock, Activity } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { SecurityAlert, SecurityMetrics } from '../types/monitoring.types';
import { securityMonitoringService } from '../services/securityMonitoringService';

export const SecurityDashboard: React.FC = () => {
  const [alerts, setAlerts] = useState<SecurityAlert[]>([]);
  const [metrics, setMetrics] = useState<SecurityMetrics | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [alertsData, metricsData] = await Promise.all([
        securityMonitoringService.getAlerts('open'),
        securityMonitoringService.getMetrics(
          new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
          new Date()
        ),
      ]);
      setAlerts(alertsData);
      setMetrics(metricsData);
    } catch (error) {
      console.error('Failed to load security data:', error);
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return 'bg-error-100 text-error-800';
      case 'high': return 'bg-warning-100 text-warning-800';
      case 'medium': return 'bg-primary-100 text-primary-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-6">Security Dashboard</h1>

      {metrics && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <Card className="p-4">
            <div className="flex items-center space-x-3">
              <Shield className="w-8 h-8 text-primary-600" />
              <div>
                <div className="text-sm text-neutral-600">Total Alerts</div>
                <div className="text-2xl font-bold">{metrics.totalAlerts}</div>
              </div>
            </div>
          </Card>
          <Card className="p-4">
            <div className="flex items-center space-x-3">
              <AlertTriangle className="w-8 h-8 text-warning-600" />
              <div>
                <div className="text-sm text-neutral-600">Failed Logins</div>
                <div className="text-2xl font-bold">{metrics.failedLogins}</div>
              </div>
            </div>
          </Card>
          <Card className="p-4">
            <div className="flex items-center space-x-3">
              <Lock className="w-8 h-8 text-success-600" />
              <div>
                <div className="text-sm text-neutral-600">Blocked IPs</div>
                <div className="text-2xl font-bold">{metrics.blockedIPs}</div>
              </div>
            </div>
          </Card>
          <Card className="p-4">
            <div className="flex items-center space-x-3">
              <Activity className="w-8 h-8 text-error-600" />
              <div>
                <div className="text-sm text-neutral-600">Suspicious</div>
                <div className="text-2xl font-bold">{metrics.suspiciousActivities}</div>
              </div>
            </div>
          </Card>
        </div>
      )}

      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Recent Security Alerts</h2>
        <div className="space-y-3">
          {alerts.slice(0, 10).map((alert) => (
            <div key={alert.id} className="flex items-start justify-between p-3 bg-neutral-50 rounded-lg">
              <div className="flex-1">
                <div className="flex items-center space-x-2 mb-1">
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getSeverityColor(alert.severity)}`}>
                    {alert.severity}
                  </span>
                  <span className="font-medium">{alert.title}</span>
                </div>
                <p className="text-sm text-neutral-600">{alert.description}</p>
                <p className="text-xs text-neutral-500 mt-1">
                  {new Date(alert.timestamp).toLocaleString()}
                </p>
              </div>
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
};
