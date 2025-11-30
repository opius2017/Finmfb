import { Request, Response, NextFunction } from 'express';
import { config } from '@config/index';

/**
 * API version middleware
 * Adds version information to response headers
 */
export const apiVersion = (req: Request, res: Response, next: NextFunction): void => {
  // Add API version to response headers
  res.setHeader('X-API-Version', config.API_VERSION);
  res.setHeader('X-API-Deprecated', 'false');
  
  next();
};

/**
 * Deprecation warning middleware
 * Warns clients about deprecated API versions
 */
export const deprecationWarning = (deprecatedVersion: string, sunsetDate: string) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    res.setHeader('X-API-Deprecated', 'true');
    res.setHeader('X-API-Deprecated-Version', deprecatedVersion);
    res.setHeader('X-API-Sunset-Date', sunsetDate);
    res.setHeader(
      'Warning',
      `299 - "This API version is deprecated and will be removed on ${sunsetDate}"`
    );
    
    next();
  };
};

/**
 * Version validation middleware
 * Ensures requested version is supported
 */
export const validateVersion = (supportedVersions: string[]) => {
  return (req: Request, res: Response, next: NextFunction): void => {
    // Extract version from URL path (e.g., /api/v1/...)
    const versionMatch = req.path.match(/^\/api\/(v\d+)\//);
    
    if (!versionMatch) {
      res.status(400).json({
        success: false,
        error: {
          code: 'INVALID_API_VERSION',
          message: 'API version not specified in URL',
          supportedVersions,
        },
        timestamp: new Date(),
      });
      return;
    }

    const requestedVersion = versionMatch[1];

    if (!supportedVersions.includes(requestedVersion)) {
      res.status(400).json({
        success: false,
        error: {
          code: 'UNSUPPORTED_API_VERSION',
          message: `API version '${requestedVersion}' is not supported`,
          supportedVersions,
        },
        timestamp: new Date(),
      });
      return;
    }

    // Attach version to request
    (req as any).apiVersion = requestedVersion;
    
    next();
  };
};

/**
 * Version router factory
 * Creates versioned route handlers
 */
export const createVersionedRouter = () => {
  const versions = new Map<string, any>();

  return {
    /**
     * Register a version handler
     */
    version: (version: string, handler: any) => {
      versions.set(version, handler);
    },

    /**
     * Get middleware that routes to correct version
     */
    middleware: () => {
      return (req: Request, res: Response, next: NextFunction) => {
        const versionMatch = req.path.match(/^\/api\/(v\d+)\//);
        
        if (!versionMatch) {
          return next();
        }

        const requestedVersion = versionMatch[1];
        const handler = versions.get(requestedVersion);

        if (!handler) {
          return next();
        }

        // Call version-specific handler
        return handler(req, res, next);
      };
    },

    /**
     * Get all registered versions
     */
    getVersions: () => Array.from(versions.keys()),
  };
};

export default {
  apiVersion,
  deprecationWarning,
  validateVersion,
  createVersionedRouter,
};
