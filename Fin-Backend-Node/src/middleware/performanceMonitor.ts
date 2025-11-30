import { Request, Response, NextFunction } from 'express';
import { ResponseTimeTracker, PerformanceMetrics, MemoryMonitor } from '@utils/performance';

/**
 * Performance monitoring middleware
 */
export function performanceMonitor() {
  return (req: Request, res: Response, next: NextFunction) => {
    const startTime = Date.now();

    // Capture response
    res.on('finish', () => {
      const duration = Date.now() - startTime;
      const isError = res.statusCode >= 400;

      // Record metrics
      ResponseTimeTracker.record(duration);
      PerformanceMetrics.recordRequest(duration, isError);

      // Log slow requests
      if (duration > 1000) {
        console.warn(`Slow request: ${req.method} ${req.path} took ${duration}ms`);
      }

      // Add performance headers
      res.setHeader('X-Response-Time', `${duration}ms`);
    });

    next();
  };
}

/**
 * Memory monitoring middleware
 */
export function memoryMonitor() {
  // Check memory every 30 seconds
  setInterval(() => {
    if (MemoryMonitor.isMemoryHigh(0.85)) {
      console.warn('High memory usage detected:', MemoryMonitor.getUsage());
      
      // Trigger garbage collection if available
      if (global.gc) {
        global.gc();
        console.log('Garbage collection triggered');
      }
    }
  }, 30000);

  return (req: Request, res: Response, next: NextFunction) => next();
}

/**
 * Request timeout middleware
 */
export function requestTimeout(timeout: number = 30000) {
  return (req: Request, res: Response, next: NextFunction) => {
    const timer = setTimeout(() => {
      if (!res.headersSent) {
        res.status(408).json({
          success: false,
          error: {
            code: 'REQUEST_TIMEOUT',
            message: 'Request timeout',
          },
        });
      }
    }, timeout);

    res.on('finish', () => clearTimeout(timer));
    next();
  };
}

/**
 * Response caching middleware
 */
export function responseCache(ttl: number = 60) {
  const cache = new Map<string, { data: any; expiry: number }>();

  return (req: Request, res: Response, next: NextFunction) => {
    // Only cache GET requests
    if (req.method !== 'GET') {
      return next();
    }

    const key = req.originalUrl;
    const cached = cache.get(key);

    if (cached && cached.expiry > Date.now()) {
      PerformanceMetrics.recordCache(true);
      return res.json(cached.data);
    }

    PerformanceMetrics.recordCache(false);

    // Override res.json to cache response
    const originalJson = res.json;
    res.json = function (data: any) {
      cache.set(key, {
        data,
        expiry: Date.now() + ttl * 1000,
      });
      return originalJson.call(this, data);
    };

    next();
  };
}

/**
 * Compression middleware configuration
 */
export const compressionOptions = {
  filter: (req: Request, res: Response) => {
    if (req.headers['x-no-compression']) {
      return false;
    }
    return true;
  },
  threshold: 1024, // Only compress responses > 1KB
  level: 6, // Compression level (0-9)
};

/**
 * Performance metrics endpoint
 */
export function getPerformanceMetrics(req: Request, res: Response): void {
  const metrics = {
    responseTime: ResponseTimeTracker.getStats(),
    performance: PerformanceMetrics.getMetrics(),
    memory: MemoryMonitor.getUsage(),
    uptime: process.uptime(),
    timestamp: new Date().toISOString(),
  };

  res.json({
    success: true,
    data: metrics,
  });
}
