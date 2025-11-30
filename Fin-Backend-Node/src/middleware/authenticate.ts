import { Request, Response, NextFunction } from 'express';
import { verifyAccessToken } from '@utils/jwt';
import { RepositoryFactory } from '@repositories/index';
import { createUnauthorizedError } from './errorHandler';

// Extend Express Request type
declare global {
  namespace Express {
    interface Request {
      user?: {
        id: string;
        email: string;
        roleId: string;
        role?: any;
      };
    }
  }
}

/**
 * Authentication middleware
 * Verifies JWT token and attaches user to request
 */
export const authenticate = async (
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    // Get token from Authorization header
    const authHeader = req.headers.authorization;
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      throw createUnauthorizedError('No token provided');
    }

    const token = authHeader.substring(7); // Remove 'Bearer ' prefix

    // Verify token
    let payload;
    try {
      payload = verifyAccessToken(token);
    } catch (error) {
      throw createUnauthorizedError(
        error instanceof Error ? error.message : 'Invalid token'
      );
    }

    // Get user from database
    const userRepository = RepositoryFactory.getUserRepository();
    const user = await userRepository.findByIdWithPermissions(payload.userId);

    if (!user) {
      throw createUnauthorizedError('User not found');
    }

    // Check if user is active
    if (!user.isActive) {
      throw createUnauthorizedError('Account is inactive');
    }

    // Check if account is locked
    if (user.lockedUntil && user.lockedUntil > new Date()) {
      throw createUnauthorizedError('Account is locked');
    }

    // Attach user to request
    req.user = {
      id: user.id,
      email: user.email,
      roleId: user.roleId,
      role: user.role,
    };

    next();
  } catch (error) {
    next(error);
  }
};

/**
 * Optional authentication middleware
 * Attaches user if token is valid, but doesn't fail if no token
 */
export const optionalAuthenticate = async (
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    const authHeader = req.headers.authorization;
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return next();
    }

    const token = authHeader.substring(7);

    try {
      const payload = verifyAccessToken(token);
      const userRepository = RepositoryFactory.getUserRepository();
      const user = await userRepository.findByIdWithPermissions(payload.userId);

      if (user && user.isActive) {
        req.user = {
          id: user.id,
          email: user.email,
          roleId: user.roleId,
          role: user.role,
        };
      }
    } catch {
      // Ignore token errors for optional auth
    }

    next();
  } catch (error) {
    next(error);
  }
};

export default authenticate;
