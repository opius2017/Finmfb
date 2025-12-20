// Loan Management Types

export interface LoanApplication {
  id: string;
  applicationNumber: string;
  memberId: string;
  memberName: string;
  loanType: LoanType;
  requestedAmount: number;
  purpose: string;
  tenor: number; // in months
  guarantors: Guarantor[];
  status: LoanApplicationStatus;
  submittedAt: Date;
  approvedAt?: Date;
  disbursedAt?: Date;
  rejectedAt?: Date;
  rejectionReason?: string;
}

export type LoanType = 'normal' | 'commodity' | 'car';

export type LoanApplicationStatus =
  | 'draft'
  | 'submitted'
  | 'under_review'
  | 'committee_review'
  | 'approved'
  | 'registered'
  | 'disbursed'
  | 'rejected'
  | 'cancelled';

export interface Guarantor {
  id: string;
  memberId: string;
  memberName: string;
  guaranteedAmount: number;
  freeEquity: number;
  consentStatus: 'pending' | 'approved' | 'declined';
  consentDate?: Date;
}

export interface LoanEligibility {
  isEligible: boolean;
  reasons: string[];
  maxLoanAmount: number;
  requiredSavings: number;
  currentSavings: number;
  deductionRateHeadroom: number;
  membershipDuration: number; // in months
}

export interface LoanConfiguration {
  id: string;
  loanType: LoanType;
  interestRate: number; // annual percentage
  maxTenor: number; // in months
  minTenor: number;
  savingsMultiplier: number; // e.g., 200% = 2.0
  maxDeductionRate: number; // e.g., 45% = 0.45
  requiresCollateral: boolean;
  requiresGuarantors: boolean;
  minGuarantors: number;
  description: string;
  isActive: boolean;
}

export interface MonthlyLoanThreshold {
  id: string;
  month: string; // YYYY-MM
  maxAmount: number;
  currentAmount: number;
  remainingAmount: number;
  registeredLoans: number;
  queuedLoans: number;
}

export interface Loan {
  id: string;
  loanNumber: string; // e.g., LH/2025/001
  applicationId: string;
  memberId: string;
  memberName: string;
  loanType: LoanType;
  principalAmount: number;
  interestRate: number;
  tenor: number;
  monthlyEMI: number;
  totalInterest: number;
  totalRepayment: number;
  disbursedAmount: number;
  outstandingBalance: number;
  principalPaid: number;
  interestPaid: number;
  penaltiesPaid: number;
  status: LoanStatus;
  disbursementDate: Date;
  firstPaymentDate: Date;
  maturityDate: Date;
  guarantors: Guarantor[];
  amortizationSchedule: AmortizationEntry[];
  createdAt: Date;
  updatedAt: Date;
}

export type LoanStatus =
  | 'active'
  | 'current'
  | 'delinquent'
  | 'defaulted'
  | 'restructured'
  | 'closed'
  | 'written_off';

export interface AmortizationEntry {
  installmentNumber: number;
  dueDate: Date;
  openingBalance: number;
  principalDue: number;
  interestDue: number;
  totalDue: number;
  principalPaid: number;
  interestPaid: number;
  totalPaid: number;
  closingBalance: number;
  status: 'pending' | 'paid' | 'partial' | 'overdue';
  paidDate?: Date;
}

export interface LoanRepayment {
  id: string;
  loanId: string;
  amount: number;
  principalAmount: number;
  interestAmount: number;
  penaltyAmount: number;
  paymentDate: Date;
  paymentMethod: 'payroll' | 'bank_transfer' | 'cash' | 'other';
  transactionReference: string;
  notes?: string;
}

export interface LoanCommitteeReview {
  id: string;
  applicationId: string;
  reviewerId: string;
  reviewerName: string;
  decision: 'approved' | 'rejected' | 'pending';
  comments: string;
  repaymentScore: number;
  riskAssessment: string;
  recommendedAmount?: number;
  reviewedAt: Date;
}

export interface LoanRegister {
  id: string;
  month: string; // YYYY-MM
  loans: LoanRegisterEntry[];
  totalAmount: number;
  totalLoans: number;
}

export interface LoanRegisterEntry {
  serialNumber: string; // e.g., LH/2025/001
  loanId: string;
  memberId: string;
  memberName: string;
  loanType: LoanType;
  amount: number;
  registeredDate: Date;
}

export interface MemberSavings {
  memberId: string;
  memberName: string;
  totalSavings: number;
  monthlyContribution: number;
  shareCapital: number;
  freeEquity: number;
  lockedEquity: number;
  lastContributionDate: Date;
}

export interface LoanCalculatorInput {
  loanType: LoanType;
  principalAmount: number;
  tenor: number;
}

export interface LoanCalculatorOutput {
  monthlyEMI: number;
  totalInterest: number;
  totalRepayment: number;
  eligibility: LoanEligibility;
  amortizationSchedule: AmortizationEntry[];
}

export interface CommodityItem {
  id: string;
  name: string;
  description: string;
  category: string;
  costPrice: number;
  sellingPrice: number;
  stockQuantity: number;
  supplier: string;
  imageUrl?: string;
  isAvailable: boolean;
}

export interface CommodityRequest {
  id: string;
  memberId: string;
  itemId: string;
  itemName: string;
  quantity: number;
  totalAmount: number;
  status: 'pending' | 'approved' | 'fulfilled' | 'rejected';
  voucherNumber?: string;
  requestedAt: Date;
  fulfilledAt?: Date;
}

export interface DeductionSchedule {
  month: string; // YYYY-MM
  entries: DeductionEntry[];
  totalSavings: number;
  totalLoanDeductions: number;
  totalDeductions: number;
}

export interface DeductionEntry {
  memberId: string;
  memberName: string;
  payrollPin: string;
  monthlySavings: number;
  ongoingLoanDeductions: number;
  newLoanDeductions: number;
  totalDeduction: number;
  loanReferences: string[];
}

export interface ActualDeduction {
  memberId: string;
  payrollPin: string;
  savingsDeducted: number;
  loanRepaymentDeducted: number;
  totalDeducted: number;
  deductionDate: Date;
  remittanceNotes?: string;
  status: string;
}

export interface LoanDelinquency {
  loanId: string;
  loanNumber: string;
  memberId: string;
  memberName: string;
  daysOverdue: number;
  overdueAmount: number;
  penaltyAmount: number;
  lastPaymentDate?: Date;
  nextActionDate: Date;
  guarantorsNotified: boolean;
}

export interface LoanClearanceCertificate {
  id: string;
  loanId: string;
  loanNumber: string;
  memberId: string;
  memberName: string;
  principalAmount: number;
  totalPaid: number;
  clearanceDate: Date;
  issuedBy: string;
  certificateNumber: string;
}

export interface PortfolioReport {
  totalPortfolioValue: number;
  totalActiveLoans: number;
  portfolioAtRisk: number;
  par30: number;
  par60: number;
  par90: number;
}
