// Report Types
export interface Report {
  id: string;
  name: string;
  description: string;
  type: ReportType;
  category: ReportCategory;
  dataSource: DataSource;
  fields: ReportField[];
  filters: ReportFilter[];
  groupings: ReportGrouping[];
  sortings: ReportSorting[];
  calculations: CalculatedField[];
  formatting: ReportFormatting;
  schedule?: ReportSchedule;
  isPublic: boolean;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export type ReportType = 'tabular' | 'summary' | 'matrix' | 'chart' | 'dashboard';

export type ReportCategory = 
  | 'financial' 
  | 'operational' 
  | 'analytical' 
  | 'compliance' 
  | 'custom';

export interface DataSource {
  id: string;
  name: string;
  type: 'table' | 'view' | 'query' | 'api';
  connection: string;
  query?: string;
  parameters?: DataSourceParameter[];
}

export interface DataSourceParameter {
  name: string;
  type: 'string' | 'number' | 'date' | 'boolean';
  required: boolean;
  defaultValue?: any;
}

export interface ReportField {
  id: string;
  name: string;
  label: string;
  dataType: FieldDataType;
  source: string;
  visible: boolean;
  width?: number;
  alignment?: 'left' | 'center' | 'right';
  format?: FieldFormat;
  aggregation?: AggregationType;
}

export type FieldDataType = 
  | 'string' 
  | 'number' 
  | 'currency' 
  | 'percentage' 
  | 'date' 
  | 'datetime' 
  | 'boolean';

export interface FieldFormat {
  type: 'number' | 'currency' | 'date' | 'custom';
  decimals?: number;
  currencySymbol?: string;
  dateFormat?: string;
  customFormat?: string;
}

export type AggregationType = 'sum' | 'avg' | 'count' | 'min' | 'max' | 'distinct';

export interface ReportFilter {
  id: string;
  field: string;
  operator: FilterOperator;
  value: any;
  dataType: FieldDataType;
  isParameter?: boolean;
}

export type FilterOperator = 
  | 'equals' 
  | 'not_equals' 
  | 'greater_than' 
  | 'less_than' 
  | 'between' 
  | 'in' 
  | 'not_in' 
  | 'contains' 
  | 'starts_with' 
  | 'ends_with' 
  | 'is_null' 
  | 'is_not_null';

export interface ReportGrouping {
  id: string;
  field: string;
  level: number;
  showSubtotals: boolean;
  subtotalFields?: string[];
}

export interface ReportSorting {
  field: string;
  direction: 'asc' | 'desc';
  priority: number;
}

export interface CalculatedField {
  id: string;
  name: string;
  label: string;
  formula: string;
  dataType: FieldDataType;
  format?: FieldFormat;
}

export interface ReportFormatting {
  pageSize?: 'A4' | 'Letter' | 'Legal';
  orientation?: 'portrait' | 'landscape';
  margins?: Margins;
  header?: ReportSection;
  footer?: ReportSection;
  conditionalFormatting?: ConditionalFormat[];
}

export interface Margins {
  top: number;
  right: number;
  bottom: number;
  left: number;
}

export interface ReportSection {
  enabled: boolean;
  content: string;
  height?: number;
}

export interface ConditionalFormat {
  id: string;
  field: string;
  condition: string;
  style: FormatStyle;
}

export interface FormatStyle {
  backgroundColor?: string;
  textColor?: string;
  fontWeight?: 'normal' | 'bold';
  fontStyle?: 'normal' | 'italic';
}

export interface ReportSchedule {
  enabled: boolean;
  frequency: 'daily' | 'weekly' | 'monthly' | 'quarterly';
  dayOfWeek?: number;
  dayOfMonth?: number;
  time: string;
  recipients: string[];
  format: 'pdf' | 'excel' | 'csv';
}

export interface ReportExecution {
  id: string;
  reportId: string;
  reportName: string;
  executedBy: string;
  executedAt: Date;
  parameters: Record<string, any>;
  status: 'running' | 'completed' | 'failed';
  duration?: number;
  rowCount?: number;
  fileUrl?: string;
  error?: string;
}

export interface ReportData {
  columns: ReportColumn[];
  rows: any[];
  totalRows: number;
  executionTime: number;
}

export interface ReportColumn {
  name: string;
  label: string;
  dataType: FieldDataType;
  format?: FieldFormat;
}

export interface ReportTemplate {
  id: string;
  name: string;
  description: string;
  category: ReportCategory;
  thumbnail?: string;
  config: Partial<Report>;
}
