import { TwoFactorSetup, TwoFactorVerification, TwoFactorStatus, TrustedDevice } from '../types/2fa.types';

export class TwoFactorService {
  private apiEndpoint = '/api/auth/2fa';

  async getStatus(): Promise<TwoFactorStatus> {
    const response = await fetch(`${this.apiEndpoint}/status`);
    if (!response.ok) throw new Error('Failed to fetch 2FA status');
    return response.json();
  }

  async setupAuthenticator(): Promise<TwoFactorSetup> {
    const response = await fetch(`${this.apiEndpoint}/setup/authenticator`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to setup authenticator');
    return response.json();
  }

  async setupSMS(phoneNumber: string): Promise<TwoFactorSetup> {
    const response = await fetch(`${this.apiEndpoint}/setup/sms`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ phoneNumber }),
    });
    if (!response.ok) throw new Error('Failed to setup SMS');
    return response.json();
  }

  async setupEmail(email: string): Promise<TwoFactorSetup> {
    const response = await fetch(`${this.apiEndpoint}/setup/email`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email }),
    });
    if (!response.ok) throw new Error('Failed to setup email');
    return response.json();
  }

  async verifySetup(method: string, code: string): Promise<{ success: boolean; backupCodes?: string[] }> {
    const response = await fetch(`${this.apiEndpoint}/verify-setup`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ method, code }),
    });
    if (!response.ok) throw new Error('Failed to verify setup');
    return response.json();
  }

  async verify(verification: TwoFactorVerification): Promise<{ success: boolean; token?: string }> {
    const response = await fetch(`${this.apiEndpoint}/verify`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(verification),
    });
    if (!response.ok) throw new Error('Failed to verify 2FA');
    return response.json();
  }

  async disable(method: string, password: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/disable`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ method, password }),
    });
    if (!response.ok) throw new Error('Failed to disable 2FA');
  }

  async regenerateBackupCodes(password: string): Promise<string[]> {
    const response = await fetch(`${this.apiEndpoint}/backup-codes/regenerate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password }),
    });
    if (!response.ok) throw new Error('Failed to regenerate backup codes');
    return response.json();
  }

  async getTrustedDevices(): Promise<TrustedDevice[]> {
    const response = await fetch(`${this.apiEndpoint}/trusted-devices`);
    if (!response.ok) throw new Error('Failed to fetch trusted devices');
    return response.json();
  }

  async removeTrustedDevice(deviceId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/trusted-devices/${deviceId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to remove trusted device');
  }

  async sendCode(method: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/send-code`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ method }),
    });
    if (!response.ok) throw new Error('Failed to send code');
  }

  validateCode(code: string): boolean {
    return /^\d{6}$/.test(code);
  }

  formatBackupCode(code: string): string {
    return code.replace(/(.{4})/g, '$1-').slice(0, -1);
  }
}

export const twoFactorService = new TwoFactorService();
