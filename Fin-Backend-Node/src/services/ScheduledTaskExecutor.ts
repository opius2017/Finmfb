import cron from 'node-cron';
import { JobService } from './JobService';
import { executeInTransaction } from '@config/database';
import { ExtendedCalculationService } from './CalculationService';

export interface ScheduledTask {
  id: string;
  name: string;
  description: string;
  cronExpression: string;
  jobType: string;
  payload: Record<string, any>;
  isActive: boolean;
  lastRunAt?: Date;
  nextRunAt?: Date;
}

export class ScheduledTaskExecutor {
  private jobService: JobService;
  private calculationService: ExtendedCalculationService;
  private scheduledTasks: Map<string, cron.ScheduledTask> = new Map();

  constructor() {
    this.jobService = new JobService();
    this.calculationService = new ExtendedCalculationService();
  }

  /**
   * Initialize and start all scheduled tasks
   */
  async initialize(): Promise<void> {
    console.log('Initializing scheduled tasks...');

    // Register default scheduled tasks
    await this.registerDefaultTasks();

    // Load custom tasks from database
    await this.loadCustomTasks();

    console.log(`Initialized ${this.scheduledTasks.size} scheduled tasks`);
  }

  /**
   * Register default scheduled tasks
   */
  private async registerDefaultTasks(): Promise<void> {
    // Daily interest posting (runs at 1 AM every day)
    this.scheduleTask({
      id: 'daily-interest-posting',
      name: 'Daily Interest Posting',
      description: 'Post interest to all active savings accounts and loans',
      cronExpression: '0 1 * * *',
      jobType: 'post-interest',
      payload: {
        accountTypes: ['savings', 'fixed_deposit'],
      },
      isActive: true,
    });

    // Loan payment reminders (runs at 9 AM every day)
    this.scheduleTask({
      id: 'loan-payment-reminders',
      name: 'Loan Payment Reminders',
      description: 'Send reminders for upcoming loan payments',
      cronExpression: '0 9 * * *',
      jobType: 'send-loan-reminders',
      payload: {
        reminderType: 'upcoming',
        daysBeforeDue: 3,
      },
      isActive: true,
    });

    // Overdue loan reminders (runs at 10 AM every day)
    this.scheduleTask({
      id: 'overdue-loan-reminders',
      name: 'Overdue Loan Reminders',
      description: 'Send reminders for overdue loan payments',
      cronExpression: '0 10 * * *',
      jobType: 'send-loan-reminders',
      payload: {
        reminderType: 'overdue',
        daysOverdue: 1,
      },
      isActive: true,
    });

    // Monthly financial reports (runs at 2 AM on the 1st of every month)
    this.scheduleTask({
      id: 'monthly-financial-reports',
      name: 'Monthly Financial Reports',
      description: 'Generate and send monthly financial reports',
      cronExpression: '0 2 1 * *',
      jobType: 'generate-financial-report',
      payload: {
        reportType: 'monthly',
        format: 'pdf',
      },
      isActive: true,
    });

    // Weekly backup (runs at 3 AM every Sunday)
    this.scheduleTask({
      id: 'weekly-backup',
      name: 'Weekly System Backup',
      description: 'Perform weekly system backup',
      cronExpression: '0 3 * * 0',
      jobType: 'system-backup',
      payload: {
        backupType: 'full',
      },
      isActive: true,
    });

    // Daily log cleanup (runs at 4 AM every day)
    this.scheduleTask({
      id: 'daily-log-cleanup',
      name: 'Daily Log Cleanup',
      description: 'Clean up old system logs',
      cronExpression: '0 4 * * *',
      jobType: 'cleanup-logs',
      payload: {
        olderThanDays: 90,
      },
      isActive: true,
    });

    // Recurring transactions (runs every hour)
    this.scheduleTask({
      id: 'recurring-transactions',
      name: 'Process Recurring Transactions',
      description: 'Process scheduled recurring transactions',
      cronExpression: '0 * * * *',
      jobType: 'process-recurring-transactions',
      payload: {},
      isActive: true,
    });

    // Account dormancy check (runs at 5 AM on the 1st of every month)
    this.scheduleTask({
      id: 'account-dormancy-check',
      name: 'Account Dormancy Check',
      description: 'Check and mark dormant accounts',
      cronExpression: '0 5 1 * *',
      jobType: 'check-dormant-accounts',
      payload: {
        inactiveDays: 180,
      },
      isActive: true,
    });

    // Loan maturity check (runs at 6 AM every day)
    this.scheduleTask({
      id: 'loan-maturity-check',
      name: 'Loan Maturity Check',
      description: 'Check for maturing loans and send notifications',
      cronExpression: '0 6 * * *',
      jobType: 'check-loan-maturity',
      payload: {
        daysBeforeMaturity: 7,
      },
      isActive: true,
    });
  }

  /**
   * Load custom tasks from database
   */
  private async loadCustomTasks(): Promise<void> {
    try {
      const customTasks = await executeInTransaction(async (prisma) => {
        // Assuming we have a scheduledTasks table
        // This is a placeholder - adjust based on actual schema
        return [];
      });

      for (const task of customTasks) {
        this.scheduleTask(task as ScheduledTask);
      }
    } catch (error) {
      console.error('Failed to load custom tasks:', error);
    }
  }

  /**
   * Schedule a task
   */
  scheduleTask(task: ScheduledTask): void {
    if (!task.isActive) {
      console.log(`Task ${task.name} is inactive, skipping...`);
      return;
    }

    try {
      // Validate cron expression
      if (!cron.validate(task.cronExpression)) {
        console.error(`Invalid cron expression for task ${task.name}: ${task.cronExpression}`);
        return;
      }

      // Create scheduled task
      const scheduledTask = cron.schedule(
        task.cronExpression,
        async () => {
          await this.executeTask(task);
        },
        {
          scheduled: true,
          timezone: process.env.TZ || 'UTC',
        }
      );

      this.scheduledTasks.set(task.id, scheduledTask);
      console.log(`Scheduled task: ${task.name} (${task.cronExpression})`);
    } catch (error) {
      console.error(`Failed to schedule task ${task.name}:`, error);
    }
  }

  /**
   * Execute a scheduled task
   */
  private async executeTask(task: ScheduledTask): Promise<void> {
    console.log(`Executing scheduled task: ${task.name}`);

    try {
      // Update last run time
      await this.updateTaskRunTime(task.id);

      // Queue the job
      await this.jobService.addJob(
        this.getQueueForJobType(task.jobType),
        task.jobType,
        task.payload,
        {
          attempts: 3,
          backoff: {
            type: 'exponential',
            delay: 5000,
          },
        }
      );

      // Log successful execution
      await this.logTaskExecution(task, 'success');
    } catch (error) {
      console.error(`Failed to execute task ${task.name}:`, error);
      await this.logTaskExecution(task, 'failed', error instanceof Error ? error.message : 'Unknown error');
    }
  }

  /**
   * Stop a scheduled task
   */
  stopTask(taskId: string): void {
    const task = this.scheduledTasks.get(taskId);
    if (task) {
      task.stop();
      this.scheduledTasks.delete(taskId);
      console.log(`Stopped task: ${taskId}`);
    }
  }

  /**
   * Stop all scheduled tasks
   */
  stopAll(): void {
    console.log('Stopping all scheduled tasks...');
    this.scheduledTasks.forEach((task, id) => {
      task.stop();
    });
    this.scheduledTasks.clear();
    console.log('All scheduled tasks stopped');
  }

  /**
   * Get list of all scheduled tasks
   */
  getScheduledTasks(): Array<{ id: string; name: string; cronExpression: string; isActive: boolean }> {
    const tasks: Array<{ id: string; name: string; cronExpression: string; isActive: boolean }> = [];
    
    this.scheduledTasks.forEach((_, id) => {
      // Would fetch full task details from storage
      tasks.push({
        id,
        name: id,
        cronExpression: '',
        isActive: true,
      });
    });

    return tasks;
  }

  /**
   * Manually trigger a task
   */
  async triggerTask(taskId: string): Promise<void> {
    // Would fetch task details and execute
    console.log(`Manually triggering task: ${taskId}`);
  }

  /**
   * Update task run time
   */
  private async updateTaskRunTime(taskId: string): Promise<void> {
    try {
      await executeInTransaction(async (prisma) => {
        await prisma.systemLog.create({
          data: {
            level: 'INFO',
            message: `Scheduled task executed: ${taskId}`,
            metadata: {
              taskId,
              executedAt: new Date(),
            },
          },
        });
      });
    } catch (error) {
      console.error('Failed to update task run time:', error);
    }
  }

  /**
   * Log task execution
   */
  private async logTaskExecution(
    task: ScheduledTask,
    status: 'success' | 'failed',
    error?: string
  ): Promise<void> {
    try {
      await executeInTransaction(async (prisma) => {
        await prisma.systemLog.create({
          data: {
            level: status === 'success' ? 'INFO' : 'ERROR',
            message: `Scheduled task ${status}: ${task.name}`,
            metadata: {
              taskId: task.id,
              taskName: task.name,
              jobType: task.jobType,
              status,
              error,
              executedAt: new Date(),
            },
          },
        });
      });
    } catch (error) {
      console.error('Failed to log task execution:', error);
    }
  }

  /**
   * Get queue name for job type
   */
  private getQueueForJobType(jobType: string): string {
    const queueMap: Record<string, string> = {
      'post-interest': 'interest-posting',
      'send-loan-reminders': 'loan-reminders',
      'generate-financial-report': 'report-generation',
      'system-backup': 'system-maintenance',
      'cleanup-logs': 'system-maintenance',
      'process-recurring-transactions': 'recurring-transactions',
      'check-dormant-accounts': 'account-maintenance',
      'check-loan-maturity': 'loan-maintenance',
    };

    return queueMap[jobType] || 'system-maintenance';
  }
}

export default ScheduledTaskExecutor;
