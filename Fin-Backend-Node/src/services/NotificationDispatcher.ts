import { executeInTransaction } from '@config/database';
import { JobService } from './JobService';

export interface NotificationPayload {
  type: 'email' | 'sms' | 'push' | 'in_app';
  recipients: string[]; // User IDs or email addresses
  subject?: string;
  message: string;
  template?: string;
  data?: Record<string, any>;
  priority?: 'high' | 'normal' | 'low';
}

export interface WorkflowNotificationContext {
  workflowId: string;
  workflowType: string;
  entityType: string;
  entityId: string;
  currentStep: string;
  action: string;
  actorId?: string;
  comment?: string;
}

export class NotificationDispatcher {
  private jobService: JobService;

  constructor() {
    this.jobService = new JobService();
  }

  /**
   * Send workflow notification
   */
  async sendWorkflowNotification(
    context: WorkflowNotificationContext,
    approvers: string[]
  ): Promise<void> {
    const notification = this.buildWorkflowNotification(context, approvers);
    
    // Queue notification job
    await this.jobService.addJob(
      'email-notifications',
      'send-bulk-email',
      {
        recipients: notification.recipients,
        subject: notification.subject,
        template: notification.template,
        data: notification.data,
      },
      {
        attempts: 3,
        backoff: {
          type: 'exponential',
          delay: 2000,
        },
      }
    );

    // Log notification
    await this.logNotification(notification, context);
  }

  /**
   * Send approval request notification
   */
  async sendApprovalRequest(
    workflowId: string,
    workflowType: string,
    entityType: string,
    entityId: string,
    approvers: string[],
    initiatorName: string,
    amount?: number
  ): Promise<void> {
    const context: WorkflowNotificationContext = {
      workflowId,
      workflowType,
      entityType,
      entityId,
      currentStep: 'approval_request',
      action: 'requested',
    };

    const notification: NotificationPayload = {
      type: 'email',
      recipients: approvers,
      subject: `Approval Required: ${workflowType}`,
      message: `${initiatorName} has submitted a ${workflowType} request that requires your approval.`,
      template: 'approval-request',
      data: {
        workflowType,
        entityType,
        entityId,
        initiatorName,
        amount,
        approvalUrl: `/workflows/${workflowId}/approve`,
      },
      priority: amount && amount > 100000 ? 'high' : 'normal',
    };

    await this.dispatchNotification(notification);
    await this.logNotification(notification, context);
  }

  /**
   * Send approval granted notification
   */
  async sendApprovalGranted(
    workflowId: string,
    workflowType: string,
    entityType: string,
    entityId: string,
    initiatorId: string,
    approverName: string
  ): Promise<void> {
    const context: WorkflowNotificationContext = {
      workflowId,
      workflowType,
      entityType,
      entityId,
      currentStep: 'approval_granted',
      action: 'approved',
    };

    const notification: NotificationPayload = {
      type: 'email',
      recipients: [initiatorId],
      subject: `Approved: ${workflowType}`,
      message: `Your ${workflowType} request has been approved by ${approverName}.`,
      template: 'approval-granted',
      data: {
        workflowType,
        entityType,
        entityId,
        approverName,
        detailsUrl: `/${entityType}/${entityId}`,
      },
      priority: 'normal',
    };

    await this.dispatchNotification(notification);
    await this.logNotification(notification, context);
  }

  /**
   * Send approval rejected notification
   */
  async sendApprovalRejected(
    workflowId: string,
    workflowType: string,
    entityType: string,
    entityId: string,
    initiatorId: string,
    approverName: string,
    reason: string
  ): Promise<void> {
    const context: WorkflowNotificationContext = {
      workflowId,
      workflowType,
      entityType,
      entityId,
      currentStep: 'approval_rejected',
      action: 'rejected',
      comment: reason,
    };

    const notification: NotificationPayload = {
      type: 'email',
      recipients: [initiatorId],
      subject: `Rejected: ${workflowType}`,
      message: `Your ${workflowType} request has been rejected by ${approverName}.`,
      template: 'approval-rejected',
      data: {
        workflowType,
        entityType,
        entityId,
        approverName,
        reason,
        detailsUrl: `/${entityType}/${entityId}`,
      },
      priority: 'high',
    };

    await this.dispatchNotification(notification);
    await this.logNotification(notification, context);
  }

  /**
   * Send workflow timeout notification
   */
  async sendWorkflowTimeout(
    workflowId: string,
    workflowType: string,
    entityType: string,
    entityId: string,
    approvers: string[],
    escalationUsers: string[]
  ): Promise<void> {
    const context: WorkflowNotificationContext = {
      workflowId,
      workflowType,
      entityType,
      entityId,
      currentStep: 'timeout',
      action: 'timeout',
    };

    // Notify original approvers
    const approverNotification: NotificationPayload = {
      type: 'email',
      recipients: approvers,
      subject: `Reminder: Approval Required - ${workflowType}`,
      message: `This ${workflowType} request is still pending your approval.`,
      template: 'approval-reminder',
      data: {
        workflowType,
        entityType,
        entityId,
        approvalUrl: `/workflows/${workflowId}/approve`,
      },
      priority: 'high',
    };

    // Notify escalation users
    const escalationNotification: NotificationPayload = {
      type: 'email',
      recipients: escalationUsers,
      subject: `Escalation: Approval Timeout - ${workflowType}`,
      message: `A ${workflowType} request has timed out and requires immediate attention.`,
      template: 'approval-escalation',
      data: {
        workflowType,
        entityType,
        entityId,
        originalApprovers: approvers,
        approvalUrl: `/workflows/${workflowId}/approve`,
      },
      priority: 'high',
    };

    await Promise.all([
      this.dispatchNotification(approverNotification),
      this.dispatchNotification(escalationNotification),
    ]);

    await this.logNotification(approverNotification, context);
    await this.logNotification(escalationNotification, context);
  }

  /**
   * Send workflow completed notification
   */
  async sendWorkflowCompleted(
    workflowId: string,
    workflowType: string,
    entityType: string,
    entityId: string,
    initiatorId: string,
    stakeholders: string[]
  ): Promise<void> {
    const context: WorkflowNotificationContext = {
      workflowId,
      workflowType,
      entityType,
      entityId,
      currentStep: 'completed',
      action: 'completed',
    };

    const notification: NotificationPayload = {
      type: 'email',
      recipients: [initiatorId, ...stakeholders],
      subject: `Completed: ${workflowType}`,
      message: `The ${workflowType} workflow has been completed successfully.`,
      template: 'workflow-completed',
      data: {
        workflowType,
        entityType,
        entityId,
        detailsUrl: `/${entityType}/${entityId}`,
      },
      priority: 'normal',
    };

    await this.dispatchNotification(notification);
    await this.logNotification(notification, context);
  }

  /**
   * Build workflow notification from context
   */
  private buildWorkflowNotification(
    context: WorkflowNotificationContext,
    recipients: string[]
  ): NotificationPayload {
    const actionMessages: Record<string, string> = {
      requested: 'has been submitted and requires your approval',
      approved: 'has been approved',
      rejected: 'has been rejected',
      timeout: 'has timed out and requires immediate attention',
      completed: 'has been completed successfully',
    };

    const message = `${context.workflowType} ${actionMessages[context.action] || 'requires attention'}.`;

    return {
      type: 'email',
      recipients,
      subject: `Workflow Notification: ${context.workflowType}`,
      message,
      template: `workflow-${context.action}`,
      data: {
        ...context,
        detailsUrl: `/workflows/${context.workflowId}`,
      },
      priority: context.action === 'timeout' ? 'high' : 'normal',
    };
  }

  /**
   * Dispatch notification through appropriate channel
   */
  private async dispatchNotification(notification: NotificationPayload): Promise<void> {
    const queueName = this.getQueueName(notification.type);
    const jobType = this.getJobType(notification.type);

    await this.jobService.addJob(
      queueName,
      jobType,
      {
        ...notification,
        sentAt: new Date(),
      },
      {
        attempts: 3,
        backoff: {
          type: 'exponential',
          delay: 2000,
        },
        removeOnComplete: 100,
      }
    );
  }

  /**
   * Log notification to database
   */
  private async logNotification(
    notification: NotificationPayload,
    context: WorkflowNotificationContext
  ): Promise<void> {
    try {
      await executeInTransaction(async (prisma) => {
        await prisma.systemLog.create({
          data: {
            level: 'INFO',
            message: `Notification sent: ${notification.subject}`,
            metadata: {
              type: notification.type,
              recipients: notification.recipients,
              workflowId: context.workflowId,
              workflowType: context.workflowType,
              action: context.action,
              priority: notification.priority,
            },
          },
        });
      });
    } catch (error) {
      console.error('Failed to log notification:', error);
    }
  }

  /**
   * Get queue name for notification type
   */
  private getQueueName(type: string): string {
    switch (type) {
      case 'email':
        return 'email-notifications';
      case 'sms':
        return 'sms-notifications';
      case 'push':
        return 'push-notifications';
      case 'in_app':
        return 'in-app-notifications';
      default:
        return 'email-notifications';
    }
  }

  /**
   * Get job type for notification type
   */
  private getJobType(type: string): string {
    switch (type) {
      case 'email':
        return 'send-email';
      case 'sms':
        return 'send-sms';
      case 'push':
        return 'send-push-notification';
      case 'in_app':
        return 'send-in-app-notification';
      default:
        return 'send-email';
    }
  }

  /**
   * Track notification delivery status
   */
  async trackDeliveryStatus(
    notificationId: string,
    status: 'sent' | 'delivered' | 'failed' | 'bounced',
    error?: string
  ): Promise<void> {
    try {
      await executeInTransaction(async (prisma) => {
        await prisma.systemLog.create({
          data: {
            level: status === 'failed' || status === 'bounced' ? 'ERROR' : 'INFO',
            message: `Notification ${status}: ${notificationId}`,
            metadata: {
              notificationId,
              status,
              error,
              timestamp: new Date(),
            },
          },
        });
      });
    } catch (error) {
      console.error('Failed to track notification status:', error);
    }
  }
}

export default NotificationDispatcher;
