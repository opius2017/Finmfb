import { Request, Response, NextFunction } from 'express';
import { createBadRequestError } from './errorHandler';
import crypto from 'crypto';

/**
 * CSRF Protection Middleware
 */
export function csrfProtection() {
  return (req: Request, res: Response, next: NextFunction) => {
    // Skip CSRF for GET, HEAD, OPTIONS
    if (['GET', 'HEAD', 'OPTIONS'].includes(req.method)) {
      return next();
    }

    // Skip CSRF for API endpoints with Bearer token
    if (req.headers.authorization?.startsWith('Bearer ')) {
      return next();
    }

    const token = req.headers['x-csrf-token'] as string;
    const sessionToken = req.session?.csrfToken;

    if (!token || !sessionToken || token !== sessionToken) {
      return res.status(403).json({
        success: false,
        error: {
          code: 'CSRF_TOKEN_INVALID',
          message: 'Invalid CSRF token',
        },
      });
    }

    next();
  };
}

/**
 * Generate CSRF token
 */
export function generateCsrfToken(req: Request): string {
  const token = crypto.randomBytes(32).toString('hex');
  if (req.session) {
    req.session.csrfToken = token;
  }
  return token;
}

/**
 * XSS Protection Middleware
 */
export function xssProtection() {
  return (req: Request, res: Response, next: NextFunction) => {
    // Sanitize request body
    if (req.body) {
      req.body = sanitizeObject(req.body);
    }

    // Sanitize query parameters
    if (req.query) {
      req.query = sanitizeObject(req.query);
    }

    next();
  };
}

/**
 * Sanitize object to prevent XSS
 */
function sanitizeObject(obj: any): any {
  if (typeof obj === 'string') {
    return sanitizeString(obj);
  }

  if (Array.isArray(obj)) {
    return obj.map(sanitizeObject);
  }

  if (obj && typeof obj === 'object') {
    const sanitized: any = {};
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        sanitized[key] = sanitizeObject(obj[key]);
      }
    }
    return sanitized;
  }

  return obj;
}

/**
 * Sanitize string to prevent XSS
 */
function sanitizeString(str: string): string {
  return str
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;');
}

/**
 * SQL Injection Prevention (for raw queries)
 */
export function sanitizeSqlInput(input: string): string {
  return input
    .replace(/'/g, "''")
    .replace(/;/g, '')
    .replace(/--/g, '')
    .replace(/\/\*/g, '')
    .replace(/\*\//g, '');
}

/**
 * Input Validation Middleware
 */
export function validateInput(schema: any) {
  return (req: Request, res: Response, next: NextFunction) => {
    try {
      const validated = schema.parse(req.body);
      req.body = validated;
      next();
    } catch (error) {
      next(createBadRequestError('Invalid input data'));
    }
  };
}

/**
 * Request Size Limiter
 */
export function requestSizeLimiter(maxSize: number = 10 * 1024 * 1024) {
  return (req: Request, res: Response, next: NextFunction) => {
    const contentLength = parseInt(req.headers['content-length'] || '0');

    if (contentLength > maxSize) {
      return res.status(413).json({
        success: false,
        error: {
          code: 'PAYLOAD_TOO_LARGE',
          message: `Request size exceeds maximum of ${maxSize} bytes`,
        },
      });
    }

    next();
  };
}

/**
 * IP Whitelist Middleware
 */
export function ipWhitelist(allowedIps: string[]) {
  return (req: Request, res: Response, next: NextFunction) => {
    const clientIp =
      (req.headers['x-forwarded-for'] as string)?.split(',')[0] ||
      req.connection.remoteAddress ||
      '';

    if (!allowedIps.includes(clientIp)) {
      return res.status(403).json({
        success: false,
        error: {
          code: 'IP_NOT_ALLOWED',
          message: 'Access denied from this IP address',
        },
      });
    }

    next();
  };
}

/**
 * Account Lockout Middleware
 */
const loginAttempts = new Map<string, { count: number; lockedUntil?: Date }>();

export function accountLockout(maxAttempts: number = 5, lockoutDuration: number = 30 * 60 * 1000) {
  return {
    check: (identifier: string): boolean => {
      const attempts = loginAttempts.get(identifier);
      if (!attempts) return true;

      if (attempts.lockedUntil && attempts.lockedUntil > new Date()) {
        return false;
      }

      if (attempts.lockedUntil && attempts.lockedUntil <= new Date()) {
        loginAttempts.delete(identifier);
        return true;
      }

      return attempts.count < maxAttempts;
    },

    recordFailure: (identifier: string): void => {
      const attempts = loginAttempts.get(identifier) || { count: 0 };
      attempts.count++;

      if (attempts.count >= maxAttempts) {
        attempts.lockedUntil = new Date(Date.now() + lockoutDuration);
      }

      loginAttempts.set(identifier, attempts);
    },

    recordSuccess: (identifier: string): void => {
      loginAttempts.delete(identifier);
    },

    getRemainingAttempts: (identifier: string): number => {
      const attempts = loginAttempts.get(identifier);
      if (!attempts) return maxAttempts;
      return Math.max(0, maxAttempts - attempts.count);
    },

    getLockoutTime: (identifier: string): Date | null => {
      const attempts = loginAttempts.get(identifier);
      return attempts?.lockedUntil || null;
    },
  };
}

/**
 * Security Headers Middleware
 */
export function securityHeaders() {
  return (req: Request, res: Response, next: NextFunction) => {
    // Prevent clickjacking
    res.setHeader('X-Frame-Options', 'DENY');

    // Prevent MIME type sniffing
    res.setHeader('X-Content-Type-Options', 'nosniff');

    // Enable XSS filter
    res.setHeader('X-XSS-Protection', '1; mode=block');

    // Referrer policy
    res.setHeader('Referrer-Policy', 'strict-origin-when-cross-origin');

    // Content Security Policy
    res.setHeader(
      'Content-Security-Policy',
      "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;"
    );

    // Permissions Policy
    res.setHeader(
      'Permissions-Policy',
      'geolocation=(), microphone=(), camera=()'
    );

    next();
  };
}

/**
 * Sensitive Data Masking
 */
export function maskSensitiveData(data: any, fields: string[]): any {
  if (!data || typeof data !== 'object') {
    return data;
  }

  const masked = { ...data };

  for (const field of fields) {
    if (masked[field]) {
      const value = String(masked[field]);
      if (value.length <= 4) {
        masked[field] = '****';
      } else {
        masked[field] = value.slice(0, 2) + '****' + value.slice(-2);
      }
    }
  }

  return masked;
}

/**
 * API Key Validation
 */
export function validateApiKey(validKeys: string[]) {
  return (req: Request, res: Response, next: NextFunction) => {
    const apiKey = req.headers['x-api-key'] as string;

    if (!apiKey || !validKeys.includes(apiKey)) {
      return res.status(401).json({
        success: false,
        error: {
          code: 'INVALID_API_KEY',
          message: 'Invalid or missing API key',
        },
      });
    }

    next();
  };
}

/**
 * Secure Cookie Options
 */
export const secureCookieOptions = {
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'strict' as const,
  maxAge: 24 * 60 * 60 * 1000, // 24 hours
};

/**
 * Password Complexity Validator
 */
export function validatePasswordComplexity(password: string): {
  isValid: boolean;
  errors: string[];
} {
  const errors: string[] = [];
  const minLength = parseInt(process.env.PASSWORD_MIN_LENGTH || '8');

  if (password.length < minLength) {
    errors.push(`Password must be at least ${minLength} characters long`);
  }

  if (process.env.PASSWORD_REQUIRE_UPPERCASE === 'true' && !/[A-Z]/.test(password)) {
    errors.push('Password must contain at least one uppercase letter');
  }

  if (process.env.PASSWORD_REQUIRE_LOWERCASE === 'true' && !/[a-z]/.test(password)) {
    errors.push('Password must contain at least one lowercase letter');
  }

  if (process.env.PASSWORD_REQUIRE_NUMBERS === 'true' && !/[0-9]/.test(password)) {
    errors.push('Password must contain at least one number');
  }

  if (process.env.PASSWORD_REQUIRE_SPECIAL === 'true' && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
    errors.push('Password must contain at least one special character');
  }

  return {
    isValid: errors.length === 0,
    errors,
  };
}

/**
 * Security Event Logger
 */
export async function logSecurityEvent(
  event: string,
  details: Record<string, any>
): Promise<void> {
  console.log('Security Event:', {
    event,
    details,
    timestamp: new Date().toISOString(),
  });

  // TODO: Send to security monitoring system
}
