// Document Retention Types
export interface RetentionPolicy {
  id: string;
  name: string;
  description: string;
  documentType: string;
  retentionPeriod: number; // in days
  retentionUnit: 'days' | 'months' | 'years';
  action: RetentionAction;
  isActive: boolean;
  priority: number;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export type RetentionAction = 'archive' | 'delete' | 'review' | 'notify';

export interface RetentionRule {
  id: string;
  policyId: string;
  condition: RuleCondition;
  value: any;
  operator: 'equals' | 'not_equals' | 'greater_than' | 'less_than' | 'contains';
}

export interface RuleCondition {
  field: string;
  type: 'metadata' | 'content' | 'date' | 'status';
}

export interface DocumentRetentionStatus {
  documentId: string;
  policyId: string;
  policyName: string;
  retentionDate: Date;
  daysRemaining: number;
  status: 'active' | 'pending_action' | 'archived' | 'deleted' | 'on_hold';
  legalHold: boolean;
  lastReviewed?: Date;
}

export interface LegalHold {
  id: string;
  name: string;
  description: string;
  reason: string;
  documentIds: string[];
  startDate: Date;
  endDate?: Date;
  isActive: boolean;
  createdBy: string;
  approvedBy?: string;
}

export interface RetentionAuditLog {
  id: string;
  documentId: string;
  policyId: string;
  action: string;
  performedBy: string;
  performedAt: Date;
  reason?: string;
  metadata?: Record<string, any>;
}

export interface RetentionReport {
  totalDocuments: number;
  activeRetention: number;
  pendingArchive: number;
  pendingDeletion: number;
  onLegalHold: number;
  byPolicy: PolicyStatistics[];
  upcomingActions: UpcomingAction[];
}

export interface PolicyStatistics {
  policyId: string;
  policyName: string;
  documentCount: number;
  averageAge: number;
  nextActionDate?: Date;
}

export interface UpcomingAction {
  documentId: string;
  documentName: string;
  policyName: string;
  action: RetentionAction;
  scheduledDate: Date;
  daysUntil: number;
}

export interface RetentionConfiguration {
  enableAutoArchive: boolean;
  enableAutoDeletion: boolean;
  requireApprovalForDeletion: boolean;
  notifyBeforeDays: number;
  archiveLocation: string;
  complianceMode: boolean;
}
