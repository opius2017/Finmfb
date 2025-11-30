/**
 * Mock Data Factory for Test Fixtures
 * Provides realistic test data for all entities in the system
 */

export interface Member {
  id: string;
  memberNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  employeeId: string;
  department: string;
  monthlyIncome: number;
  joinDate: string;
  status: 'ACTIVE' | 'INACTIVE' | 'SUSPENDED';
}

export interface LoanApplication {
  id: string;
  loanNumber: string;
  memberId: string;
  memberName: string;
  memberNumber: string;
  loanType: 'PERSONAL' | 'EMERGENCY' | 'COMMODITY' | 'SPECIAL';
  amount: number;
  tenure: number;
  interestRate: number;
  purpose: string;
  status: 'DRAFT' | 'PENDING' | 'IN_REVIEW' | 'APPROVED' | 'REJECTED' | 'DISBURSED';
  applicationDate: string;
  reviewDate?: string;
  approvalDate?: string;
  disbursementDate?: string;
}

export interface GuarantorRequest {
  id: string;
  loanApplicationId: string;
  loanNumber: string;
  applicantName: string;
  guarantorId: string;
  guarantorName: string;
  guarantorMemberNumber: string;
  amount: number;
  status: 'PENDING' | 'APPROVED' | 'REJECTED';
  requestDate: string;
  responseDate?: string;
  comments?: string;
}

export interface DeductionSchedule {
  id: string;
  loanId: string;
  loanNumber: string;
  memberId: string;
  memberName: string;
  installmentNumber: number;
  dueDate: string;
  amount: number;
  principalAmount: number;
  interestAmount: number;
  status: 'PENDING' | 'PROCESSED' | 'FAILED' | 'REVERSED';
  processedDate?: string;
}

export interface CommodityVoucher {
  id: string;
  voucherNumber: string;
  loanId: string;
  loanNumber: string;
  memberId: string;
  memberName: string;
  amount: number;
  vendor: string;
  commodityType: string;
  status: 'ISSUED' | 'REDEEMED' | 'EXPIRED' | 'CANCELLED';
  issueDate: string;
  expiryDate: string;
  redemptionDate?: string;
  qrCode: string;
}

export interface CommitteeReview {
  id: string;
  loanApplicationId: string;
  loanNumber: string;
  applicantName: string;
  amount: number;
  reviewerId: string;
  reviewerName: string;
  decision: 'PENDING' | 'APPROVED' | 'REJECTED';
  comments: string;
  reviewDate: string;
  votingRound: number;
}

export interface Transaction {
  id: string;
  transactionNumber: string;
  memberId: string;
  memberName: string;
  amount: number;
  type: 'DEDUCTION' | 'PAYMENT' | 'DISBURSEMENT' | 'REFUND';
  status: 'PENDING' | 'MATCHED' | 'UNMATCHED' | 'RECONCILED';
  transactionDate: string;
  loanId?: string;
  loanNumber?: string;
}

export interface Report {
  id: string;
  reportType: 'LOAN_PORTFOLIO' | 'DELINQUENCY' | 'DISBURSEMENT' | 'COLLECTION' | 'MEMBER_SUMMARY';
  title: string;
  generatedBy: string;
  generatedDate: string;
  startDate: string;
  endDate: string;
  format: 'PDF' | 'EXCEL' | 'CSV';
  status: 'GENERATING' | 'COMPLETED' | 'FAILED';
}

/**
 * Mock Data Factory Class
 */
class MockDataFactory {
  private counter = 1;

  private getNextId(): string {
    return `TEST-${this.counter++}`;
  }

  createMember(overrides?: Partial<Member>): Member {
    const id = this.getNextId();
    return {
      id,
      memberNumber: `MEM${id.padStart(6, '0')}`,
      firstName: 'John',
      lastName: 'Doe',
      email: `john.doe${id}@example.com`,
      phone: '+234-800-000-0000',
      employeeId: `EMP${id}`,
      department: 'Engineering',
      monthlyIncome: 500000,
      joinDate: '2023-01-15',
      status: 'ACTIVE',
      ...overrides,
    };
  }

  createLoanApplication(overrides?: Partial<LoanApplication>): LoanApplication {
    const id = this.getNextId();
    return {
      id,
      loanNumber: `LOAN${id.padStart(8, '0')}`,
      memberId: `MEM-${id}`,
      memberName: 'John Doe',
      memberNumber: `MEM${id.padStart(6, '0')}`,
      loanType: 'PERSONAL',
      amount: 500000,
      tenure: 12,
      interestRate: 12,
      purpose: 'Personal development',
      status: 'PENDING',
      applicationDate: new Date().toISOString(),
      ...overrides,
    };
  }

  createGuarantorRequest(overrides?: Partial<GuarantorRequest>): GuarantorRequest {
    const id = this.getNextId();
    return {
      id,
      loanApplicationId: `LOAN-${id}`,
      loanNumber: `LOAN${id.padStart(8, '0')}`,
      applicantName: 'John Doe',
      guarantorId: `MEM-G${id}`,
      guarantorName: 'Jane Smith',
      guarantorMemberNumber: `MEM${(parseInt(id) + 100).toString().padStart(6, '0')}`,
      amount: 500000,
      status: 'PENDING',
      requestDate: new Date().toISOString(),
      ...overrides,
    };
  }

  createDeductionSchedule(overrides?: Partial<DeductionSchedule>): DeductionSchedule {
    const id = this.getNextId();
    const dueDate = new Date();
    dueDate.setMonth(dueDate.getMonth() + 1);
    
    return {
      id,
      loanId: `LOAN-${id}`,
      loanNumber: `LOAN${id.padStart(8, '0')}`,
      memberId: `MEM-${id}`,
      memberName: 'John Doe',
      installmentNumber: 1,
      dueDate: dueDate.toISOString(),
      amount: 45000,
      principalAmount: 40000,
      interestAmount: 5000,
      status: 'PENDING',
      ...overrides,
    };
  }

  createCommodityVoucher(overrides?: Partial<CommodityVoucher>): CommodityVoucher {
    const id = this.getNextId();
    const issueDate = new Date();
    const expiryDate = new Date();
    expiryDate.setMonth(expiryDate.getMonth() + 3);
    
    return {
      id,
      voucherNumber: `VCH${id.padStart(8, '0')}`,
      loanId: `LOAN-${id}`,
      loanNumber: `LOAN${id.padStart(8, '0')}`,
      memberId: `MEM-${id}`,
      memberName: 'John Doe',
      amount: 300000,
      vendor: 'ABC Commodities Ltd',
      commodityType: 'Agricultural Inputs',
      status: 'ISSUED',
      issueDate: issueDate.toISOString(),
      expiryDate: expiryDate.toISOString(),
      qrCode: `QR-${id}`,
      ...overrides,
    };
  }

  createCommitteeReview(overrides?: Partial<CommitteeReview>): CommitteeReview {
    const id = this.getNextId();
    return {
      id,
      loanApplicationId: `LOAN-${id}`,
      loanNumber: `LOAN${id.padStart(8, '0')}`,
      applicantName: 'John Doe',
      amount: 500000,
      reviewerId: `REV-${id}`,
      reviewerName: 'Committee Member',
      decision: 'PENDING',
      comments: 'Under review',
      reviewDate: new Date().toISOString(),
      votingRound: 1,
      ...overrides,
    };
  }

  createTransaction(overrides?: Partial<Transaction>): Transaction {
    const id = this.getNextId();
    return {
      id,
      transactionNumber: `TXN${id.padStart(10, '0')}`,
      memberId: `MEM-${id}`,
      memberName: 'John Doe',
      amount: 45000,
      type: 'DEDUCTION',
      status: 'UNMATCHED',
      transactionDate: new Date().toISOString(),
      ...overrides,
    };
  }

  createReport(overrides?: Partial<Report>): Report {
    const id = this.getNextId();
    const startDate = new Date();
    startDate.setMonth(startDate.getMonth() - 1);
    
    return {
      id,
      reportType: 'LOAN_PORTFOLIO',
      title: 'Monthly Loan Portfolio Report',
      generatedBy: 'Admin User',
      generatedDate: new Date().toISOString(),
      startDate: startDate.toISOString(),
      endDate: new Date().toISOString(),
      format: 'PDF',
      status: 'COMPLETED',
      ...overrides,
    };
  }

  // Batch creation methods
  createMembers(count: number, overrides?: Partial<Member>): Member[] {
    return Array.from({ length: count }, () => this.createMember(overrides));
  }

  createLoanApplications(count: number, overrides?: Partial<LoanApplication>): LoanApplication[] {
    return Array.from({ length: count }, () => this.createLoanApplication(overrides));
  }

  createGuarantorRequests(count: number, overrides?: Partial<GuarantorRequest>): GuarantorRequest[] {
    return Array.from({ length: count }, () => this.createGuarantorRequest(overrides));
  }

  createDeductionSchedules(count: number, overrides?: Partial<DeductionSchedule>): DeductionSchedule[] {
    return Array.from({ length: count }, () => this.createDeductionSchedule(overrides));
  }

  createCommodityVouchers(count: number, overrides?: Partial<CommodityVoucher>): CommodityVoucher[] {
    return Array.from({ length: count }, () => this.createCommodityVoucher(overrides));
  }

  createCommitteeReviews(count: number, overrides?: Partial<CommitteeReview>): CommitteeReview[] {
    return Array.from({ length: count }, () => this.createCommitteeReview(overrides));
  }

  createTransactions(count: number, overrides?: Partial<Transaction>): Transaction[] {
    return Array.from({ length: count }, () => this.createTransaction(overrides));
  }

  createReports(count: number, overrides?: Partial<Report>): Report[] {
    return Array.from({ length: count }, () => this.createReport(overrides));
  }

  // Reset counter for test isolation
  reset(): void {
    this.counter = 1;
  }
}

// Export singleton instance
export const mockDataFactory = new MockDataFactory();

// Export class for custom instances
export { MockDataFactory };
