import {
  LoanApplication,
  Loan,
  LoanEligibility,
  LoanConfiguration,
  MonthlyLoanThreshold,
  LoanCalculatorInput,
  LoanCalculatorOutput,
  LoanCommitteeReview,
  LoanRegister,
  MemberSavings,
  CommodityItem,
  CommodityRequest,
  DeductionSchedule,
  ActualDeduction,
  LoanDelinquency,
  LoanClearanceCertificate,
  LoanRepayment,
} from '../types/loan.types';

export class LoanService {
  private apiEndpoint = '/api/loans';

  // Loan Applications
  async createApplication(application: Partial<LoanApplication>): Promise<LoanApplication> {
    const response = await fetch(`${this.apiEndpoint}/applications`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(application),
    });
    if (!response.ok) throw new Error('Failed to create loan application');
    return response.json();
  }

  async getApplications(status?: string): Promise<LoanApplication[]> {
    const params = status ? `?status=${status}` : '';
    const response = await fetch(`${this.apiEndpoint}/applications${params}`);
    if (!response.ok) throw new Error('Failed to fetch applications');
    return response.json();
  }

  async getApplication(applicationId: string): Promise<LoanApplication> {
    const response = await fetch(`${this.apiEndpoint}/applications/${applicationId}`);
    if (!response.ok) throw new Error('Failed to fetch application');
    return response.json();
  }

  async submitApplication(applicationId: string): Promise<LoanApplication> {
    const response = await fetch(`${this.apiEndpoint}/applications/${applicationId}/submit`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to submit application');
    return response.json();
  }

  // Eligibility Check
  async checkEligibility(memberId: string, loanType: string, amount: number): Promise<LoanEligibility> {
    const response = await fetch(`${this.apiEndpoint}/eligibility`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ memberId, loanType, amount }),
    });
    if (!response.ok) throw new Error('Failed to check eligibility');
    return response.json();
  }

  // Loan Calculator
  async calculateLoan(input: LoanCalculatorInput): Promise<LoanCalculatorOutput> {
    const response = await fetch(`${this.apiEndpoint}/calculator`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(input),
    });
    if (!response.ok) throw new Error('Failed to calculate loan');
    return response.json();
  }

  // Loan Configuration (Super Admin)
  async getConfigurations(): Promise<LoanConfiguration[]> {
    const response = await fetch(`${this.apiEndpoint}/configurations`);
    if (!response.ok) throw new Error('Failed to fetch configurations');
    return response.json();
  }

  async updateConfiguration(id: string, config: Partial<LoanConfiguration>): Promise<LoanConfiguration> {
    const response = await fetch(`${this.apiEndpoint}/configurations/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(config),
    });
    if (!response.ok) throw new Error('Failed to update configuration');
    return response.json();
  }

  // Monthly Threshold
  async getMonthlyThreshold(month: string): Promise<MonthlyLoanThreshold> {
    const response = await fetch(`${this.apiEndpoint}/threshold/${month}`);
    if (!response.ok) throw new Error('Failed to fetch threshold');
    return response.json();
  }

  async updateMonthlyThreshold(month: string, maxAmount: number): Promise<MonthlyLoanThreshold> {
    const response = await fetch(`${this.apiEndpoint}/threshold/${month}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ maxAmount }),
    });
    if (!response.ok) throw new Error('Failed to update threshold');
    return response.json();
  }

  // Committee Review
  async submitCommitteeReview(review: Partial<LoanCommitteeReview>): Promise<LoanCommitteeReview> {
    const response = await fetch(`${this.apiEndpoint}/committee/reviews`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(review),
    });
    if (!response.ok) throw new Error('Failed to submit review');
    return response.json();
  }

  async getCommitteeReviews(applicationId: string): Promise<LoanCommitteeReview[]> {
    const response = await fetch(`${this.apiEndpoint}/committee/reviews?applicationId=${applicationId}`);
    if (!response.ok) throw new Error('Failed to fetch reviews');
    return response.json();
  }

  // Loan Register
  async registerLoan(applicationId: string): Promise<Loan> {
    const response = await fetch(`${this.apiEndpoint}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ applicationId }),
    });
    if (!response.ok) throw new Error('Failed to register loan');
    return response.json();
  }

  async getLoanRegister(month: string): Promise<LoanRegister> {
    const response = await fetch(`${this.apiEndpoint}/register/${month}`);
    if (!response.ok) throw new Error('Failed to fetch loan register');
    return response.json();
  }

  // Loans
  async getLoans(status?: string): Promise<Loan[]> {
    const params = status ? `?status=${status}` : '';
    const response = await fetch(`${this.apiEndpoint}${params}`);
    if (!response.ok) throw new Error('Failed to fetch loans');
    return response.json();
  }

  async getLoan(loanId: string): Promise<Loan> {
    const response = await fetch(`${this.apiEndpoint}/${loanId}`);
    if (!response.ok) throw new Error('Failed to fetch loan');
    return response.json();
  }

  async disburseLoan(loanId: string, disbursementDetails: any): Promise<Loan> {
    const response = await fetch(`${this.apiEndpoint}/${loanId}/disburse`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(disbursementDetails),
    });
    if (!response.ok) throw new Error('Failed to disburse loan');
    return response.json();
  }

  // Repayments
  async recordRepayment(repayment: Partial<LoanRepayment>): Promise<LoanRepayment> {
    const response = await fetch(`${this.apiEndpoint}/repayments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(repayment),
    });
    if (!response.ok) throw new Error('Failed to record repayment');
    return response.json();
  }

  async getRepayments(loanId: string): Promise<LoanRepayment[]> {
    const response = await fetch(`${this.apiEndpoint}/${loanId}/repayments`);
    if (!response.ok) throw new Error('Failed to fetch repayments');
    return response.json();
  }

  // Member Savings
  async getMemberSavings(memberId: string): Promise<MemberSavings> {
    const response = await fetch(`${this.apiEndpoint}/members/${memberId}/savings`);
    if (!response.ok) throw new Error('Failed to fetch member savings');
    return response.json();
  }

  // Commodity Store
  async getCommodityItems(): Promise<CommodityItem[]> {
    const response = await fetch(`${this.apiEndpoint}/commodity/items`);
    if (!response.ok) throw new Error('Failed to fetch commodity items');
    return response.json();
  }

  async createCommodityRequest(request: Partial<CommodityRequest>): Promise<CommodityRequest> {
    const response = await fetch(`${this.apiEndpoint}/commodity/requests`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    });
    if (!response.ok) throw new Error('Failed to create commodity request');
    return response.json();
  }

  // Deduction Schedules
  async generateDeductionSchedule(month: string): Promise<DeductionSchedule> {
    const response = await fetch(`${this.apiEndpoint}/deductions/schedule/${month}/generate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to generate deduction schedule');
    return response.json();
  }

  async downloadDeductionSchedule(month: string): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/deductions/schedule/${month}/download`);
    if (!response.ok) throw new Error('Failed to download deduction schedule');
    return response.blob();
  }

  async uploadActualDeductions(month: string, file: File): Promise<void> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${this.apiEndpoint}/deductions/actual/${month}/upload`, {
      method: 'POST',
      body: formData,
    });
    if (!response.ok) throw new Error('Failed to upload actual deductions');
  }

  async reconcileDeductions(month: string): Promise<any> {
    const response = await fetch(`${this.apiEndpoint}/deductions/reconcile/${month}`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to reconcile deductions');
    return response.json();
  }

  // Delinquency Management
  async getDelinquentLoans(): Promise<LoanDelinquency[]> {
    const response = await fetch(`${this.apiEndpoint}/delinquency`);
    if (!response.ok) throw new Error('Failed to fetch delinquent loans');
    return response.json();
  }

  async sendDelinquencyNotification(loanId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${loanId}/notify-delinquency`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to send notification');
  }

  // Loan Closure
  async closeLoan(loanId: string): Promise<LoanClearanceCertificate> {
    const response = await fetch(`${this.apiEndpoint}/${loanId}/close`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to close loan');
    return response.json();
  }

  async getClearanceCertificate(loanId: string): Promise<LoanClearanceCertificate> {
    const response = await fetch(`${this.apiEndpoint}/${loanId}/clearance-certificate`);
    if (!response.ok) throw new Error('Failed to fetch clearance certificate');
    return response.json();
  }

  // Utility Methods
  calculateEMI(principal: number, annualRate: number, tenorMonths: number): number {
    const monthlyRate = annualRate / 12 / 100;
    if (monthlyRate === 0) return principal / tenorMonths;
    
    const emi = principal * monthlyRate * Math.pow(1 + monthlyRate, tenorMonths) / 
                (Math.pow(1 + monthlyRate, tenorMonths) - 1);
    return Math.round(emi * 100) / 100;
  }

  calculateTotalInterest(emi: number, tenorMonths: number, principal: number): number {
    return (emi * tenorMonths) - principal;
  }

  formatLoanNumber(serialNumber: number, year: number): string {
    return `LH/${year}/${String(serialNumber).padStart(3, '0')}`;
  }

  getLoanTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      normal: 'Normal Loan',
      commodity: 'Commodity Loan',
      car: 'Car Loan',
    };
    return labels[type] || type;
  }

  getStatusColor(status: string): string {
    const colors: Record<string, string> = {
      active: 'bg-success-100 text-success-800',
      current: 'bg-primary-100 text-primary-800',
      delinquent: 'bg-warning-100 text-warning-800',
      defaulted: 'bg-error-100 text-error-800',
      closed: 'bg-neutral-100 text-neutral-800',
    };
    return colors[status] || 'bg-neutral-100 text-neutral-800';
  }

  formatCurrency(amount: number): string {
    return `₦${amount.toLocaleString('en-NG', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}

export const loanService = new LoanService();
