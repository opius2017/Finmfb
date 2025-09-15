import { api } from './api';

export interface Account {
  id: string;
  code: string;
  name: string;
  type: 'asset' | 'liability' | 'equity' | 'revenue' | 'expense';
  parentId?: string;
  balance: number;
  children?: Account[];
  level: number;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface AccountsResponse {
  success: boolean;
  data: Account[];
}

interface AccountResponse {
  success: boolean;
  data: Account;
}

interface CreateAccountRequest {
  code: string;
  name: string;
  type: Account['type'];
  parentId?: string;
  description?: string;
}

interface UpdateAccountRequest extends Partial<CreateAccountRequest> {
  id: string;
}

export const accountsApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getAccounts: builder.query<Account[], void>({
      query: () => '/accounts',
      transformResponse: (response: AccountsResponse) => response.data,
    }),
    getAccount: builder.query<Account, string>({
      query: (id) => `/accounts/${id}`,
      transformResponse: (response: AccountResponse) => response.data,
    }),
    createAccount: builder.mutation<Account, CreateAccountRequest>({
      query: (data) => ({
        url: '/accounts',
        method: 'POST',
        body: data,
      }),
      transformResponse: (response: AccountResponse) => response.data,
    }),
    updateAccount: builder.mutation<Account, UpdateAccountRequest>({
      query: ({ id, ...data }) => ({
        url: `/accounts/${id}`,
        method: 'PUT',
        body: data,
      }),
      transformResponse: (response: AccountResponse) => response.data,
    }),
    deleteAccount: builder.mutation<void, string>({
      query: (id) => ({
        url: `/accounts/${id}`,
        method: 'DELETE',
      }),
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetAccountsQuery,
  useGetAccountQuery,
  useCreateAccountMutation,
  useUpdateAccountMutation,
  useDeleteAccountMutation,
} = accountsApi;