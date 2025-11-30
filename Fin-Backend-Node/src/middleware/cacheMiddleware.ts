import { Request, Response, NextFunction } from 'express';
import { CacheService } from '@services/CacheService';
import { logger } from '@utils/logger';

const cacheService = new CacheService();

/**
 * Cache middleware factory
 * Caches GET request responses
 */
export const cacheMiddleware = (options: {
  ttl?: number;
  keyGenerator?: (req: Request) => string;
  condition?: (req: Request) => boolean;
}) => {
  const { ttl = 300, keyGenerator, condition } = options;

  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    // Only cache GET requests
    if (req.method !== 'GET') {
      return next();
    }

    // Check condition if provided
    if (condition && !condition(req)) {
      return next();
    }

    try {
      // Generate cache key
      const cacheKey = keyGenerator
        ? keyGenerator(req)
        : cacheService.buildKey('http', req.method, req.originalUrl);

      // Try to get from cache
      const cached = await cacheService.get<any>(cacheKey);
      
      if (cached) {
        logger.debug(`Cache hit for key: ${cacheKey}`);
        res.setHeader('X-Cache', 'HIT');
        return res.status(200).json(cached);
      }

      logger.debug(`Cache miss for key: ${cacheKey}`);
      res.setHeader('X-Cache', 'MISS');

      // Store original json method
      const originalJson = res.json.bind(res);

      // Override json method to cache response
      res.json = function (body: any): Response {
        // Only cache successful responses
        if (res.statusCode >= 200 && res.statusCode < 300) {
          cacheService.set(cacheKey, body, ttl).catch((error) => {
            logger.error('Failed to cache response:', error);
          });
        }
        return originalJson(body);
      };

      next();
    } catch (error) {
      logger.error('Cache middleware error:', error);
      next();
    }
  };
};

/**
 * Cache invalidation middleware
 * Invalidates cache on write operations
 */
export const invalidateCacheMiddleware = (patterns: string[]) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    // Store original json method
    const originalJson = res.json.bind(res);

    // Override json method to invalidate cache after response
    res.json = function (body: any): Response {
      // Only invalidate on successful write operations
      if (
        res.statusCode >= 200 &&
        res.statusCode < 300 &&
        ['POST', 'PUT', 'PATCH', 'DELETE'].includes(req.method)
      ) {
        // Invalidate cache patterns asynchronously
        Promise.all(
          patterns.map((pattern) => cacheService.invalidatePattern(pattern))
        ).catch((error) => {
          logger.error('Failed to invalidate cache:', error);
        });
      }
      return originalJson(body);
    };

    next();
  };
};

/**
 * User-specific cache middleware
 */
export const userCacheMiddleware = (ttl: number = 300) => {
  return cacheMiddleware({
    ttl,
    keyGenerator: (req: Request) => {
      const userId = req.user?.id || 'anonymous';
      return cacheService.buildKey('user', userId, req.method, req.originalUrl);
    },
    condition: (req: Request) => !!req.user,
  });
};

/**
 * Public cache middleware (longer TTL)
 */
export const publicCacheMiddleware = (ttl: number = 3600) => {
  return cacheMiddleware({
    ttl,
    keyGenerator: (req: Request) => {
      return cacheService.buildKey('public', req.method, req.originalUrl);
    },
  });
};

export default {
  cacheMiddleware,
  invalidateCacheMiddleware,
  userCacheMiddleware,
  publicCacheMiddleware,
};
