import { Request, Response, NextFunction } from 'express';
import { RepositoryFactory } from '@repositories/index';
import { z } from 'zod';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';
import { executeInTransaction } from '@config/database';
import { Decimal } from '@prisma/client/runtime/library';

// Validation schemas
const createDepositSchema = z.object({
  accountId: z.string().uuid(),
  amount: z.number().positive(),
  description: z.string().optional(),
  reference: z.string().optional(),
  metadata: z.record(z.any()).optional(),
});

const createWithdrawalSchema = z.object({
  accountId: z.string().uuid(),
  amount: z.number().positive(),
  description: z.string().optional(),
  reference: z.string().optional(),
  metadata: z.record(z.any()).optional(),
});

const createTransferSchema = z.object({
  fromAccountId: z.string().uuid(),
  toAccountId: z.string().uuid(),
  amount: z.number().positive(),
  description: z.string().optional(),
  reference: z.string().optional(),
  metadata: z.record(z.any()).optional(),
});

const transactionQuerySchema = z.object({
  accountId: z.string().uuid().optional(),
  type: z.enum(['DEBIT', 'CREDIT']).optional(),
  status: z.enum(['PENDING', 'COMPLETED', 'FAILED', 'REVERSED']).optional(),
  startDate: z.string().datetime().optional(),
  endDate: z.string().datetime().optional(),
  page: z.string().optional(),
  limit: z.string().optional(),
});

export class TransactionController {
  private accountRepository = RepositoryFactory.getAccountRepository();

  /**
   * @swagger
   * /api/v1/transactions/deposit:
   *   post:
   *     summary: Create a deposit transaction
   *     tags: [Transactions]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - accountId
   *               - amount
   *             properties:
   *               accountId:
   *                 type: string
   *               amount:
   *                 type: number
   *               description:
   *                 type: string
   *               reference:
   *                 type: string
   *     responses:
   *       201:
   *         description: Deposit created successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async createDeposit(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = createDepositSchema.parse(req.body);
      const userId = req.user?.id;

      // Verify account exists and is active
      const account = await this.accountRepository.findById(validatedData.accountId);
      if (!account) {
        throw createNotFoundError('Account');
      }

      if (account.status !== 'ACTIVE') {
        throw createBadRequestError('Account is not active');
      }

      // Generate reference if not provided
      const reference = validatedData.reference || this.generateReference('DEP');

      // Create transaction in a database transaction
      const result = await executeInTransaction(async (prisma) => {
        // Create transaction record
        const transaction = await prisma.transaction.create({
          data: {
            accountId: validatedData.accountId,
            type: 'CREDIT',
            amount: new Decimal(validatedData.amount),
            description: validatedData.description || 'Deposit',
            reference,
            status: 'COMPLETED',
            metadata: validatedData.metadata,
            createdBy: userId!,
          },
        });

        // Update account balance
        await prisma.account.update({
          where: { id: validatedData.accountId },
          data: {
            balance: {
              increment: validatedData.amount,
            },
          },
        });

        return transaction;
      });

      res.status(201).json({
        success: true,
        data: result,
        message: 'Deposit created successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/transactions/withdrawal:
   *   post:
   *     summary: Create a withdrawal transaction
   *     tags: [Transactions]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - accountId
   *               - amount
   *             properties:
   *               accountId:
   *                 type: string
   *               amount:
   *                 type: number
   *               description:
   *                 type: string
   *               reference:
   *                 type: string
   *     responses:
   *       201:
   *         description: Withdrawal created successfully
   *       400:
   *         description: Insufficient balance or validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async createWithdrawal(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = createWithdrawalSchema.parse(req.body);
      const userId = req.user?.id;

      // Verify account exists and is active
      const account = await this.accountRepository.findById(validatedData.accountId);
      if (!account) {
        throw createNotFoundError('Account');
      }

      if (account.status !== 'ACTIVE') {
        throw createBadRequestError('Account is not active');
      }

      // Check sufficient balance
      if (Number(account.balance) < validatedData.amount) {
        throw createBadRequestError('Insufficient balance');
      }

      // Generate reference if not provided
      const reference = validatedData.reference || this.generateReference('WTH');

      // Create transaction in a database transaction
      const result = await executeInTransaction(async (prisma) => {
        // Create transaction record
        const transaction = await prisma.transaction.create({
          data: {
            accountId: validatedData.accountId,
            type: 'DEBIT',
            amount: new Decimal(validatedData.amount),
            description: validatedData.description || 'Withdrawal',
            reference,
            status: 'COMPLETED',
            metadata: validatedData.metadata,
            createdBy: userId!,
          },
        });

        // Update account balance
        await prisma.account.update({
          where: { id: validatedData.accountId },
          data: {
            balance: {
              decrement: validatedData.amount,
            },
          },
        });

        return transaction;
      });

      res.status(201).json({
        success: true,
        data: result,
        message: 'Withdrawal created successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/transactions/transfer:
   *   post:
   *     summary: Create a transfer transaction
   *     tags: [Transactions]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - fromAccountId
   *               - toAccountId
   *               - amount
   *             properties:
   *               fromAccountId:
   *                 type: string
   *               toAccountId:
   *                 type: string
   *               amount:
   *                 type: number
   *               description:
   *                 type: string
   *               reference:
   *                 type: string
   *     responses:
   *       201:
   *         description: Transfer created successfully
   *       400:
   *         description: Insufficient balance or validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async createTransfer(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = createTransferSchema.parse(req.body);
      const userId = req.user?.id;

      // Verify both accounts exist and are active
      const fromAccount = await this.accountRepository.findById(validatedData.fromAccountId);
      const toAccount = await this.accountRepository.findById(validatedData.toAccountId);

      if (!fromAccount) {
        throw createNotFoundError('Source account');
      }
      if (!toAccount) {
        throw createNotFoundError('Destination account');
      }

      if (fromAccount.status !== 'ACTIVE') {
        throw createBadRequestError('Source account is not active');
      }
      if (toAccount.status !== 'ACTIVE') {
        throw createBadRequestError('Destination account is not active');
      }

      // Check sufficient balance
      if (Number(fromAccount.balance) < validatedData.amount) {
        throw createBadRequestError('Insufficient balance');
      }

      // Generate reference if not provided
      const reference = validatedData.reference || this.generateReference('TRF');

      // Create transfer in a database transaction
      const result = await executeInTransaction(async (prisma) => {
        // Create debit transaction
        const debitTransaction = await prisma.transaction.create({
          data: {
            accountId: validatedData.fromAccountId,
            type: 'DEBIT',
            amount: new Decimal(validatedData.amount),
            description: validatedData.description || `Transfer to ${toAccount.accountNumber}`,
            reference,
            status: 'COMPLETED',
            metadata: {
              ...validatedData.metadata,
              transferType: 'debit',
              relatedAccount: toAccount.accountNumber,
            },
            createdBy: userId!,
          },
        });

        // Create credit transaction
        const creditTransaction = await prisma.transaction.create({
          data: {
            accountId: validatedData.toAccountId,
            type: 'CREDIT',
            amount: new Decimal(validatedData.amount),
            description: validatedData.description || `Transfer from ${fromAccount.accountNumber}`,
            reference,
            status: 'COMPLETED',
            metadata: {
              ...validatedData.metadata,
              transferType: 'credit',
              relatedAccount: fromAccount.accountNumber,
            },
            createdBy: userId!,
          },
        });

        // Update balances
        await prisma.account.update({
          where: { id: validatedData.fromAccountId },
          data: {
            balance: {
              decrement: validatedData.amount,
            },
          },
        });

        await prisma.account.update({
          where: { id: validatedData.toAccountId },
          data: {
            balance: {
              increment: validatedData.amount,
            },
          },
        });

        return { debitTransaction, creditTransaction };
      });

      res.status(201).json({
        success: true,
        data: result,
        message: 'Transfer completed successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/transactions:
   *   get:
   *     summary: List transactions
   *     tags: [Transactions]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: accountId
   *         schema:
   *           type: string
   *       - in: query
   *         name: type
   *         schema:
   *           type: string
   *           enum: [DEBIT, CREDIT]
   *       - in: query
   *         name: status
   *         schema:
   *           type: string
   *           enum: [PENDING, COMPLETED, FAILED, REVERSED]
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
   *         description: Transactions retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async list(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { accountId, type, status, startDate, endDate, page = '1', limit = '20' } = 
        transactionQuerySchema.parse(req.query);

      const pageNum = parseInt(page);
      const limitNum = parseInt(limit);
      const skip = (pageNum - 1) * limitNum;

      const where: any = {};
      if (accountId) where.accountId = accountId;
      if (type) where.type = type;
      if (status) where.status = status;
      if (startDate || endDate) {
        where.createdAt = {};
        if (startDate) where.createdAt.gte = new Date(startDate);
        if (endDate) where.createdAt.lte = new Date(endDate);
      }

      const [transactions, total] = await Promise.all([
        executeInTransaction(async (prisma) => {
          return prisma.transaction.findMany({
            where,
            skip,
            take: limitNum,
            include: {
              account: {
                include: {
                  member: true,
                },
              },
            },
            orderBy: {
              createdAt: 'desc',
            },
          });
        }),
        executeInTransaction(async (prisma) => {
          return prisma.transaction.count({ where });
        }),
      ]);

      res.json({
        success: true,
        data: transactions,
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
   * /api/v1/transactions/{id}:
   *   get:
   *     summary: Get transaction by ID
   *     tags: [Transactions]
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
   *         description: Transaction retrieved successfully
   *       404:
   *         description: Transaction not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getById(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const transaction = await executeInTransaction(async (prisma) => {
        return prisma.transaction.findUnique({
          where: { id },
          include: {
            account: {
              include: {
                member: true,
              },
            },
          },
        });
      });

      if (!transaction) {
        throw createNotFoundError('Transaction');
      }

      res.json({
        success: true,
        data: transaction,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/transactions/{id}/reverse:
   *   post:
   *     summary: Reverse a transaction
   *     tags: [Transactions]
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
   *               - reason
   *             properties:
   *               reason:
   *                 type: string
   *     responses:
   *       200:
   *         description: Transaction reversed successfully
   *       400:
   *         description: Transaction cannot be reversed
   *       404:
   *         description: Transaction not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async reverseTransaction(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { reason } = z.object({ reason: z.string() }).parse(req.body);
      const userId = req.user?.id;

      // Get original transaction
      const originalTransaction = await executeInTransaction(async (prisma) => {
        return prisma.transaction.findUnique({
          where: { id },
          include: {
            account: true,
          },
        });
      });

      if (!originalTransaction) {
        throw createNotFoundError('Transaction');
      }

      // Check if transaction can be reversed
      if (originalTransaction.status === 'REVERSED') {
        throw createBadRequestError('Transaction is already reversed');
      }

      if (originalTransaction.status !== 'COMPLETED') {
        throw createBadRequestError('Only completed transactions can be reversed');
      }

      // Create reversal in a database transaction
      const result = await executeInTransaction(async (prisma) => {
        // Create reversal transaction (opposite type)
        const reversalType = originalTransaction.type === 'DEBIT' ? 'CREDIT' : 'DEBIT';
        const reversalTransaction = await prisma.transaction.create({
          data: {
            accountId: originalTransaction.accountId,
            type: reversalType,
            amount: originalTransaction.amount,
            description: `Reversal: ${originalTransaction.description}`,
            reference: `REV-${originalTransaction.reference}`,
            status: 'COMPLETED',
            metadata: {
              reversalOf: originalTransaction.id,
              reason,
            },
            createdBy: userId!,
          },
        });

        // Update original transaction status
        await prisma.transaction.update({
          where: { id },
          data: {
            status: 'REVERSED',
            metadata: {
              ...(originalTransaction.metadata as any),
              reversedBy: userId,
              reversedAt: new Date(),
              reversalReason: reason,
              reversalTransactionId: reversalTransaction.id,
            },
          },
        });

        // Update account balance
        if (originalTransaction.type === 'DEBIT') {
          // Original was debit, so add back
          await prisma.account.update({
            where: { id: originalTransaction.accountId },
            data: {
              balance: {
                increment: Number(originalTransaction.amount),
              },
            },
          });
        } else {
          // Original was credit, so subtract
          await prisma.account.update({
            where: { id: originalTransaction.accountId },
            data: {
              balance: {
                decrement: Number(originalTransaction.amount),
              },
            },
          });
        }

        return { originalTransaction, reversalTransaction };
      });

      res.json({
        success: true,
        data: result,
        message: 'Transaction reversed successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * Generate unique transaction reference
   */
  private generateReference(prefix: string): string {
    const timestamp = Date.now().toString();
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    return `${prefix}${timestamp}${random}`;
  }
}

export default TransactionController;
