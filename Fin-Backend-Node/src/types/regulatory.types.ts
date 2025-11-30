// Regulatory Reporting Types

export enum ReportType {
  CBN_PRUDENTIAL = 'CBN_PRUDENTIAL',
  CBN_CAPITAL_ADEQUACY = 'CBN_CAPITAL_ADEQUACY',
  CBN_LIQUIDITY = 'CBN_LIQUIDITY',
  FIRS_VAT = 'FIRS_VAT',
  FIRS_WHT = 'FIRS_WHT',
  FIRS_CIT = 'FIRS_CIT',
  IFRS9_ECL = 'IFRS9_ECL'
}

export enum ReportStatus {
  DRAFT = 'DRAFT',
  SUBMITTED = 'SUBMITTED',
  APPROVED = 'APPROVED',
  FILED = 'FILED'
}

export enum ComplianceCategory {
  CBN = 'CBN',
  NDIC = 'NDIC',
  FIRS = 'FIRS',
  IFRS = 'IFRS',
  INTERNAL = 'INTERNAL'
}

export enum ComplianceFrequency {
  DAILY = 'DAILY',
  WEEKLY = 'WEEKLY',
  MONTHLY = 'MONTHLY',
  QUARTERLY = 'QUARTERLY',
  ANNUALLY = 'ANNUALLY'
}

export enum ComplianceStatus {
  PENDING = 'PENDING',
  IN_PROGRESS = 'IN_PROGRESS',
  COMPLETED = 'COMPLETED',
  OVERDUE = 'OVERDUE'
}

export enum CompliancePriority {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL'
}

export enum AlertType {
  CAPITAL_ADEQUACY = 'CAPITAL_ADEQUACY',
  LIQUIDITY_RATIO = 'LIQUIDITY_RATIO',
  EXPOSURE_LIMIT = 'EXPOSURE_LIMIT',
  COMPLIANCE_DEADLINE = 'COMPLIANCE_DEADLINE',
  THRESHOLD_BREACH = 'THRESHOLD_BREACH'
}

export enum AlertSeverity {
  INFO = 'INFO',
  WARNING = 'WARNING',
  CRITICAL = 'CRITICAL'
}

export enum TaxType {
  VAT = 'VAT',
  WHT_SERVICES = 'WHT_SERVICES',
  WHT_RENT = 'WHT_RENT',
  WHT_DIVIDENDS = 'WHT_DIVIDENDS',
  WHT_INTEREST = 'WHT_INTEREST',
  CIT = 'CIT'
}

export enum TaxStatus {
  CALCULATED = 'CALCULATED',
  PAID = 'PAID',
  FILED = 'FILED'
}

export interface RegulatoryReport {
  id: string;
  reportType: ReportType;
  reportPeriodStart: Date;
  reportPeriodEnd: Date;
  fiscalYear: number;
  status: ReportStatus;
  data: any;
  fileUrl?: string;
  submissionDate?: Date;
  submissionReference?: string;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface ComplianceChecklist {
  id: string;
  title: string;
  description?: string;
  category: ComplianceCategory;
  frequency: ComplianceFrequency;
  dueDate: Date;
  status: ComplianceStatus;
  priority: CompliancePriority;
  responsiblePerson?: string;
  completedAt?: Date;
  completedBy?: string;
  notes?: string;
  metadata?: any;
  createdAt: Date;
  updatedAt: Date;
}

export interface RegulatoryAlert {
  id: string;
  alertType: AlertType;
  severity: AlertSeverity;
  title: string;
  message: string;
  thresholdValue?: number;
  currentValue?: number;
  entityType?: string;
  entityId?: string;
  isAcknowledged: boolean;
  acknowledgedBy?: string;
  acknowledgedAt?: Date;
  resolutionNotes?: string;
  resolvedAt?: Date;
  metadata?: any;
  createdAt: Date;
}

export interface TaxCalculation {
  id: string;
  taxType: TaxType;
  periodStart: Date;
  periodEnd: Date;
  taxableAmount: number;
  taxRate: number;
  taxAmount: number;
  transactionId?: string;
  entityType?: string;
  entityId?: string;
  status: TaxStatus;
  paymentDate?: Date;
  paymentReference?: string;
  metadata?: any;
  createdAt: Date;
  updatedAt: Date;
}

export interface ECLProvision {
  id: string;
  loanId: string;
  assessmentDate: Date;
  stage: 1 | 2 | 3;
  probabilityOfDefault: number;
  lossGivenDefault: number;
  exposureAtDefault: number;
  expectedCreditLoss: number;
  provisionAmount: number;
  daysPastDue: number;
  creditRating?: string;
  significantIncreaseInRisk: boolean;
  notes?: string;
  metadata?: any;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

// CBN Report Data Structures
export interface CBNPrudentialReturn {
  reportingPeriod: string;
  institutionName: string;
  institutionCode: string;
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  totalLoans: number;
  nonPerformingLoans: number;
  nplRatio: number;
  totalDeposits: number;
  liquidAssets: number;
  liquidityRatio: number;
  capitalAdequacyRatio: number;
  profitBeforeTax: number;
  profitAfterTax: number;
}

export interface CBNCapitalAdequacyReport {
  reportingPeriod: string;
  tier1Capital: number;
  tier2Capital: number;
  totalRegulatoryCapital: number;
  riskWeightedAssets: number;
  capitalAdequacyRatio: number;
  minimumRequirement: number;
  surplus: number;
  complianceStatus: 'COMPLIANT' | 'NON_COMPLIANT';
}

export interface CBNLiquidityReport {
  reportingPeriod: string;
  liquidAssets: number;
  totalDeposits: number;
  liquidityRatio: number;
  minimumRequirement: number;
  surplus: number;
  complianceStatus: 'COMPLIANT' | 'NON_COMPLIANT';
}

// FIRS Report Data Structures
export interface FIRSVATReturn {
  period: string;
  taxPayerName: string;
  taxPayerTIN: string;
  standardRatedSupplies: number;
  zeroRatedSupplies: number;
  exemptSupplies: number;
  totalSupplies: number;
  outputVAT: number;
  inputVAT: number;
  netVAT: number;
  vatPayable: number;
  vatRefundable: number;
}

export interface FIRSWHTSchedule {
  period: string;
  taxPayerName: string;
  taxPayerTIN: string;
  whtType: TaxType;
  transactions: Array<{
    date: Date;
    beneficiary: string;
    beneficiaryTIN?: string;
    description: string;
    grossAmount: number;
    whtRate: number;
    whtAmount: number;
  }>;
  totalGrossAmount: number;
  totalWHTAmount: number;
}

export interface FIRSCITComputation {
  assessmentYear: number;
  taxPayerName: string;
  taxPayerTIN: string;
  turnover: number;
  costOfSales: number;
  grossProfit: number;
  operatingExpenses: number;
  profitBeforeTax: number;
  taxAdjustments: number;
  assessableProfit: number;
  taxRate: number;
  taxPayable: number;
  whtCredit: number;
  netTaxPayable: number;
}

// IFRS 9 Report Data Structures
export interface IFRS9ECLReport {
  reportingDate: Date;
  stage1Loans: {
    count: number;
    totalExposure: number;
    averagePD: number;
    averageLGD: number;
    totalECL: number;
  };
  stage2Loans: {
    count: number;
    totalExposure: number;
    averagePD: number;
    averageLGD: number;
    totalECL: number;
  };
  stage3Loans: {
    count: number;
    totalExposure: number;
    averagePD: number;
    averageLGD: number;
    totalECL: number;
  };
  totalProvision: number;
  provisionCoverageRatio: number;
}
