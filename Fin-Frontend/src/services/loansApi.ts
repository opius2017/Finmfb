import { api } from './api';

interface LoanAccount {
  id: string;
  accountNumber: string;
  customerName: string;
  productName: string;
  principalAmount: number;
  outstandingPrincipal: number;
  outstandingInterest: number;
  interestRate: number;
  disbursementDate: string;
  maturityDate: string;
  status: string;
  classification: string;
  daysPastDue: number;
  provisionAmount: number;
  createdAt: string;
}

interface LoanAccountDetail {
  id: string;
  accountNumber: string;
  customer: {
    id: string;
    name: string;
    email: string;
    phoneNumber: string;
  };
  product: {
    id: string;
    productName: string;
    productType: string;
    interestRate: number;
  };
  principalAmount: number;
  outstandingPrincipal: number;
  outstandingInterest: number;
  interestRate: number;
  tenorDays: number;
  disbursementDate: string;
  maturityDate: string;
  status: string;
  classification: string;
  daysPastDue: number;
  provisionAmount: number;
  purpose?: string;
  repaymentSchedule: Array<{
    installmentNumber: number;
    dueDate: string;
    principalAmount: number;
    interestAmount: number;
    totalAmount: number;
    paidTotal: number;
    outstandingTotal: number;
    status: string;
    daysOverdue: number;
  }>;
  collaterals: Array<{
    collateralType: string;
    description: string;
    estimatedValue: number;
    status: string;
  }>;
  guarantors: Array<{
    firstName: string;
    lastName: string;
    phoneNumber: string;
    relationship?: string;
    status: string;
  }>;
  createdAt: string;
}

interface CreateLoanRequest {
  customerId: string;
  productId: string;
  principalAmount: number;
  interestRate?: number;
  tenorDays: number;
  disbursementDate: string;
  purpose?: string;
}

interface LoansResponse {
  success: boolean;
  data: LoanAccount[];
}

interface LoanDetailResponse {
  success: boolean;
  data: LoanAccountDetail;
}

interface CreateLoanResponse {
  success: boolean;
  data: {
    transactionId: string;
    message: string;
  };
}

export const loansApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getLoans: builder.query<LoansResponse, void>({
      query: () => '/loans',
      providesTags: ['Loan'],
    }),
    getLoan: builder.query<LoanDetailResponse, string>({
      query: (id) => `/loans/${id}`,
      providesTags: ['Loan'],
    }),
    createLoan: builder.mutation<CreateLoanResponse, CreateLoanRequest>({
      query: (loan) => ({
        url: '/loans',
        method: 'POST',
        body: loan,
      }),
      invalidatesTags: ['Loan'],
    }),
    disburseLoan: builder.mutation<CreateLoanResponse, { id: string; amount: number }>({
      query: ({ id, amount }) => ({
        url: `/loans/${id}/disburse`,
        method: 'POST',
        body: { amount },
      }),
      invalidatesTags: ['Loan'],
    }),
    processRepayment: builder.mutation<CreateLoanResponse, { id: string; amount: number }>({
      query: ({ id, amount }) => ({
        url: `/loans/${id}/repayment`,
        method: 'POST',
        body: { amount },
      }),
      invalidatesTags: ['Loan'],
    }),
    getRepaymentSchedule: builder.query<{ success: boolean; data: any[] }, string>({
      query: (id) => `/loans/${id}/schedule`,
      providesTags: ['Loan'],
    }),
    classifyLoans: builder.mutation<CreateLoanResponse, void>({
      query: () => ({
        url: '/loans/classify',
        method: 'POST',
      }),
      invalidatesTags: ['Loan'],
    }),
  }),
  overrideExisting: false,
});

export const { 
  useGetLoansQuery,
  useGetLoanQuery,
  useCreateLoanMutation,
  useDisburseLoanMutation,
  useProcessRepaymentMutation,
  useGetRepaymentScheduleQuery,
  useClassifyLoansMutation
} = loansApi;