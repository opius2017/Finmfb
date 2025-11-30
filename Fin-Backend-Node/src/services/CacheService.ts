import { getRedisClient } from '@config/redis';
import { logger } from '@utils/logger';
import Redis from 'ioredis';

export class CacheService {
  private redis: Redis;
  private defaultTTL: number = 3600; // 1 hour default

  constructor() {
    this.redis = getRedisClient();
  }

  /**
   * Get value from cache
   */
  async get<T>(key: string): Promise<T | null> {
    try {
      const value = await this.redis.get(key);
      if (!value) {
        return null;
      }
      return JSON.parse(value) as T;
    } catch (error) {
      logger.error(`Cache get error for key ${key}:`, error);
      return null;
    }
  }

  /**
   * Set value in cache
   */
  async set<T>(key: string, value: T, ttl?: number): Promise<void> {
    try {
      const serialized = JSON.stringify(value);
      const expiry = ttl || this.defaultTTL;
      await this.redis.setex(key, expiry, serialized);
    } catch (error) {
      logger.error(`Cache set error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Delete value from cache
   */
  async delete(key: string): Promise<void> {
    try {
      await this.redis.del(key);
    } catch (error) {
      logger.error(`Cache delete error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Delete multiple keys
   */
  async deleteMany(keys: string[]): Promise<void> {
    try {
      if (keys.length > 0) {
        await this.redis.del(...keys);
      }
    } catch (error) {
      logger.error('Cache deleteMany error:', error);
      throw error;
    }
  }

  /**
   * Invalidate cache by pattern
   */
  async invalidatePattern(pattern: string): Promise<number> {
    try {
      const keys = await this.redis.keys(pattern);
      if (keys.length > 0) {
        await this.redis.del(...keys);
      }
      return keys.length;
    } catch (error) {
      logger.error(`Cache invalidatePattern error for pattern ${pattern}:`, error);
      throw error;
    }
  }

  /**
   * Get or set (cache-aside pattern)
   */
  async getOrSet<T>(
    key: string,
    factory: () => Promise<T>,
    ttl?: number
  ): Promise<T> {
    try {
      // Try to get from cache
      const cached = await this.get<T>(key);
      if (cached !== null) {
        return cached;
      }

      // If not in cache, get from factory
      const value = await factory();

      // Store in cache
      await this.set(key, value, ttl);

      return value;
    } catch (error) {
      logger.error(`Cache getOrSet error for key ${key}:`, error);
      // If cache fails, still return the value from factory
      return factory();
    }
  }

  /**
   * Check if key exists
   */
  async exists(key: string): Promise<boolean> {
    try {
      const result = await this.redis.exists(key);
      return result === 1;
    } catch (error) {
      logger.error(`Cache exists error for key ${key}:`, error);
      return false;
    }
  }

  /**
   * Get TTL for key
   */
  async getTTL(key: string): Promise<number> {
    try {
      return await this.redis.ttl(key);
    } catch (error) {
      logger.error(`Cache getTTL error for key ${key}:`, error);
      return -1;
    }
  }

  /**
   * Set expiration for key
   */
  async expire(key: string, ttl: number): Promise<void> {
    try {
      await this.redis.expire(key, ttl);
    } catch (error) {
      logger.error(`Cache expire error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Increment value
   */
  async increment(key: string, amount: number = 1): Promise<number> {
    try {
      return await this.redis.incrby(key, amount);
    } catch (error) {
      logger.error(`Cache increment error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Decrement value
   */
  async decrement(key: string, amount: number = 1): Promise<number> {
    try {
      return await this.redis.decrby(key, amount);
    } catch (error) {
      logger.error(`Cache decrement error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Add to set
   */
  async addToSet(key: string, ...members: string[]): Promise<number> {
    try {
      return await this.redis.sadd(key, ...members);
    } catch (error) {
      logger.error(`Cache addToSet error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Remove from set
   */
  async removeFromSet(key: string, ...members: string[]): Promise<number> {
    try {
      return await this.redis.srem(key, ...members);
    } catch (error) {
      logger.error(`Cache removeFromSet error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Get set members
   */
  async getSetMembers(key: string): Promise<string[]> {
    try {
      return await this.redis.smembers(key);
    } catch (error) {
      logger.error(`Cache getSetMembers error for key ${key}:`, error);
      return [];
    }
  }

  /**
   * Check if member in set
   */
  async isInSet(key: string, member: string): Promise<boolean> {
    try {
      const result = await this.redis.sismember(key, member);
      return result === 1;
    } catch (error) {
      logger.error(`Cache isInSet error for key ${key}:`, error);
      return false;
    }
  }

  /**
   * Add to hash
   */
  async setHash(key: string, field: string, value: string): Promise<void> {
    try {
      await this.redis.hset(key, field, value);
    } catch (error) {
      logger.error(`Cache setHash error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Get from hash
   */
  async getHash(key: string, field: string): Promise<string | null> {
    try {
      return await this.redis.hget(key, field);
    } catch (error) {
      logger.error(`Cache getHash error for key ${key}:`, error);
      return null;
    }
  }

  /**
   * Get all hash fields
   */
  async getAllHash(key: string): Promise<Record<string, string>> {
    try {
      return await this.redis.hgetall(key);
    } catch (error) {
      logger.error(`Cache getAllHash error for key ${key}:`, error);
      return {};
    }
  }

  /**
   * Delete hash field
   */
  async deleteHashField(key: string, field: string): Promise<void> {
    try {
      await this.redis.hdel(key, field);
    } catch (error) {
      logger.error(`Cache deleteHashField error for key ${key}:`, error);
      throw error;
    }
  }

  /**
   * Cache key builder with namespace
   */
  buildKey(namespace: string, ...parts: string[]): string {
    return `${namespace}:${parts.join(':')}`;
  }

  /**
   * Flush all cache (use with caution!)
   */
  async flushAll(): Promise<void> {
    try {
      await this.redis.flushdb();
      logger.warn('Cache flushed');
    } catch (error) {
      logger.error('Cache flush error:', error);
      throw error;
    }
  }
}

export default CacheService;
