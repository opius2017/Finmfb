/**
 * Real-Time Dashboard Service
 * Handles real-time updates for dashboard widgets
 * 
 * Note: This is a mock implementation. In production, replace with SignalR:
 * npm install @microsoft/signalr
 */

type EventCallback = (data: any) => void;

export interface DashboardUpdate {
  widgetId: string;
  data: any;
  timestamp: Date;
}

export interface MetricUpdate {
  metricId: string;
  value: number | string;
  trend?: {
    value: number;
    direction: 'up' | 'down';
  };
}

class RealtimeService {
  private subscribers: Map<string, Set<EventCallback>> = new Map();
  private connectionStatus: 'connected' | 'disconnected' | 'connecting' = 'disconnected';
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 3000;
  private simulationInterval: NodeJS.Timeout | null = null;

  /**
   * Initialize the real-time connection
   */
  async connect(): Promise<void> {
    if (this.connectionStatus === 'connected' || this.connectionStatus === 'connecting') {
      return;
    }

    this.connectionStatus = 'connecting';
    console.log('[RealtimeService] Connecting...');

    try {
      // Simulate connection delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      this.connectionStatus = 'connected';
      this.reconnectAttempts = 0;
      console.log('[RealtimeService] Connected successfully');

      // Start simulation for demo purposes
      this.startSimulation();

      // Notify connection status subscribers
      this.emit('connection-status', { status: 'connected' });
    } catch (error) {
      console.error('[RealtimeService] Connection failed:', error);
      this.connectionStatus = 'disconnected';
      this.handleReconnect();
    }
  }

  /**
   * Disconnect from real-time service
   */
  disconnect(): void {
    console.log('[RealtimeService] Disconnecting...');
    this.connectionStatus = 'disconnected';
    this.stopSimulation();
    this.emit('connection-status', { status: 'disconnected' });
  }

  /**
   * Subscribe to dashboard updates
   */
  subscribeToDashboard(userId: string, callback: EventCallback): () => void {
    const event = `dashboard:${userId}`;
    return this.subscribe(event, callback);
  }

  /**
   * Subscribe to specific metric updates
   */
  subscribeToMetric(metricId: string, callback: EventCallback): () => void {
    const event = `metric:${metricId}`;
    return this.subscribe(event, callback);
  }

  /**
   * Subscribe to widget updates
   */
  subscribeToWidget(widgetId: string, callback: EventCallback): () => void {
    const event = `widget:${widgetId}`;
    return this.subscribe(event, callback);
  }

  /**
   * Generic subscribe method
   */
  public subscribe(event: string, callback: (data: any) => void): () => void {
    if (!this.subscribers.has(event)) {
      this.subscribers.set(event, new Set());
    }
    
    this.subscribers.get(event)!.add(callback);
    console.log(`[RealtimeService] Subscribed to ${event}`);

    // Return unsubscribe function
    return () => {
      const callbacks = this.subscribers.get(event);
      if (callbacks) {
        callbacks.delete(callback);
        if (callbacks.size === 0) {
          this.subscribers.delete(event);
        }
      }
      console.log(`[RealtimeService] Unsubscribed from ${event}`);
    };
  }

  /**
   * Emit event to subscribers
   */
  private emit(event: string, data: any): void {
    const callbacks = this.subscribers.get(event);
    if (callbacks) {
      callbacks.forEach(callback => {
        try {
          callback(data);
        } catch (error) {
          console.error(`[RealtimeService] Error in callback for ${event}:`, error);
        }
      });
    }
  }

  /**
   * Handle reconnection logic
   */
  private async handleReconnect(): Promise<void> {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('[RealtimeService] Max reconnection attempts reached');
      this.emit('connection-status', { status: 'failed' });
      return;
    }

    this.reconnectAttempts++;
    const delay = this.reconnectDelay * this.reconnectAttempts;
    
    console.log(`[RealtimeService] Reconnecting in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
    
    await new Promise(resolve => setTimeout(resolve, delay));
    await this.connect();
  }

  /**
   * Get current connection status
   */
  getConnectionStatus(): 'connected' | 'disconnected' | 'connecting' {
    return this.connectionStatus;
  }

  /**
   * Start simulation for demo purposes
   * Remove this in production when using real SignalR
   */
  private startSimulation(): void {
    this.simulationInterval = setInterval(() => {
      // Simulate random metric updates
      const metrics = ['total-revenue', 'active-users', 'pending-transactions', 'cash-balance'];
      const randomMetric = metrics[Math.floor(Math.random() * metrics.length)];
      
      this.emit(`metric:${randomMetric}`, {
        metricId: randomMetric,
        value: Math.floor(Math.random() * 100000),
        trend: {
          value: Math.floor(Math.random() * 20),
          direction: Math.random() > 0.5 ? 'up' : 'down',
        },
        timestamp: new Date(),
      });
    }, 5000); // Update every 5 seconds
  }

  /**
   * Stop simulation
   */
  private stopSimulation(): void {
    if (this.simulationInterval) {
      clearInterval(this.simulationInterval);
      this.simulationInterval = null;
    }
  }
}

// Export singleton instance
export const realtimeService = new RealtimeService();

/**
 * React Hook for real-time updates
 */
export const useRealtimeConnection = () => {
  const [status, setStatus] = React.useState(realtimeService.getConnectionStatus());

  React.useEffect(() => {
    const unsubscribe = realtimeService.subscribe('connection-status', (data) => {
      setStatus(data.status);
    });

    // Connect on mount
    realtimeService.connect();

    return () => {
      unsubscribe();
    };
  }, []);

  return {
    status,
    connect: () => realtimeService.connect(),
    disconnect: () => realtimeService.disconnect(),
  };
};

// Add React import for the hook
import React from 'react';
