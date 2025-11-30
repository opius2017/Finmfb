// Three-Way Matching Types
export interface PurchaseOrder {
  id: string;
  poNumber: string;
  vendorId: string;
  vendorName: string;
  orderDate: Date;
  expectedDeliveryDate: Date;
  status: POStatus;
  lines: POLineItem[];
  total: number;
  createdBy: string;
  createdAt: Date;
}

export type POStatus = 'draft' | 'approved' | 'sent' | 'partial-received' | 'received' | 'closed';

export interface POLineItem {
  id: string;
  itemCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  receivedQuantity: number;
  invoicedQuantity: number;
}

export interface GoodsReceipt {
  id: string;
  grnNumber: string;
  purchaseOrderId: string;
  poNumber: string;
  vendorId: string;
  vendorName: string;
  receiptDate: Date;
  receivedBy: string;
  status: GRNStatus;
  lines: GRNLineItem[];
  notes?: string;
}

export type GRNStatus = 'draft' | 'completed' | 'invoiced' | 'closed';

export interface GRNLineItem {
  id: string;
  poLineId: string;
  itemCode: string;
  description: string;
  orderedQuantity: number;
  receivedQuantity: number;
  rejectedQuantity: number;
  acceptedQuantity: number;
  unitPrice: number;
  amount: number;
}

export interface ThreeWayMatchResult {
  matchStatus: 'matched' | 'partial-match' | 'exception' | 'not-matched';
  overallScore: number;
  variances: MatchVariance[];
  recommendations: string[];
  canAutoApprove: boolean;
  requiresReview: boolean;
}

export interface MatchVariance {
  type: VarianceType;
  field: string;
  poValue: number;
  grnValue: number;
  invoiceValue: number;
  variance: number;
  variancePercentage: number;
  withinTolerance: boolean;
  toleranceLimit: number;
  severity: 'info' | 'warning' | 'error';
}

export type VarianceType = 
  | 'quantity' 
  | 'price' 
  | 'total' 
  | 'tax' 
  | 'discount' 
  | 'shipping';

export interface MatchingTolerance {
  quantityVariance: number; // percentage
  priceVariance: number; // percentage
  totalVariance: number; // absolute amount
  autoApproveThreshold: number; // percentage
}

export interface MatchingRule {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
  conditions: MatchingCondition[];
  action: 'approve' | 'flag' | 'reject';
  priority: number;
}

export interface MatchingCondition {
  field: string;
  operator: 'equals' | 'greater_than' | 'less_than' | 'between';
  value: any;
}

export interface MatchingHistory {
  id: string;
  invoiceId: string;
  purchaseOrderId?: string;
  goodsReceiptId?: string;
  matchedAt: Date;
  matchedBy: string;
  matchResult: ThreeWayMatchResult;
  action: 'approved' | 'rejected' | 'overridden';
  notes?: string;
}

export interface MatchException {
  id: string;
  invoiceId: string;
  exceptionType: string;
  severity: 'low' | 'medium' | 'high';
  description: string;
  variances: MatchVariance[];
  status: 'open' | 'investigating' | 'resolved' | 'approved';
  assignedTo?: string;
  resolution?: string;
  resolvedAt?: Date;
  resolvedBy?: string;
}
