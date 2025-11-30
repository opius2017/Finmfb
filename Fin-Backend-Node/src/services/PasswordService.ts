import bcrypt from 'bcrypt';
import crypto from 'crypto';
import { RepositoryFactory } from '@repositories/index';
import { getPrismaClient } from '@config/database';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';
import { config } from '@config/index';

export class PasswordService {
  private userRepository = RepositoryFactory.getUserRepository();
  private prisma = getPrismaClient();

  /**
   * Hash password
   */
  async hashPassword(password: string): Promise<string> {
    return bcrypt.hash(password, 12);
  }

  /**
   * Compare password with hash
   */
  async comparePassword(password: string, hash: string): Promise<boolean> {
    return bcrypt.compare(password, hash);
  }

  /**
   * Validate password complexity
   */
  validatePasswordComplexity(password: string): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    if (password.length < config.PASSWORD_MIN_LENGTH) {
      errors.push(`Password must be at least ${config.PASSWORD_MIN_LENGTH} characters long`);
    }

    if (config.PASSWORD_REQUIRE_UPPERCASE && !/[A-Z]/.test(password)) {
      errors.push('Password must contain at least one uppercase letter');
    }

    if (config.PASSWORD_REQUIRE_LOWERCASE && !/[a-z]/.test(password)) {
      errors.push('Password must contain at least one lowercase letter');
    }

    if (config.PASSWORD_REQUIRE_NUMBERS && !/\d/.test(password)) {
      errors.push('Password must contain at least one number');
    }

    if (config.PASSWORD_REQUIRE_SPECIAL && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
      errors.push('Password must contain at least one special character');
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }

  /**
   * Change password
   */
  async changePassword(
    userId: string,
    oldPassword: string,
    newPassword: string
  ): Promise<void> {
    // Get user
    const user = await this.userRepository.findById(userId);
    if (!user) {
      throw createNotFoundError('User');
    }

    // Verify old password
    const isOldPasswordValid = await this.comparePassword(
      oldPassword,
      user.passwordHash
    );
    if (!isOldPasswordValid) {
      throw createBadRequestError('Current password is incorrect');
    }

    // Validate new password complexity
    const validation = this.validatePasswordComplexity(newPassword);
    if (!validation.isValid) {
      throw createBadRequestError('Password does not meet complexity requirements', {
        errors: validation.errors,
      });
    }

    // Check if new password is same as old password
    const isSamePassword = await this.comparePassword(newPassword, user.passwordHash);
    if (isSamePassword) {
      throw createBadRequestError('New password must be different from current password');
    }

    // Hash new password
    const newPasswordHash = await this.hashPassword(newPassword);

    // Update password
    await this.userRepository.updatePassword(userId, newPasswordHash);

    // Invalidate all sessions (force re-login)
    await this.prisma.session.deleteMany({
      where: { userId },
    });
  }

  /**
   * Request password reset
   */
  async requestPasswordReset(email: string): Promise<string> {
    // Find user
    const user = await this.userRepository.findByEmail(email);
    if (!user) {
      // Don't reveal if email exists
      return 'If the email exists, a password reset link has been sent';
    }

    // Generate reset token
    const resetToken = crypto.randomBytes(32).toString('hex');
    const resetTokenHash = crypto
      .createHash('sha256')
      .update(resetToken)
      .digest('hex');

    // Store reset token (expires in 1 hour)
    const expiresAt = new Date();
    expiresAt.setHours(expiresAt.getHours() + 1);

    await this.prisma.configuration.upsert({
      where: { key: `password_reset:${user.id}` },
      update: {
        value: {
          token: resetTokenHash,
          expiresAt: expiresAt.toISOString(),
        },
      },
      create: {
        key: `password_reset:${user.id}`,
        value: {
          token: resetTokenHash,
          expiresAt: expiresAt.toISOString(),
        },
        category: 'password_reset',
      },
    });

    // TODO: Send email with reset link
    // await emailService.sendPasswordResetEmail(user.email, resetToken);

    return resetToken; // In production, don't return token, just send email
  }

  /**
   * Reset password with token
   */
  async resetPassword(token: string, newPassword: string): Promise<void> {
    // Hash token
    const tokenHash = crypto.createHash('sha256').update(token).digest('hex');

    // Find reset token
    const resetConfigs = await this.prisma.configuration.findMany({
      where: {
        category: 'password_reset',
      },
    });

    let userId: string | null = null;
    for (const config of resetConfigs) {
      const value = config.value as any;
      if (value.token === tokenHash) {
        // Check if token is expired
        if (new Date(value.expiresAt) < new Date()) {
          throw createBadRequestError('Password reset token has expired');
        }
        userId = config.key.replace('password_reset:', '');
        break;
      }
    }

    if (!userId) {
      throw createBadRequestError('Invalid or expired password reset token');
    }

    // Validate new password
    const validation = this.validatePasswordComplexity(newPassword);
    if (!validation.isValid) {
      throw createBadRequestError('Password does not meet complexity requirements', {
        errors: validation.errors,
      });
    }

    // Hash new password
    const newPasswordHash = await this.hashPassword(newPassword);

    // Update password
    await this.userRepository.updatePassword(userId, newPasswordHash);

    // Delete reset token
    await this.prisma.configuration.delete({
      where: { key: `password_reset:${userId}` },
    });

    // Invalidate all sessions
    await this.prisma.session.deleteMany({
      where: { userId },
    });
  }

  /**
   * Verify password reset token
   */
  async verifyResetToken(token: string): Promise<boolean> {
    const tokenHash = crypto.createHash('sha256').update(token).digest('hex');

    const resetConfigs = await this.prisma.configuration.findMany({
      where: {
        category: 'password_reset',
      },
    });

    for (const config of resetConfigs) {
      const value = config.value as any;
      if (value.token === tokenHash) {
        // Check if token is expired
        if (new Date(value.expiresAt) < new Date()) {
          return false;
        }
        return true;
      }
    }

    return false;
  }

  /**
   * Clean expired reset tokens
   */
  async cleanExpiredResetTokens(): Promise<number> {
    const resetConfigs = await this.prisma.configuration.findMany({
      where: {
        category: 'password_reset',
      },
    });

    let deletedCount = 0;
    for (const config of resetConfigs) {
      const value = config.value as any;
      if (new Date(value.expiresAt) < new Date()) {
        await this.prisma.configuration.delete({
          where: { id: config.id },
        });
        deletedCount++;
      }
    }

    return deletedCount;
  }
}

export default PasswordService;
