import { Request, Response, NextFunction } from 'express';
import { RBACService } from '@services/RBACService';
import { Permission } from '@/types';
import { createForbiddenError, createUnauthorizedError } from './errorHandler';

const rbacService = new RBACService();

/**
 * Authorization middleware - check if user has required permission
 */
export const authorize = (resource: string, action: string) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const userId = req.user?.id;
      if (!userId) {
        throw createUnauthorizedError('Not authenticated');
      }

      const permission: Permission = { resource, action: action as any };
      const hasPermission = await rbacService.checkPermission(userId, permission);

      if (!hasPermission) {
        throw createForbiddenError(
          `You do not have permission to ${action} ${resource}`
        );
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

/**
 * Authorization middleware - check if user has any of the required permissions
 */
export const authorizeAny = (permissions: Array<{ resource: string; action: string }>) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const userId = req.user?.id;
      if (!userId) {
        throw createUnauthorizedError('Not authenticated');
      }

      const permissionObjects: Permission[] = permissions.map((p) => ({
        resource: p.resource,
        action: p.action as any,
      }));

      const hasPermission = await rbacService.checkAnyPermission(
        userId,
        permissionObjects
      );

      if (!hasPermission) {
        throw createForbiddenError('You do not have the required permissions');
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

/**
 * Authorization middleware - check if user has all required permissions
 */
export const authorizeAll = (permissions: Array<{ resource: string; action: string }>) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const userId = req.user?.id;
      if (!userId) {
        throw createUnauthorizedError('Not authenticated');
      }

      const permissionObjects: Permission[] = permissions.map((p) => ({
        resource: p.resource,
        action: p.action as any,
      }));

      const hasPermission = await rbacService.checkAllPermissions(
        userId,
        permissionObjects
      );

      if (!hasPermission) {
        throw createForbiddenError('You do not have all the required permissions');
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

/**
 * Role-based authorization middleware
 */
export const requireRole = (roleName: string) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const userId = req.user?.id;
      if (!userId) {
        throw createUnauthorizedError('Not authenticated');
      }

      const hasRole = await rbacService.hasRole(userId, roleName);

      if (!hasRole) {
        throw createForbiddenError(`You must have the ${roleName} role`);
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

/**
 * Multiple roles authorization middleware
 */
export const requireAnyRole = (roleNames: string[]) => {
  return async (req: Request, res: Response, next: NextFunction): Promise<void> => {
    try {
      const userId = req.user?.id;
      if (!userId) {
        throw createUnauthorizedError('Not authenticated');
      }

      const hasRole = await rbacService.hasAnyRole(userId, roleNames);

      if (!hasRole) {
        throw createForbiddenError(
          `You must have one of the following roles: ${roleNames.join(', ')}`
        );
      }

      next();
    } catch (error) {
      next(error);
    }
  };
};

export default {
  authorize,
  authorizeAny,
  authorizeAll,
  requireRole,
  requireAnyRole,
};
