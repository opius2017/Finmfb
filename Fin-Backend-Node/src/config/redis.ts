import Redis, { RedisOptions } from 'ioredis';
import { config } from './index';
import { logger } from '@utils/logger';

// Redis client singleton
let redisClient: Redis | null = null;

/**
 * Redis connection options
 */
const redisOptions: RedisOptions = {
  host: config.REDIS_HOST,
  port: config.REDIS_PORT,
  password: config.REDIS_PASSWORD,
  db: config.REDIS_DB,
  retryStrategy: (times: number) => {
    const delay = Math.min(times * 50, 2000);
    logger.warn(`Redis connection retry attempt ${times}, delay: ${delay}ms`);
    return delay;
  },
  maxRetriesPerRequest: 3,
  enableReadyCheck: true,
  lazyConnect: false,
};

/**
 * Get Redis client instance
 */
export const getRedisClient = (): Redis => {
  if (!redisClient) {
    redisClient = new Redis(redisOptions);

    // Connection events
    redisClient.on('connect', () => {
      logger.info('Redis client connecting...');
    });

    redisClient.on('ready', () => {
      logger.info('✅ Redis client connected and ready');
    });

    redisClient.on('error', (error) => {
      logger.error('Redis client error:', error);
    });

    redisClient.on('close', () => {
      logger.warn('Redis client connection closed');
    });

    redisClient.on('reconnecting', () => {
      logger.info('Redis client reconnecting...');
    });

    redisClient.on('end', () => {
      logger.warn('Redis client connection ended');
    });
  }

  return redisClient;
};

/**
 * Connect to Redis
 */
export const connectRedis = async (): Promise<void> => {
  try {
    const client = getRedisClient();
    await client.ping();
    logger.info('Redis connection established successfully');
  } catch (error) {
    logger.error('Failed to connect to Redis:', error);
    throw error;
  }
};

/**
 * Disconnect from Redis
 */
export const disconnectRedis = async (): Promise<void> => {
  try {
    if (redisClient) {
      await redisClient.quit();
      redisClient = null;
      logger.info('Redis disconnected successfully');
    }
  } catch (error) {
    logger.error('Failed to disconnect from Redis:', error);
    throw error;
  }
};

/**
 * Check Redis health
 */
export const checkRedisHealth = async (): Promise<boolean> => {
  try {
    const client = getRedisClient();
    const result = await client.ping();
    return result === 'PONG';
  } catch (error) {
    logger.error('Redis health check failed:', error);
    return false;
  }
};

/**
 * Get Redis info
 */
export const getRedisInfo = async (): Promise<{
  connected: boolean;
  uptime?: number;
  usedMemory?: string;
  connectedClients?: number;
}> => {
  try {
    const client = getRedisClient();
    const info = await client.info();
    
    // Parse info string
    const lines = info.split('\r\n');
    const data: any = {};
    
    lines.forEach((line) => {
      if (line && !line.startsWith('#')) {
        const [key, value] = line.split(':');
        if (key && value) {
          data[key] = value;
        }
      }
    });

    return {
      connected: true,
      uptime: parseInt(data.uptime_in_seconds || '0', 10),
      usedMemory: data.used_memory_human,
      connectedClients: parseInt(data.connected_clients || '0', 10),
    };
  } catch (error) {
    logger.error('Failed to get Redis info:', error);
    return { connected: false };
  }
};

/**
 * Flush Redis database (use with caution!)
 */
export const flushRedis = async (): Promise<void> => {
  if (config.NODE_ENV === 'production') {
    throw new Error('Cannot flush Redis in production');
  }

  try {
    const client = getRedisClient();
    await client.flushdb();
    logger.warn('Redis database flushed');
  } catch (error) {
    logger.error('Failed to flush Redis:', error);
    throw error;
  }
};

export default {
  getRedisClient,
  connectRedis,
  disconnectRedis,
  checkRedisHealth,
  getRedisInfo,
  flushRedis,
};
