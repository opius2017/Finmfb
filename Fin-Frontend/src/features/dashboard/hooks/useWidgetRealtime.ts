import { useEffect, useState, useCallback } from 'react';
import { realtimeService } from '../services/realtimeService';
import type { Widget } from '../types/widget.types';

export interface UseWidgetRealtimeOptions {
  widgetId: string;
  enabled?: boolean;
  onUpdate?: (data: any) => void;
}

export const useWidgetRealtime = ({
  widgetId,
  enabled = true,
  onUpdate,
}: UseWidgetRealtimeOptions) => {
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null);
  const [updateCount, setUpdateCount] = useState(0);

  useEffect(() => {
    if (!enabled) return;

    const unsubscribe = realtimeService.subscribeToWidget(
      widgetId,
      (data) => {
        setLastUpdate(new Date());
        setUpdateCount(prev => prev + 1);
        
        if (onUpdate) {
          onUpdate(data);
        }
      }
    );

    return () => {
      unsubscribe();
    };
  }, [widgetId, enabled, onUpdate]);

  return {
    lastUpdate,
    updateCount,
    isConnected: realtimeService.getConnectionStatus() === 'connected',
  };
};

export interface UseMetricRealtimeOptions {
  metricId: string;
  enabled?: boolean;
  onUpdate?: (data: any) => void;
}

export const useMetricRealtime = ({
  metricId,
  enabled = true,
  onUpdate,
}: UseMetricRealtimeOptions) => {
  const [data, setData] = useState<any>(null);
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null);

  useEffect(() => {
    if (!enabled) return;

    const unsubscribe = realtimeService.subscribeToMetric(
      metricId,
      (update) => {
        setData(update);
        setLastUpdate(new Date());
        
        if (onUpdate) {
          onUpdate(update);
        }
      }
    );

    return () => {
      unsubscribe();
    };
  }, [metricId, enabled, onUpdate]);

  return {
    data,
    lastUpdate,
    isConnected: realtimeService.getConnectionStatus() === 'connected',
  };
};

export interface UseDashboardRealtimeOptions {
  userId: string;
  enabled?: boolean;
  onUpdate?: (data: any) => void;
}

export const useDashboardRealtime = ({
  userId,
  enabled = true,
  onUpdate,
}: UseDashboardRealtimeOptions) => {
  const [updates, setUpdates] = useState<any[]>([]);
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null);

  const clearUpdates = useCallback(() => {
    setUpdates([]);
  }, []);

  useEffect(() => {
    if (!enabled) return;

    const unsubscribe = realtimeService.subscribeToDashboard(
      userId,
      (data) => {
        setUpdates(prev => [...prev, data]);
        setLastUpdate(new Date());
        
        if (onUpdate) {
          onUpdate(data);
        }
      }
    );

    return () => {
      unsubscribe();
    };
  }, [userId, enabled, onUpdate]);

  return {
    updates,
    lastUpdate,
    clearUpdates,
    isConnected: realtimeService.getConnectionStatus() === 'connected',
  };
};
