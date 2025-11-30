import { Request, Response, NextFunction } from 'express';
import { logger } from '@utils/logger';
import { ZodError } from 'zod';

export enum ErrorCode {
  // Client Errors (4xx)
  BAD_REQUEST = 'BAD_REQUEST',
  UNAUTHORIZED = 'UNAUTHORIZED',
  FORBIDDEN = 'FORBIDDEN',
  NOT_FOUND = 'NOT_FOUND',
  CONFLICT = 'CONFLICT',
  VALIDATION_ERROR = 'VALIDATION_ERROR',
  RATE_LIMIT_EXCEEDED = 'RATE_LIMIT_EXCEEDED',

  // Server Errors (5xx)
  INTERNAL_ERROR = 'INTERNAL_ERROR',
  DATABASE_ERROR = 'DATABASE_ERROR',
  EXTERNAL_SERVICE_ERROR = 'EXTERNAL_SERVICE_ERROR',
  CALCULATION_ERROR = 'CALCULATION_ERROR',
}

export interface APIError {
  code: ErrorCode;
  message: string;
  details?: unknown;
  timestamp: Date;
  path: string;
  correlationId: string;
}

export class AppError extends Error {
  constructor(
    public statusCode: number,
    public code: ErrorCode,
    message: string,
    public details?: unknown
  ) {
    super(message);
    this.name = 'AppError';
    Error.captureStackTrace(this, this.constructor);
  }
}

export const errorHandler = (
  err: Error,
  req: Request,
  res: Response,
  _next: NextFunction
): void => {
  let statusCode = 500;
  let errorCode = ErrorCode.INTERNAL_ERROR;
  let message = 'An unexpected error occurred';
  let details: unknown = undefined;

  // Handle known error types
  if (err instanceof AppError) {
    statusCode = err.statusCode;
    errorCode = err.code;
    message = err.message;
    details = err.details;
  } else if (err instanceof ZodError) {
    statusCode = 400;
    errorCode = ErrorCode.VALIDATION_ERROR;
    message = 'Validation error';
    details = err.errors.map((e) => ({
      path: e.path.join('.'),
      message: e.message,
    }));
  } else if (err.name === 'JsonWebTokenError') {
    statusCode = 401;
    errorCode = ErrorCode.UNAUTHORIZED;
    message = 'Invalid token';
  } else if (err.name === 'TokenExpiredError') {
    statusCode = 401;
    errorCode = ErrorCode.UNAUTHORIZED;
    message = 'Token expired';
  }

  // Log error
  logger.error('Error occurred', {
    correlationId: req.correlationId,
    error: {
      name: err.name,
      message: err.message,
      stack: err.stack,
      code: errorCode,
      statusCode,
    },
    request: {
      method: req.method,
      path: req.path,
      query: req.query,
      body: req.body,
      ip: req.ip,
    },
  });

  // Send error response
  const errorResponse: APIError = {
    code: errorCode,
    message,
    details,
    timestamp: new Date(),
    path: req.path,
    correlationId: req.correlationId,
  };

  res.status(statusCode).json({ error: errorResponse });
};

// Helper functions to create common errors
export const createBadRequestError = (message: string, details?: unknown): AppError => {
  return new AppError(400, ErrorCode.BAD_REQUEST, message, details);
};

export const createUnauthorizedError = (message = 'Unauthorized'): AppError => {
  return new AppError(401, ErrorCode.UNAUTHORIZED, message);
};

export const createForbiddenError = (message = 'Forbidden'): AppError => {
  return new AppError(403, ErrorCode.FORBIDDEN, message);
};

export const createNotFoundError = (resource: string): AppError => {
  return new AppError(404, ErrorCode.NOT_FOUND, `${resource} not found`);
};

export const createConflictError = (message: string): AppError => {
  return new AppError(409, ErrorCode.CONFLICT, message);
};

export const createValidationError = (message: string, details?: unknown): AppError => {
  return new AppError(400, ErrorCode.VALIDATION_ERROR, message, details);
};
