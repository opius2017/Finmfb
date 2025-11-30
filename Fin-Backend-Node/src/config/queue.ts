import { Queue, Worker, QueueEvents, Job } from 'bullmq';
import { getRedisClient } from './redis';
import { logger } from '@utils/logger';
import { config } from './index';

// Queue names
export enum QueueName {
  REPORTS = 'reports',
  EMAILS = 'emails',
  IMPORTS = 'imports',
  EXPORTS = 'exports',
  NOTIFICATIONS = 'notifications',
  CALCULATIONS = 'calculations',
  INTEGRATIONS = 'integrations',
}

// Job priorities
export enum JobPriority {
  CRITICAL = 1,
  HIGH = 2,
  NORMAL = 3,
  LOW = 4,
}

// Queue configuration
const queueConfig = {
  connection: getRedisClient(),
  defaultJobOptions: {
    attempts: config.JOB_RETRY_ATTEMPTS,
    backoff: {
      type: 'exponential' as const,
      delay: config.JOB_RETRY_DELAY,
    },
    removeOnComplete: {
      count: 100, // Keep last 100 completed jobs
      age: 24 * 3600, // Keep for 24 hours
    },
    removeOnFail: {
      count: 500, // Keep last 500 failed jobs
      age: 7 * 24 * 3600, // Keep for 7 days
    },
  },
};

// Queue instances
const queues = new Map<string, Queue>();

/**
 * Get or create queue
 */
export const getQueue = (name: QueueName): Queue => {
  if (!queues.has(name)) {
    const queue = new Queue(name, queueConfig);
    
    queue.on('error', (error) => {
      logger.error(`Queue ${name} error:`, error);
    });

    queues.set(name, queue);
    logger.info(`Queue ${name} initialized`);
  }

  return queues.get(name)!;
};

/**
 * Add job to queue
 */
export const addJob = async <T = any>(
  queueName: QueueName,
  jobName: string,
  data: T,
  options?: {
    priority?: JobPriority;
    delay?: number;
    attempts?: number;
    removeOnComplete?: boolean;
    removeOnFail?: boolean;
  }
): Promise<Job<T>> => {
  const queue = getQueue(queueName);
  
  const job = await queue.add(jobName, data, {
    priority: options?.priority || JobPriority.NORMAL,
    delay: options?.delay,
    attempts: options?.attempts,
    removeOnComplete: options?.removeOnComplete,
    removeOnFail: options?.removeOnFail,
  });

  logger.info(`Job added to queue ${queueName}:`, {
    jobId: job.id,
    jobName,
    priority: options?.priority,
  });

  return job;
};

/**
 * Create worker for queue
 */
export const createWorker = <T = any>(
  queueName: QueueName,
  processor: (job: Job<T>) => Promise<any>,
  options?: {
    concurrency?: number;
    limiter?: {
      max: number;
      duration: number;
    };
  }
): Worker<T> => {
  const worker = new Worker<T>(
    queueName,
    async (job) => {
      logger.info(`Processing job ${job.id} from queue ${queueName}:`, {
        jobName: job.name,
        attemptsMade: job.attemptsMade,
      });

      try {
        const result = await processor(job);
        
        logger.info(`Job ${job.id} completed successfully:`, {
          jobName: job.name,
          duration: Date.now() - job.timestamp,
        });

        return result;
      } catch (error) {
        logger.error(`Job ${job.id} failed:`, {
          jobName: job.name,
          error: error instanceof Error ? error.message : 'Unknown error',
          attemptsMade: job.attemptsMade,
        });
        throw error;
      }
    },
    {
      connection: getRedisClient(),
      concurrency: options?.concurrency || config.QUEUE_CONCURRENCY,
      limiter: options?.limiter,
    }
  );

  worker.on('completed', (job) => {
    logger.debug(`Worker completed job ${job.id} from queue ${queueName}`);
  });

  worker.on('failed', (job, error) => {
    logger.error(`Worker failed job ${job?.id} from queue ${queueName}:`, error);
  });

  worker.on('error', (error) => {
    logger.error(`Worker error in queue ${queueName}:`, error);
  });

  logger.info(`Worker created for queue ${queueName} with concurrency ${options?.concurrency || config.QUEUE_CONCURRENCY}`);

  return worker;
};

/**
 * Create queue events listener
 */
export const createQueueEvents = (queueName: QueueName): QueueEvents => {
  const queueEvents = new QueueEvents(queueName, {
    connection: getRedisClient(),
  });

  queueEvents.on('completed', ({ jobId }) => {
    logger.debug(`Job ${jobId} completed in queue ${queueName}`);
  });

  queueEvents.on('failed', ({ jobId, failedReason }) => {
    logger.error(`Job ${jobId} failed in queue ${queueName}:`, failedReason);
  });

  queueEvents.on('progress', ({ jobId, data }) => {
    logger.debug(`Job ${jobId} progress in queue ${queueName}:`, data);
  });

  return queueEvents;
};

/**
 * Get queue statistics
 */
export const getQueueStats = async (queueName: QueueName) => {
  const queue = getQueue(queueName);
  
  const [waiting, active, completed, failed, delayed] = await Promise.all([
    queue.getWaitingCount(),
    queue.getActiveCount(),
    queue.getCompletedCount(),
    queue.getFailedCount(),
    queue.getDelayedCount(),
  ]);

  return {
    waiting,
    active,
    completed,
    failed,
    delayed,
    total: waiting + active + completed + failed + delayed,
  };
};

/**
 * Get job by ID
 */
export const getJob = async (queueName: QueueName, jobId: string): Promise<Job | undefined> => {
  const queue = getQueue(queueName);
  return queue.getJob(jobId);
};

/**
 * Remove job
 */
export const removeJob = async (queueName: QueueName, jobId: string): Promise<void> => {
  const job = await getJob(queueName, jobId);
  if (job) {
    await job.remove();
    logger.info(`Job ${jobId} removed from queue ${queueName}`);
  }
};

/**
 * Retry failed job
 */
export const retryJob = async (queueName: QueueName, jobId: string): Promise<void> => {
  const job = await getJob(queueName, jobId);
  if (job) {
    await job.retry();
    logger.info(`Job ${jobId} retried in queue ${queueName}`);
  }
};

/**
 * Clean queue
 */
export const cleanQueue = async (
  queueName: QueueName,
  grace: number = 0,
  limit: number = 1000,
  type: 'completed' | 'failed' | 'delayed' = 'completed'
): Promise<string[]> => {
  const queue = getQueue(queueName);
  const jobs = await queue.clean(grace, limit, type);
  logger.info(`Cleaned ${jobs.length} ${type} jobs from queue ${queueName}`);
  return jobs;
};

/**
 * Pause queue
 */
export const pauseQueue = async (queueName: QueueName): Promise<void> => {
  const queue = getQueue(queueName);
  await queue.pause();
  logger.info(`Queue ${queueName} paused`);
};

/**
 * Resume queue
 */
export const resumeQueue = async (queueName: QueueName): Promise<void> => {
  const queue = getQueue(queueName);
  await queue.resume();
  logger.info(`Queue ${queueName} resumed`);
};

/**
 * Close all queues
 */
export const closeAllQueues = async (): Promise<void> => {
  for (const [name, queue] of queues) {
    await queue.close();
    logger.info(`Queue ${name} closed`);
  }
  queues.clear();
};

export default {
  QueueName,
  JobPriority,
  getQueue,
  addJob,
  createWorker,
  createQueueEvents,
  getQueueStats,
  getJob,
  removeJob,
  retryJob,
  cleanQueue,
  pauseQueue,
  resumeQueue,
  closeAllQueues,
};
