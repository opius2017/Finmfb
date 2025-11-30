import bcrypt from 'bcrypt';
import { User, Session } from '@prisma/client';
import { RepositoryFactory } from '@repositories/index';
import { generateTokens, verifyRefreshToken } from '@utils/jwt';
import { LoginCredentials, AuthTokens } from '@/types';
import { createUnauthorizedError, createNotFoundError, AppError } from '@middleware/errorHandler';
import { getPrismaClient } from '@config/database';
import { config } from '@config/index';

export class AuthService {
  private userRepository = RepositoryFactory.getUserRepository();
  private prisma = getPrismaClient();

  /**
   * Login user with email and password
   */
  async login(credentials: LoginCredentials): Promise<AuthTokens> {
    const { email, password } = credentials;

    // Find user with role and permissions
    const user = await this.userRepository.findByEmail(email);
    if (!user) {
      throw createUnauthorizedError('Invalid email or password');
    }

    // Check if user is active
    if (!user.isActive) {
      throw createUnauthorizedError('Account is inactive');
    }

    // Check if account is locked
    if (user.lockedUntil && user.lockedUntil > new Date()) {
      const minutesLeft = Math.ceil(
        (user.lockedUntil.getTime() - Date.now()) / 60000
      );
      throw createUnauthorizedError(
        `Account is locked. Try again in ${minutesLeft} minutes`
      );
    }

    // Verify password
    const isPasswordValid = await bcrypt.compare(password, user.passwordHash);
    if (!isPasswordValid) {
      // Increment failed login count
      await this.userRepository.incrementFailedLogin(user.id);

      // Lock account if max attempts reached
      if (user.failedLoginCount + 1 >= config.MAX_LOGIN_ATTEMPTS) {
        await this.userRepository.lockAccount(
          user.id,
          config.LOCKOUT_DURATION_MINUTES
        );
        throw createUnauthorizedError(
          `Account locked due to too many failed login attempts. Try again in ${config.LOCKOUT_DURATION_MINUTES} minutes`
        );
      }

      throw createUnauthorizedError('Invalid email or password');
    }

    // Create session
    const session = await this.createSession(user.id);

    // Generate tokens
    const tokens = generateTokens(user.id, user.email, user.roleId, session.id);

    // Update last login and reset failed attempts
    await this.userRepository.updateLastLogin(user.id);

    return tokens;
  }

  /**
   * Refresh access token using refresh token
   */
  async refreshToken(refreshToken: string): Promise<AuthTokens> {
    // Verify refresh token
    let payload;
    try {
      payload = verifyRefreshToken(refreshToken);
    } catch (error) {
      throw createUnauthorizedError('Invalid or expired refresh token');
    }

    // Find session
    const session = await this.prisma.session.findUnique({
      where: { id: payload.sessionId },
      include: {
        user: {
          include: {
            role: true,
          },
        },
      },
    });

    if (!session) {
      throw createUnauthorizedError('Session not found');
    }

    // Check if session is expired
    if (session.expiresAt < new Date()) {
      await this.prisma.session.delete({ where: { id: session.id } });
      throw createUnauthorizedError('Session expired');
    }

    // Check if refresh token matches
    if (session.refreshToken !== refreshToken) {
      throw createUnauthorizedError('Invalid refresh token');
    }

    // Check if user is active
    if (!session.user.isActive) {
      throw createUnauthorizedError('Account is inactive');
    }

    // Generate new tokens with token rotation
    const newSession = await this.rotateSession(session.id, session.userId);
    const tokens = generateTokens(
      session.user.id,
      session.user.email,
      session.user.roleId,
      newSession.id
    );

    return tokens;
  }

  /**
   * Logout user
   */
  async logout(userId: string, refreshToken?: string): Promise<void> {
    if (refreshToken) {
      // Delete specific session
      await this.prisma.session.deleteMany({
        where: {
          userId,
          refreshToken,
        },
      });
    } else {
      // Delete all user sessions
      await this.prisma.session.deleteMany({
        where: { userId },
      });
    }
  }

  /**
   * Logout from all devices
   */
  async logoutAll(userId: string): Promise<void> {
    await this.prisma.session.deleteMany({
      where: { userId },
    });
  }

  /**
   * Get user sessions
   */
  async getUserSessions(userId: string): Promise<Session[]> {
    return this.prisma.session.findMany({
      where: { userId },
      orderBy: { createdAt: 'desc' },
    });
  }

  /**
   * Revoke session
   */
  async revokeSession(sessionId: string): Promise<void> {
    await this.prisma.session.delete({
      where: { id: sessionId },
    });
  }

  /**
   * Clean expired sessions
   */
  async cleanExpiredSessions(): Promise<number> {
    const result = await this.prisma.session.deleteMany({
      where: {
        expiresAt: {
          lt: new Date(),
        },
      },
    });

    return result.count;
  }

  /**
   * Create new session
   */
  private async createSession(userId: string): Promise<Session> {
    const expiresAt = new Date();
    expiresAt.setDate(expiresAt.getDate() + 7); // 7 days

    // Generate temporary refresh token (will be replaced with actual token)
    const tempRefreshToken = `temp_${Date.now()}_${Math.random()}`;

    const session = await this.prisma.session.create({
      data: {
        userId,
        refreshToken: tempRefreshToken,
        expiresAt,
      },
    });

    return session;
  }

  /**
   * Rotate session (refresh token rotation)
   */
  private async rotateSession(
    oldSessionId: string,
    userId: string
  ): Promise<Session> {
    // Delete old session
    await this.prisma.session.delete({
      where: { id: oldSessionId },
    });

    // Create new session
    return this.createSession(userId);
  }

  /**
   * Update session refresh token
   */
  async updateSessionRefreshToken(
    sessionId: string,
    refreshToken: string
  ): Promise<void> {
    await this.prisma.session.update({
      where: { id: sessionId },
      data: { refreshToken },
    });
  }
}

export default AuthService;
