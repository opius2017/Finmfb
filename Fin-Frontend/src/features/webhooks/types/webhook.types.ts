// Webhook Types
export interface Webhook {
  id: string;
  name: string;
  url: string;
  events: WebhookEvent[];
  status: WebhookStatus;
  secret: string;
  headers: Record<string, string>;
  retryPolicy: RetryPolicy;
  filters?: WebhookFilter[];
  createdBy: string;
  createdAt: Date;
  lastTriggeredAt?: Date;
}

export type WebhookStatus = 'active' | 'inactive' | 'failed';

export type WebhookEvent = 
  | 'invoice.created'
  | 'invoice.updated'
  | 'invoice.paid'
  | 'payment.received'
  | 'payment.failed'
  | 'customer.created'
  | 'customer.updated'
  | 'transaction.created'
  | 'reconciliation.completed'
  | 'report.generated'
  | 'user.created'
  | 'user.updated';

export interface RetryPolicy {
  maxAttempts: number;
  backoffMultiplier: number;
  initialDelay: number; // in seconds
  maxDelay: number; // in seconds
}

export interface WebhookFilter {
  field: string;
  operator: 'equals' | 'not_equals' | 'contains' | 'greater_than' | 'less_than';
  value: any;
}

export interface WebhookDelivery {
  id: string;
  webhookId: string;
  webhookName: string;
  event: WebhookEvent;
  payload: any;
  status: DeliveryStatus;
  statusCode?: number;
  responseBody?: string;
  attempts: DeliveryAttempt[];
  createdAt: Date;
  deliveredAt?: Date;
}

export type DeliveryStatus = 'pending' | 'delivered' | 'failed' | 'retrying';

export interface DeliveryAttempt {
  attemptNumber: number;
  timestamp: Date;
  statusCode?: number;
  responseTime?: number;
  error?: string;
  success: boolean;
}

export interface WebhookPayload {
  event: WebhookEvent;
  timestamp: Date;
  data: any;
  metadata?: Record<string, any>;
}

export interface WebhookSignature {
  algorithm: 'sha256' | 'sha512';
  header: string;
  secret: string;
}

export interface WebhookStats {
  webhookId: string;
  webhookName: string;
  period: DateRange;
  totalDeliveries: number;
  successfulDeliveries: number;
  failedDeliveries: number;
  averageResponseTime: number;
  successRate: number;
  eventBreakdown: EventStats[];
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface EventStats {
  event: WebhookEvent;
  count: number;
  successCount: number;
  failureCount: number;
}

export interface WebhookTest {
  webhookId: string;
  event: WebhookEvent;
  payload: any;
  result: WebhookTestResult;
}

export interface WebhookTestResult {
  success: boolean;
  statusCode?: number;
  responseTime: number;
  responseBody?: string;
  error?: string;
  timestamp: Date;
}

export interface WebhookLog {
  id: string;
  webhookId: string;
  event: WebhookEvent;
  action: LogAction;
  details: string;
  timestamp: Date;
  performedBy?: string;
}

export type LogAction = 
  | 'created' 
  | 'updated' 
  | 'deleted' 
  | 'activated' 
  | 'deactivated' 
  | 'delivery_success' 
  | 'delivery_failure' 
  | 'retry_exhausted';
