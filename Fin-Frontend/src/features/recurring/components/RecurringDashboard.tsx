import React, { useState, useEffect } from 'react';
import { Calendar, TrendingUp, AlertCircle, CheckCircle, Clock, DollarSign } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { RecurringDashboard as DashboardData, ScheduleItem } from '../types/recurring.types';
import { recurringService } from '../services/recurringService';

export const RecurringDashboard: React.FC = () => {
  const [dashboard, setDashboard] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(false);
  const [calendarView, setCalendarView] = useState(false);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    setLoading(true);
    try {
      const data = await recurringService.getDashboard();
      setDashboard(data);
    } catch (error) {
      console.error('Failed to load dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const groupScheduleByDate = (schedule: ScheduleItem[]) => {
    const grouped: Record<string, ScheduleItem[]> = {};
    
    schedule.forEach(item => {
      const dateKey = new Date(item.scheduledDate).toDateString();
      if (!grouped[dateKey]) {
        grouped[dateKey] = [];
      }
      grouped[dateKey].push(item);
    });

    return grouped;
  };

  if (loading || !dashboard) {
    return (
      <div className="p-6">
        <div className="animate-pulse">
          <div className="h-8 bg-neutral-200 rounded w-1/4 mb-6"></div>
          <div className="grid grid-cols-4 gap-4">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="h-24 bg-neutral-200 rounded"></div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  const groupedSchedule = groupScheduleByDate(dashboard.upcomingSchedule);

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Recurring Transactions Dashboard</h1>
        <div className="flex space-x-3">
          <Button
            variant={calendarView ? 'primary' : 'outline'}
            onClick={() => setCalendarView(!calendarView)}
          >
            <Calendar className="w-4 h-4 mr-2" />
            {calendarView ? 'List View' : 'Calendar View'}
          </Button>
          <Button variant="outline" onClick={loadDashboard}>
            Refresh
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Total Templates</div>
            <TrendingUp className="w-5 h-5 text-primary-600" />
          </div>
          <div className="text-2xl font-bold">{dashboard.totalTemplates}</div>
          <div className="text-xs text-neutral-500 mt-1">
            {dashboard.activeTemplates} active
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Upcoming</div>
            <Clock className="w-5 h-5 text-warning-600" />
          </div>
          <div className="text-2xl font-bold text-warning-600">
            {dashboard.upcomingExecutions}
          </div>
          <div className="text-xs text-neutral-500 mt-1">
            Next 30 days
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Failed</div>
            <AlertCircle className="w-5 h-5 text-error-600" />
          </div>
          <div className="text-2xl font-bold text-error-600">
            {dashboard.failedExecutions}
          </div>
          <div className="text-xs text-neutral-500 mt-1">
            Requires attention
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Monthly Amount</div>
            <DollarSign className="w-5 h-5 text-success-600" />
          </div>
          <div className="text-2xl font-bold">
            ₦{dashboard.totalMonthlyAmount.toLocaleString()}
          </div>
          <div className="text-xs text-neutral-500 mt-1">
            Estimated
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Executions */}
        <Card className="p-6">
          <div className="flex items-center space-x-3 mb-4">
            <CheckCircle className="w-6 h-6 text-success-600" />
            <h2 className="text-lg font-semibold">Recent Executions</h2>
          </div>

          <div className="space-y-3">
            {dashboard.recentExecutions.slice(0, 5).map((execution) => (
              <div
                key={execution.id}
                className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
              >
                <div className="flex-1">
                  <div className="font-medium text-sm">{execution.templateName}</div>
                  <div className="text-xs text-neutral-600">
                    {execution.executedDate 
                      ? new Date(execution.executedDate).toLocaleString()
                      : new Date(execution.scheduledDate).toLocaleString()}
                  </div>
                </div>
                <div className="text-right">
                  <div className="font-medium">
                    ₦{execution.amount.toLocaleString()}
                  </div>
                  <div className={`text-xs ${
                    execution.status === 'executed' ? 'text-success-600' :
                    execution.status === 'failed' ? 'text-error-600' :
                    'text-warning-600'
                  }`}>
                    {execution.status}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>

        {/* Upcoming Schedule */}
        <Card className="p-6">
          <div className="flex items-center space-x-3 mb-4">
            <Calendar className="w-6 h-6 text-primary-600" />
            <h2 className="text-lg font-semibold">Upcoming Schedule</h2>
          </div>

          {calendarView ? (
            <div className="space-y-4">
              {Object.entries(groupedSchedule).slice(0, 7).map(([date, items]) => (
                <div key={date}>
                  <div className="text-sm font-semibold text-neutral-700 mb-2">
                    {new Date(date).toLocaleDateString('en-US', { 
                      weekday: 'short', 
                      month: 'short', 
                      day: 'numeric' 
                    })}
                  </div>
                  <div className="space-y-2">
                    {items.map((item, idx) => (
                      <div
                        key={idx}
                        className="flex items-center justify-between p-2 bg-primary-50 rounded text-sm"
                      >
                        <span className="font-medium">{item.templateName}</span>
                        <span className="text-primary-700">
                          ₦{item.amount.toLocaleString()}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="space-y-3">
              {dashboard.upcomingSchedule.slice(0, 7).map((item, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
                >
                  <div className="flex-1">
                    <div className="font-medium text-sm">{item.templateName}</div>
                    <div className="text-xs text-neutral-600">
                      {new Date(item.scheduledDate).toLocaleDateString()}
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="font-medium">
                      ₦{item.amount.toLocaleString()}
                    </div>
                    <div className="text-xs text-neutral-600">
                      {item.status}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>
      </div>

      {/* Template Status Breakdown */}
      <Card className="p-6 mt-6">
        <h2 className="text-lg font-semibold mb-4">Template Status</h2>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="p-4 bg-success-50 rounded-lg">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-success-900">Active</span>
              <CheckCircle className="w-5 h-5 text-success-600" />
            </div>
            <div className="text-2xl font-bold text-success-700">
              {dashboard.activeTemplates}
            </div>
            <div className="text-xs text-success-600 mt-1">
              {((dashboard.activeTemplates / dashboard.totalTemplates) * 100).toFixed(0)}% of total
            </div>
          </div>

          <div className="p-4 bg-warning-50 rounded-lg">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-warning-900">Paused</span>
              <Clock className="w-5 h-5 text-warning-600" />
            </div>
            <div className="text-2xl font-bold text-warning-700">
              {dashboard.pausedTemplates}
            </div>
            <div className="text-xs text-warning-600 mt-1">
              Temporarily inactive
            </div>
          </div>

          <div className="p-4 bg-error-50 rounded-lg">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-error-900">Failed</span>
              <AlertCircle className="w-5 h-5 text-error-600" />
            </div>
            <div className="text-2xl font-bold text-error-700">
              {dashboard.failedExecutions}
            </div>
            <div className="text-xs text-error-600 mt-1">
              Needs review
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};
