// @ts-nocheck
import { api } from './api';

interface MakerCheckerTransaction {
  id: string;
  transactionReference: string;
  entityName: string;
  operation: string;
  amount?: number;
  priority: number;
  makerName: string;
  makerTimestamp: string;
  checkerName?: string;
  checkerTimestamp?: string;
  status: string;
  expiryDate?: string;
  description: string;
}

interface MakerCheckerTransactionDetail {
  id: string;
  transactionReference: string;
  entityName: string;
  entityId: string;
  operation: string;
  requestData: string;
  amount?: number;
  priority: number;
  makerName: string;
  makerTimestamp: string;
  checkerName?: string;
  checkerTimestamp?: string;
  checkerComments?: string;
  status: string;
  rejectionReason?: string;
  expiryDate?: string;
  description: string;
}

interface MakerCheckerStatistics {
  totalPendingApprovals: number;
  myPendingTransactions: number;
  approvedToday: number;
  rejectedToday: number;
  highPriorityPending: number;
}

interface MakerCheckerResponse {
  success: boolean;
  data: MakerCheckerTransaction[];
}

interface MakerCheckerDetailResponse {
  success: boolean;
  data: MakerCheckerTransactionDetail;
}

interface MakerCheckerStatisticsResponse {
  success: boolean;
  data: MakerCheckerStatistics;
}

export const makerCheckerApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getPendingTransactions: builder.query<MakerCheckerResponse, void>({
      query: () => '/makerchecker/pending',
      providesTags: ['MakerChecker'],
    }),
    getTransaction: builder.query<MakerCheckerDetailResponse, string>({
      query: (id) => `/makerchecker/${id}`,
      providesTags: ['MakerChecker'],
    }),
    getMyTransactions: builder.query<MakerCheckerResponse, void>({
      query: () => '/makerchecker/my-transactions',
      providesTags: ['MakerChecker'],
    }),
    getStatistics: builder.query<MakerCheckerStatisticsResponse, void>({
      query: () => '/makerchecker/statistics',
      providesTags: ['MakerChecker'],
    }),
    approveTransaction: builder.mutation<{ success: boolean; data: any }, { id: string; comments?: string }>({
      query: ({ id, comments }) => ({
        url: `/makerchecker/${id}/approve`,
        method: 'POST',
        body: { comments },
      }),
      invalidatesTags: ['MakerChecker'],
    }),
    rejectTransaction: builder.mutation<{ success: boolean; data: any }, { id: string; rejectionReason: string }>({
      query: ({ id, rejectionReason }) => ({
        url: `/makerchecker/${id}/reject`,
        method: 'POST',
        body: { rejectionReason },
      }),
      invalidatesTags: ['MakerChecker'],
    }),
  }),
  overrideExisting: false,
});

export const { 
  useGetPendingTransactionsQuery,
  useGetTransactionQuery,
  useGetMyTransactionsQuery,
  useGetStatisticsQuery,
  useApproveTransactionMutation,
  useRejectTransactionMutation
} = makerCheckerApi;