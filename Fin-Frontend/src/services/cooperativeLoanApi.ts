import { apiClient } from './axiosApi';

// =============================================
// Member Services
// =============================================
export const memberApi = {
  getProfile: (memberId: string) => 
    apiClient.get(`/members/${memberId}`),
  
  updateSavings: (memberId: string, amount: number) =>
    apiClient.post(`/members/${memberId}/savings`, { amount }),
  
  updateMonthlyContribution: (memberId: string, amount: number) =>
    apiClient.put(`/members/${memberId}/contribution`, { amount }),
  
  updateSalaryInfo: (memberId: string, data: any) =>
    apiClient.put(`/members/${memberId}/salary`, data),
};

// =============================================
// Eligibility Services
// =============================================
export const eligibilityApi = {
  checkEligibility: (data: {
    memberId: string;
    loanAmount: number;
    loanType: 'Normal' | 'Commodity' | 'Car';
    tenorMonths: number;
    interestRate: number;
  }) =>
    apiClient.post('/eligibility/check', data),
  
  getSavingsMultiplier: (loanType: string) =>
    apiClient.get(`/eligibility/multiplier/${loanType}`),
};

// =============================================
// Guarantor Services
// =============================================
export const guarantorApi = {
  checkEligibility: (memberId: string, amount: number) =>
    apiClient.get(`/guarantor/eligibility/${memberId}`, {
      params: { guaranteedAmount: amount }
    }),
  
  requestConsent: (data: {
    applicationId: string;
    guarantorMemberId: string;
    applicantMemberId: string;
    guaranteedAmount: number;
    message?: string;
  }) =>
    apiClient.post('/guarantor/consent/request', data),
  
  getConsentByToken: (token: string) =>
    apiClient.get(`/guarantor/consent/${token}`),
  
  approveConsent: (token: string, notes?: string) =>
    apiClient.post(`/guarantor/consent/${token}/approve`, { notes }),
  
  declineConsent: (token: string, reason: string) =>
    apiClient.post(`/guarantor/consent/${token}/decline`, { reason }),
  
  getObligations: (memberId: string) =>
    apiClient.get(`/guarantor/obligations/${memberId}`),
  
  getPendingConsents: (memberId: string) =>
    apiClient.get(`/guarantor/consent/pending/${memberId}`),
};

// =============================================
// Committee Services
// =============================================
export const committeeApi = {
  getCreditProfile: (memberId: string) =>
    apiClient.get(`/loancommittee/credit-profile/${memberId}`),
  
  getRepaymentScore: (memberId: string) =>
    apiClient.get(`/loancommittee/repayment-score/${memberId}`),
  
  submitReview: (data: {
    applicationId: string;
    reviewerUserId: string;
    reviewerName: string;
    decision: 'Approved' | 'Rejected' | 'ApprovedWithConditions' | 'RequiresMoreInformation';
    comments: string;
    creditScore?: number;
    riskRating?: string;
    repaymentScore?: number;
    savingsConsistency?: boolean;
    previousLoanPerformance?: boolean;
    recommendedAmount?: number;
    recommendedTenor?: number;
    recommendedInterestRate?: number;
    recommendedAction?: string;
  }) =>
    apiClient.post('/loancommittee/review', data),
  
  getApplicationReviews: (applicationId: string) =>
    apiClient.get(`/loancommittee/reviews/${applicationId}`),
  
  getPendingApplications: () =>
    apiClient.get('/loancommittee/pending-applications'),
  
  checkApprovalStatus: (applicationId: string, requiredApprovals: number = 2) =>
    apiClient.get(`/loancommittee/approval-status/${applicationId}`, {
      params: { requiredApprovals }
    }),
};

// =============================================
// Register Services
// =============================================
export const registerApi = {
  registerLoan: (data: {
    loanId: string;
    applicationId: string;
    memberId: string;
    registeredBy: string;
    notes?: string;
  }) =>
    apiClient.post('/loanregister/register', data),
  
  getBySerialNumber: (serialNumber: string) =>
    apiClient.get(`/loanregister/serial/${serialNumber}`),
  
  getByLoanId: (loanId: string) =>
    apiClient.get(`/loanregister/loan/${loanId}`),
  
  getMemberEntries: (memberId: string) =>
    apiClient.get(`/loanregister/member/${memberId}`),
  
  getMonthlyRegister: (year: number, month: number) =>
    apiClient.get(`/loanregister/monthly/${year}/${month}`),
  
  exportMonthlyRegister: (year: number, month: number) =>
    apiClient.get(`/loanregister/monthly/${year}/${month}/export`, {
      responseType: 'blob'
    }),
  
  getStatistics: (year: number, month?: number) =>
    apiClient.get(`/loanregister/statistics/${year}`, {
      params: { month }
    }),
  
  getNextSerialNumber: (year: number, month: number) =>
    apiClient.get(`/loanregister/next-serial/${year}/${month}`),
};

// =============================================
// Threshold Services
// =============================================
export const thresholdApi = {
  getThreshold: (year: number, month: number) =>
    apiClient.get(`/threshold/${year}/${month}`),
  
  checkThreshold: (year: number, month: number, amount: number) =>
    apiClient.get(`/threshold/${year}/${month}/check`, {
      params: { amount }
    }),
  
  updateThreshold: (year: number, month: number, maxAmount: number) =>
    apiClient.put(`/threshold/${year}/${month}`, { maximumAmount: maxAmount }),
  
  getQueuedApplications: (year: number, month: number) =>
    apiClient.get(`/threshold/${year}/${month}/queued`),
  
  getHistory: (year: number) =>
    apiClient.get(`/threshold/history/${year}`),
  
  getUtilizationReport: (year: number, month?: number) =>
    apiClient.get(`/threshold/utilization/${year}`, {
      params: { month }
    }),
  
  triggerRollover: () =>
    apiClient.post('/threshold/rollover'),
};

// =============================================
// Loan Calculator Services
// =============================================
export const calculatorApi = {
  calculateEMI: (principal: number, annualRate: number, tenorMonths: number) =>
    apiClient.post('/calculator/emi', { principal, annualRate, tenorMonths }),
  
  generateAmortizationSchedule: (data: {
    principalAmount: number;
    annualInterestRate: number;
    tenorMonths: number;
    disbursementDate: string;
    firstPaymentDate: string;
  }) =>
    apiClient.post('/calculator/amortization', data),
  
  calculatePenalty: (overdueAmount: number, daysOverdue: number, penaltyRate: number) =>
    apiClient.post('/calculator/penalty', { overdueAmount, daysOverdue, penaltyRate }),
  
  calculateEarlyRepayment: (outstandingPrincipal: number, lastPaymentDate: string, annualRate: number) =>
    apiClient.post('/calculator/early-repayment', { outstandingPrincipal, lastPaymentDate, annualRate }),
};

// =============================================
// Type Definitions
// =============================================

export interface Member {
  id: string;
  memberNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  totalSavings: number;
  monthlyContribution: number;
  shareCapital: number;
  freeEquity: number;
  lockedEquity: number;
  membershipDate: string;
  isActive: boolean;
  status: string;
  creditScore?: number;
  riskRating?: string;
  activeLoansCount: number;
  totalOutstandingLoans: number;
}

export interface EligibilityResult {
  isEligible: boolean;
  reasons: string[];
  details: {
    requestedAmount: number;
    requiredSavings: number;
    actualSavings: number;
    savingsMultiplier: number;
    meetsSavingsRequirement: boolean;
    membershipDurationMonths: number;
    requiredMembershipMonths: number;
    meetsMembershipDuration: boolean;
    monthlyEMI: number;
    deductionRate: number;
    maxDeductionRate: number;
    meetsDeductionRateRequirement: boolean;
  };
}

export interface GuarantorConsent {
  id: string;
  applicationId: string;
  guarantorMemberId: string;
  applicantMemberId: string;
  guaranteedAmount: number;
  consentToken: string;
  status: 'Pending' | 'Approved' | 'Declined' | 'Expired' | 'Revoked';
  requestedAt: string;
  respondedAt?: string;
  expiresAt?: string;
  declineReason?: string;
  notes?: string;
}

export interface CommitteeReview {
  id: string;
  applicationId: string;
  reviewerUserId: string;
  reviewerName: string;
  decision: 'Pending' | 'Approved' | 'Rejected' | 'ApprovedWithConditions' | 'RequiresMoreInformation';
  reviewDate: string;
  comments: string;
  creditScore?: number;
  riskRating?: string;
  repaymentScore?: number;
  recommendedAmount?: number;
  recommendedTenor?: number;
}

export interface LoanRegister {
  id: string;
  serialNumber: string;
  loanId: string;
  applicationId: string;
  memberId: string;
  memberNumber: string;
  memberName: string;
  principalAmount: number;
  interestRate: number;
  tenorMonths: number;
  monthlyEMI: number;
  registrationDate: string;
  disbursementDate: string;
  maturityDate: string;
  loanType: string;
  status: string;
}

export interface MonthlyThreshold {
  id: string;
  year: number;
  month: number;
  maximumAmount: number;
  allocatedAmount: number;
  remainingAmount: number;
  totalApplicationsApproved: number;
  totalApplicationsRegistered: number;
  totalApplicationsQueued: number;
  status: 'Open' | 'Exhausted' | 'Closed';
}

export interface MemberCreditProfile {
  memberId: string;
  memberNumber: string;
  fullName: string;
  membershipDate: string;
  membershipDurationMonths: number;
  totalSavings: number;
  monthlyContribution: number;
  freeEquity: number;
  lockedEquity: number;
  savingsConsistency: boolean;
  totalLoansCount: number;
  activeLoansCount: number;
  closedLoansCount: number;
  defaultedLoansCount: number;
  repaymentScore: number;
  creditScore: number;
  riskRating: string;
  loanHistory: Array<{
    loanNumber: string;
    principalAmount: number;
    disbursementDate: string;
    status: string;
    outstandingBalance: number;
    repaymentPerformance: string;
  }>;
}