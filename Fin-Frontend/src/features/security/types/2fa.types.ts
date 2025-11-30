// Two-Factor Authentication Types
export interface TwoFactorSetup {
  userId: string;
  method: TwoFactorMethod;
  secret?: string;
  qrCode?: string;
  backupCodes?: string[];
  verified: boolean;
  createdAt: Date;
}

export type TwoFactorMethod = 'sms' | 'email' | 'authenticator' | 'backup-code';

export interface TwoFactorVerification {
  userId: string;
  method: TwoFactorMethod;
  code: string;
  trustDevice?: boolean;
}

export interface TwoFactorStatus {
  enabled: boolean;
  methods: TwoFactorMethod[];
  primaryMethod?: TwoFactorMethod;
  backupCodesRemaining: number;
  lastVerified?: Date;
}

export interface TrustedDevice {
  id: string;
  userId: string;
  deviceName: string;
  deviceType: string;
  browser: string;
  ipAddress: string;
  trusted: boolean;
  trustedAt: Date;
  expiresAt: Date;
  lastUsed: Date;
}

export interface TwoFactorChallenge {
  challengeId: string;
  userId: string;
  method: TwoFactorMethod;
  expiresAt: Date;
  attempts: number;
  maxAttempts: number;
}

export interface BackupCode {
  code: string;
  used: boolean;
  usedAt?: Date;
}
