import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { AuthService } from '@services/AuthService';
import { asyncHandler } from '@utils/asyncHandler';
import { emailSchema, passwordSchema } from '@utils/validation';

// Validation schemas
const loginSchema = z.object({
  email: emailSchema,
  password: z.string().min(1, 'Password is required'),
});

const refreshTokenSchema = z.object({
  refreshToken: z.string().min(1, 'Refresh token is required'),
});

export class AuthController {
  private authService = new AuthService();

  /**
   * Login
   * POST /api/v1/auth/login
   */
  login = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    // Validate request body
    const { email, password } = loginSchema.parse(req.body);

    // Login user
    const tokens = await this.authService.login({ email, password });

    // Update session with actual refresh token
    const payload = require('@utils/jwt').verifyRefreshToken(tokens.refreshToken);
    await this.authService.updateSessionRefreshToken(
      payload.sessionId,
      tokens.refreshToken
    );

    res.status(200).json({
      success: true,
      data: tokens,
      message: 'Login successful',
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Refresh token
   * POST /api/v1/auth/refresh
   */
  refreshToken = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      // Validate request body
      const { refreshToken } = refreshTokenSchema.parse(req.body);

      // Refresh tokens
      const tokens = await this.authService.refreshToken(refreshToken);

      // Update session with new refresh token
      const payload = require('@utils/jwt').verifyRefreshToken(
        tokens.refreshToken
      );
      await this.authService.updateSessionRefreshToken(
        payload.sessionId,
        tokens.refreshToken
      );

      res.status(200).json({
        success: true,
        data: tokens,
        message: 'Token refreshed successfully',
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Logout
   * POST /api/v1/auth/logout
   */
  logout = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = req.user?.id;
    if (!userId) {
      res.status(401).json({
        success: false,
        message: 'Not authenticated',
        timestamp: new Date(),
      });
      return;
    }

    const { refreshToken } = req.body;

    await this.authService.logout(userId, refreshToken);

    res.status(200).json({
      success: true,
      message: 'Logout successful',
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Logout from all devices
   * POST /api/v1/auth/logout-all
   */
  logoutAll = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      const userId = req.user?.id;
      if (!userId) {
        res.status(401).json({
          success: false,
          message: 'Not authenticated',
          timestamp: new Date(),
        });
        return;
      }

      await this.authService.logoutAll(userId);

      res.status(200).json({
        success: true,
        message: 'Logged out from all devices',
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Get current user
   * GET /api/v1/auth/me
   */
  getCurrentUser = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      const userId = req.user?.id;
      if (!userId) {
        res.status(401).json({
          success: false,
          message: 'Not authenticated',
          timestamp: new Date(),
        });
        return;
      }

      const userRepository = require('@repositories/index').RepositoryFactory.getUserRepository();
      const user = await userRepository.findByIdWithPermissions(userId);

      if (!user) {
        res.status(404).json({
          success: false,
          message: 'User not found',
          timestamp: new Date(),
        });
        return;
      }

      // Remove sensitive data
      const { passwordHash, mfaSecret, ...userData } = user;

      res.status(200).json({
        success: true,
        data: userData,
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Get user sessions
   * GET /api/v1/auth/sessions
   */
  getSessions = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      const userId = req.user?.id;
      if (!userId) {
        res.status(401).json({
          success: false,
          message: 'Not authenticated',
          timestamp: new Date(),
        });
        return;
      }

      const sessions = await this.authService.getUserSessions(userId);

      res.status(200).json({
        success: true,
        data: sessions,
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Revoke session
   * DELETE /api/v1/auth/sessions/:sessionId
   */
  revokeSession = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      const { sessionId } = req.params;

      await this.authService.revokeSession(sessionId);

      res.status(200).json({
        success: true,
        message: 'Session revoked successfully',
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );
}

export default AuthController;
