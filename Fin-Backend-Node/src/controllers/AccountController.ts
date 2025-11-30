import { Request, Response, NextFunction } from 'express';
import { RepositoryFactory } from '@repositories/index';
import { z } from 'zod';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';
import { Decimal } from '@prisma/client/runtime/library';

// Validation schemas
const createAccountSchema = z.object({
  memberId: z.string().uuid(),
  type: z.enum(['SAVINGS', 'SHARES', 'CASH']),
  branchId: z.string().uuid().optional(),
  initialDeposit: z.number().min(0).optional(),
});

const updateAccountSchema = z.object({
  type: z.enum(['SAVINGS', 'SHARES', 'CASH']).optional(),
  branchId: z.string().uuid().optional(),
});

const accountStatementSchema = z.object({
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
});

export class AccountController {
  private accountRepository = RepositoryFactory.getAccountRepository();
  private memberRepository = RepositoryFactory.getMemberRepository();

  /**
   * @swagger
   * /api/v1/accounts:
   *   post:
   *     summary: Open a new account
   *     tags: [Accounts]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - memberId
   *               - type
   *             properties:
   *               memberId:
   *                 type: string
   *               type:
   *                 type: string
   *                 enum: [SAVINGS, SHARES, CASH]
   *               branchId:
   *                 type: string
   *               initialDeposit:
   *                 type: number
   *     responses:
   *       201:
   *         description: Account opened successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async create(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = createAccountSchema.parse(req.body);

      // Verify member exists
      const member = await this.memberRepository.findById(validatedData.memberId);
      if (!member) {
        throw createNotFoundError('Member');
      }

      // Check if member is active
      if (member.status !== 'ACTIVE') {
        throw createBadRequestError('Member must be active to open an account');
      }

      // Generate account number
      const accountNumber = await this.generateAccountNumber(validatedData.type);

      const account = await this.accountRepository.create({
        accountNumber,
        memberId: validatedData.memberId,
        type: validatedData.type,
        balance: validatedData.initialDeposit || 0,
        branchId: validatedData.branchId || member.branchId,
      });

      res.status(201).json({
        success: true,
        data: account,
        message: 'Account opened successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/accounts:
   *   get:
   *     summary: List all accounts
   *     tags: [Accounts]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: memberId
   *         schema:
   *           type: string
   *       - in: query
   *         name: type
   *         schema:
   *           type: string
   *           enum: [SAVINGS, SHARES, CASH]
   *       - in: query
   *         name: status
   *         schema:
   *           type: string
   *           enum: [ACTIVE, DORMANT, CLOSED]
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
   *         description: Accounts retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async list(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { memberId, type, status, page = '1', limit = '20' } = req.query;

      const pageNum = parseInt(page as string);
      const limitNum = parseInt(limit as string);
      const skip = (pageNum - 1) * limitNum;

      const where: any = {};
      if (memberId) where.memberId = memberId;
      if (type) where.type = type;
      if (status) where.status = status;

      const accounts = await this.accountRepository.findMany({
        where,
        skip,
        take: limitNum,
        include: {
          member: true,
          branch: true,
        },
      });

      const total = await this.accountRepository.count({ where });

      res.json({
        success: true,
        data: accounts,
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
   * /api/v1/accounts/{id}:
   *   get:
   *     summary: Get account by ID
   *     tags: [Accounts]
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
   *         description: Account retrieved successfully
   *       404:
   *         description: Account not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getById(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const account = await this.accountRepository.findById(id, {
        include: {
          member: true,
          branch: true,
          transactions: {
            take: 10,
            orderBy: {
              createdAt: 'desc',
            },
          },
        },
      });

      if (!account) {
        throw createNotFoundError('Account');
      }

      res.json({
        success: true,
        data: account,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/accounts/number/{accountNumber}:
   *   get:
   *     summary: Get account by account number
   *     tags: [Accounts]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: accountNumber
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Account retrieved successfully
   *       404:
   *         description: Account not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getByAccountNumber(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { accountNumber } = req.params;

      const account = await this.accountRepository.findByAccountNumber(accountNumber);

      if (!account) {
        throw createNotFoundError('Account');
      }

      res.json({
        success: true,
        data: account,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/accounts/{id}/balance:
   *   get:
   *     summary: Get account balance
   *     tags: [Accounts]
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
   *         description: Balance retrieved successfully
   *       404:
   *         description: Account not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getBalance(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const account = await this.accountRepository.findById(id);

      if (!account) {
        throw createNotFoundError('Account');
      }

      res.json({
        success: true,
        data: {
          accountId: account.id,
          accountNumber: account.accountNumber,
          balance: account.balance,
          type: account.type,
          status: account.status,
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/accounts/{id}/statement:
   *   get:
   *     summary: Generate account statement
   *     tags: [Accounts]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *       - in: query
   *         name: startDate
   *         required: true
   *         schema:
   *           type: string
   *           format: date-time
   *       - in: query
   *         name: endDate
   *         required: true
   *         schema:
   *           type: string
   *           format: date-time
   *     responses:
   *       200:
   *         description: Statement generated successfully
   *       404:
   *         description: Account not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async generateStatement(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { startDate, endDate } = accountStatementSchema.parse(req.query);

      const account = await this.accountRepository.findById(id, {
        include: {
          member: true,
          transactions: {
            where: {
              createdAt: {
                gte: new Date(startDate),
                lte: new Date(endDate),
              },
            },
            orderBy: {
              createdAt: 'asc',
            },
          },
        },
      });

      if (!account) {
        throw createNotFoundError('Account');
      }

      // Calculate running balance
      let runningBalance = new Decimal(0);
      const transactionsWithBalance = account.transactions.map((txn) => {
        if (txn.type === 'CREDIT') {
          runningBalance = runningBalance.add(txn.amount);
        } else {
          runningBalance = runningBalance.sub(txn.amount);
        }

        return {
          ...txn,
          runningBalance: runningBalance.toFixed(2),
        };
      });

      res.json({
        success: true,
        data: {
          account: {
            accountNumber: account.accountNumber,
            type: account.type,
            memberName: `${account.member.firstName} ${account.member.lastName}`,
          },
          period: {
            startDate,
            endDate,
          },
          openingBalance: account.balance,
          closingBalance: runningBalance.toFixed(2),
          transactions: transactionsWithBalance,
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/accounts/{id}:
   *   put:
   *     summary: Update account
   *     tags: [Accounts]
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
   *               type:
   *                 type: string
   *                 enum: [SAVINGS, SHARES, CASH]
   *               branchId:
   *                 type: string
   *     responses:
   *       200:
   *         description: Account updated successfully
   *       404:
   *         description: Account not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async update(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = updateAccountSchema.parse(req.body);

      const account = await this.accountRepository.findById(id);
      if (!account) {
        throw createNotFoundError('Account');
      }

      const updatedAccount = await this.accountRepository.update(id, validatedData);

      res.json({
        success: true,
        data: updatedAccount,
        message: 'Account updated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/accounts/{id}/close:
   *   post:
   *     summary: Close account
   *     tags: [Accounts]
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
   *         description: Account closed successfully
   *       400:
   *         description: Cannot close account with balance
   *       404:
   *         description: Account not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async close(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const account = await this.accountRepository.findById(id);
      if (!account) {
        throw createNotFoundError('Account');
      }

      // Check if account has balance
      if (Number(account.balance) > 0) {
        throw createBadRequestError('Cannot close account with outstanding balance');
      }

      await this.accountRepository.closeAccount(id);

      res.json({
        success: true,
        message: 'Account closed successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * Generate unique account number
   */
  private async generateAccountNumber(type: string): Promise<string> {
    const typePrefix: Record<string, string> = {
      SAVINGS: '01',
      SHARES: '02',
      CASH: '03',
    };

    const prefix = typePrefix[type] || '00';
    const timestamp = Date.now().toString().slice(-8);
    const random = Math.floor(Math.random() * 100).toString().padStart(2, '0');
    const accountNumber = `${prefix}${timestamp}${random}`;

    // Check if it already exists
    const existing = await this.accountRepository.findByAccountNumber(accountNumber);
    if (existing) {
      return this.generateAccountNumber(type);
    }

    return accountNumber;
  }
}

export default AccountController;
