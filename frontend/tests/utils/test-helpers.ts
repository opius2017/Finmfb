import { vi } from 'vitest';
import { mockDataFactory } from './mock-data';

/**
 * Test Helper Functions
 */

/**
 * Wait for a specific amount of time
 */
export const wait = (ms: number): Promise<void> => {
  return new Promise((resolve) => setTimeout(resolve, ms));
};

/**
 * Wait for an element to appear in the DOM
 */
export const waitForElement = async (
  selector: string,
  timeout = 3000
): Promise<Element | null> => {
  const startTime = Date.now();
  
  while (Date.now() - startTime < timeout) {
    const element = document.querySelector(selector);
    if (element) return element;
    await wait(50);
  }
  
  return null;
};

/**
 * Mock API response helper
 */
export const mockApiResponse = <T>(data: T, delay = 0): Promise<T> => {
  return new Promise((resolve) => {
    setTimeout(() => resolve(data), delay);
  });
};

/**
 * Mock API error helper
 */
export const mockApiError = (message: string, status = 500, delay = 0): Promise<never> => {
  return new Promise((_, reject) => {
    setTimeout(() => {
      reject({
        response: {
          data: { message },
          status,
        },
      });
    }, delay);
  });
};

/**
 * Create mock API service
 */
export const createMockApiService = () => {
  return {
    // Authentication
    login: vi.fn().mockResolvedValue({
      user: {
        id: 'test-user-1',
        email: 'test@example.com',
        name: 'Test User',
        role: 'MEMBER',
      },
      token: 'test-token',
    }),
    logout: vi.fn().mockResolvedValue(undefined),

    // Loan Calculator
    calculateEMI: vi.fn().mockResolvedValue({
      monthlyEMI: 44244.42,
      totalInterest: 30933.04,
      totalPayment: 530933.04,
    }),
    generateAmortizationSchedule: vi.fn().mockResolvedValue({
      loanNumber: 'LOAN00000001',
      principal: 500000,
      annualInterestRate: 12,
      tenureMonths: 12,
      monthlyEMI: 44244.42,
      totalInterest: 30933.04,
      totalPayment: 530933.04,
      installments: mockDataFactory.createDeductionSchedules(12),
    }),

    // Eligibility
    checkEligibility: vi.fn().mockResolvedValue({
      isEligible: true,
      eligibilityScore: 85,
      maximumAmount: 1000000,
      reasons: [],
    }),
    getMaximumEligibleAmount: vi.fn().mockResolvedValue({
      maximumAmount: 1000000,
    }),
    getEligibilityReport: vi.fn().mockResolvedValue({
      memberId: 'MEM-1',
      eligibilityScore: 85,
      factors: [],
    }),

    // Loan Applications
    createLoanApplication: vi.fn().mockResolvedValue(
      mockDataFactory.createLoanApplication()
    ),
    getLoanApplication: vi.fn().mockResolvedValue(
      mockDataFactory.createLoanApplication()
    ),
    getLoanApplications: vi.fn().mockResolvedValue({
      data: mockDataFactory.createLoanApplications(10),
      total: 10,
      page: 1,
      pageSize: 10,
    }),
    submitLoanApplication: vi.fn().mockResolvedValue({
      success: true,
      message: 'Application submitted successfully',
    }),

    // Guarantors
    addGuarantor: vi.fn().mockResolvedValue(
      mockDataFactory.createGuarantorRequest()
    ),
    checkGuarantorEligibility: vi.fn().mockResolvedValue({
      isEligible: true,
      availableCapacity: 500000,
    }),
    processGuarantorConsent: vi.fn().mockResolvedValue({
      success: true,
      message: 'Consent processed successfully',
    }),
    getGuarantorDashboard: vi.fn().mockResolvedValue({
      pendingRequests: mockDataFactory.createGuarantorRequests(5),
      activeGuarantees: mockDataFactory.createGuarantorRequests(3, { status: 'APPROVED' }),
      totalExposure: 1500000,
      availableCapacity: 500000,
    }),

    // Committee
    getPendingReviews: vi.fn().mockResolvedValue(
      mockDataFactory.createCommitteeReviews(5)
    ),
    getCommitteeDashboard: vi.fn().mockResolvedValue({
      pendingReviews: mockDataFactory.createCommitteeReviews(5),
      recentDecisions: mockDataFactory.createCommitteeReviews(10, { decision: 'APPROVED' }),
      statistics: {
        totalReviewed: 100,
        approved: 85,
        rejected: 15,
      },
    }),
    submitCommitteeDecision: vi.fn().mockResolvedValue({
      success: true,
      message: 'Decision submitted successfully',
    }),
    getMemberCreditProfile: vi.fn().mockResolvedValue({
      member: mockDataFactory.createMember(),
      creditScore: 750,
      loanHistory: mockDataFactory.createLoanApplications(5),
    }),

    // Deduction Schedules
    generateDeductionSchedule: vi.fn().mockResolvedValue({
      id: 'schedule-1',
      month: 1,
      year: 2024,
      totalAmount: 500000,
      totalMembers: 50,
      status: 'DRAFT',
    }),
    getDeductionSchedules: vi.fn().mockResolvedValue({
      data: mockDataFactory.createDeductionSchedules(20),
      total: 20,
    }),
    approveDeductionSchedule: vi.fn().mockResolvedValue({
      success: true,
      message: 'Schedule approved successfully',
    }),
    exportDeductionSchedule: vi.fn().mockResolvedValue(new Blob()),

    // Reconciliation
    importActualDeductions: vi.fn().mockResolvedValue({
      success: true,
      imported: 50,
      failed: 0,
    }),
    performReconciliation: vi.fn().mockResolvedValue({
      matched: 45,
      unmatched: 5,
      discrepancies: 2,
    }),
    getReconciliation: vi.fn().mockResolvedValue({
      scheduleId: 'schedule-1',
      matched: mockDataFactory.createTransactions(45, { status: 'MATCHED' }),
      unmatched: mockDataFactory.createTransactions(5, { status: 'UNMATCHED' }),
    }),

    // Commodity Vouchers
    generateVoucher: vi.fn().mockResolvedValue(
      mockDataFactory.createCommodityVoucher()
    ),
    validateVoucher: vi.fn().mockResolvedValue({
      isValid: true,
      voucher: mockDataFactory.createCommodityVoucher(),
    }),
    redeemVoucher: vi.fn().mockResolvedValue({
      success: true,
      message: 'Voucher redeemed successfully',
    }),

    // Reports
    getLoanPortfolioReport: vi.fn().mockResolvedValue({
      totalLoans: 100,
      totalDisbursed: 50000000,
      activeLoans: 85,
      completedLoans: 15,
      data: mockDataFactory.createLoanApplications(10),
    }),
    getDelinquencyReport: vi.fn().mockResolvedValue({
      totalDelinquent: 10,
      totalDelinquentAmount: 5000000,
      data: mockDataFactory.createLoanApplications(10),
    }),
    exportReport: vi.fn().mockResolvedValue(new Blob()),

    // Background Jobs
    getRecurringJobs: vi.fn().mockResolvedValue([]),
    triggerDelinquencyCheck: vi.fn().mockResolvedValue({ success: true }),
    triggerScheduleGeneration: vi.fn().mockResolvedValue({ success: true }),
  };
};

/**
 * Setup mock localStorage
 */
export const setupMockLocalStorage = () => {
  const store: Record<string, string> = {};

  return {
    getItem: vi.fn((key: string) => store[key] || null),
    setItem: vi.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: vi.fn((key: string) => {
      delete store[key];
    }),
    clear: vi.fn(() => {
      Object.keys(store).forEach((key) => delete store[key]);
    }),
  };
};

/**
 * Mock file for upload testing
 */
export const createMockFile = (
  name: string,
  size: number,
  type: string
): File => {
  const blob = new Blob(['a'.repeat(size)], { type });
  return new File([blob], name, { type });
};

/**
 * Simulate user typing
 */
export const typeIntoInput = async (
  input: HTMLInputElement,
  text: string
): Promise<void> => {
  input.focus();
  for (const char of text) {
    input.value += char;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await wait(10);
  }
};

/**
 * Get form data as object
 */
export const getFormData = (form: HTMLFormElement): Record<string, any> => {
  const formData = new FormData(form);
  const data: Record<string, any> = {};
  
  formData.forEach((value, key) => {
    data[key] = value;
  });
  
  return data;
};

/**
 * Check if element is visible
 */
export const isElementVisible = (element: HTMLElement): boolean => {
  return !!(
    element.offsetWidth ||
    element.offsetHeight ||
    element.getClientRects().length
  );
};

/**
 * Get computed style property
 */
export const getStyleProperty = (
  element: HTMLElement,
  property: string
): string => {
  return window.getComputedStyle(element).getPropertyValue(property);
};

/**
 * Check color contrast ratio
 */
export const getContrastRatio = (
  foreground: string,
  background: string
): number => {
  // Simplified contrast ratio calculation
  // In real implementation, use a proper color contrast library
  const getLuminance = (color: string): number => {
    // This is a simplified version
    // Real implementation should parse RGB values properly
    return 0.5;
  };

  const l1 = getLuminance(foreground);
  const l2 = getLuminance(background);
  const lighter = Math.max(l1, l2);
  const darker = Math.min(l1, l2);

  return (lighter + 0.05) / (darker + 0.05);
};

/**
 * Simulate network delay
 */
export const simulateNetworkDelay = (min = 100, max = 500): Promise<void> => {
  const delay = Math.random() * (max - min) + min;
  return wait(delay);
};

/**
 * Create mock intersection observer
 */
export const createMockIntersectionObserver = () => {
  return class MockIntersectionObserver {
    observe = vi.fn();
    disconnect = vi.fn();
    unobserve = vi.fn();
  };
};

/**
 * Reset all mocks
 */
export const resetAllMocks = () => {
  vi.clearAllMocks();
  vi.resetAllMocks();
  localStorage.clear();
  sessionStorage.clear();
};
