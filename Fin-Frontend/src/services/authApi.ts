// @ts-nocheck
import { api } from './api';

// Auth interfaces
interface LoginRequest {
  email: string;
  password: string;
}

interface MfaLoginRequest {
  userId: string;
  challengeId: string;
  code: string;
  rememberDevice: boolean;
}

interface MfaBackupLoginRequest {
  userId: string;
  backupCode: string;
  rememberDevice: boolean;
}

interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
}

interface User {
  userId: string;
  username: string;
  email: string;
  roles: string[];
  token?: string;
}

interface LoginResponse {
  success: boolean;
  message: string;
  data?: {
    userId: string;
    username: string;
    email: string;
    token?: string;
    requiresMfa: boolean;
    mfaType?: string;
    mfaChallengeId?: string;
    mfaChallenge?: {
      challengeId: string;
      expiresAt: string;
    };
    roles: string[];
  };
}

// MFA interfaces
interface MfaSetupResponse {
  success: boolean;
  message: string;
  data: {
    secret: string;
    qrCodeUrl: string;
    backupCodes: string[];
  };
}

interface MfaVerifyRequest {
  code: string;
  secret: string;
}

interface MfaVerifyChallengeRequest {
  challengeId: string;
  code: string;
}

interface MfaChallengeRequest {
  operation: string;
}

interface MfaChallengeResponse {
  success: boolean;
  challengeId: string;
  expiresAt: string;
}

interface MfaStatusResponse {
  success: boolean;
  message: string;
  data: {
    isEnabled: boolean;
    lastVerified: string | null;
  };
}

interface MfaDeviceInfo {
  name: string;
  type: string;
  browser: string;
  operatingSystem: string;
  ipAddress?: string;
}

interface TrustedDevice {
  id: string;
  deviceName: string;
  deviceType: string;
  browser: string;
  operatingSystem: string;
  lastUsed: string;
  location: string;
  ipAddress: string;
  isCurrent: boolean;
}

interface SecurityActivity {
  id: string;
  userId: string;
  eventType: string;
  timestamp: string;
  ipAddress: string;
  location: string;
  deviceInfo: string;
  status: string;
}

interface SecurityPreferences {
  emailNotificationsEnabled: boolean;
  loginNotificationsEnabled: boolean;
  unusualActivityNotificationsEnabled: boolean;
}

interface RevokeDevicesRequest {
  currentDeviceId: string;
}

export const authApi = api.injectEndpoints({
  endpoints: (builder) => ({
    // Authentication endpoints
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        body: credentials,
      }),
    }),
    
    verifyMfaLogin: builder.mutation<LoginResponse, {
      userId: string;
      challengeId: string;
      code: string;
      rememberDevice: boolean;
    }>({
      query: (data) => ({
        url: '/auth/verify-mfa',
        method: 'POST',
        body: data,
      }),
    }),
    
    verifyBackupCodeLogin: builder.mutation<LoginResponse, {
      userId: string;
      backupCode: string;
      rememberDevice: boolean;
    }>({
      query: (data) => ({
        url: '/auth/verify-backup-code',
        method: 'POST',
        body: data,
      }),
    }),
    
    register: builder.mutation<{
      success: boolean;
      message: string;
      data?: { userId: string; email: string; }
    }, {
      email: string;
      password: string;
      confirmPassword: string;
    }>({
      query: (data) => ({
        url: '/auth/register',
        method: 'POST',
        body: data,
      }),
    }),

    getMfaStatus: builder.query<MfaStatusResponse, void>({
      query: () => ({
        url: '/auth/mfa-status',
        method: 'GET',
      }),
    }),
    
    // MFA endpoints
    setupMfa: builder.query<MfaSetupResponse, void>({
      query: () => ({
        url: '/mfa/setup',
        method: 'GET',
      }),
    }),
    
    verifyMfaSetup: builder.mutation<{
      success: boolean; 
      message: string; 
      data: boolean;
    }, {
      code: string;
      secret: string;
    }>({
      query: (data) => ({
        url: '/mfa/verify-setup',
        method: 'POST',
        body: data,
      }),
    }),
    
    disableMfa: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, {
      code: string;
    }>({
      query: (data) => ({
        url: '/mfa/disable',
        method: 'POST',
        body: data,
      }),
    }),
    
    validateMfaCode: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, {
      userId: string;
      code: string;
    }>({
      query: (data) => ({
        url: '/mfa/validate',
        method: 'POST',
        body: data,
      }),
    }),
    
    validateBackupCode: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, {
      userId: string;
      backupCode: string;
    }>({
      query: (data) => ({
        url: '/mfa/validate-backup',
        method: 'POST',
        body: data,
      }),
    }),
    
    regenerateBackupCodes: builder.mutation<{
      success: boolean;
      message: string;
      data: string[];
    }, {
      code: string;
    }>({
      query: (data) => ({
        url: '/mfa/regenerate-backup-codes',
        method: 'POST',
        body: data,
      }),
    }),
    
    createMfaChallenge: builder.mutation<{
      success: boolean;
      message: string;
      data: MfaChallengeResponse;
    }, {
      operation: string;
    }>({
      query: (data) => ({
        url: '/mfa/create-challenge',
        method: 'POST',
        body: data,
      }),
    }),
    
    verifyMfaChallenge: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, {
      challengeId: string;
      code: string;
    }>({
      query: (data) => ({
        url: '/mfa/verify-challenge',
        method: 'POST',
        body: data,
      }),
    }),
    
    // Trusted Devices endpoints
    addTrustedDevice: builder.mutation<{
      success: boolean;
      message: string;
      data: string;
    }, MfaDeviceInfo>({
      query: (data) => ({
        url: '/mfa/add-trusted-device',
        method: 'POST',
        body: data,
      }),
    }),
    
    getTrustedDevices: builder.query<{
      success: boolean;
      message: string;
      data: TrustedDevice[];
    }, void>({
      query: () => ({
        url: '/mfa/trusted-devices',
        method: 'GET',
      }),
    }),
    
    revokeTrustedDevice: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, string>({
      query: (deviceId) => ({
        url: `/mfa/trusted-device/${deviceId}`,
        method: 'DELETE',
      }),
    }),
    
    revokeAllTrustedDevicesExceptCurrent: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, {
      currentDeviceId: string;
    }>({
      query: (data) => ({
        url: '/mfa/trusted-devices/revoke-all-except-current',
        method: 'DELETE',
        body: data,
      }),
    }),
    
    // Security Activity and Preferences endpoints
    getSecurityActivity: builder.query<{
      success: boolean;
      message: string;
      data: SecurityActivity[];
    }, {
      limit?: number;
    }>({
      query: (params) => ({
        url: '/mfa/security-activity',
        method: 'GET',
        params: params,
      }),
    }),
    
    getSecurityPreferences: builder.query<{
      success: boolean;
      message: string;
      data: SecurityPreferences;
    }, void>({
      query: () => ({
        url: '/mfa/security-preferences',
        method: 'GET',
      }),
    }),
    
    updateSecurityPreferences: builder.mutation<{
      success: boolean;
      message: string;
      data: boolean;
    }, SecurityPreferences>({
      query: (data) => ({
        url: '/mfa/security-preferences',
        method: 'PUT',
        body: data,
      }),
    }),
  }),
  overrideExisting: false,
});

export const {
  // Authentication hooks
  useLoginMutation,
  useVerifyMfaLoginMutation,
  useVerifyBackupCodeLoginMutation,
  useRegisterMutation,
  useGetMfaStatusQuery,
  
  // MFA hooks
  useSetupMfaQuery,
  useVerifyMfaSetupMutation,
  useDisableMfaMutation,
  useValidateMfaCodeMutation,
  useValidateBackupCodeMutation,
  useRegenerateBackupCodesMutation,
  useCreateMfaChallengeMutation,
  useVerifyMfaChallengeMutation,
  
  // Trusted Devices hooks
  useAddTrustedDeviceMutation,
  useGetTrustedDevicesQuery,
  useRevokeTrustedDeviceMutation,
  useRevokeAllTrustedDevicesExceptCurrentMutation,
  
  // Security Activity and Preferences hooks
  useGetSecurityActivityQuery,
  useGetSecurityPreferencesQuery,
  useUpdateSecurityPreferencesMutation,
} = authApi;