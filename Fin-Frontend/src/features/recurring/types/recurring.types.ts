// Recurring Transaction Types
export interface RecurringTemplate {
  id: string;
  name: string;
  description: string;
  transactionType: TransactionType;
  frequency: FrequencyConfig;
  amount: AmountConfig;
  accounts: AccountMapping;
  status: TemplateStatus;
  startDate: Date;
  endDate?: Date;
  nextRunDate: Date;
  lastRunDate?: Date;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export type TransactionType = 'journal' | 'invoice' | 'payment' | 'receipt';

export type TemplateStatus = 'active' | 'paused' | 'inactive' | 'expired';

export interface FrequencyConfig {
  type: FrequencyType;
  interval: number;
  dayOfWeek?: number; // 0-6 (Sunday-Saturday)
  dayOfMonth?: number; // 1-31
  monthOfYear?: number; // 1-12
  customSchedule?: CustomSchedule;
}

export type FrequencyType = 'daily' | 'weekly' | 'monthly' | 'quarterly' | 'yearly' | 'custom';

export interface CustomSchedule {
  dates: Date[];
  pattern?: string; // Cron-like pattern
}

export interface AmountConfig {
  type: AmountType;
  fixedAmount?: number;
  formula?: string;
  variables?: AmountVariable[];
  lastCalculatedAmount?: number;
}

export type AmountType = 'fixed' | 'variable' | 'formula-based' | 'index-linked';

export interface AmountVariable {
  name: string;
  source: 'manual' | 'database' | 'api' | 'calculation';
  query?: string;
  defaultValue?: number;
}

export interface AccountMapping {
  debitAccount: string;
  creditAccount: string;
  costCenter?: string;
  department?: string;
  project?: string;
}

export interface RecurringExecution {
  id: string;
  templateId: string;
  templateName: string;
  scheduledDate: Date;
  executedDate?: Date;
  status: ExecutionStatus;
  amount: number;
  transactionId?: string;
  error?: string;
  approvedBy?: string;
  approvedAt?: Date;
}

export type ExecutionStatus = 'pending' | 'approved' | 'executed' | 'failed' | 'cancelled';

export interface TemplateHistory {
  id: string;
  templateId: string;
  action: HistoryAction;
  changes: Record<string, any>;
  performedBy: string;
  performedAt: Date;
  reason?: string;
}

export type HistoryAction = 'created' | 'updated' | 'activated' | 'paused' | 'deleted' | 'executed';

export interface RecurringDashboard {
  totalTemplates: number;
  activeTemplates: number;
  pausedTemplates: number;
  upcomingExecutions: number;
  failedExecutions: number;
  totalMonthlyAmount: number;
  recentExecutions: RecurringExecution[];
  upcomingSchedule: ScheduleItem[];
}

export interface ScheduleItem {
  templateId: string;
  templateName: string;
  scheduledDate: Date;
  amount: number;
  status: string;
}

export interface TemplatePreview {
  template: RecurringTemplate;
  nextExecutions: PreviewExecution[];
  estimatedMonthlyAmount: number;
  estimatedYearlyAmount: number;
}

export interface PreviewExecution {
  date: Date;
  amount: number;
  description: string;
}

export interface ApprovalWorkflow {
  id: string;
  executionId: string;
  requiredApprovers: string[];
  approvals: Approval[];
  status: 'pending' | 'approved' | 'rejected';
  createdAt: Date;
}

export interface Approval {
  approverId: string;
  approverName: string;
  decision: 'approved' | 'rejected';
  comment?: string;
  timestamp: Date;
}
