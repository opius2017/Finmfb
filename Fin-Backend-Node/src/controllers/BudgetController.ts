import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';
import { executeInTransaction } from '@config/database';
import { Decimal } from '@prisma/client/runtime/library';

// Validation schemas
const createBudgetSchema = z.object({
  name: z.string().min(3).max(200),
  description: z.string().optional(),
  fiscalYear: z.number().int().min(2020).max(2100),
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
  branchId: z.string().uuid().optional(),
  items: z.array(z.object({
    name: z.string().min(3),
    category: z.string(),
    amount: z.number().positive(),
    description: z.string().optional(),
  })),
});

const updateBudgetSchema = z.object({
  name: z.string().min(3).max(200).optional(),
  description: z.string().optional(),
  startDate: z.string().datetime().optional(),
  endDate: z.string().datetime().optional(),
});

const recordActualSchema = z.object({
  budgetItemId: z.string().uuid(),
  amount: z.number().positive(),
  date: z.string().datetime(),
  category: z.string(),
  description: z.string().optional(),
  reference: z.string().optional(),
});

const approveBudgetSchema = z.object({
  notes: z.string().optional(),
});

export class BudgetController {
  /**
   * @swagger
   * /api/v1/budgets:
   *   post:
   *     summary: Create a new budget
   *     tags: [Budgets]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - name
   *               - fiscalYear
   *               - startDate
   *               - endDate
   *               - items
   *             properties:
   *               name:
   *                 type: string
   *               description:
   *                 type: string
   *               fiscalYear:
   *                 type: integer
   *               startDate:
   *                 type: string
   *                 format: date-time
   *               endDate:
   *                 type: string
   *                 format: date-time
   *               branchId:
   *                 type: string
   *               items:
   *                 type: array
   *                 items:
   *                   type: object
   *                   properties:
   *                     name:
   *                       type: string
   *                     category:
   *                       type: string
   *                     amount:
   *                       type: number
   *                     description:
   *                       type: string
   *     responses:
   *       201:
   *         description: Budget created successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async create(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = createBudgetSchema.parse(req.body);
      const userId = req.user?.id;

      // Validate date range
      const startDate = new Date(validatedData.startDate);
      const endDate = new Date(validatedData.endDate);

      if (endDate <= startDate) {
        throw createBadRequestError('End date must be after start date');
      }

      // Calculate total amount
      const totalAmount = validatedData.items.reduce((sum, item) => sum + item.amount, 0);

      // Create budget with items in transaction
      const budget = await executeInTransaction(async (prisma) => {
        const newBudget = await prisma.budget.create({
          data: {
            name: validatedData.name,
            description: validatedData.description,
            fiscalYear: validatedData.fiscalYear,
            startDate,
            endDate,
            totalAmount: new Decimal(totalAmount),
            status: 'DRAFT',
            branchId: validatedData.branchId,
            createdBy: userId!,
          },
        });

        // Create budget items
        await prisma.budgetItem.createMany({
          data: validatedData.items.map((item) => ({
            budgetId: newBudget.id,
            name: item.name,
            category: item.category,
            amount: new Decimal(item.amount),
            description: item.description,
          })),
        });

        // Fetch complete budget with items
        return prisma.budget.findUnique({
          where: { id: newBudget.id },
          include: {
            items: true,
            branch: true,
          },
        });
      });

      res.status(201).json({
        success: true,
        data: budget,
        message: 'Budget created successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/budgets:
   *   get:
   *     summary: List budgets
   *     tags: [Budgets]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: fiscalYear
   *         schema:
   *           type: integer
   *       - in: query
   *         name: status
   *         schema:
   *           type: string
   *           enum: [DRAFT, APPROVED, ACTIVE, CLOSED]
   *       - in: query
   *         name: branchId
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
   *           default: 20
   *     responses:
   *       200:
   *         description: Budgets retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async list(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { fiscalYear, status, branchId, page = '1', limit = '20' } = req.query;

      const pageNum = parseInt(page as string);
      const limitNum = parseInt(limit as string);
      const skip = (pageNum - 1) * limitNum;

      const where: any = {};
      if (fiscalYear) where.fiscalYear = parseInt(fiscalYear as string);
      if (status) where.status = status;
      if (branchId) where.branchId = branchId;

      const [budgets, total] = await Promise.all([
        executeInTransaction(async (prisma) => {
          return prisma.budget.findMany({
            where,
            skip,
            take: limitNum,
            include: {
              branch: true,
              items: true,
            },
            orderBy: {
              createdAt: 'desc',
            },
          });
        }),
        executeInTransaction(async (prisma) => {
          return prisma.budget.count({ where });
        }),
      ]);

      res.json({
        success: true,
        data: budgets,
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
   * /api/v1/budgets/{id}:
   *   get:
   *     summary: Get budget by ID
   *     tags: [Budgets]
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
   *         description: Budget retrieved successfully
   *       404:
   *         description: Budget not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getById(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const budget = await executeInTransaction(async (prisma) => {
        return prisma.budget.findUnique({
          where: { id },
          include: {
            branch: true,
            items: {
              include: {
                actuals: {
                  orderBy: {
                    date: 'desc',
                  },
                },
              },
            },
            actuals: {
              orderBy: {
                date: 'desc',
              },
            },
          },
        });
      });

      if (!budget) {
        throw createNotFoundError('Budget');
      }

      // Calculate variance for each item
      const itemsWithVariance = budget.items.map((item) => {
        const actualSpent = item.actuals.reduce(
          (sum, actual) => sum + Number(actual.amount),
          0
        );
        const budgetAmount = Number(item.amount);
        const variance = budgetAmount - actualSpent;
        const variancePercentage = budgetAmount > 0 ? (variance / budgetAmount) * 100 : 0;

        return {
          ...item,
          actualSpent: new Decimal(actualSpent),
          variance: new Decimal(variance),
          variancePercentage: variancePercentage.toFixed(2),
          utilizationPercentage: budgetAmount > 0 ? ((actualSpent / budgetAmount) * 100).toFixed(2) : '0',
        };
      });

      res.json({
        success: true,
        data: {
          ...budget,
          items: itemsWithVariance,
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/budgets/{id}:
   *   put:
   *     summary: Update budget
   *     tags: [Budgets]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             properties:
   *               name:
   *                 type: string
   *               description:
   *                 type: string
   *               startDate:
   *                 type: string
   *                 format: date-time
   *               endDate:
   *                 type: string
   *                 format: date-time
   *     responses:
   *       200:
   *         description: Budget updated successfully
   *       400:
   *         description: Cannot update approved budget
   *       404:
   *         description: Budget not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async update(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = updateBudgetSchema.parse(req.body);

      const budget = await executeInTransaction(async (prisma) => {
        return prisma.budget.findUnique({ where: { id } });
      });

      if (!budget) {
        throw createNotFoundError('Budget');
      }

      if (budget.status !== 'DRAFT') {
        throw createBadRequestError('Only draft budgets can be updated');
      }

      const updateData: any = {};
      if (validatedData.name) updateData.name = validatedData.name;
      if (validatedData.description !== undefined) updateData.description = validatedData.description;
      if (validatedData.startDate) updateData.startDate = new Date(validatedData.startDate);
      if (validatedData.endDate) updateData.endDate = new Date(validatedData.endDate);

      const updatedBudget = await executeInTransaction(async (prisma) => {
        return prisma.budget.update({
          where: { id },
          data: updateData,
          include: {
            items: true,
            branch: true,
          },
        });
      });

      res.json({
        success: true,
        data: updatedBudget,
        message: 'Budget updated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/budgets/{id}/approve:
   *   post:
   *     summary: Approve budget
   *     tags: [Budgets]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             properties:
   *               notes:
   *                 type: string
   *     responses:
   *       200:
   *         description: Budget approved successfully
   *       400:
   *         description: Only draft budgets can be approved
   *       404:
   *         description: Budget not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async approve(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { notes } = approveBudgetSchema.parse(req.body);
      const userId = req.user?.id;

      const budget = await executeInTransaction(async (prisma) => {
        return prisma.budget.findUnique({ where: { id } });
      });

      if (!budget) {
        throw createNotFoundError('Budget');
      }

      if (budget.status !== 'DRAFT') {
        throw createBadRequestError('Only draft budgets can be approved');
      }

      const updatedBudget = await executeInTransaction(async (prisma) => {
        return prisma.budget.update({
          where: { id },
          data: {
            status: 'APPROVED',
            metadata: {
              approvedBy: userId,
              approvedAt: new Date(),
              approvalNotes: notes,
            },
          },
          include: {
            items: true,
            branch: true,
          },
        });
      });

      res.json({
        success: true,
        data: updatedBudget,
        message: 'Budget approved successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/budgets/{id}/actuals:
   *   post:
   *     summary: Record actual expense
   *     tags: [Budgets]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - budgetItemId
   *               - amount
   *               - date
   *               - category
   *             properties:
   *               budgetItemId:
   *                 type: string
   *               amount:
   *                 type: number
   *               date:
   *                 type: string
   *                 format: date-time
   *               category:
   *                 type: string
   *               description:
   *                 type: string
   *               reference:
   *                 type: string
   *     responses:
   *       201:
   *         description: Actual expense recorded successfully
   *       400:
   *         description: Validation error
   *       404:
   *         description: Budget not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async recordActual(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = recordActualSchema.parse(req.body);
      const userId = req.user?.id;

      const budget = await executeInTransaction(async (prisma) => {
        return prisma.budget.findUnique({
          where: { id },
          include: {
            items: true,
          },
        });
      });

      if (!budget) {
        throw createNotFoundError('Budget');
      }

      // Verify budget item belongs to this budget
      const budgetItem = budget.items.find((item) => item.id === validatedData.budgetItemId);
      if (!budgetItem) {
        throw createBadRequestError('Budget item not found in this budget');
      }

      const actual = await executeInTransaction(async (prisma) => {
        return prisma.budgetActual.create({
          data: {
            budgetId: id,
            budgetItemId: validatedData.budgetItemId,
            amount: new Decimal(validatedData.amount),
            date: new Date(validatedData.date),
            category: validatedData.category,
            description: validatedData.description,
            reference: validatedData.reference,
            createdBy: userId!,
          },
        });
      });

      res.status(201).json({
        success: true,
        data: actual,
        message: 'Actual expense recorded successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/budgets/{id}/variance:
   *   get:
   *     summary: Get budget variance analysis
   *     tags: [Budgets]
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
   *         description: Variance analysis retrieved successfully
   *       404:
   *         description: Budget not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getVariance(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const budget = await executeInTransaction(async (prisma) => {
        return prisma.budget.findUnique({
          where: { id },
          include: {
            items: {
              include: {
                actuals: true,
              },
            },
          },
        });
      });

      if (!budget) {
        throw createNotFoundError('Budget');
      }

      // Calculate variance for each item
      const itemVariances = budget.items.map((item) => {
        const budgetAmount = Number(item.amount);
        const actualSpent = item.actuals.reduce(
          (sum, actual) => sum + Number(actual.amount),
          0
        );
        const variance = budgetAmount - actualSpent;
        const variancePercentage = budgetAmount > 0 ? (variance / budgetAmount) * 100 : 0;
        const status = variance < 0 ? 'OVER_BUDGET' : variance === 0 ? 'ON_BUDGET' : 'UNDER_BUDGET';

        return {
          itemId: item.id,
          itemName: item.name,
          category: item.category,
          budgetAmount: new Decimal(budgetAmount),
          actualSpent: new Decimal(actualSpent),
          variance: new Decimal(variance),
          variancePercentage: variancePercentage.toFixed(2),
          utilizationPercentage: budgetAmount > 0 ? ((actualSpent / budgetAmount) * 100).toFixed(2) : '0',
          status,
        };
      });

      // Calculate overall variance
      const totalBudget = Number(budget.totalAmount);
      const totalActual = itemVariances.reduce((sum, item) => sum + Number(item.actualSpent), 0);
      const totalVariance = totalBudget - totalActual;
      const totalVariancePercentage = totalBudget > 0 ? (totalVariance / totalBudget) * 100 : 0;

      res.json({
        success: true,
        data: {
          budgetId: budget.id,
          budgetName: budget.name,
          fiscalYear: budget.fiscalYear,
          totalBudget: budget.totalAmount,
          totalActual: new Decimal(totalActual),
          totalVariance: new Decimal(totalVariance),
          totalVariancePercentage: totalVariancePercentage.toFixed(2),
          utilizationPercentage: totalBudget > 0 ? ((totalActual / totalBudget) * 100).toFixed(2) : '0',
          items: itemVariances,
          overBudgetItems: itemVariances.filter((item) => item.status === 'OVER_BUDGET').length,
          underBudgetItems: itemVariances.filter((item) => item.status === 'UNDER_BUDGET').length,
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/budgets/{id}:
   *   delete:
   *     summary: Delete budget
   *     tags: [Budgets]
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
   *         description: Budget deleted successfully
   *       400:
   *         description: Cannot delete approved budget
   *       404:
   *         description: Budget not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async delete(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const budget = await executeInTransaction(async (prisma) => {
        return prisma.budget.findUnique({ where: { id } });
      });

      if (!budget) {
        throw createNotFoundError('Budget');
      }

      if (budget.status !== 'DRAFT') {
        throw createBadRequestError('Only draft budgets can be deleted');
      }

      await executeInTransaction(async (prisma) => {
        // Delete budget items first (cascade should handle this, but being explicit)
        await prisma.budgetItem.deleteMany({
          where: { budgetId: id },
        });

        // Delete budget
        await prisma.budget.delete({
          where: { id },
        });
      });

      res.json({
        success: true,
        message: 'Budget deleted successfully',
      });
    } catch (error) {
      next(error);
    }
  }
}

export default BudgetController;
