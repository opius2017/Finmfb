// Third-Party Integration Types
export interface Integration {
  id: string;
  name: string;
  provider: IntegrationProvider;
  type: IntegrationType;
  status: IntegrationStatus;
  config: IntegrationConfig;
  credentials: IntegrationCredentials;
  syncSettings: SyncSettings;
  lastSyncAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export type IntegrationProvider = 
  | 'quickbooks' 
  | 'paystack' 
  | 'flutterwave' 
  | 'open-banking' 
  | 'sendgrid' 
  | 'twilio';

export type IntegrationType = 
  | 'accounting' 
  | 'payment' 
  | 'banking' 
  | 'email' 
  | 'sms';

export type IntegrationStatus = 'active' | 'inactive' | 'error' | 'pending';

export interface IntegrationConfig {
  baseUrl?: string;
  apiVersion?: string;
  environment: 'production' | 'sandbox';
  webhookUrl?: string;
  features: string[];
  customSettings?: Record<string, any>;
}

export interface IntegrationCredentials {
  apiKey?: string;
  secretKey?: string;
  publicKey?: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: Date;
}

export interface SyncSettings {
  autoSync: boolean;
  syncInterval: number; // in minutes
  syncDirection: 'bidirectional' | 'inbound' | 'outbound';
  syncEntities: string[];
  lastSyncStatus?: SyncStatus;
}

export type SyncStatus = 'success' | 'partial' | 'failed';

export interface SyncLog {
  id: string;
  integrationId: string;
  integrationName: string;
  startedAt: Date;
  completedAt?: Date;
  status: SyncStatus;
  entitiesSynced: EntitySyncResult[];
  errors: SyncError[];
  summary: SyncSummary;
}

export interface EntitySyncResult {
  entity: string;
  created: number;
  updated: number;
  failed: number;
  skipped: number;
}

export interface SyncError {
  entity: string;
  entityId?: string;
  error: string;
  timestamp: Date;
}

export interface SyncSummary {
  totalRecords: number;
  successfulRecords: number;
  failedRecords: number;
  duration: number; // in seconds
}

export interface PaymentGatewayConfig {
  provider: 'paystack' | 'flutterwave';
  publicKey: string;
  secretKey: string;
  webhookSecret: string;
  supportedCurrencies: string[];
  supportedPaymentMethods: PaymentMethod[];
}

export type PaymentMethod = 
  | 'card' 
  | 'bank_transfer' 
  | 'ussd' 
  | 'mobile_money' 
  | 'qr';

export interface PaymentTransaction {
  id: string;
  reference: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  paymentMethod: PaymentMethod;
  customer: PaymentCustomer;
  metadata?: Record<string, any>;
  createdAt: Date;
  paidAt?: Date;
}

export type PaymentStatus = 
  | 'pending' 
  | 'processing' 
  | 'success' 
  | 'failed' 
  | 'cancelled';

export interface PaymentCustomer {
  email: string;
  name?: string;
  phone?: string;
}

export interface BankingConnection {
  id: string;
  provider: string;
  bankName: string;
  accountNumber: string;
  accountName: string;
  accountType: 'savings' | 'current' | 'corporate';
  balance: number;
  currency: string;
  status: 'connected' | 'disconnected' | 'error';
  lastSyncAt?: Date;
}

export interface BankTransaction {
  id: string;
  connectionId: string;
  date: Date;
  description: string;
  amount: number;
  type: 'debit' | 'credit';
  balance: number;
  reference?: string;
  category?: string;
}

export interface EmailConfig {
  provider: 'sendgrid';
  apiKey: string;
  fromEmail: string;
  fromName: string;
  replyToEmail?: string;
  templates: EmailTemplate[];
}

export interface EmailTemplate {
  id: string;
  name: string;
  subject: string;
  templateId: string;
  variables: string[];
}

export interface SMSConfig {
  provider: 'twilio';
  accountSid: string;
  authToken: string;
  fromNumber: string;
  supportedCountries: string[];
}

export interface IntegrationStats {
  integrationId: string;
  integrationName: string;
  period: DateRange;
  totalSyncs: number;
  successfulSyncs: number;
  failedSyncs: number;
  totalRecordsSynced: number;
  averageSyncDuration: number;
  lastError?: string;
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface IntegrationTest {
  integrationId: string;
  testType: 'connection' | 'sync' | 'webhook';
  result: TestResult;
}

export interface TestResult {
  success: boolean;
  message: string;
  details?: any;
  timestamp: Date;
}
