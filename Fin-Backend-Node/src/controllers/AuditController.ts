import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { executeInTransaction } from '@config/database';
import { createBadRequestError } from '@middleware/errorHandler';

const auditQuerySchema = z.object({
  userId: z.string().uuid().optional(),
  action: z.string().optional(),
  entityType: z.string().optional(),
  startDate: z.string().datetime().optional(),
  endDate: z.string().datetime().optional(),
  ipAddress: z.string().optional(),
  page: z.string().optional(),
  limit: z.string().optional(),
});

const exportAuditSchema = z.object({
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
  format: z.enum(['csv', 'json', 'xlsx']).default('csv'),
  filters: z
    .object({
      userId: z.string().uuid().optional(),
      action: z.string().optional(),
      entityType: z.string().optional(),
    })
    .optional(),
});

export class AuditController {
  /**
   * @swagger
   * /api/v1/audit/logs:
   *   get:
   *     summary: Query audit logs
   *     tags: [Audit]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: userId
   *         schema:
   *           type: string
   *       - in: query
   *         name: action
   *         schema:
   *           type: string
   *       - in: query
   *         name: entityType
   *         schema:
   *           type: string
   *       - in: query
   *         name: startDate
   *         schema:
   *           type: string
   *           format: date-time
   *       - in: query
   *         name: endDate
   *         schema:
   *           type: string
   *           format: date-time
   *       - in: query
   *         name: ipAddress
   *         schema:
   *           type: string
   *       - in: query
   *         name: page
   *         schema:
   *           type: integer
   *           default: 1
   *       - in: query
   *         name: limit
   *         schema:
   *           type: integer
   *           default: 50
   *     responses:
   *       200:
   *         description: Audit logs retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async queryLogs(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const {
        userId,
        action,
        entityType,
        startDate,
        endDate,
        ipAddress,
        page = '1',
        limit = '50',
      } = auditQuerySchema.parse(req.query);

      const pageNum = parseInt(page);
      const limitNum = Math.min(parseInt(limit), 1000); // Max 1000 records
      const skip = (pageNum - 1) * limitNum;

      const where: any = {};
      if (userId) where.userId = userId;
      if (action) where.action = action;
      if (entityType) where.entityType = entityType;
      if (ipAddress) where.ipAddress = ipAddress;
      if (startDate || endDate) {
        where.createdAt = {};
        if (startDate) where.createdAt.gte = new Date(startDate);
        if (endDate) where.createdAt.lte = new Date(endDate);
      }

      const [logs, total] = await Promise.all([
        executeInTransaction(async (prisma) => {
          return prisma.auditLog.findMany({
            where,
            skip,
            take: limitNum,
            orderBy: {
              createdAt: 'desc',
            },
          });
        }),
        executeInTransaction(async (prisma) => {
          return prisma.auditLog.count({ where });
        }),
      ]);

      res.json({
        success: true,
        data: logs,
        pagination: {
          page: pageNum,
          limit: limitNum,
          total,
          pages: Math.ceil(total / limitNum),
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/audit/logs/{id}:
   *   get:
   *     summary: Get audit log by ID
   *     tags: [Audit]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Audit log retrieved successfully
   *       404:
   *         description: Audit log not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getLogById(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const log = await executeInTransaction(async (prisma) => {
        return prisma.auditLog.findUnique({
          where: { id },
        });
      });

      if (!log) {
        return res.status(404).json({
          success: false,
          error: {
            code: 'NOT_FOUND',
            message: 'Audit log not found',
          },
        });
      }

      res.json({
        success: true,
        data: log,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/audit/stats:
   *   get:
   *     summary: Get audit statistics
   *     tags: [Audit]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: startDate
   *         schema:
   *           type: string
   *           format: date-time
   *       - in: query
   *         name: endDate
   *         schema:
   *           type: string
   *           format: date-time
   *     responses:
   *       200:
   *         description: Audit statistics retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getStats(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { startDate, endDate } = req.query;

      const where: any = {};
      if (startDate || endDate) {
        where.createdAt = {};
        if (startDate) where.createdAt.gte = new Date(startDate as string);
        if (endDate) where.createdAt.lte = new Date(endDate as string);
      }

      const [totalLogs, actionStats, entityTypeStats, userStats, recentActivity] =
        await Promise.all([
          // Total logs count
          executeInTransaction(async (prisma) => {
            return prisma.auditLog.count({ where });
          }),

          // Actions breakdown
          executeInTransaction(async (prisma) => {
            return prisma.auditLog.groupBy({
              by: ['action'],
              where,
              _count: {
                action: true,
              },
              orderBy: {
                _count: {
                  action: 'desc',
                },
              },
              take: 10,
            });
          }),

          // Entity types breakdown
          executeInTransaction(async (prisma) => {
            return prisma.auditLog.groupBy({
              by: ['entityType'],
              where,
              _count: {
                entityType: true,
              },
              orderBy: {
                _count: {
                  entityType: 'desc',
                },
              },
              take: 10,
            });
          }),

          // Top users
          executeInTransaction(async (prisma) => {
            return prisma.auditLog.groupBy({
              by: ['userId'],
              where: {
                ...where,
                userId: { not: null },
              },
              _count: {
                userId: true,
              },
              orderBy: {
                _count: {
                  userId: 'desc',
                },
              },
              take: 10,
            });
          }),

          // Recent activity (last 24 hours)
          executeInTransaction(async (prisma) => {
            return prisma.auditLog.findMany({
              where: {
                createdAt: {
                  gte: new Date(Date.now() - 24 * 60 * 60 * 1000),
                },
              },
              take: 10,
              orderBy: {
                createdAt: 'desc',
              },
            });
          }),
        ]);

      res.json({
        success: true,
        data: {
          totalLogs,
          actionStats: actionStats.map((stat) => ({
            action: stat.action,
            count: stat._count.action,
          })),
          entityTypeStats: entityTypeStats.map((stat) => ({
            entityType: stat.entityType,
            count: stat._count.entityType,
          })),
          userStats: userStats.map((stat) => ({
            userId: stat.userId,
            count: stat._count.userId,
          })),
          recentActivity,
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/audit/export:
   *   post:
   *     summary: Export audit logs
   *     tags: [Audit]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - startDate
   *               - endDate
   *             properties:
   *               startDate:
   *                 type: string
   *                 format: date-time
   *               endDate:
   *                 type: string
   *                 format: date-time
   *               format:
   *                 type: string
   *                 enum: [csv, json, xlsx]
   *               filters:
   *                 type: object
   *                 properties:
   *                   userId:
   *                     type: string
   *                   action:
   *                     type: string
   *                   entityType:
   *                     type: string
   *     responses:
   *       200:
   *         description: Export initiated successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async exportLogs(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = exportAuditSchema.parse(req.body);
      const userId = req.user?.id;

      // Validate date range (max 1 year)
      const startDate = new Date(validatedData.startDate);
      const endDate = new Date(validatedData.endDate);
      const daysDiff = Math.ceil(
        (endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24)
      );

      if (daysDiff > 365) {
        throw createBadRequestError('Date range cannot exceed 365 days');
      }

      if (endDate <= startDate) {
        throw createBadRequestError('End date must be after start date');
      }

      // TODO: Queue export job using JobService
      // For now, return a job ID
      const exportJobId = `export-${Date.now()}`;

      res.json({
        success: true,
        data: {
          exportJobId,
          status: 'queued',
          estimatedCompletionTime: new Date(Date.now() + 5 * 60 * 1000), // 5 minutes
        },
        message: 'Audit log export initiated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/audit/retention:
   *   post:
   *     summary: Apply data retention policy
   *     tags: [Audit]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - retentionDays
   *             properties:
   *               retentionDays:
   *                 type: integer
   *                 minimum: 30
   *                 maximum: 2555
   *               dryRun:
   *                 type: boolean
   *                 default: true
   *     responses:
   *       200:
   *         description: Retention policy applied successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async applyRetentionPolicy(
    req: Request,
    res: Response,
    next: NextFunction
  ): Promise<void> {
    try {
      const { retentionDays, dryRun = true } = req.body;

      if (retentionDays < 30 || retentionDays > 2555) {
        throw createBadRequestError('Retention days must be between 30 and 2555 (7 years)');
      }

      const cutoffDate = new Date();
      cutoffDate.setDate(cutoffDate.getDate() - retentionDays);

      // Count logs to be deleted
      const count = await executeInTransaction(async (prisma) => {
        return prisma.auditLog.count({
          where: {
            createdAt: {
              lt: cutoffDate,
            },
          },
        });
      });

      if (!dryRun) {
        // Actually delete the logs
        await executeInTransaction(async (prisma) => {
          await prisma.auditLog.deleteMany({
            where: {
              createdAt: {
                lt: cutoffDate,
              },
            },
          });
        });
      }

      res.json({
        success: true,
        data: {
          dryRun,
          retentionDays,
          cutoffDate,
          logsAffected: count,
          message: dryRun
            ? `Would delete ${count} audit logs`
            : `Deleted ${count} audit logs`,
        },
      });
    } catch (error) {
      next(error);
    }
  }
}

export default AuditController;
