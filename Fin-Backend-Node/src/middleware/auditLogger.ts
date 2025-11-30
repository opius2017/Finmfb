import { Request, Response, NextFunction } from 'express';
import { executeInTransaction } from '@config/database';
import { v4 as uuidv4 } from 'uuid';

export interface AuditableRequest extends Request {
  auditLog?: {
    startTime: number;
    correlationId: string;
    action?: string;
    resource?: string;
    resourceId?: string;
  };
}

/**
 * Audit logging middleware
 * Logs all API requests and responses for compliance and security
 */
export function auditLogger() {
  return (req: AuditableRequest, res: Response, next: NextFunction) => {
    const startTime = Date.now();
    const correlationId = (req.headers['x-correlation-id'] as string) || uuidv4();

    // Add audit context to request
    req.auditLog = {
      startTime,
      correlationId,
    };

    // Add correlation ID to response headers
    res.setHeader('X-Correlation-ID', correlationId);

    // Capture response
    const originalJson = res.json;
    let responseBody: any;

    res.json = function (body: any) {
      responseBody = body;
      return originalJson.call(this, body);
    };

    // Log after response is sent
    res.on('finish', async () => {
      try {
        await logAuditEntry(req, res, responseBody);
      } catch (error) {
        console.error('Failed to log audit entry:', error);
      }
    });

    next();
  };
}

/**
 * Set audit context for specific actions
 */
export function setAuditContext(
  action: string,
  resource: string,
  resourceId?: string
) {
  return (req: AuditableRequest, res: Response, next: NextFunction) => {
    if (req.auditLog) {
      req.auditLog.action = action;
      req.auditLog.resource = resource;
      req.auditLog.resourceId = resourceId || extractResourceId(req);
    }
    next();
  };
}

/**
 * Log audit entry to database
 */
async function logAuditEntry(
  req: AuditableRequest,
  res: Response,
  responseBody: any
): Promise<void> {
  const endTime = Date.now();
  const responseTime = req.auditLog ? endTime - req.auditLog.startTime : 0;

  // Determine action and resource from route
  const { action, resource, resourceId } = determineActionAndResource(req);

  // Skip logging for health checks and static assets
  if (shouldSkipLogging(req.path)) {
    return;
  }

  // Prepare audit log entry
  const auditEntry = {
    userId: req.user?.id,
    action: req.auditLog?.action || action,
    entityType: req.auditLog?.resource || resource,
    entityId: req.auditLog?.resourceId || resourceId || '',
    ipAddress: getClientIpAddress(req),
    userAgent: req.get('User-Agent') || '',
    changes: {
      method: req.method,
      endpoint: req.originalUrl,
      requestBody: shouldLogRequestBody(req) ? sanitizeRequestBody(req.body) : undefined,
      responseStatus: res.statusCode,
      responseTime,
      correlationId: req.auditLog?.correlationId || '',
      query: req.query,
      params: req.params,
      success: res.statusCode < 400,
    },
  };

  // Save to database
  try {
    await executeInTransaction(async (prisma) => {
      await prisma.auditLog.create({
        data: auditEntry,
      });
    });
  } catch (error) {
    // Log to console if database logging fails
    console.error('Failed to save audit log:', error);
  }
}

/**
 * Determine action and resource from request
 */
function determineActionAndResource(req: Request): {
  action: string;
  resource: string;
  resourceId?: string;
} {
  const method = req.method.toLowerCase();
  const path = req.route?.path || req.path;
  const pathSegments = path.split('/').filter(Boolean);

  // Extract resource from path (e.g., /api/v1/members -> members)
  let resource = 'unknown';
  if (pathSegments.length >= 3) {
    resource = pathSegments[2]; // Skip 'api' and 'v1'
  }

  // Determine action based on method and path pattern
  let action = method;
  if (method === 'get' && req.params.id) {
    action = 'read';
  } else if (method === 'get') {
    action = 'list';
  } else if (method === 'post') {
    action = 'create';
  } else if (method === 'put' || method === 'patch') {
    action = 'update';
  } else if (method === 'delete') {
    action = 'delete';
  }

  // Special actions based on path
  if (path.includes('/approve')) {
    action = 'approve';
  } else if (path.includes('/reject')) {
    action = 'reject';
  } else if (path.includes('/disburse')) {
    action = 'disburse';
  } else if (path.includes('/reverse')) {
    action = 'reverse';
  } else if (path.includes('/download')) {
    action = 'download';
  } else if (path.includes('/upload')) {
    action = 'upload';
  }

  return {
    action,
    resource,
    resourceId: req.params.id,
  };
}

/**
 * Extract resource ID from request
 */
function extractResourceId(req: Request): string | undefined {
  return (
    req.params.id ||
    req.params.memberId ||
    req.params.accountId ||
    req.params.loanId ||
    req.params.documentId
  );
}

/**
 * Get client IP address
 */
function getClientIpAddress(req: Request): string {
  return (
    (req.headers['x-forwarded-for'] as string)?.split(',')[0] ||
    (req.connection as any).remoteAddress ||
    (req.socket as any).remoteAddress ||
    ''
  );
}

/**
 * Check if request body should be logged
 */
function shouldLogRequestBody(req: Request): boolean {
  // Don't log sensitive endpoints
  const sensitiveEndpoints = ['/auth/login', '/password', '/mfa'];
  return !sensitiveEndpoints.some((endpoint) => req.path.includes(endpoint));
}

/**
 * Check if path should skip logging
 */
function shouldSkipLogging(path: string): boolean {
  const skipPaths = ['/health', '/ready', '/api-docs', '/api/docs', '/favicon.ico'];
  return skipPaths.some((skipPath) => path.startsWith(skipPath));
}

/**
 * Sanitize request body (remove sensitive fields)
 */
function sanitizeRequestBody(body: any): any {
  if (!body || typeof body !== 'object') {
    return body;
  }

  const sensitiveFields = [
    'password',
    'passwordHash',
    'token',
    'refreshToken',
    'secret',
    'key',
    'pin',
    'mfaSecret',
    'apiKey',
    'accessToken',
  ];

  const sanitized = { ...body };

  for (const field of sensitiveFields) {
    if (sanitized[field]) {
      sanitized[field] = '[REDACTED]';
    }
  }

  return sanitized;
}

/**
 * Audit decorator for service methods
 */
export function auditAction(action: string, resource: string) {
  return function (target: any, propertyName: string, descriptor: PropertyDescriptor) {
    const method = descriptor.value;

    descriptor.value = async function (...args: any[]) {
      const startTime = Date.now();
      const correlationId = uuidv4();

      try {
        const result = await method.apply(this, args);
        const endTime = Date.now();

        // Log successful action
        await logServiceAction({
          action,
          resource,
          success: true,
          duration: endTime - startTime,
          correlationId,
          args: sanitizeArgs(args),
          result: sanitizeResult(result),
        });

        return result;
      } catch (error) {
        const endTime = Date.now();

        // Log failed action
        await logServiceAction({
          action,
          resource,
          success: false,
          duration: endTime - startTime,
          correlationId,
          args: sanitizeArgs(args),
          error: error instanceof Error ? error.message : 'Unknown error',
        });

        throw error;
      }
    };
  };
}

/**
 * Log service action
 */
async function logServiceAction(data: {
  action: string;
  resource: string;
  success: boolean;
  duration: number;
  correlationId: string;
  args?: any;
  result?: any;
  error?: string;
}): Promise<void> {
  try {
    await executeInTransaction(async (prisma) => {
      await prisma.systemLog.create({
        data: {
          level: data.success ? 'INFO' : 'ERROR',
          message: `Service action: ${data.action} on ${data.resource}`,
          context: {
            action: data.action,
            resource: data.resource,
            success: data.success,
            duration: data.duration,
            correlationId: data.correlationId,
            args: data.args,
            result: data.result,
            error: data.error,
          },
        },
      });
    });
  } catch (error) {
    console.error('Failed to log service action:', error);
  }
}

/**
 * Sanitize function arguments
 */
function sanitizeArgs(args: any[]): any[] {
  return args.map((arg) => {
    if (typeof arg === 'object' && arg !== null) {
      return sanitizeRequestBody(arg);
    }
    return arg;
  });
}

/**
 * Sanitize function result
 */
function sanitizeResult(result: any): any {
  if (typeof result === 'object' && result !== null) {
    return sanitizeRequestBody(result);
  }
  return result;
}
