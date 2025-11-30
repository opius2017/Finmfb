// Security Monitoring Types
export interface SecurityAlert {
  id: string;
  type: AlertType;
  severity: AlertSeverity;
  title: string;
  description: string;
  userId?: string;
  userName?: string;
  ipAddress?: string;
  timestamp: Date;
  status: 'open' | 'investigating' | 'resolved' | 'false-positive';
  resolvedBy?: string;
  resolvedAt?: Date;
  resolution?: string;
}

export type AlertType = 
  | 'failed-login' 
  | 'suspicious-activity' 
  | 'unusual-transaction' 
  | 'unauthorized-access' 
  | 'data-breach' 
  | 'malware';

export type AlertSeverity = 'low' | 'medium' | 'high' | 'critical';

export interface SecurityMetrics {
  period: DateRange;
  totalAlerts: number;
  alertsBySeverity: SeverityCount[];
  alertsByType: TypeCount[];
  failedLogins: number;
  blockedIPs: number;
  suspiciousActivities: number;
}

export interface SeverityCount {
  severity: AlertSeverity;
  count: number;
}

export interface TypeCount {
  type: AlertType;
  count: number;
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface IPWhitelist {
  id: string;
  ipAddress: string;
  description: string;
  addedBy: string;
  addedAt: Date;
  expiresAt?: Date;
}

export interface TrustedDevice {
  id: string;
  userId: string;
  deviceId: string;
  deviceName: string;
  deviceType: string;
  browser: string;
  trusted: boolean;
  trustedAt: Date;
  lastUsed: Date;
}
