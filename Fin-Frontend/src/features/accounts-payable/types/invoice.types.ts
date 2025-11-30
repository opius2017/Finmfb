// Invoice Types for AP Module
export interface VendorInvoice {
  id: string;
  invoiceNumber: string;
  vendorId: string;
  vendorName: string;
  invoiceDate: Date;
  dueDate: Date;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  total: number;
  status: InvoiceStatus;
  captureMethod: CaptureMethod;
  ocrData?: OCRData;
  lines: InvoiceLineItem[];
  attachments: InvoiceAttachment[];
  matching?: ThreeWayMatch;
  createdAt: Date;
  createdBy: string;
}

export type InvoiceStatus = 
  | 'draft' 
  | 'pending-validation' 
  | 'validated' 
  | 'pending-approval' 
  | 'approved' 
  | 'rejected' 
  | 'scheduled' 
  | 'paid';

export type CaptureMethod = 
  | 'manual' 
  | 'upload' 
  | 'email' 
  | 'mobile-camera' 
  | 'scanner';

export interface OCRData {
  extractedText: string;
  confidence: number;
  fields: OCRField[];
  processingTime: number;
  ocrEngine: string;
}

export interface OCRField {
  name: string;
  value: string;
  confidence: number;
  boundingBox?: BoundingBox;
}

export interface BoundingBox {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface InvoiceLineItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  taxRate: number;
  taxAmount: number;
  accountId?: string;
  costCenterId?: string;
}

export interface InvoiceAttachment {
  id: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  url: string;
  uploadedAt: Date;
  uploadedBy: string;
}

export interface ThreeWayMatch {
  purchaseOrderId?: string;
  goodsReceiptId?: string;
  matchStatus: MatchStatus;
  variances: MatchVariance[];
  matchedAt?: Date;
  matchedBy?: string;
}

export type MatchStatus = 
  | 'not-matched' 
  | 'matched' 
  | 'partial-match' 
  | 'exception';

export interface MatchVariance {
  type: 'quantity' | 'price' | 'total';
  expected: number;
  actual: number;
  variance: number;
  variancePercentage: number;
  withinTolerance: boolean;
}

export interface DuplicateCheckResult {
  isDuplicate: boolean;
  duplicates: VendorInvoice[];
  confidence: number;
}

export interface InvoiceValidationResult {
  isValid: boolean;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export interface ValidationError {
  field: string;
  message: string;
  severity: 'error' | 'warning';
}

export interface ValidationWarning {
  field: string;
  message: string;
  suggestion?: string;
}
