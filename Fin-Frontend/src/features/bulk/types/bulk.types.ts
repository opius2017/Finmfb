// Bulk Operations Types
export interface BulkImport {
  id: string;
  name: string;
  entityType: EntityType;
  fileName: string;
  fileSize: number;
  status: ImportStatus;
  mapping: ColumnMapping[];
  validationResults: ValidationResult;
  importResults?: ImportResult;
  createdBy: string;
  createdAt: Date;
  startedAt?: Date;
  completedAt?: Date;
}

export type EntityType = 
  | 'customers' 
  | 'vendors' 
  | 'invoices' 
  | 'payments' 
  | 'products' 
  | 'transactions' 
  | 'accounts';

export type ImportStatus = 
  | 'uploaded' 
  | 'mapping' 
  | 'validating' 
  | 'validated' 
  | 'importing' 
  | 'completed' 
  | 'failed' 
  | 'cancelled';

export interface ColumnMapping {
  sourceColumn: string;
  targetField: string;
  dataType: DataType;
  required: boolean;
  defaultValue?: any;
  transformation?: TransformationType;
}

export type DataType = 'string' | 'number' | 'date' | 'boolean' | 'email' | 'phone';

export type TransformationType = 
  | 'uppercase' 
  | 'lowercase' 
  | 'trim' 
  | 'date_format' 
  | 'number_format';

export interface ValidationResult {
  isValid: boolean;
  totalRows: number;
  validRows: number;
  invalidRows: number;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}

export interface ValidationError {
  row: number;
  column: string;
  value: any;
  error: string;
  severity: 'error' | 'warning';
}

export interface ValidationWarning {
  row: number;
  column: string;
  message: string;
}

export interface ImportResult {
  totalRecords: number;
  successfulRecords: number;
  failedRecords: number;
  skippedRecords: number;
  duration: number; // in seconds
  errors: ImportError[];
}

export interface ImportError {
  row: number;
  data: Record<string, any>;
  error: string;
}

export interface ImportTemplate {
  id: string;
  name: string;
  entityType: EntityType;
  columns: TemplateColumn[];
  sampleData: any[];
  version: string;
  createdAt: Date;
}

export interface TemplateColumn {
  name: string;
  dataType: DataType;
  required: boolean;
  description: string;
  example: string;
  validation?: ValidationRule[];
}

export interface ValidationRule {
  type: 'min' | 'max' | 'pattern' | 'enum' | 'custom';
  value: any;
  message: string;
}

export interface ImportHistory {
  id: string;
  importId: string;
  entityType: EntityType;
  fileName: string;
  status: ImportStatus;
  recordsImported: number;
  recordsFailed: number;
  importedBy: string;
  importedAt: Date;
  canRollback: boolean;
}

export interface BulkOperation {
  id: string;
  type: BulkOperationType;
  entityType: EntityType;
  selectedIds: string[];
  status: OperationStatus;
  results?: BulkOperationResult;
  createdBy: string;
  createdAt: Date;
  completedAt?: Date;
}

export type BulkOperationType = 
  | 'approve' 
  | 'reject' 
  | 'delete' 
  | 'export' 
  | 'print' 
  | 'email' 
  | 'update';

export type OperationStatus = 'pending' | 'processing' | 'completed' | 'failed';

export interface BulkOperationResult {
  totalRecords: number;
  successfulRecords: number;
  failedRecords: number;
  errors: string[];
}

export interface ExportConfig {
  entityType: EntityType;
  format: ExportFormat;
  columns: string[];
  filters?: Record<string, any>;
  includeHeaders: boolean;
}

export type ExportFormat = 'csv' | 'excel' | 'pdf' | 'json';

export interface ImportPreview {
  columns: string[];
  rows: any[][];
  totalRows: number;
  previewRows: number;
}
