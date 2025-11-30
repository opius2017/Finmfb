import { Router } from 'express';
import { PasswordController } from '@controllers/PasswordController';
import { authenticate } from '@middleware/authenticate';

const router = Router();
const passwordController = new PasswordController();

/**
 * @swagger
 * /api/v1/password/change:
 *   post:
 *     summary: Change password
 *     tags: [Password]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - oldPassword
 *               - newPassword
 *               - confirmPassword
 *             properties:
 *               oldPassword:
 *                 type: string
 *               newPassword:
 *                 type: string
 *               confirmPassword:
 *                 type: string
 *     responses:
 *       200:
 *         description: Password changed successfully
 *       400:
 *         description: Invalid request
 *       401:
 *         description: Not authenticated
 */
router.post('/change', authenticate, passwordController.changePassword);

/**
 * @swagger
 * /api/v1/password/reset-request:
 *   post:
 *     summary: Request password reset
 *     tags: [Password]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - email
 *             properties:
 *               email:
 *                 type: string
 *                 format: email
 *     responses:
 *       200:
 *         description: Reset email sent if account exists
 */
router.post('/reset-request', passwordController.requestReset);

/**
 * @swagger
 * /api/v1/password/reset:
 *   post:
 *     summary: Reset password with token
 *     tags: [Password]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - token
 *               - newPassword
 *               - confirmPassword
 *             properties:
 *               token:
 *                 type: string
 *               newPassword:
 *                 type: string
 *               confirmPassword:
 *                 type: string
 *     responses:
 *       200:
 *         description: Password reset successfully
 *       400:
 *         description: Invalid or expired token
 */
router.post('/reset', passwordController.resetPassword);

/**
 * @swagger
 * /api/v1/password/verify-token:
 *   post:
 *     summary: Verify password reset token
 *     tags: [Password]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             required:
 *               - token
 *             properties:
 *               token:
 *                 type: string
 *     responses:
 *       200:
 *         description: Token verification result
 */
router.post('/verify-token', passwordController.verifyToken);

export default router;
