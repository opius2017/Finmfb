// Vendor Management Types
export interface Vendor {
  id: string;
  vendorCode: string;
  name: string;
  type: VendorType;
  category: string;
  status: VendorStatus;
  contactPerson: string;
  email: string;
  phone: string;
  address: Address;
  taxId: string;
  bankAccounts: BankAccount[];
  paymentTerms: PaymentTerms;
  creditLimit?: number;
  rating: VendorRating;
  performance: VendorPerformance;
  documents: VendorDocument[];
  portalAccess?: VendorPortalAccess;
  createdAt: Date;
  createdBy: string;
  updatedAt: Date;
  updatedBy: string;
}

export type VendorType = 'supplier' | 'contractor' | 'service-provider' | 'consultant';

export type VendorStatus = 'active' | 'inactive' | 'suspended' | 'blacklisted';

export interface Address {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export interface BankAccount {
  id: string;
  accountNumber: string;
  accountName: string;
  bankName: string;
  bankCode: string;
  branchCode?: string;
  swiftCode?: string;
  iban?: string;
  isPrimary: boolean;
}

export interface PaymentTerms {
  termsDays: number;
  discountDays?: number;
  discountPercentage?: number;
  paymentMethod: 'bank-transfer' | 'check' | 'cash' | 'wire';
}

export interface VendorRating {
  overall: number; // 1-5
  quality: number;
  delivery: number;
  pricing: number;
  service: number;
  lastUpdated: Date;
  reviewCount: number;
}

export interface VendorPerformance {
  totalOrders: number;
  totalSpend: number;
  onTimeDeliveryRate: number;
  qualityAcceptanceRate: number;
  averageLeadTime: number;
  defectRate: number;
  lastOrderDate?: Date;
  averageOrderValue: number;
}

export interface VendorDocument {
  id: string;
  type: DocumentType;
  fileName: string;
  fileUrl: string;
  expiryDate?: Date;
  uploadedAt: Date;
  uploadedBy: string;
}

export type DocumentType = 
  | 'tax-certificate' 
  | 'business-license' 
  | 'insurance' 
  | 'contract' 
  | 'w9-form' 
  | 'other';

export interface VendorPortalAccess {
  enabled: boolean;
  username: string;
  lastLogin?: Date;
  permissions: PortalPermission[];
}

export type PortalPermission = 
  | 'view-orders' 
  | 'submit-invoices' 
  | 'view-payments' 
  | 'update-profile';

export interface VendorAgingReport {
  asOfDate: Date;
  vendors: VendorAgingEntry[];
  totals: AgingTotals;
}

export interface VendorAgingEntry {
  vendorId: string;
  vendorName: string;
  current: number;
  days1to30: number;
  days31to60: number;
  days61to90: number;
  over90: number;
  total: number;
}

export interface AgingTotals {
  current: number;
  days1to30: number;
  days31to60: number;
  days61to90: number;
  over90: number;
  total: number;
}

export interface VendorStatement {
  vendorId: string;
  vendorName: string;
  statementDate: Date;
  periodFrom: Date;
  periodTo: Date;
  openingBalance: number;
  transactions: StatementTransaction[];
  closingBalance: number;
  aging: {
    current: number;
    overdue: number;
    total: number;
  };
}

export interface StatementTransaction {
  date: Date;
  type: 'invoice' | 'payment' | 'credit-note' | 'debit-note';
  reference: string;
  description: string;
  debit: number;
  credit: number;
  balance: number;
}

export interface VendorCommunication {
  id: string;
  vendorId: string;
  type: CommunicationType;
  subject: string;
  message: string;
  sentBy: string;
  sentAt: Date;
  status: 'sent' | 'delivered' | 'read' | 'failed';
  attachments?: string[];
}

export type CommunicationType = 
  | 'email' 
  | 'sms' 
  | 'portal-message' 
  | 'phone-call' 
  | 'meeting';

export interface VendorEvaluation {
  id: string;
  vendorId: string;
  evaluationDate: Date;
  evaluatedBy: string;
  period: DateRange;
  criteria: EvaluationCriteria[];
  overallScore: number;
  comments: string;
  recommendations: string[];
  nextReviewDate: Date;
}

export interface EvaluationCriteria {
  name: string;
  weight: number;
  score: number;
  comments?: string;
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface VendorStatistics {
  totalVendors: number;
  activeVendors: number;
  totalSpend: number;
  averagePaymentDays: number;
  topVendors: TopVendor[];
  spendByCategory: CategorySpend[];
}

export interface TopVendor {
  vendorId: string;
  vendorName: string;
  totalSpend: number;
  orderCount: number;
  rating: number;
}

export interface CategorySpend {
  category: string;
  amount: number;
  percentage: number;
  vendorCount: number;
}
