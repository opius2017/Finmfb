// @ts-nocheck
import { api } from './api';

interface Employee {
  id: string;
  employeeNumber: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  department: string;
  position: string;
  basicSalary: number;
  grossSalary: number;
  status: string;
  hireDate: string;
  createdAt: string;
}

interface PayrollEntry {
  id: string;
  payrollNumber: string;
  employeeName: string;
  employeeNumber: string;
  department: string;
  position: string;
  basicSalary: number;
  grossEarnings: number;
  totalDeductions: number;
  netPay: number;
  payPeriod: string;
  status: string;
  payrollDate: string;
  createdAt: string;
}

interface PayrollSummaryReport {
  period: string;
  totalEmployees: number;
  totalGrossEarnings: number;
  totalDeductions: number;
  totalNetPay: number;
  totalPAYE: number;
  totalPension: number;
  totalNHF: number;
  departmentBreakdown: Array<{
    department: string;
    employeeCount: number;
    totalGross: number;
    totalNet: number;
  }>;
}

interface EmployeesResponse {
  success: boolean;
  data: Employee[];
}

interface PayrollResponse {
  success: boolean;
  data: PayrollEntry[];
}

interface PayrollSummaryResponse {
  success: boolean;
  data: PayrollSummaryReport;
}

export const payrollApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getEmployees: builder.query<EmployeesResponse, void>({
      query: () => '/payroll/employees',
      providesTags: ['Employee'],
    }),
    getEmployee: builder.query<{ success: boolean; data: any }, string>({
      query: (id) => `/payroll/employees/${id}`,
      providesTags: ['Employee'],
    }),
    createEmployee: builder.mutation<{ success: boolean; data: Employee }, any>({
      query: (employee) => ({
        url: '/payroll/employees',
        method: 'POST',
        body: employee,
      }),
      invalidatesTags: ['Employee'],
    }),
    getPayrollEntries: builder.query<PayrollResponse, string | undefined>({
      query: (period) => ({
        url: '/payroll/payroll',
        params: period ? { period } : {},
      }),
      providesTags: ['Payroll'],
    }),
    processPayroll: builder.mutation<{ success: boolean; data: any }, any>({
      query: (request) => ({
        url: '/payroll/payroll/process',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['Payroll'],
    }),
    approvePayroll: builder.mutation<{ success: boolean; data: any }, string>({
      query: (id) => ({
        url: `/payroll/payroll/${id}/approve`,
        method: 'POST',
      }),
      invalidatesTags: ['Payroll'],
    }),
    batchApprovePayroll: builder.mutation<{ success: boolean; data: any }, { payrollIds: string[] }>({
      query: (request) => ({
        url: '/payroll/payroll/batch-approve',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['Payroll'],
    }),
    getPayrollSummary: builder.query<PayrollSummaryResponse, string>({
      query: (period) => `/payroll/reports/payroll-summary?period=${period}`,
      providesTags: ['Payroll'],
    }),
  }),
  overrideExisting: false,
});

export const { 
  useGetEmployeesQuery,
  useGetEmployeeQuery,
  useCreateEmployeeMutation,
  useGetPayrollEntriesQuery,
  useProcessPayrollMutation,
  useApprovePayrollMutation,
  useBatchApprovePayrollMutation,
  useGetPayrollSummaryQuery
} = payrollApi;