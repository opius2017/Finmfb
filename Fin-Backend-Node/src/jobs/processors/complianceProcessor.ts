import { Job } from 'bull';
import ComplianceService from '../../services/ComplianceService';
import { logger } from '../../utils/logger';

/**
 * Process compliance deadline checks
 */
export async function processComplianceDeadlines(job: Job): Promise<void> {
  try {
    logger.info('Starting compliance deadline check job', { jobId: job.id });

    await ComplianceService.checkComplianceDeadlines();

    logger.info('Compliance deadline check completed successfully', { jobId: job.id });
  } catch (error) {
    logger.error('Error in compliance deadline check job', {
      jobId: job.id,
      error: (error as Error).message,
      stack: (error as Error).stack
    });
    throw error;
  }
}

/**
 * Process recurring checklist creation
 */
export async function processRecurringChecklists(job: Job): Promise<void> {
  try {
    logger.info('Starting recurring checklist creation job', { jobId: job.id });

    await ComplianceService.createRecurringChecklists();

    logger.info('Recurring checklist creation completed successfully', { jobId: job.id });
  } catch (error) {
    logger.error('Error in recurring checklist creation job', {
      jobId: job.id,
      error: (error as Error).message,
      stack: (error as Error).stack
    });
    throw error;
  }
}

/**
 * Process overdue checklist items
 */
export async function processOverdueChecklists(job: Job): Promise<void> {
  try {
    logger.info('Starting overdue checklist check job', { jobId: job.id });

    const overdueItems = await ComplianceService.getOverdueItems();

    logger.info('Overdue checklist check completed', {
      jobId: job.id,
      overdueCount: overdueItems.length
    });
  } catch (error) {
    logger.error('Error in overdue checklist check job', {
      jobId: job.id,
      error: (error as Error).message,
      stack: (error as Error).stack
    });
    throw error;
  }
}
