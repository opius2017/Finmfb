import { api } from './api';

interface DashboardOverview {
  success: boolean;
  data: {
    totalCustomers: number;
    totalDepositAccounts: number;
    totalDeposits: number;
    totalTransactionsToday: number;
  };
}

interface ExecutiveDashboard {
  success: boolean;
  data: {
    totalAssets: number;
    totalLiabilities: number;
    netWorth: number;
    monthlyRevenue: number;
    monthlyExpenses: number;
    netIncome: number;
    customerGrowthRate: number;
    portfolioAtRisk: number;
    capitalAdequacyRatio: number;
    liquidityRatio: number;
    revenueByMonth: Array<{ month: string; value: number }>;
    topPerformingBranches: Array<{ branchName: string; performance: number }>;
    riskMetrics: {
      creditRisk: number;
      operationalRisk: number;
      marketRisk: number;
    };
  };
}

interface LoanDashboard {
  success: boolean;
  data: {
    totalPortfolio: number;
    activeLoans: number;
    performingLoans: number;
    nonPerformingLoans: number;
    portfolioAtRisk: number;
    averageInterestRate: number;
    totalProvisions: number;
    loansByClassification: Array<{ classification: string; count: number; amount: number }>;
    monthlyDisbursements: Array<{ month: string; value: number }>;
    repaymentTrends: Array<{ month: string; value: number }>;
  };
}

interface DepositDashboard {
  success: boolean;
  data: {
    totalDeposits: number;
    activeAccounts: number;
    newAccountsThisMonth: number;
    averageAccountBalance: number;
    totalInterestPaid: number;
    dormantAccounts: number;
    depositsByProduct: Array<{ productName: string; amount: number; count: number }>;
    monthlyGrowth: Array<{ month: string; value: number }>;
    transactionVolume: Array<{ month: string; value: number }>;
  };
}

interface InventoryDashboard {
  success: boolean;
  data: {
    totalItems: number;
    totalValue: number;
    lowStockItems: number;
    outOfStockItems: number;
    topSellingItems: Array<{ itemName: string; quantity: number; value: number }>;
    stockMovements: Array<{ month: string; value: number }>;
    categoryBreakdown: Array<{ category: string; count: number; value: number }>;
  };
}

interface PayrollDashboard {
  success: boolean;
  data: {
    totalEmployees: number;
    monthlyPayroll: number;
    averageSalary: number;
    totalDeductions: number;
    payrollByDepartment: Array<{ department: string; employeeCount: number; totalPayroll: number }>;
    monthlyTrends: Array<{ month: string; value: number }>;
    statutoryCompliance: {
      payeCompliance: number;
      pensionCompliance: number;
      nhfCompliance: number;
    };
  };
}

export interface DepositSweepResult {
  accountNumber: string;
  amountSwept: number;
  sweepType: string;
  status: string;
  message?: string;
}

export interface DormancyTrackingResult {
  accountNumber: string;
  lastTransactionDate?: string;
  status: string;
  message?: string;
}

export const dashboardApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getDashboardOverview: builder.query<DashboardOverview, void>({
      query: () => '/dashboard/overview',
  providesTags: [{ type: 'Dashboard' }],
    }),
    getExecutiveDashboard: builder.query<ExecutiveDashboard, void>({
      query: () => '/dashboard/executive',
  providesTags: [{ type: 'Dashboard' }],
    }),
    getLoanDashboard: builder.query<LoanDashboard, void>({
      query: () => '/dashboard/loans',
  providesTags: [{ type: 'Dashboard' }],
    }),
    getDepositDashboard: builder.query<DepositDashboard, void>({
      query: () => '/dashboard/deposits',
  providesTags: [{ type: 'Dashboard' }],
    }),
    getInventoryDashboard: builder.query<InventoryDashboard, void>({
      query: () => '/dashboard/inventory',
  providesTags: [{ type: 'Dashboard' }],
    }),
    getPayrollDashboard: builder.query<PayrollDashboard, void>({
      query: () => '/dashboard/payroll',
  providesTags: [{ type: 'Dashboard' }],
    }),
    runDepositSweeps: builder.mutation<DepositSweepResult[], void>({
      query: () => ({
        url: '/deposit-operations/run-sweeps',
        method: 'POST',
      }),
    }),
    trackDormancy: builder.mutation<DormancyTrackingResult[], { dormancyDays?: number }>({
      query: ({ dormancyDays = 90 } = {}) => ({
        url: `/deposit-operations/track-dormancy?dormancyDays=${dormancyDays}`,
        method: 'POST',
      }),
    }),
  }),
  overrideExisting: false,
});

export const { 
  useGetDashboardOverviewQuery,
  useGetExecutiveDashboardQuery,
  useGetLoanDashboardQuery,
  useGetDepositDashboardQuery,
  useGetInventoryDashboardQuery,
  useGetPayrollDashboardQuery,
  useRunDepositSweepsMutation,
  useTrackDormancyMutation,
} = dashboardApi;