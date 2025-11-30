import axios, { AxiosInstance, AxiosError } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('authToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Handle unauthorized - redirect to login
          localStorage.removeItem('authToken');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Authentication
  async login(email: string, password: string) {
    const response = await this.api.post('/auth/login', { email, password });
    if (response.data.token) {
      localStorage.setItem('authToken', response.data.token);
    }
    return response.data;
  }

  async logout() {
    localStorage.removeItem('authToken');
  }

  // Loan Calculator
  async calculateEMI(data: { principal: number; annualInterestRate: number; tenureMonths: number }) {
    const response = await this.api.post('/loan-calculator/calculate-emi', data);
    return response.data;
  }

  async generateAmortizationSchedule(data: {
    principal: number;
    annualInterestRate: number;
    tenureMonths: number;
    startDate: string;
    loanNumber: string;
  }) {
    const response = await this.api.post('/loan-calculator/amortization-schedule', data);
    return response.data;
  }

  // Eligibility
  async checkEligibility(data: {
    memberId: string;
    loanProductId: string;
    requestedAmount: number;
    tenureMonths: number;
  }) {
    const response = await this.api.post('/loan-eligibility/check', data);
    return response.data;
  }

  async getMaximumEligibleAmount(memberId: string, loanProductId: string) {
    const response = await this.api.get(`/loan-eligibility/maximum-amount/${memberId}`, {
      params: { loanProductId },
    });
    return response.data;
  }

  async getEligibilityReport(memberId: string, loanProductId: string) {
    const response = await this.api.get(`/loan-eligibility/report/${memberId}`, {
      params: { loanProductId },
    });
    return response.data;
  }

  // Loan Applications
  async createLoanApplication(data: any) {
    const response = await this.api.post('/loan-applications', data);
    return response.data;
  }

  async getLoanApplication(id: string) {
    const response = await this.api.get(`/loan-applications/${id}`);
    return response.data;
  }

  async getLoanApplications(params?: any) {
    const response = await this.api.get('/loan-applications', { params });
    return response.data;
  }

  async submitLoanApplication(id: string) {
    const response = await this.api.post(`/loan-applications/${id}/submit`);
    return response.data;
  }

  // Guarantors
  async addGuarantor(data: {
    loanApplicationId: string;
    memberId: string;
    guaranteeAmount: number;
    requestedBy: string;
  }) {
    const response = await this.api.post('/guarantors', data);
    return response.data;
  }

  async checkGuarantorEligibility(memberId: string, guaranteeAmount: number) {
    const response = await this.api.get(`/guarantors/eligibility/${memberId}`, {
      params: { guaranteeAmount },
    });
    return response.data;
  }

  async processGuarantorConsent(guarantorId: string, data: {
    consentStatus: string;
    notes?: string;
    processedBy: string;
  }) {
    const response = await this.api.post(`/guarantors/${guarantorId}/consent`, data);
    return response.data;
  }

  async getGuarantorDashboard(memberId: string) {
    const response = await this.api.get(`/guarantors/dashboard/${memberId}`);
    return response.data;
  }

  // Committee
  async getPendingReviews() {
    const response = await this.api.get('/committee/reviews/pending');
    return response.data;
  }

  async getCommitteeDashboard() {
    const response = await this.api.get('/committee/dashboard');
    return response.data;
  }

  async submitCommitteeDecision(reviewId: string, data: {
    decision: string;
    notes?: string;
    reviewedBy: string;
  }) {
    const response = await this.api.post(`/committee/reviews/${reviewId}/decision`, data);
    return response.data;
  }

  async getMemberCreditProfile(memberId: string) {
    const response = await this.api.get(`/committee/credit-profile/${memberId}`);
    return response.data;
  }

  // Deduction Schedules
  async generateDeductionSchedule(data: { month: number; year: number; createdBy: string }) {
    const response = await this.api.post('/deduction-schedules/generate', data);
    return response.data;
  }

  async getDeductionSchedules(params?: any) {
    const response = await this.api.get('/deduction-schedules', { params });
    return response.data;
  }

  async approveDeductionSchedule(id: string, approvedBy: string) {
    const response = await this.api.post(`/deduction-schedules/${id}/approve`, { approvedBy });
    return response.data;
  }

  async exportDeductionSchedule(id: string) {
    const response = await this.api.get(`/deduction-schedules/${id}/export`, {
      responseType: 'blob',
    });
    return response.data;
  }

  // Reconciliation
  async importActualDeductions(scheduleId: string, file: File, importedBy: string) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('scheduleId', scheduleId);
    formData.append('importedBy', importedBy);

    const response = await this.api.post('/deduction-reconciliation/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  }

  async performReconciliation(scheduleId: string) {
    const response = await this.api.post(`/deduction-reconciliation/reconcile/${scheduleId}`);
    return response.data;
  }

  async getReconciliation(id: string) {
    const response = await this.api.get(`/deduction-reconciliation/${id}`);
    return response.data;
  }

  // Commodity Vouchers
  async generateVoucher(data: any) {
    const response = await this.api.post('/commodity-vouchers/generate', data);
    return response.data;
  }

  async validateVoucher(voucherCode: string) {
    const response = await this.api.post(`/commodity-vouchers/${voucherCode}/validate`);
    return response.data;
  }

  async redeemVoucher(voucherCode: string, data: any) {
    const response = await this.api.post(`/commodity-vouchers/${voucherCode}/redeem`, data);
    return response.data;
  }

  // Background Jobs
  async getRecurringJobs() {
    const response = await this.api.get('/admin/background-jobs/recurring');
    return response.data;
  }

  async triggerDelinquencyCheck() {
    const response = await this.api.post('/admin/background-jobs/trigger/delinquency-check');
    return response.data;
  }

  async triggerScheduleGeneration(year?: number, month?: number) {
    const url = year && month
      ? `/admin/background-jobs/trigger/schedule-generation/${year}/${month}`
      : '/admin/background-jobs/trigger/schedule-generation';
    const response = await this.api.post(url);
    return response.data;
  }

  // Reports
  async getLoanPortfolioReport(params?: any) {
    const response = await this.api.get('/reports/loan-portfolio', { params });
    return response.data;
  }

  async getDelinquencyReport(params?: any) {
    const response = await this.api.get('/reports/delinquency', { params });
    return response.data;
  }

  async exportReport(reportType: string, params?: any) {
    const response = await this.api.get(`/reports/${reportType}/export`, {
      params,
      responseType: 'blob',
    });
    return response.data;
  }
}

export const apiService = new ApiService();
export default apiService;
