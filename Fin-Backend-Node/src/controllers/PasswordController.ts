import { Request, Response } from 'express';
import { z } from 'zod';
import { PasswordService } from '@services/PasswordService';
import { asyncHandler } from '@utils/asyncHandler';
import { passwordSchema, emailSchema } from '@utils/validation';

// Validation schemas
const changePasswordSchema = z.object({
  oldPassword: z.string().min(1, 'Current password is required'),
  newPassword: passwordSchema,
  confirmPassword: z.string().min(1, 'Password confirmation is required'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

const requestResetSchema = z.object({
  email: emailSchema,
});

const resetPasswordSchema = z.object({
  token: z.string().min(1, 'Reset token is required'),
  newPassword: passwordSchema,
  confirmPassword: z.string().min(1, 'Password confirmation is required'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

const verifyTokenSchema = z.object({
  token: z.string().min(1, 'Reset token is required'),
});

export class PasswordController {
  private passwordService = new PasswordService();

  /**
   * Change password
   * POST /api/v1/password/change
   */
  changePassword = asyncHandler(
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

      // Validate request body
      const { oldPassword, newPassword } = changePasswordSchema.parse(req.body);

      // Change password
      await this.passwordService.changePassword(userId, oldPassword, newPassword);

      res.status(200).json({
        success: true,
        message: 'Password changed successfully. Please login again.',
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Request password reset
   * POST /api/v1/password/reset-request
   */
  requestReset = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      // Validate request body
      const { email } = requestResetSchema.parse(req.body);

      // Request password reset
      await this.passwordService.requestPasswordReset(email);

      // Always return success to prevent email enumeration
      res.status(200).json({
        success: true,
        message: 'If the email exists, a password reset link has been sent',
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Reset password
   * POST /api/v1/password/reset
   */
  resetPassword = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      // Validate request body
      const { token, newPassword } = resetPasswordSchema.parse(req.body);

      // Reset password
      await this.passwordService.resetPassword(token, newPassword);

      res.status(200).json({
        success: true,
        message: 'Password reset successfully. Please login with your new password.',
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );

  /**
   * Verify reset token
   * POST /api/v1/password/verify-token
   */
  verifyToken = asyncHandler(
    async (req: Request, res: Response): Promise<void> => {
      // Validate request body
      const { token } = verifyTokenSchema.parse(req.body);

      // Verify token
      const isValid = await this.passwordService.verifyResetToken(token);

      res.status(200).json({
        success: true,
        data: { isValid },
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    }
  );
}

export default PasswordController;
