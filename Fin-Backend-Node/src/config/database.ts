import { PrismaClient } from '@prisma/client';
import { logger } from '@utils/logger';
import { config } from './index';

// Prisma Client singleton
let prisma: PrismaClient;

/**
 * Get Prisma Client instance with connection pooling
 */
export const getPrismaClient = (): PrismaClient => {
  if (!prisma) {
    prisma = new PrismaClient({
      datasources: {
        db: {
          url: config.DATABASE_URL,
        },
      },
      log: [
        {
          emit: 'event',
          level: 'query',
        },
        {
          emit: 'event',
          level: 'error',
        },
        {
          emit: 'event',
          level: 'warn',
        },
      ],
    });

    // Log queries in development
    if (config.NODE_ENV === 'development') {
      prisma.$on('query' as never, (e: any) => {
        logger.debug('Prisma Query', {
          query: e.query,
          params: e.params,
          duration: `${e.duration}ms`,
        });
      });
    }

    // Log errors
    prisma.$on('error' as never, (e: any) => {
      logger.error('Prisma Error', {
        message: e.message,
        target: e.target,
      });
    });

    // Log warnings
    prisma.$on('warn' as never, (e: any) => {
      logger.warn('Prisma Warning', {
        message: e.message,
      });
    });

    logger.info('Prisma Client initialized');
  }

  return prisma;
};

/**
 * Connect to database
 */
export const connectDatabase = async (): Promise<void> => {
  try {
    const client = getPrismaClient();
    await client.$connect();
    logger.info('Database connected successfully');
  } catch (error) {
    logger.error('Failed to connect to database', error);
    throw error;
  }
};

/**
 * Disconnect from database
 */
export const disconnectDatabase = async (): Promise<void> => {
  try {
    if (prisma) {
      await prisma.$disconnect();
      logger.info('Database disconnected successfully');
    }
  } catch (error) {
    logger.error('Failed to disconnect from database', error);
    throw error;
  }
};

/**
 * Check database health
 */
export const checkDatabaseHealth = async (): Promise<boolean> => {
  try {
    const client = getPrismaClient();
    await client.$queryRaw`SELECT 1`;
    return true;
  } catch (error) {
    logger.error('Database health check failed', error);
    return false;
  }
};

/**
 * Execute in transaction
 */
export const executeInTransaction = async <T>(
  callback: (tx: PrismaClient) => Promise<T>
): Promise<T> => {
  const client = getPrismaClient();
  return client.$transaction(async (tx) => {
    return callback(tx as PrismaClient);
  });
};

export default getPrismaClient;
