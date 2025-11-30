import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { NotificationService } from '@services/NotificationService';

const sendNotificationSchema = z.object({
  type: z.enum(['email', 'sms', 'push', 'in_app']),
  recipients: z.array(z.string()),
  subject: z.string().optional(),
  message: z.string(),
  template: z.string().optional(),
  data: z.record(z.any()).optional(),
  priority: z.enum(['high', 'normal', 'low']).optional(),
});

const updatePreferencesSchema = z.object({
  emailEnabled: z.boolean().optional(),
  smsEnabled: z.boolean().optional(),
  pushEnabled: z.boolean().optional(),
  inAppEnabled: z.boolean().optional(),
  categories: z
    .object({
      transactions: z.boolean().optional(),
      loans: z.boolean().optional(),
      payments: z.boolean().optional(),
      marketing: z.boolean().optional(),
      security: z.boolean().optional(),
    })
    .optional(),
});

export class NotificationController {
  private notificationService = new NotificationService();

  /**
   * @swagger
   * /api/v1/notifications/send:
   *   post:
   *     summary: Send notification
   *     tags: [Notifications]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - type
   *               - recipients
   *               - message
   *             properties:
   *               type:
   *                 type: string
   *                 enum: [email, sms, push, in_app]
   *               recipients:
   *                 type: array
   *                 items:
   *                   type: string
   *               subject:
   *                 type: string
   *               message:
   *                 type: string
   *               template:
   *                 type: string
   *               data:
   *                 type: object
   *               priority:
   *                 type: string
   *                 enum: [high, normal, low]
   *     responses:
   *       200:
   *         description: Notification sent successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async sendNotification(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = sendNotificationSchema.parse(req.body);

      await this.notificationService.sendNotification(validatedData);

      res.json({
        success: true,
        message: 'Notification sent successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/notifications/preferences:
   *   get:
   *     summary: Get user notification preferences
   *     tags: [Notifications]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Preferences retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getPreferences(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const userId = req.user?.id;

      const preferences = await this.notificationService.getUserPreferences(userId!);

      res.json({
        success: true,
        data: preferences,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/notifications/preferences:
   *   put:
   *     summary: Update notification preferences
   *     tags: [Notifications]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             properties:
   *               emailEnabled:
   *                 type: boolean
   *               smsEnabled:
   *                 type: boolean
   *               pushEnabled:
   *                 type: boolean
   *               inAppEnabled:
   *                 type: boolean
   *               categories:
   *                 type: object
   *                 properties:
   *                   transactions:
   *                     type: boolean
   *                   loans:
   *                     type: boolean
   *                   payments:
   *                     type: boolean
   *                   marketing:
   *                     type: boolean
   *                   security:
   *                     type: boolean
   *     responses:
   *       200:
   *         description: Preferences updated successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async updatePreferences(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const userId = req.user?.id;
      const validatedData = updatePreferencesSchema.parse(req.body);

      const preferences = await this.notificationService.updateUserPreferences(
        userId!,
        validatedData
      );

      res.json({
        success: true,
        data: preferences,
        message: 'Preferences updated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/notifications/history:
   *   get:
   *     summary: Get notification history
   *     tags: [Notifications]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: page
   *         schema:
   *           type: integer
   *           default: 1
   *       - in: query
   *         name: limit
   *         schema:
   *           type: integer
   *           default: 20
   *     responses:
   *       200:
   *         description: History retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getHistory(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const userId = req.user?.id;
      const page = parseInt(req.query.page as string) || 1;
      const limit = parseInt(req.query.limit as string) || 20;

      const result = await this.notificationService.getNotificationHistory(
        userId!,
        page,
        limit
      );

      res.json({
        success: true,
        data: result.data,
        pagination: result.pagination,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/notifications/test:
   *   post:
   *     summary: Send test notification
   *     tags: [Notifications]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - type
   *               - recipient
   *             properties:
   *               type:
   *                 type: string
   *                 enum: [email, sms, push]
   *               recipient:
   *                 type: string
   *     responses:
   *       200:
   *         description: Test notification sent
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async sendTestNotification(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { type, recipient } = req.body;

      await this.notificationService.sendTestNotification(type, recipient);

      res.json({
        success: true,
        message: 'Test notification sent successfully',
      });
    } catch (error) {
      next(error);
    }
  }
}

export default NotificationController;
