export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
}

export interface Tenant {
  id: string;
  name: string;
  code: string;
  logoUrl: string;
  organizationType: string;
}

export interface Customer {
  id: string;
  customerNumber: string;
  name: string;
  email: string;
  phoneNumber: string;
  customerType: string;
  status: string;
  createdAt: string;
}

export interface CustomerDetail {
  id: string;
  customerNumber: string;
  customerType: number;
  firstName?: string;
  lastName?: string;
  middleName?: string;
  companyName?: string;
  email: string;
  phoneNumber: string;
  address?: string;
  city?: string;
  state?: string;
  status: number;
  bvn?: string;
  nin?: string;
  createdAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface DashboardStats {
  totalCustomers: number;
  totalDepositAccounts: number;
  totalDeposits: number;
  totalTransactionsToday: number;
}