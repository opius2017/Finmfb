import { CacheService } from './CacheService';
import { getRedisInfo } from '@config/redis';
import { logger } from '@utils/logger';

export class CacheMetricsService {
  private cacheService = new CacheService();
  private metricsKey = 'cache:metrics';

  /**
   * Record cache hit
   */
  async recordHit(operation: string): Promise<void> {
    try {
      const key = this.cacheService.buildKey(this.metricsKey, 'hits', operation);
      await this.cacheService.increment(key);
    } catch (error) {
      logger.error('Failed to record cache hit:', error);
    }
  }

  /**
   * Record cache miss
   */
  async recordMiss(operation: string): Promise<void> {
    try {
      const key = this.cacheService.buildKey(this.metricsKey, 'misses', operation);
      await this.cacheService.increment(key);
    } catch (error) {
      logger.error('Failed to record cache miss:', error);
    }
  }

  /**
   * Get cache hit rate
   */
  async getHitRate(operation?: string): Promise<{
    hits: number;
    misses: number;
    total: number;
    hitRate: number;
  }> {
    try {
      const hitsKey = operation
        ? this.cacheService.buildKey(this.metricsKey, 'hits', operation)
        : this.cacheService.buildKey(this.metricsKey, 'hits', '*');
      
      const missesKey = operation
        ? this.cacheService.buildKey(this.metricsKey, 'misses', operation)
        : this.cacheService.buildKey(this.metricsKey, 'misses', '*');

      const hits = parseInt((await this.cacheService.get<string>(hitsKey)) || '0', 10);
      const misses = parseInt((await this.cacheService.get<string>(missesKey)) || '0', 10);
      const total = hits + misses;
      const hitRate = total > 0 ? (hits / total) * 100 : 0;

      return { hits, misses, total, hitRate };
    } catch (error) {
      logger.error('Failed to get cache hit rate:', error);
      return { hits: 0, misses: 0, total: 0, hitRate: 0 };
    }
  }

  /**
   * Get cache statistics
   */
  async getCacheStats(): Promise<{
    redis: any;
    hitRate: any;
    keyCount: number;
  }> {
    try {
      const [redisInfo, hitRate] = await Promise.all([
        getRedisInfo(),
        this.getHitRate(),
      ]);

      // Get key count by pattern
      const keyCount = await this.getKeyCount();

      return {
        redis: redisInfo,
        hitRate,
        keyCount,
      };
    } catch (error) {
      logger.error('Failed to get cache stats:', error);
      return {
        redis: { connected: false },
        hitRate: { hits: 0, misses: 0, total: 0, hitRate: 0 },
        keyCount: 0,
      };
    }
  }

  /**
   * Get key count
   */
  async getKeyCount(): Promise<number> {
    try {
      const redis = this.cacheService['redis'];
      const keys = await redis.keys('*');
      return keys.length;
    } catch (error) {
      logger.error('Failed to get key count:', error);
      return 0;
    }
  }

  /**
   * Get keys by pattern
   */
  async getKeysByPattern(pattern: string): Promise<string[]> {
    try {
      const redis = this.cacheService['redis'];
      return await redis.keys(pattern);
    } catch (error) {
      logger.error('Failed to get keys by pattern:', error);
      return [];
    }
  }

  /**
   * Get memory usage by pattern
   */
  async getMemoryUsage(pattern: string): Promise<{
    keys: number;
    totalMemory: number;
  }> {
    try {
      const redis = this.cacheService['redis'];
      const keys = await redis.keys(pattern);
      
      let totalMemory = 0;
      for (const key of keys) {
        const memory = await redis.memory('USAGE', key);
        totalMemory += memory || 0;
      }

      return {
        keys: keys.length,
        totalMemory,
      };
    } catch (error) {
      logger.error('Failed to get memory usage:', error);
      return { keys: 0, totalMemory: 0 };
    }
  }

  /**
   * Reset metrics
   */
  async resetMetrics(): Promise<void> {
    try {
      await this.cacheService.invalidatePattern(`${this.metricsKey}:*`);
      logger.info('Cache metrics reset');
    } catch (error) {
      logger.error('Failed to reset metrics:', error);
      throw error;
    }
  }

  /**
   * Get top keys by memory usage
   */
  async getTopKeysByMemory(limit: number = 10): Promise<Array<{
    key: string;
    memory: number;
    ttl: number;
  }>> {
    try {
      const redis = this.cacheService['redis'];
      const keys = await redis.keys('*');
      
      const keyStats = await Promise.all(
        keys.map(async (key) => {
          const [memory, ttl] = await Promise.all([
            redis.memory('USAGE', key),
            redis.ttl(key),
          ]);
          return { key, memory: memory || 0, ttl };
        })
      );

      return keyStats
        .sort((a, b) => b.memory - a.memory)
        .slice(0, limit);
    } catch (error) {
      logger.error('Failed to get top keys by memory:', error);
      return [];
    }
  }

  /**
   * Get cache health
   */
  async getCacheHealth(): Promise<{
    healthy: boolean;
    hitRate: number;
    memoryUsage?: string;
    keyCount: number;
  }> {
    try {
      const stats = await this.getCacheStats();
      
      return {
        healthy: stats.redis.connected,
        hitRate: stats.hitRate.hitRate,
        memoryUsage: stats.redis.usedMemory,
        keyCount: stats.keyCount,
      };
    } catch (error) {
      logger.error('Failed to get cache health:', error);
      return {
        healthy: false,
        hitRate: 0,
        keyCount: 0,
      };
    }
  }
}

export default CacheMetricsService;
