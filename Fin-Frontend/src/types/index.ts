export interface User {
  userId: string;
  username: string;
  email: string;
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
  firstName?: string;
  lastName?: string;
  companyName?: string;
  name: string; // Computed property: firstName + lastName for individuals, companyName for companies
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