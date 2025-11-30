import rateLimit from 'express-rate-limit';
import RedisStore from 'rate-limit-redis';
import Redis from 'ioredis';
import { config } from '@config/index';
import { Request, Response } from 'express';

// Create Redis client for rate limiting
const redisClient = new Redis({
  host: config.REDIS_HOST,
  port: config.REDIS_PORT,
  password: config.REDIS_PASSWORD,
  db: config.REDIS_DB,
  retryStrategy: (times: number) => {
    const delay = Math.min(times * 50, 2000);
    return delay;
  },
});

/**
 * Default rate limiter
 * 100 requests per 15 minutes per IP
 */
export const defaultRateLimiter = rateLimit({
  windowMs: config.RATE_LIMIT_WINDOW_MS,
  max: config.RATE_LIMIT_MAX_REQUESTS,
  standardHeaders: true,
  legacyHeaders: false,
  store: new RedisStore({
    client: redisClient,
    prefix: 'rl:default:',
  }),
  message: {
    error: {
      code: 'RATE_LIMIT_EXCEEDED',
      message: 'Too many requests, please try again later',
    },
  },
  handler: (req: Request, res: Response) => {
    res.status(429).json({
      success: false,
      error: {
        code: 'RATE_LIMIT_EXCEEDED',
        message: 'Too many requests, please try again later',
        retryAfter: res.getHeader('Retry-After'),
      },
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  },
});

/**
 * Strict rate limiter for authentication endpoints
 * 5 requests per 15 minutes per IP
 */
export const authRateLimiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 5,
  skipSuccessfulRequests: true, // Don't count successful requests
  standardHeaders: true,
  legacyHeaders: false,
  store: new RedisStore({
    client: redisClient,
    prefix: 'rl:auth:',
  }),
  message: {
    error: {
      code: 'RATE_LIMIT_EXCEEDED',
      message: 'Too many authentication attempts, please try again later',
    },
  },
  handler: (req: Request, res: Response) => {
    res.status(429).json({
      success: false,
      error: {
        code: 'RATE_LIMIT_EXCEEDED',
        message: 'Too many authentication attempts, please try again later',
        retryAfter: res.getHeader('Retry-After'),
      },
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  },
});

/**
 * Lenient rate limiter for public endpoints
 * 200 requests per 15 minutes per IP
 */
export const publicRateLimiter = rateLimit({
  windowMs: 15 * 60 * 1000,
  max: 200,
  standardHeaders: true,
  legacyHeaders: false,
  store: new RedisStore({
    client: redisClient,
    prefix: 'rl:public:',
  }),
  handler: (req: Request, res: Response) => {
    res.status(429).json({
      success: false,
      error: {
        code: 'RATE_LIMIT_EXCEEDED',
        message: 'Too many requests, please try again later',
        retryAfter: res.getHeader('Retry-After'),
      },
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  },
});

/**
 * User-based rate limiter
 * Uses user ID instead of IP for authenticated requests
 */
export const createUserRateLimiter = (maxRequests: number, windowMs: number = 15 * 60 * 1000) => {
  return rateLimit({
    windowMs,
    max: maxRequests,
    standardHeaders: true,
    legacyHeaders: false,
    store: new RedisStore({
      client: redisClient,
      prefix: 'rl:user:',
    }),
    keyGenerator: (req: Request) => {
      // Use user ID if authenticated, otherwise fall back to IP
      return req.user?.id || req.ip || 'unknown';
    },
    handler: (req: Request, res: Response) => {
      res.status(429).json({
        success: false,
        error: {
          code: 'RATE_LIMIT_EXCEEDED',
          message: 'Too many requests, please try again later',
          retryAfter: res.getHeader('Retry-After'),
        },
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    },
  });
};

/**
 * Role-based rate limiter
 * Different limits for different user roles
 */
export const roleBasedRateLimiter = rateLimit({
  windowMs: 15 * 60 * 1000,
  max: async (req: Request) => {
    // Get user role
    const role = req.user?.role?.name;

    // Set limits based on role
    switch (role) {
      case 'admin':
        return 1000; // Admins get higher limits
      case 'manager':
        return 500;
      case 'officer':
        return 200;
      default:
        return 100; // Default for regular users
    }
  },
  standardHeaders: true,
  legacyHeaders: false,
  store: new RedisStore({
    client: redisClient,
    prefix: 'rl:role:',
  }),
  keyGenerator: (req: Request) => {
    return req.user?.id || req.ip || 'unknown';
  },
  handler: (req: Request, res: Response) => {
    res.status(429).json({
      success: false,
      error: {
        code: 'RATE_LIMIT_EXCEEDED',
        message: 'Too many requests, please try again later',
        retryAfter: res.getHeader('Retry-After'),
      },
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  },
});

/**
 * Custom rate limiter factory
 */
export const createRateLimiter = (options: {
  windowMs?: number;
  max: number;
  prefix?: string;
  skipSuccessfulRequests?: boolean;
}) => {
  return rateLimit({
    windowMs: options.windowMs || 15 * 60 * 1000,
    max: options.max,
    skipSuccessfulRequests: options.skipSuccessfulRequests || false,
    standardHeaders: true,
    legacyHeaders: false,
    store: new RedisStore({
      client: redisClient,
      prefix: options.prefix || 'rl:custom:',
    }),
    handler: (req: Request, res: Response) => {
      res.status(429).json({
        success: false,
        error: {
          code: 'RATE_LIMIT_EXCEEDED',
          message: 'Too many requests, please try again later',
          retryAfter: res.getHeader('Retry-After'),
        },
        timestamp: new Date(),
        correlationId: req.correlationId,
      });
    },
  });
};

export default {
  defaultRateLimiter,
  authRateLimiter,
  publicRateLimiter,
  createUserRateLimiter,
  roleBasedRateLimiter,
  createRateLimiter,
};
