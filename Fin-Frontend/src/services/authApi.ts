import { api } from './api';

interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
  mfaCode?: string;
  mfaToken?: string;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
  mfaEnabled: boolean;
}

interface LoginResponse {
  success: boolean;
  message?: string;
  requiresMfa?: boolean;
  mfaToken?: string;
  data?: {
    token: string;
    expiryDate: string;
    user: User;
    tenant: {
      id: string;
      name: string;
      code: string;
      logoUrl: string;
      organizationType: string;
    };
  };
}

interface MfaSetupResponse {
  success: boolean;
  data: {
    secret: string;
    qrCode: string;
    backupCodes: string[];
  };
}

interface VerifyMfaResponse {
  success: boolean;
  data: {
    verified: boolean;
  };
}

export const authApi = api.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        body: credentials,
      }),
    }),
    setupMfa: builder.mutation<MfaSetupResponse, void>({
      query: () => ({
        url: '/auth/mfa/setup',
        method: 'POST',
      }),
    }),
    verifyMfa: builder.mutation<VerifyMfaResponse, { code: string; setupToken?: string }>({
      query: (data) => ({
        url: '/auth/mfa/verify',
        method: 'POST',
        body: data,
      }),
    }),
    disableMfa: builder.mutation<{ success: boolean }, { code: string }>({
      query: (data) => ({
        url: '/auth/mfa/disable',
        method: 'POST',
        body: data,
      }),
    }),
    validateMfaBackupCode: builder.mutation<
      { success: boolean; token: string },
      { code: string; mfaToken: string }
    >({
      query: (data) => ({
        url: '/auth/mfa/validate-backup',
        method: 'POST',
        body: data,
      }),
    }),
  }),
  overrideExisting: false,
});

export const {
  useLoginMutation,
  useSetupMfaMutation,
  useVerifyMfaMutation,
  useDisableMfaMutation,
  useValidateMfaBackupCodeMutation,
} = authApi;