import { executeInTransaction } from '@config/database';

/**
 * Query Performance Monitor
 */
export class QueryPerformanceMonitor {
  private static slowQueryThreshold = 1000; // 1 second
  private static queryStats = new Map<string, { count: number; totalTime: number; avgTime: number }>();

  /**
   * Monitor query execution time
   */
  static async monitorQuery<T>(
    queryName: string,
    queryFn: () => Promise<T>
  ): Promise<T> {
    const startTime = Date.now();

    try {
      const result = await queryFn();
      const duration = Date.now() - startTime;

      // Log slow queries
      if (duration > this.slowQueryThreshold) {
        console.warn(`Slow query detected: ${queryName} took ${duration}ms`);
        await this.logSlowQuery(queryName, duration);
      }

      // Update stats
      this.updateStats(queryName, duration);

      return result;
    } catch (error) {
      const duration = Date.now() - startTime;
      console.error(`Query failed: ${queryName} after ${duration}ms`, error);
      throw error;
    }
  }

  /**
   * Update query statistics
   */
  private static updateStats(queryName: string, duration: number): void {
    const stats = this.queryStats.get(queryName) || { count: 0, totalTime: 0, avgTime: 0 };
    stats.count++;
    stats.totalTime += duration;
    stats.avgTime = stats.totalTime / stats.count;
    this.queryStats.set(queryName, stats);
  }

  /**
   * Log slow query
   */
  private static async logSlowQuery(queryName: string, duration: number): Promise<void> {
    try {
      await executeInTransaction(async (prisma) => {
        await prisma.systemLog.create({
          data: {
            level: 'WARN',
            message: `Slow query: ${queryName}`,
            context: {
              queryName,
              duration,
              threshold: this.slowQueryThreshold,
              timestamp: new Date(),
            },
          },
        });
      });
    } catch (error) {
      console.error('Failed to log slow query:', error);
    }
  }

  /**
   * Get query statistics
   */
  static getStats(): Map<string, { count: number; totalTime: number; avgTime: number }> {
    return this.queryStats;
  }

  /**
   * Reset statistics
   */
  static resetStats(): void {
    this.queryStats.clear();
  }

  /**
   * Set slow query threshold
   */
  static setSlowQueryThreshold(ms: number): void {
    this.slowQueryThreshold = ms;
  }
}

/**
 * Pagination Helper
 */
export class PaginationHelper {
  /**
   * Calculate pagination metadata
   */
  static getPaginationMeta(page: number, limit: number, total: number) {
    return {
      page,
      limit,
      total,
      totalPages: Math.ceil(total / limit),
      hasNextPage: page < Math.ceil(total / limit),
      hasPrevPage: page > 1,
    };
  }

  /**
   * Get skip value for pagination
   */
  static getSkip(page: number, limit: number): number {
    return (page - 1) * limit;
  }

  /**
   * Validate pagination parameters
   */
  static validateParams(page: number, limit: number): { page: number; limit: number } {
    const validPage = Math.max(1, page);
    const validLimit = Math.min(Math.max(1, limit), 100); // Max 100 items per page
    return { page: validPage, limit: validLimit };
  }
}

/**
 * Response Compression Helper
 */
export class CompressionHelper {
  /**
   * Check if response should be compressed
   */
  static shouldCompress(contentType: string, size: number): boolean {
    const compressibleTypes = [
      'text/',
      'application/json',
      'application/javascript',
      'application/xml',
    ];

    const isCompressible = compressibleTypes.some((type) => contentType.includes(type));
    const isLargeEnough = size > 1024; // Only compress if > 1KB

    return isCompressible && isLargeEnough;
  }
}

/**
 * Batch Processing Helper
 */
export class BatchProcessor {
  /**
   * Process items in batches
   */
  static async processBatch<T, R>(
    items: T[],
    batchSize: number,
    processor: (batch: T[]) => Promise<R[]>
  ): Promise<R[]> {
    const results: R[] = [];

    for (let i = 0; i < items.length; i += batchSize) {
      const batch = items.slice(i, i + batchSize);
      const batchResults = await processor(batch);
      results.push(...batchResults);
    }

    return results;
  }

  /**
   * Process items in parallel batches
   */
  static async processParallelBatches<T, R>(
    items: T[],
    batchSize: number,
    concurrency: number,
    processor: (batch: T[]) => Promise<R[]>
  ): Promise<R[]> {
    const batches: T[][] = [];
    for (let i = 0; i < items.length; i += batchSize) {
      batches.push(items.slice(i, i + batchSize));
    }

    const results: R[] = [];
    for (let i = 0; i < batches.length; i += concurrency) {
      const concurrentBatches = batches.slice(i, i + concurrency);
      const batchPromises = concurrentBatches.map(processor);
      const batchResults = await Promise.all(batchPromises);
      results.push(...batchResults.flat());
    }

    return results;
  }
}

/**
 * Field Selection Helper (Sparse Fieldsets)
 */
export class FieldSelector {
  /**
   * Parse field selection from query string
   */
  static parseFields(fieldsQuery?: string): Record<string, boolean> | undefined {
    if (!fieldsQuery) return undefined;

    const fields = fieldsQuery.split(',').map((f) => f.trim());
    const select: Record<string, boolean> = {};

    fields.forEach((field) => {
      select[field] = true;
    });

    return select;
  }

  /**
   * Apply field selection to object
   */
  static selectFields<T extends Record<string, any>>(
    obj: T,
    fields?: string[]
  ): Partial<T> {
    if (!fields || fields.length === 0) return obj;

    const selected: Partial<T> = {};
    fields.forEach((field) => {
      if (field in obj) {
        selected[field as keyof T] = obj[field];
      }
    });

    return selected;
  }
}

/**
 * Response Time Tracker
 */
export class ResponseTimeTracker {
  private static responseTimes: number[] = [];
  private static maxSamples = 1000;

  /**
   * Record response time
   */
  static record(duration: number): void {
    this.responseTimes.push(duration);

    // Keep only last N samples
    if (this.responseTimes.length > this.maxSamples) {
      this.responseTimes.shift();
    }
  }

  /**
   * Get statistics
   */
  static getStats(): {
    count: number;
    avg: number;
    min: number;
    max: number;
    p50: number;
    p95: number;
    p99: number;
  } {
    if (this.responseTimes.length === 0) {
      return { count: 0, avg: 0, min: 0, max: 0, p50: 0, p95: 0, p99: 0 };
    }

    const sorted = [...this.responseTimes].sort((a, b) => a - b);
    const count = sorted.length;
    const sum = sorted.reduce((a, b) => a + b, 0);

    return {
      count,
      avg: sum / count,
      min: sorted[0],
      max: sorted[count - 1],
      p50: sorted[Math.floor(count * 0.5)],
      p95: sorted[Math.floor(count * 0.95)],
      p99: sorted[Math.floor(count * 0.99)],
    };
  }

  /**
   * Reset statistics
   */
  static reset(): void {
    this.responseTimes = [];
  }
}

/**
 * Memory Usage Monitor
 */
export class MemoryMonitor {
  /**
   * Get memory usage
   */
  static getUsage(): {
    rss: string;
    heapTotal: string;
    heapUsed: string;
    external: string;
    arrayBuffers: string;
  } {
    const usage = process.memoryUsage();
    return {
      rss: this.formatBytes(usage.rss),
      heapTotal: this.formatBytes(usage.heapTotal),
      heapUsed: this.formatBytes(usage.heapUsed),
      external: this.formatBytes(usage.external),
      arrayBuffers: this.formatBytes(usage.arrayBuffers),
    };
  }

  /**
   * Format bytes to human readable
   */
  private static formatBytes(bytes: number): string {
    const units = ['B', 'KB', 'MB', 'GB'];
    let size = bytes;
    let unitIndex = 0;

    while (size >= 1024 && unitIndex < units.length - 1) {
      size /= 1024;
      unitIndex++;
    }

    return `${size.toFixed(2)} ${units[unitIndex]}`;
  }

  /**
   * Check if memory usage is high
   */
  static isMemoryHigh(threshold: number = 0.9): boolean {
    const usage = process.memoryUsage();
    const heapUsedPercent = usage.heapUsed / usage.heapTotal;
    return heapUsedPercent > threshold;
  }

  /**
   * Log memory usage
   */
  static logUsage(): void {
    const usage = this.getUsage();
    console.log('Memory Usage:', usage);
  }
}

/**
 * Database Connection Pool Monitor
 */
export class ConnectionPoolMonitor {
  private static poolStats = {
    active: 0,
    idle: 0,
    waiting: 0,
  };

  /**
   * Update pool statistics
   */
  static updateStats(active: number, idle: number, waiting: number): void {
    this.poolStats = { active, idle, waiting };
  }

  /**
   * Get pool statistics
   */
  static getStats(): { active: number; idle: number; waiting: number } {
    return { ...this.poolStats };
  }

  /**
   * Check if pool is healthy
   */
  static isHealthy(): boolean {
    const { active, idle, waiting } = this.poolStats;
    const total = active + idle;
    const maxConnections = parseInt(process.env.DATABASE_POOL_MAX || '50');

    // Pool is unhealthy if:
    // 1. Too many waiting connections
    // 2. Pool is at max capacity
    return waiting < 10 && total < maxConnections;
  }
}

/**
 * Performance Metrics Collector
 */
export class PerformanceMetrics {
  private static metrics = {
    requests: 0,
    errors: 0,
    totalResponseTime: 0,
    cacheHits: 0,
    cacheMisses: 0,
  };

  /**
   * Record request
   */
  static recordRequest(duration: number, isError: boolean = false): void {
    this.metrics.requests++;
    this.metrics.totalResponseTime += duration;
    if (isError) {
      this.metrics.errors++;
    }
  }

  /**
   * Record cache hit/miss
   */
  static recordCache(isHit: boolean): void {
    if (isHit) {
      this.metrics.cacheHits++;
    } else {
      this.metrics.cacheMisses++;
    }
  }

  /**
   * Get metrics
   */
  static getMetrics(): {
    requests: number;
    errors: number;
    errorRate: number;
    avgResponseTime: number;
    cacheHitRate: number;
  } {
    const { requests, errors, totalResponseTime, cacheHits, cacheMisses } = this.metrics;
    const totalCache = cacheHits + cacheMisses;

    return {
      requests,
      errors,
      errorRate: requests > 0 ? (errors / requests) * 100 : 0,
      avgResponseTime: requests > 0 ? totalResponseTime / requests : 0,
      cacheHitRate: totalCache > 0 ? (cacheHits / totalCache) * 100 : 0,
    };
  }

  /**
   * Reset metrics
   */
  static reset(): void {
    this.metrics = {
      requests: 0,
      errors: 0,
      totalResponseTime: 0,
      cacheHits: 0,
      cacheMisses: 0,
    };
  }
}

/**
 * Debounce utility
 */
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout | null = null;

  return function (...args: Parameters<T>) {
    if (timeout) clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), wait);
  };
}

/**
 * Throttle utility
 */
export function throttle<T extends (...args: any[]) => any>(
  func: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle: boolean;

  return function (...args: Parameters<T>) {
    if (!inThrottle) {
      func(...args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}

/**
 * Memoization utility
 */
export function memoize<T extends (...args: any[]) => any>(
  func: T,
  ttl: number = 60000
): T {
  const cache = new Map<string, { value: any; expiry: number }>();

  return ((...args: Parameters<T>) => {
    const key = JSON.stringify(args);
    const cached = cache.get(key);

    if (cached && cached.expiry > Date.now()) {
      return cached.value;
    }

    const result = func(...args);
    cache.set(key, { value: result, expiry: Date.now() + ttl });

    return result;
  }) as T;
}
