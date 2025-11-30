/**
 * Accounts Receivable Types
 */

export type AgingBucket = '0-30' | '31-60' | '61-90' | '90+';

export interface Invoice {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  invoiceDate: Date;
  dueDate: Date;
  amount: number;
  paidAmount: number;
  balance: number;
  status: 'draft' | 'sent' | 'viewed' | 'partial' | 'paid' | 'overdue' | 'cancelled';
  daysPastDue: number;
  agingBucket: AgingBucket;
}

export interface AgingReport {
  asOfDate: Date;
  totalOutstanding: number;
  buckets: {
    '0-30': { count: number; amount: number };
    '31-60': { count: number; amount: number };
    '61-90': { count: number; amount: number };
    '90+': { count: number; amount: number };
  };
  customers: CustomerAging[];
}

export interface CustomerAging {
  customerId: string;
  customerName: string;
  totalOutstanding: number;
  current: number;
  days30: number;
  days60: number;
  days90: number;
  over90: number;
  invoices: Invoice[];
}

export interface CreditLimit {
  customerId: string;
  limit: number;
  utilized: number;
  available: number;
  lastReviewDate: Date;
}

export interface DunningSchedule {
  invoiceId: string;
  level: number;
  scheduledDate: Date;
  sent: boolean;
  sentDate?: Date;
}

export interface ECLCalculation {
  customerId: string;
  stage: 1 | 2 | 3;
  provisionRate: number;
  provisionAmount: number;
  lastAssessmentDate: Date;
}
