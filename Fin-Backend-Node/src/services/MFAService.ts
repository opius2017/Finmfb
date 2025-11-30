import { authenticator } from 'otplib';
import { RepositoryFactory } from '@repositories/index';
import { config } from '@config/index';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';

export class MFAService {
  private userRepository = RepositoryFactory.getUserRepository();

  constructor() {
    // Configure TOTP
    authenticator.options = {
      window: 1, // Allow 1 step before/after current time
    };
  }

  /**
   * Generate MFA secret
   */
  generateSecret(email: string): { secret: string; qrCode: string } {
    const secret = authenticator.generateSecret();
    const otpauth = authenticator.keyuri(email, config.MFA_ISSUER, secret);

    return {
      secret,
      qrCode: otpauth, // Can be converted to QR code on frontend
    };
  }

  /**
   * Verify TOTP code
   */
  verifyCode(secret: string, code: string): boolean {
    try {
      return authenticator.verify({ token: code, secret });
    } catch {
      return false;
    }
  }

  /**
   * Enable MFA for user
   */
  async enableMFA(userId: string, code: string): Promise<{ backupCodes: string[] }> {
    const user = await this.userRepository.findById(userId);
    if (!user) {
      throw createNotFoundError('User');
    }

    if (user.mfaEnabled) {
      throw createBadRequestError('MFA is already enabled');
    }

    if (!user.mfaSecret) {
      throw createBadRequestError('MFA secret not found. Please setup MFA first.');
    }

    // Verify code
    const isValid = this.verifyCode(user.mfaSecret, code);
    if (!isValid) {
      throw createBadRequestError('Invalid verification code');
    }

    // Enable MFA
    await this.userRepository.enableMFA(userId, user.mfaSecret);

    // Generate backup codes
    const backupCodes = this.generateBackupCodes();

    // TODO: Store backup codes securely (hashed)

    return { backupCodes };
  }

  /**
   * Disable MFA for user
   */
  async disableMFA(userId: string, code: string): Promise<void> {
    const user = await this.userRepository.findById(userId);
    if (!user) {
      throw createNotFoundError('User');
    }

    if (!user.mfaEnabled) {
      throw createBadRequestError('MFA is not enabled');
    }

    if (!user.mfaSecret) {
      throw createBadRequestError('MFA secret not found');
    }

    // Verify code
    const isValid = this.verifyCode(user.mfaSecret, code);
    if (!isValid) {
      throw createBadRequestError('Invalid verification code');
    }

    // Disable MFA
    await this.userRepository.disableMFA(userId);
  }

  /**
   * Verify MFA code for user
   */
  async verifyUserCode(userId: string, code: string): Promise<boolean> {
    const user = await this.userRepository.findById(userId);
    if (!user || !user.mfaEnabled || !user.mfaSecret) {
      return false;
    }

    return this.verifyCode(user.mfaSecret, code);
  }

  /**
   * Setup MFA (generate secret)
   */
  async setupMFA(userId: string): Promise<{ secret: string; qrCode: string }> {
    const user = await this.userRepository.findById(userId);
    if (!user) {
      throw createNotFoundError('User');
    }

    if (user.mfaEnabled) {
      throw createBadRequestError('MFA is already enabled. Disable it first to setup again.');
    }

    // Generate secret
    const { secret, qrCode } = this.generateSecret(user.email);

    // Store secret temporarily (not enabled yet)
    await this.userRepository.update(userId, { mfaSecret: secret });

    return { secret, qrCode };
  }

  /**
   * Generate backup codes
   */
  private generateBackupCodes(count: number = 10): string[] {
    const codes: string[] = [];
    for (let i = 0; i < count; i++) {
      const code = Math.random().toString(36).substring(2, 10).toUpperCase();
      codes.push(code);
    }
    return codes;
  }
}

export default MFAService;
