import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';

/**
 * Budget creation data
 */
export interface CreateBudgetData {
  name: string;
  description?: string;
  startDate: Date;
  endDate: Date;
  fiscalYear: number;
  branchId?: string;
  items: {
    name: string;
    category: string;
    amount: number;
    description?: string;
  }[];
  metadata?: Record<string, any>;
  createdBy: string;
}

/**
 * Budget update data
 */
export interface UpdateBudgetData {
  name?: string;
  description?: string;
  startDate?: Date;
  endDate?: Date;
  status?: string;
  metadata?: Record<string, any>;
}

/**
 * Budget item update data
 */
export interface UpdateBudgetItemData {
  name?: string;
  category?: string;
  amount?: number;
  description?: string;
}

/**
 * Budget actual recording data
 */
export interface RecordBudgetActualData {
  budgetItemId: string;
  amount: number;
  date: Date;
  description?: string;
  reference?: string;
  metadata?: Record<string, any>;
  createdBy: string;
}

/**
 * Budget query parameters
 */
export interface BudgetQueryParams {
  status?: string;
  fiscalYear?: number;
  branchId?: string;
  startDate?: Date;
  endDate?: Date;
  search?: string;
  page?: number;
  limit?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export class BudgetService {
  private prisma = getPrismaClient();

  /**
   * Create budget with items
   */
  async createBudget(data: CreateBudgetData) {
    try {
      logger.info('Creating budget', {
        name: data.name,
        itemsCount: data.items.length,
      });

      // Validate dates
      if (data.startDate >= data.endDate) {
        throw new Error('Start date must be before end date');
      }

      // Validate items
      if (!data.items || data.items.length === 0) {
        throw new Error('Budget must have at least one item');
      }

      // Calculate total budget amount
      const totalAmount = data.items.reduce((sum, item) => sum + item.amount, 0);

      // Create budget with items
      const budget = await this.prisma.$transaction(async (tx) => {
        // Create budget
        const newBudget = await tx.budget.create({
          data: {
            name: data.name,
            description: data.description,
            startDate: data.startDate,
            endDate: data.endDate,
            fiscalYear: data.fiscalYear,
            totalAmount,
            status: 'DRAFT',
            branchId: data.branchId,
            metadata: data.metadata as Prisma.JsonObject,
            createdBy: data.createdBy,
          },
        });

        // Create budget items
        await tx.budgetItem.createMany({
          data: data.items.map(item => ({
            budgetId: newBudget.id,
            name: item.name,
            category: item.category,
            amount: item.amount,
            description: item.description,
          })),
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.createdBy,
            action: 'BUDGET_CREATE',
            entityType: 'Budget',
            entityId: newBudget.id,
            changes: {
              name: data.name,
              totalAmount,
              itemsCount: data.items.length,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return newBudget;
      });

      logger.info('Budget created successfully', {
        budgetId: budget.id,
      });

      // Return budget with items
      return this.getBudgetById(budget.id);
    } catch (error) {
      logger.error('Error creating budget:', error);
      throw error;
    }
  }

  /**
   * Get budget by ID
   */
  async getBudgetById(id: string) {
    try {
      const budget = await this.prisma.budget.findUnique({
        where: { id },
        include: {
          items: {
            orderBy: {
              category: 'asc',
            },
          },
          branch: true,
        },
      });

      if (!budget) {
        throw new Error('Budget not found');
      }

      return budget;
    } catch (error) {
      logger.error('Error getting budget:', error);
      throw error;
    }
  }

  /**
   * Update budget
   */
  async updateBudget(id: string, data: UpdateBudgetData, userId: string) {
    try {
      logger.info('Updating budget', {
        budgetId: id,
      });

      // Get existing budget
      const existingBudget = await this.prisma.budget.findUnique({
        where: { id },
      });

      if (!existingBudget) {
        throw new Error('Budget not found');
      }

      // Validate status transition
      if (data.status && !this.isValidStatusTransition(existingBudget.status, data.status)) {
        throw new Error(`Invalid status transition from ${existingBudget.status} to ${data.status}`);
      }

      // Validate dates if provided
      if (data.startDate && data.endDate && data.startDate >= data.endDate) {
        throw new Error('Start date must be before end date');
      }

      // Update budget
      const budget = await this.prisma.$transaction(async (tx) => {
        const updated = await tx.budget.update({
          where: { id },
          data: {
            ...data,
            metadata: data.metadata as Prisma.JsonObject,
            updatedAt: new Date(),
          },
          include: {
            items: true,
            branch: true,
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BUDGET_UPDATE',
            entityType: 'Budget',
            entityId: id,
            changes: data as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return updated;
      });

      logger.info('Budget updated successfully', {
        budgetId: id,
      });

      return budget;
    } catch (error) {
      logger.error('Error updating budget:', error);
      throw error;
    }
  }

  /**
   * Delete budget
   */
  async deleteBudget(id: string, userId: string) {
    try {
      logger.info('Deleting budget', {
        budgetId: id,
      });

      const budget = await this.prisma.budget.findUnique({
        where: { id },
      });

      if (!budget) {
        throw new Error('Budget not found');
      }

      // Only allow deletion of DRAFT budgets
      if (budget.status !== 'DRAFT') {
        throw new Error('Only draft budgets can be deleted');
      }

      await this.prisma.$transaction(async (tx) => {
        // Delete budget items
        await tx.budgetItem.deleteMany({
          where: { budgetId: id },
        });

        // Delete budget actuals
        await tx.budgetActual.deleteMany({
          where: { budgetId: id },
        });

        // Delete budget
        await tx.budget.delete({
          where: { id },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BUDGET_DELETE',
            entityType: 'Budget',
            entityId: id,
            changes: {
              name: budget.name,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });
      });

      logger.info('Budget deleted successfully', {
        budgetId: id,
      });
    } catch (error) {
      logger.error('Error deleting budget:', error);
      throw error;
    }
  }

  /**
   * Query budgets
   */
  async queryBudgets(params: BudgetQueryParams) {
    try {
      const {
        status,
        fiscalYear,
        branchId,
        startDate,
        endDate,
        search,
        page = 1,
        limit = 20,
        sortBy = 'createdAt',
        sortOrder = 'desc',
      } = params;

      // Build where clause
      const where: Prisma.BudgetWhereInput = {
        ...(status && { status }),
        ...(fiscalYear && { fiscalYear }),
        ...(branchId && { branchId }),
        ...(startDate && {
          startDate: {
            gte: startDate,
          },
        }),
        ...(endDate && {
          endDate: {
            lte: endDate,
          },
        }),
        ...(search && {
          OR: [
            { name: { contains: search, mode: 'insensitive' } },
            { description: { contains: search, mode: 'insensitive' } },
          ],
        }),
      };

      // Get total count
      const total = await this.prisma.budget.count({ where });

      // Get budgets
      const budgets = await this.prisma.budget.findMany({
        where,
        include: {
          items: true,
          branch: true,
        },
        orderBy: {
          [sortBy]: sortOrder,
        },
        skip: (page - 1) * limit,
        take: limit,
      });

      return {
        data: budgets,
        pagination: {
          page,
          limit,
          total,
          totalPages: Math.ceil(total / limit),
        },
      };
    } catch (error) {
      logger.error('Error querying budgets:', error);
      throw error;
    }
  }

  /**
   * Add budget item
   */
  async addBudgetItem(
    budgetId: string,
    itemData: {
      name: string;
      category: string;
      amount: number;
      description?: string;
    },
    userId: string
  ) {
    try {
      logger.info('Adding budget item', {
        budgetId,
        itemName: itemData.name,
      });

      const budget = await this.prisma.budget.findUnique({
        where: { id: budgetId },
      });

      if (!budget) {
        throw new Error('Budget not found');
      }

      if (budget.status !== 'DRAFT') {
        throw new Error('Can only add items to draft budgets');
      }

      const item = await this.prisma.$transaction(async (tx) => {
        // Create budget item
        const newItem = await tx.budgetItem.create({
          data: {
            budgetId,
            name: itemData.name,
            category: itemData.category,
            amount: itemData.amount,
            description: itemData.description,
          },
        });

        // Update budget total amount
        await tx.budget.update({
          where: { id: budgetId },
          data: {
            totalAmount: {
              increment: itemData.amount,
            },
            updatedAt: new Date(),
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BUDGET_ITEM_ADD',
            entityType: 'BudgetItem',
            entityId: newItem.id,
            changes: itemData as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return newItem;
      });

      logger.info('Budget item added successfully', {
        itemId: item.id,
      });

      return item;
    } catch (error) {
      logger.error('Error adding budget item:', error);
      throw error;
    }
  }

  /**
   * Update budget item
   */
  async updateBudgetItem(
    itemId: string,
    data: UpdateBudgetItemData,
    userId: string
  ) {
    try {
      logger.info('Updating budget item', {
        itemId,
      });

      const existingItem = await this.prisma.budgetItem.findUnique({
        where: { id: itemId },
        include: {
          budget: true,
        },
      });

      if (!existingItem) {
        throw new Error('Budget item not found');
      }

      if (existingItem.budget.status !== 'DRAFT') {
        throw new Error('Can only update items in draft budgets');
      }

      const item = await this.prisma.$transaction(async (tx) => {
        // Update budget item
        const updated = await tx.budgetItem.update({
          where: { id: itemId },
          data,
        });

        // If amount changed, update budget total
        if (data.amount !== undefined) {
          const amountDiff = data.amount - Number(existingItem.amount);
          await tx.budget.update({
            where: { id: existingItem.budgetId },
            data: {
              totalAmount: {
                increment: amountDiff,
              },
              updatedAt: new Date(),
            },
          });
        }

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BUDGET_ITEM_UPDATE',
            entityType: 'BudgetItem',
            entityId: itemId,
            changes: data as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return updated;
      });

      logger.info('Budget item updated successfully', {
        itemId,
      });

      return item;
    } catch (error) {
      logger.error('Error updating budget item:', error);
      throw error;
    }
  }

  /**
   * Delete budget item
   */
  async deleteBudgetItem(itemId: string, userId: string) {
    try {
      logger.info('Deleting budget item', {
        itemId,
      });

      const item = await this.prisma.budgetItem.findUnique({
        where: { id: itemId },
        include: {
          budget: true,
        },
      });

      if (!item) {
        throw new Error('Budget item not found');
      }

      if (item.budget.status !== 'DRAFT') {
        throw new Error('Can only delete items from draft budgets');
      }

      await this.prisma.$transaction(async (tx) => {
        // Delete budget item
        await tx.budgetItem.delete({
          where: { id: itemId },
        });

        // Update budget total amount
        await tx.budget.update({
          where: { id: item.budgetId },
          data: {
            totalAmount: {
              decrement: item.amount,
            },
            updatedAt: new Date(),
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'BUDGET_ITEM_DELETE',
            entityType: 'BudgetItem',
            entityId: itemId,
            changes: {
              name: item.name,
              amount: item.amount,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });
      });

      logger.info('Budget item deleted successfully', {
        itemId,
      });
    } catch (error) {
      logger.error('Error deleting budget item:', error);
      throw error;
    }
  }

  /**
   * Record budget actual
   */
  async recordBudgetActual(data: RecordBudgetActualData) {
    try {
      logger.info('Recording budget actual', {
        budgetItemId: data.budgetItemId,
        amount: data.amount,
      });

      // Validate budget item exists
      const budgetItem = await this.prisma.budgetItem.findUnique({
        where: { id: data.budgetItemId },
        include: {
          budget: true,
        },
      });

      if (!budgetItem) {
        throw new Error('Budget item not found');
      }

      // Validate budget is active
      if (budgetItem.budget.status !== 'ACTIVE') {
        throw new Error('Budget must be active to record actuals');
      }

      // Validate date is within budget period
      if (data.date < budgetItem.budget.startDate || data.date > budgetItem.budget.endDate) {
        throw new Error('Date must be within budget period');
      }

      // Create budget actual
      const actual = await this.prisma.$transaction(async (tx) => {
        const newActual = await tx.budgetActual.create({
          data: {
            budgetId: budgetItem.budgetId,
            budgetItemId: data.budgetItemId,
            amount: data.amount,
            date: data.date,
            category: budgetItem.category,
            description: data.description,
            reference: data.reference,
            metadata: data.metadata as Prisma.JsonObject,
            createdBy: data.createdBy,
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.createdBy,
            action: 'BUDGET_ACTUAL_RECORD',
            entityType: 'BudgetActual',
            entityId: newActual.id,
            changes: {
              budgetItemId: data.budgetItemId,
              amount: data.amount,
              date: data.date.toISOString(),
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return newActual;
      });

      logger.info('Budget actual recorded successfully', {
        actualId: actual.id,
      });

      return actual;
    } catch (error) {
      logger.error('Error recording budget actual:', error);
      throw error;
    }
  }

  /**
   * Get budget actuals
   */
  async getBudgetActuals(budgetId: string, startDate?: Date, endDate?: Date) {
    try {
      const where: Prisma.BudgetActualWhereInput = {
        budgetId,
        ...(startDate && {
          date: {
            gte: startDate,
          },
        }),
        ...(endDate && {
          date: {
            ...((startDate && { gte: startDate }) || {}),
            lte: endDate,
          },
        }),
      };

      const actuals = await this.prisma.budgetActual.findMany({
        where,
        include: {
          budgetItem: true,
        },
        orderBy: {
          date: 'desc',
        },
      });

      return actuals;
    } catch (error) {
      logger.error('Error getting budget actuals:', error);
      throw error;
    }
  }

  /**
   * Validate status transition
   */
  private isValidStatusTransition(currentStatus: string, newStatus: string): boolean {
    const validTransitions: Record<string, string[]> = {
      DRAFT: ['ACTIVE', 'CANCELLED'],
      ACTIVE: ['CLOSED', 'CANCELLED'],
      CLOSED: [],
      CANCELLED: [],
    };

    return validTransitions[currentStatus]?.includes(newStatus) || false;
  }

  /**
   * Get budget categories
   */
  async getBudgetCategories(budgetId?: string) {
    try {
      const where: Prisma.BudgetItemWhereInput = {
        ...(budgetId && { budgetId }),
      };

      const items = await this.prisma.budgetItem.findMany({
        where,
        select: {
          category: true,
        },
        distinct: ['category'],
      });

      return items.map(item => item.category);
    } catch (error) {
      logger.error('Error getting budget categories:', error);
      throw error;
    }
  }
}

export default BudgetService;
