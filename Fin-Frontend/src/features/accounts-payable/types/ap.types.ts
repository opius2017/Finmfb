/**
 * Accounts Payable Types
 */

export interface Bill {
  id: string;
  billNumber: string;
  vendorId: string;
  vendorName: string;
  billDate: Date;
  dueDate: Date;
  amount: number;
  paidAmount: number;
  balance: number;
  status: 'draft' | 'pending-approval' | 'approved' | 'scheduled' | 'paid' | 'cancelled';
  purchaseOrderId?: string;
  goodsReceiptId?: string;
  matchStatus: 'matched' | 'partial' | 'unmatched' | 'exception';
}

export interface InvoiceOCRData {
  vendorName: string;
  vendorAddress?: string;
  invoiceNumber: string;
  invoiceDate: Date;
  dueDate?: Date;
  totalAmount: number;
  taxAmount?: number;
  lineItems: Array<{
    description: string;
    quantity: number;
    unitPrice: number;
    amount: number;
  }>;
  confidence: number;
}

export interface ThreeWayMatch {
  purchaseOrder: PurchaseOrder;
  goodsReceipt: GoodsReceipt;
  invoice: Bill;
  variances: Variance[];
  matchStatus: 'matched' | 'partial' | 'exception';
}

export interface PurchaseOrder {
  id: string;
  poNumber: string;
  vendorId: string;
  items: POItem[];
  total: number;
}

export interface POItem {
  itemId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
}

export interface GoodsReceipt {
  id: string;
  grNumber: string;
  poId: string;
  items: GRItem[];
  receivedDate: Date;
}

export interface GRItem {
  itemId: string;
  quantityReceived: number;
}

export interface Variance {
  type: 'quantity' | 'price' | 'amount';
  field: string;
  expected: number;
  actual: number;
  difference: number;
  percentage: number;
  withinTolerance: boolean;
}

export interface BatchPayment {
  id: string;
  name: string;
  paymentDate: Date;
  bills: Bill[];
  totalAmount: number;
  status: 'draft' | 'pending' | 'processing' | 'completed' | 'failed';
  bankFileGenerated: boolean;
}

export interface VendorAging {
  vendorId: string;
  vendorName: string;
  totalOutstanding: number;
  current: number;
  days30: number;
  days60: number;
  days90: number;
  over90: number;
}
