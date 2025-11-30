import { Request, Response, NextFunction } from 'express';
import {
  ResponseTimeTracker,
  PerformanceMetrics,
  MemoryMonitor,
  ConnectionPoolMonitor,
  QueryPerformanceMonitor,
} from '@utils/performance';

export class MetricsController {
  /**
   * @swagger
   * /api/v1/metrics/performance:
   *   get:
   *     summary: Get performance metrics
   *     tags: [Metrics]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Performance metrics retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getPerformanceMetrics(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const metrics = {
        responseTime: ResponseTimeTracker.getStats(),
        performance: PerformanceMetrics.getMetrics(),
        memory: MemoryMonitor.getUsage(),
        connectionPool: ConnectionPoolMonitor.getStats(),
        uptime: process.uptime(),
        timestamp: new Date().toISOString(),
      };

      res.json({
        success: true,
        data: metrics,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/metrics/queries:
   *   get:
   *     summary: Get query performance statistics
   *     tags: [Metrics]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Query statistics retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getQueryStats(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const stats = QueryPerformanceMonitor.getStats();
      const statsArray = Array.from(stats.entries()).map(([query, data]) => ({
        query,
        ...data,
      }));

      res.json({
        success: true,
        data: statsArray,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/metrics/health:
   *   get:
   *     summary: Get system health status
   *     tags: [Metrics]
   *     responses:
   *       200:
   *         description: Health status retrieved successfully
   */
  async getHealthStatus(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const memoryHigh = MemoryMonitor.isMemoryHigh(0.85);
      const poolHealthy = ConnectionPoolMonitor.isHealthy();
      const performanceMetrics = PerformanceMetrics.getMetrics();

      const isHealthy = !memoryHigh && poolHealthy && performanceMetrics.errorRate < 5;

      const health = {
        status: isHealthy ? 'healthy' : 'degraded',
        checks: {
          memory: {
            status: memoryHigh ? 'warning' : 'ok',
            usage: MemoryMonitor.getUsage(),
          },
          connectionPool: {
            status: poolHealthy ? 'ok' : 'warning',
            stats: ConnectionPoolMonitor.getStats(),
          },
          errorRate: {
            status: performanceMetrics.errorRate < 5 ? 'ok' : 'warning',
            rate: performanceMetrics.errorRate,
          },
        },
        uptime: process.uptime(),
        timestamp: new Date().toISOString(),
      };

      const statusCode = isHealthy ? 200 : 503;
      res.status(statusCode).json({
        success: isHealthy,
        data: health,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/metrics/reset:
   *   post:
   *     summary: Reset performance metrics
   *     tags: [Metrics]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Metrics reset successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async resetMetrics(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      ResponseTimeTracker.reset();
      PerformanceMetrics.reset();
      QueryPerformanceMonitor.resetStats();

      res.json({
        success: true,
        message: 'Performance metrics reset successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/metrics/system:
   *   get:
   *     summary: Get system information
   *     tags: [Metrics]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: System information retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getSystemInfo(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const systemInfo = {
        node: {
          version: process.version,
          platform: process.platform,
          arch: process.arch,
        },
        process: {
          pid: process.pid,
          uptime: process.uptime(),
          memoryUsage: MemoryMonitor.getUsage(),
        },
        environment: process.env.NODE_ENV || 'development',
        timestamp: new Date().toISOString(),
      };

      res.json({
        success: true,
        data: systemInfo,
      });
    } catch (error) {
      next(error);
    }
  }
}

export default MetricsController;
