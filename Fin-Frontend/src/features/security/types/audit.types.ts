// Audit Trail Types
export interface AuditLog {
  id: string;
  userId: string;
  userName: string;
  action: AuditAction;
  resource: string;
  resourceId?: string;
  changes?: AuditChange[];
  ipAddress: string;
  userAgent: string;
  location?: string;
  timestamp: Date;
  status: 'success' | 'failure';
  errorMessage?: string;
}

export type AuditAction = 
  | 'login' 
  | 'logout' 
  | 'create' 
  | 'read' 
  | 'update' 
  | 'delete' 
  | 'approve' 
  | 'reject' 
  | 'export' 
  | 'import';

export interface AuditChange {
  field: string;
  oldValue: any;
  newValue: any;
}

export interface AuditFilter {
  userId?: string;
  action?: AuditAction;
  resource?: string;
  fromDate?: Date;
  toDate?: Date;
  status?: string;
  search?: string;
}

export interface AuditReport {
  period: DateRange;
  totalEvents: number;
  eventsByAction: ActionCount[];
  eventsByUser: UserCount[];
  eventsByResource: ResourceCount[];
  failedAttempts: number;
}

export interface ActionCount {
  action: AuditAction;
  count: number;
}

export interface UserCount {
  userId: string;
  userName: string;
  count: number;
}

export interface ResourceCount {
  resource: string;
  count: number;
}

export interface DateRange {
  from: Date;
  to: Date;
}
