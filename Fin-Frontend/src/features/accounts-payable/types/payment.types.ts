// Payment Types for AP Module
export interface PaymentBatch {
  id: string;
  batchNumber: string;
  batchDate: Date;
  scheduledDate: Date;
  status: BatchStatus;
  paymentMethod: PaymentMethod;
  bankAccountId: string;
  bankAccountName: string;
  totalAmount: number;
  paymentCount: number;
  payments: BatchPayment[];
  fileFormat?: string;
  fileName?: string;
  fileUrl?: string;
  createdBy: string;
  createdAt: Date;
  approvedBy?: string;
  approvedAt?: Date;
  processedAt?: Date;
}

export type BatchStatus = 
  | 'draft' 
  | 'pending-approval' 
  | 'approved' 
  | 'processing' 
  | 'completed' 
  | 'failed' 
  | 'cancelled';

export type PaymentMethod = 
  | 'bank-transfer' 
  | 'ach' 
  | 'wire' 
  | 'check' 
  | 'cash';

export interface BatchPayment {
  id: string;
  batchId: string;
  invoiceId: string;
  invoiceNumber: string;
  vendorId: string;
  vendorName: string;
  vendorBankAccount: BankAccount;
  amount: number;
  dueDate: Date;
  earlyPaymentDiscount?: number;
  discountedAmount?: number;
  status: PaymentStatus;
  reference?: string;
  confirmationNumber?: string;
  failureReason?: string;
  processedAt?: Date;
}

export type PaymentStatus = 
  | 'pending' 
  | 'scheduled' 
  | 'processing' 
  | 'completed' 
  | 'failed' 
  | 'cancelled';

export interface BankAccount {
  accountNumber: string;
  accountName: string;
  bankName: string;
  bankCode: string;
  branchCode?: string;
  swiftCode?: string;
  iban?: string;
}

export interface PaymentFile {
  format: FileFormat;
  content: string;
  fileName: string;
  recordCount: number;
  totalAmount: number;
  generatedAt: Date;
}

export type FileFormat = 
  | 'NACHA' 
  | 'SEPA' 
  | 'BACS' 
  | 'MT103' 
  | 'CSV' 
  | 'CUSTOM';

export interface PaymentFilter {
  vendorId?: string;
  dueDateFrom?: Date;
  dueDateTo?: Date;
  minAmount?: number;
  maxAmount?: number;
  status?: string;
  hasEarlyPaymentDiscount?: boolean;
}

export interface PaymentSchedule {
  id: string;
  name: string;
  frequency: 'daily' | 'weekly' | 'biweekly' | 'monthly';
  dayOfWeek?: number;
  dayOfMonth?: number;
  enabled: boolean;
  filters: PaymentFilter;
  autoApprove: boolean;
  notifyRecipients: string[];
}

export interface PaymentConfirmation {
  batchId: string;
  paymentId: string;
  confirmationNumber: string;
  status: 'success' | 'failed';
  processedAt: Date;
  failureReason?: string;
}

export interface PaymentRegister {
  id: string;
  period: DateRange;
  payments: PaymentRegisterEntry[];
  totalPaid: number;
  totalCount: number;
  generatedAt: Date;
}

export interface PaymentRegisterEntry {
  paymentDate: Date;
  batchNumber: string;
  vendorName: string;
  invoiceNumber: string;
  amount: number;
  paymentMethod: string;
  reference: string;
  status: string;
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface PaymentApproval {
  batchId: string;
  approver: string;
  approvalLevel: number;
  status: 'pending' | 'approved' | 'rejected';
  comments?: string;
  approvedAt?: Date;
}

export interface PaymentStatistics {
  totalBatches: number;
  totalPayments: number;
  totalAmount: number;
  pendingApproval: number;
  scheduled: number;
  completed: number;
  failed: number;
  averageBatchSize: number;
  averagePaymentAmount: number;
}
