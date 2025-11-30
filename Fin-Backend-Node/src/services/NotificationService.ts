import { executeInTransaction } from '@config/database';
import { EmailService } from './EmailService';
import { createBadRequestError } from '@middleware/errorHandler';

export interface NotificationPayload {
  type: 'email' | 'sms' | 'push' | 'in_app';
  recipients: string[];
  subject?: string;
  message: string;
  template?: string;
  data?: Record<string, any>;
  priority?: 'high' | 'normal' | 'low';
}

export interface NotificationPreferences {
  userId: string;
  emailEnabled: boolean;
  smsEnabled: boolean;
  pushEnabled: boolean;
  inAppEnabled: boolean;
  categories: {
    transactions: boolean;
    loans: boolean;
    payments: boolean;
    marketing: boolean;
    security: boolean;
  };
}

export class NotificationService {
  private emailService: EmailService;

  constructor() {
    this.emailService = new EmailService();
  }

  /**
   * Send notification
   */
  async sendNotification(payload: NotificationPayload): Promise<void> {
    try {
      switch (payload.type) {
        case 'email':
          await this.sendEmailNotification(payload);
          break;
        case 'sms':
          await this.sendSMSNotification(payload);
          break;
        case 'push':
          await this.sendPushNotification(payload);
          break;
        case 'in_app':
          await this.sendInAppNotification(payload);
          break;
        default:
          throw createBadRequestError(`Unsupported notification type: ${payload.type}`);
      }

      // Log notification
      await this.logNotification(payload, 'sent');
    } catch (error) {
      await this.logNotification(payload, 'failed', error instanceof Error ? error.message : 'Unknown error');
      throw error;
    }
  }

  /**
   * Send email notification
   */
  private async sendEmailNotification(payload: NotificationPayload): Promise<void> {
    if (payload.template) {
      await this.emailService.sendEmail({
        to: payload.recipients,
        subject: payload.subject || 'Notification',
        template: payload.template,
        data: payload.data,
      });
    } else {
      await this.emailService.sendEmail({
        to: payload.recipients,
        subject: payload.subject || 'Notification',
        html: payload.message,
      });
    }
  }

  /**
   * Send SMS notification
   */
  private async sendSMSNotification(payload: NotificationPayload): Promise<void> {
    // TODO: Integrate with SMS provider (Twilio, Africa's Talking, etc.)
    console.log('SMS notification:', {
      recipients: payload.recipients,
      message: payload.message,
    });

    // Placeholder implementation
    // In production, integrate with actual SMS gateway
  }

  /**
   * Send push notification
   */
  private async sendPushNotification(payload: NotificationPayload): Promise<void> {
    // TODO: Integrate with Firebase Cloud Messaging or similar
    console.log('Push notification:', {
      recipients: payload.recipients,
      message: payload.message,
      data: payload.data,
    });

    // Placeholder implementation
    // In production, integrate with FCM or similar service
  }

  /**
   * Send in-app notification
   */
  private async sendInAppNotification(payload: NotificationPayload): Promise<void> {
    // Store notification in database for in-app display
    await executeInTransaction(async (prisma) => {
      for (const recipient of payload.recipients) {
        await prisma.systemLog.create({
          data: {
            level: 'INFO',
            message: payload.message,
            context: {
              type: 'in_app_notification',
              userId: recipient,
              subject: payload.subject,
              data: payload.data,
              priority: payload.priority,
              createdAt: new Date(),
            },
          },
        });
      }
    });
  }

  /**
   * Send bulk notifications
   */
  async sendBulkNotifications(
    type: 'email' | 'sms' | 'push',
    recipients: string[],
    payload: Omit<NotificationPayload, 'type' | 'recipients'>
  ): Promise<void> {
    const notification: NotificationPayload = {
      type,
      recipients,
      ...payload,
    };

    await this.sendNotification(notification);
  }

  /**
   * Get user notification preferences
   */
  async getUserPreferences(userId: string): Promise<NotificationPreferences | null> {
    // TODO: Implement preferences storage in database
    // For now, return default preferences
    return {
      userId,
      emailEnabled: true,
      smsEnabled: true,
      pushEnabled: true,
      inAppEnabled: true,
      categories: {
        transactions: true,
        loans: true,
        payments: true,
        marketing: false,
        security: true,
      },
    };
  }

  /**
   * Update user notification preferences
   */
  async updateUserPreferences(
    userId: string,
    preferences: Partial<NotificationPreferences>
  ): Promise<NotificationPreferences> {
    // TODO: Implement preferences storage in database
    console.log('Updating preferences for user:', userId, preferences);

    return {
      userId,
      emailEnabled: preferences.emailEnabled ?? true,
      smsEnabled: preferences.smsEnabled ?? true,
      pushEnabled: preferences.pushEnabled ?? true,
      inAppEnabled: preferences.inAppEnabled ?? true,
      categories: preferences.categories || {
        transactions: true,
        loans: true,
        payments: true,
        marketing: false,
        security: true,
      },
    };
  }

  /**
   * Send notification respecting user preferences
   */
  async sendNotificationWithPreferences(
    userId: string,
    category: keyof NotificationPreferences['categories'],
    payload: Omit<NotificationPayload, 'recipients'>
  ): Promise<void> {
    const preferences = await this.getUserPreferences(userId);

    if (!preferences) {
      throw createBadRequestError('User preferences not found');
    }

    // Check if category is enabled
    if (!preferences.categories[category]) {
      console.log(`Notification skipped: ${category} disabled for user ${userId}`);
      return;
    }

    // Check if notification type is enabled
    const typeEnabled = {
      email: preferences.emailEnabled,
      sms: preferences.smsEnabled,
      push: preferences.pushEnabled,
      in_app: preferences.inAppEnabled,
    }[payload.type];

    if (!typeEnabled) {
      console.log(`Notification skipped: ${payload.type} disabled for user ${userId}`);
      return;
    }

    // Send notification
    await this.sendNotification({
      ...payload,
      recipients: [userId],
    });
  }

  /**
   * Get notification history
   */
  async getNotificationHistory(
    userId: string,
    page: number = 1,
    limit: number = 20
  ): Promise<{
    data: any[];
    pagination: { page: number; limit: number; total: number; pages: number };
  }> {
    const skip = (page - 1) * limit;

    const [notifications, total] = await Promise.all([
      executeInTransaction(async (prisma) => {
        return prisma.systemLog.findMany({
          where: {
            context: {
              path: ['userId'],
              equals: userId,
            },
          },
          skip,
          take: limit,
          orderBy: {
            createdAt: 'desc',
          },
        });
      }),
      executeInTransaction(async (prisma) => {
        return prisma.systemLog.count({
          where: {
            context: {
              path: ['userId'],
              equals: userId,
            },
          },
        });
      }),
    ]);

    return {
      data: notifications,
      pagination: {
        page,
        limit,
        total,
        pages: Math.ceil(total / limit),
      },
    };
  }

  /**
   * Log notification
   */
  private async logNotification(
    payload: NotificationPayload,
    status: 'sent' | 'failed',
    error?: string
  ): Promise<void> {
    try {
      await executeInTransaction(async (prisma) => {
        await prisma.systemLog.create({
          data: {
            level: status === 'sent' ? 'INFO' : 'ERROR',
            message: `Notification ${status}: ${payload.type}`,
            context: {
              type: payload.type,
              recipients: payload.recipients,
              subject: payload.subject,
              template: payload.template,
              priority: payload.priority,
              status,
              error,
              timestamp: new Date(),
            },
          },
        });
      });
    } catch (error) {
      console.error('Failed to log notification:', error);
    }
  }

  /**
   * Send test notification
   */
  async sendTestNotification(
    type: 'email' | 'sms' | 'push',
    recipient: string
  ): Promise<void> {
    await this.sendNotification({
      type,
      recipients: [recipient],
      subject: 'Test Notification',
      message: 'This is a test notification from FinMFB',
      priority: 'normal',
    });
  }
}

export default NotificationService;
