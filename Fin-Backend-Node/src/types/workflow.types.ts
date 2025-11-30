/**
 * Workflow Types and Interfaces
 */

export enum WorkflowStatus {
  PENDING = 'PENDING',
  IN_PROGRESS = 'IN_PROGRESS',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  COMPLETED = 'COMPLETED',
  CANCELLED = 'CANCELLED',
  TIMEOUT = 'TIMEOUT',
}

export enum WorkflowStepType {
  APPROVAL = 'approval',
  NOTIFICATION = 'notification',
  CALCULATION = 'calculation',
  INTEGRATION = 'integration',
  CONDITION = 'condition',
  PARALLEL = 'parallel',
}

export enum ApprovalStatus {
  PENDING = 'PENDING',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  SKIPPED = 'SKIPPED',
}

export interface WorkflowDefinition {
  id: string;
  name: string;
  type: string;
  description?: string;
  version: number;
  steps: WorkflowStep[];
  rules: WorkflowRule[];
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface WorkflowStep {
  id: string;
  name: string;
  type: WorkflowStepType;
  description?: string;
  config: WorkflowStepConfig;
  nextSteps: string[];
  timeout?: number; // in seconds
  retryPolicy?: RetryPolicy;
}

export interface WorkflowStepConfig {
  // Approval step config
  approvers?: ApproverConfig;
  approvalType?: 'any' | 'all' | 'majority';
  
  // Notification step config
  recipients?: string[];
  template?: string;
  channel?: 'email' | 'sms' | 'push' | 'in-app';
  
  // Calculation step config
  calculation?: string;
  formula?: string;
  
  // Integration step config
  service?: string;
  endpoint?: string;
  method?: string;
  
  // Condition step config
  condition?: string;
  trueStep?: string;
  falseStep?: string;
  
  // Parallel step config
  parallelSteps?: string[];
  
  // Custom config
  [key: string]: any;
}

export interface ApproverConfig {
  type: 'role' | 'user' | 'dynamic';
  roles?: string[];
  users?: string[];
  dynamicRule?: string;
  minApprovals?: number;
}

export interface RetryPolicy {
  maxAttempts: number;
  delayMs: number;
  backoffMultiplier?: number;
}

export interface WorkflowRule {
  id: string;
  name: string;
  condition: string;
  action: string;
  priority: number;
}

export interface WorkflowInstance {
  id: string;
  workflowId: string;
  entityType: string;
  entityId: string;
  initiatorId: string;
  status: WorkflowStatus;
  currentStep?: string;
  data: Record<string, any>;
  context: WorkflowContext;
  steps: WorkflowStepInstance[];
  startedAt: Date;
  completedAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface WorkflowStepInstance {
  id: string;
  stepId: string;
  workflowInstanceId: string;
  status: WorkflowStatus;
  assignedTo?: string[];
  approvals?: StepApproval[];
  result?: any;
  error?: string;
  startedAt?: Date;
  completedAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface StepApproval {
  id: string;
  stepInstanceId: string;
  approverId: string;
  status: ApprovalStatus;
  comments?: string;
  approvedAt?: Date;
  createdAt: Date;
}

export interface WorkflowContext {
  entityType: string;
  entityId: string;
  initiatorId: string;
  data: Record<string, any>;
  metadata?: Record<string, any>;
}

export interface WorkflowEvent {
  id: string;
  workflowInstanceId: string;
  type: string;
  data: any;
  timestamp: Date;
}

export interface WorkflowTransition {
  from: string;
  to: string;
  condition?: string;
  action?: string;
}

export interface CreateWorkflowInput {
  workflowType: string;
  entityType: string;
  entityId: string;
  initiatorId: string;
  data: Record<string, any>;
}

export interface ApproveStepInput {
  workflowInstanceId: string;
  stepId: string;
  approverId: string;
  comments?: string;
}

export interface RejectStepInput {
  workflowInstanceId: string;
  stepId: string;
  approverId: string;
  reason: string;
  comments?: string;
}

export interface WorkflowQuery {
  status?: WorkflowStatus;
  entityType?: string;
  entityId?: string;
  initiatorId?: string;
  assignedTo?: string;
  startDate?: Date;
  endDate?: Date;
}

export default {
  WorkflowStatus,
  WorkflowStepType,
  ApprovalStatus,
};
