import { User, Prisma } from '@prisma/client';
import { BaseRepository } from './BaseRepository';

export class UserRepository extends BaseRepository<User> {
  constructor() {
    super('user');
  }

  /**
   * Find user by email
   */
  async findByEmail(email: string): Promise<User | null> {
    return this.model.findUnique({
      where: { email },
      include: {
        role: {
          include: {
            permissions: true,
          },
        },
      },
    });
  }

  /**
   * Find user with role and permissions
   */
  async findByIdWithPermissions(id: string): Promise<User | null> {
    return this.model.findUnique({
      where: { id },
      include: {
        role: {
          include: {
            permissions: true,
          },
        },
      },
    });
  }

  /**
   * Update last login timestamp
   */
  async updateLastLogin(id: string): Promise<User> {
    return this.model.update({
      where: { id },
      data: {
        lastLoginAt: new Date(),
        failedLoginCount: 0,
      },
    });
  }

  /**
   * Increment failed login count
   */
  async incrementFailedLogin(id: string): Promise<User> {
    return this.model.update({
      where: { id },
      data: {
        failedLoginCount: {
          increment: 1,
        },
      },
    });
  }

  /**
   * Lock user account
   */
  async lockAccount(id: string, durationMinutes: number): Promise<User> {
    const lockedUntil = new Date();
    lockedUntil.setMinutes(lockedUntil.getMinutes() + durationMinutes);

    return this.model.update({
      where: { id },
      data: {
        lockedUntil,
      },
    });
  }

  /**
   * Unlock user account
   */
  async unlockAccount(id: string): Promise<User> {
    return this.model.update({
      where: { id },
      data: {
        lockedUntil: null,
        failedLoginCount: 0,
      },
    });
  }

  /**
   * Update password
   */
  async updatePassword(id: string, passwordHash: string): Promise<User> {
    return this.model.update({
      where: { id },
      data: {
        passwordHash,
        passwordChangedAt: new Date(),
      },
    });
  }

  /**
   * Enable MFA
   */
  async enableMFA(id: string, secret: string): Promise<User> {
    return this.model.update({
      where: { id },
      data: {
        mfaEnabled: true,
        mfaSecret: secret,
      },
    });
  }

  /**
   * Disable MFA
   */
  async disableMFA(id: string): Promise<User> {
    return this.model.update({
      where: { id },
      data: {
        mfaEnabled: false,
        mfaSecret: null,
      },
    });
  }
}

export default UserRepository;
