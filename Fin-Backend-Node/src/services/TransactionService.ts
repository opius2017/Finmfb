import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';

/**
 * Transaction creation data
 */
export interface CreateTransactionData {
  accountId: string;
  type: 'DEBIT' | 'CREDIT';
  amount: number;
  description?: string;
  reference?: string;
  metadata?: Record<string, any>;
  userId: string;
}

/**
 * Transfer transaction data
 */
export interface CreateTransferData {
  fromAccountId: string;
  toAccountId: string;
  amount: number;
  description?: string;
  reference?: string;
  metadata?: Record<string, any>;
  userId: string;
}

/**
 * Transaction query parameters
 */
export interface TransactionQueryParams {
  accountId?: string;
  type?: 'DEBIT' | 'CREDIT';
  status?: 'PENDING' | 'COMPLETED' | 'FAILED' | 'REVERSED';
  startDate?: Date;
  endDate?: Date;
  minAmount?: number;
  maxAmount?: number;
  search?: string;
  page?: number;
  limit?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Transaction reversal data
 */
export interface ReverseTransactionData {
  transactionId: string;
  reason: string;
  userId: string;
  requiresApproval?: boolean;
}

export class TransactionService {
  private prisma = getPrismaClient();

  /**
   * Create deposit transaction
   */
  async createDeposit(data: CreateTransactionData) {
    try {
      logger.info('Creating deposit transaction', {
        accountId: data.accountId,
        amount: data.amount,
      });

      // Validate account exists and is active
      const account = await this.prisma.account.findUnique({
        where: { id: data.accountId },
        include: { member: true },
      });

      if (!account) {
        throw new Error('Account not found');
      }

      if (account.status !== 'ACTIVE') {
        throw new Error('Account is not active');
      }

      // Validate amount
      if (data.amount <= 0) {
        throw new Error('Amount must be greater than zero');
      }

      // Create transaction and update balance in a transaction
      const result = await this.prisma.$transaction(async (tx) => {
        // Create transaction
        const transaction = await tx.transaction.create({
          data: {
            accountId: data.accountId,
            type: 'DEBIT', // Debit increases account balance
            amount: data.amount,
            description: data.description || 'Deposit',
            reference: data.reference || this.generateReference('DEP'),
            status: 'COMPLETED',
            metadata: data.metadata as Prisma.JsonObject,
            createdBy: data.userId,
          },
          include: {
            account: {
              include: {
                member: true,
              },
            },
          },
        });

        // Update account balance
        await tx.account.update({
          where: { id: data.accountId },
          data: {
            balance: {
              increment: data.amount,
            },
            updatedAt: new Date(),
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'TRANSACTION_CREATE',
            entityType: 'Transaction',
            entityId: transaction.id,
            changes: {
              type: 'DEPOSIT',
              amount: data.amount,
              accountId: data.accountId,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return transaction;
      });

      logger.info('Deposit transaction created successfully', {
        transactionId: result.id,
      });

      return result;
    } catch (error) {
      logger.error('Error creating deposit transaction:', error);
      throw error;
    }
  }

  /**
   * Create withdrawal transaction
   */
  async createWithdrawal(data: CreateTransactionData) {
    try {
      logger.info('Creating withdrawal transaction', {
        accountId: data.accountId,
        amount: data.amount,
      });

      // Validate account exists and is active
      const account = await this.prisma.account.findUnique({
        where: { id: data.accountId },
      });

      if (!account) {
        throw new Error('Account not found');
      }

      if (account.status !== 'ACTIVE') {
        throw new Error('Account is not active');
      }

      // Validate amount
      if (data.amount <= 0) {
        throw new Error('Amount must be greater than zero');
      }

      // Check sufficient balance
      if (Number(account.balance) < data.amount) {
        throw new Error('Insufficient balance');
      }

      // Create transaction and update balance in a transaction
      const result = await this.prisma.$transaction(async (tx) => {
        // Create transaction
        const transaction = await tx.transaction.create({
          data: {
            accountId: data.accountId,
            type: 'CREDIT', // Credit decreases account balance
            amount: data.amount,
            description: data.description || 'Withdrawal',
            reference: data.reference || this.generateReference('WTH'),
            status: 'COMPLETED',
            metadata: data.metadata as Prisma.JsonObject,
            createdBy: data.userId,
          },
          include: {
            account: {
              include: {
                member: true,
              },
            },
          },
        });

        // Update account balance
        await tx.account.update({
          where: { id: data.accountId },
          data: {
            balance: {
              decrement: data.amount,
            },
            updatedAt: new Date(),
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'TRANSACTION_CREATE',
            entityType: 'Transaction',
            entityId: transaction.id,
            changes: {
              type: 'WITHDRAWAL',
              amount: data.amount,
              accountId: data.accountId,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return transaction;
      });

      logger.info('Withdrawal transaction created successfully', {
        transactionId: result.id,
      });

      return result;
    } catch (error) {
      logger.error('Error creating withdrawal transaction:', error);
      throw error;
    }
  }

  /**
   * Create transfer transaction
   */
  async createTransfer(data: CreateTransferData) {
    try {
      logger.info('Creating transfer transaction', {
        fromAccountId: data.fromAccountId,
        toAccountId: data.toAccountId,
        amount: data.amount,
      });

      // Validate accounts exist and are active
      const [fromAccount, toAccount] = await Promise.all([
        this.prisma.account.findUnique({
          where: { id: data.fromAccountId },
        }),
        this.prisma.account.findUnique({
          where: { id: data.toAccountId },
        }),
      ]);

      if (!fromAccount) {
        throw new Error('Source account not found');
      }

      if (!toAccount) {
        throw new Error('Destination account not found');
      }

      if (fromAccount.status !== 'ACTIVE') {
        throw new Error('Source account is not active');
      }

      if (toAccount.status !== 'ACTIVE') {
        throw new Error('Destination account is not active');
      }

      // Validate amount
      if (data.amount <= 0) {
        throw new Error('Amount must be greater than zero');
      }

      // Check sufficient balance
      if (Number(fromAccount.balance) < data.amount) {
        throw new Error('Insufficient balance in source account');
      }

      // Prevent transfer to same account
      if (data.fromAccountId === data.toAccountId) {
        throw new Error('Cannot transfer to the same account');
      }

      // Create transfer transactions
      const result = await this.prisma.$transaction(async (tx) => {
        const reference = data.reference || this.generateReference('TRF');

        // Create debit transaction (from account)
        const debitTransaction = await tx.transaction.create({
          data: {
            accountId: data.fromAccountId,
            type: 'CREDIT',
            amount: data.amount,
            description: data.description || `Transfer to ${toAccount.accountNumber}`,
            reference,
            status: 'COMPLETED',
            metadata: {
              ...data.metadata,
              transferType: 'OUTGOING',
              relatedAccountId: data.toAccountId,
            } as Prisma.JsonObject,
            createdBy: data.userId,
          },
        });

        // Create credit transaction (to account)
        const creditTransaction = await tx.transaction.create({
          data: {
            accountId: data.toAccountId,
            type: 'DEBIT',
            amount: data.amount,
            description: data.description || `Transfer from ${fromAccount.accountNumber}`,
            reference,
            status: 'COMPLETED',
            metadata: {
              ...data.metadata,
              transferType: 'INCOMING',
              relatedAccountId: data.fromAccountId,
            } as Prisma.JsonObject,
            createdBy: data.userId,
          },
        });

        // Update account balances
        await Promise.all([
          tx.account.update({
            where: { id: data.fromAccountId },
            data: {
              balance: {
                decrement: data.amount,
              },
              updatedAt: new Date(),
            },
          }),
          tx.account.update({
            where: { id: data.toAccountId },
            data: {
              balance: {
                increment: data.amount,
              },
              updatedAt: new Date(),
            },
          }),
        ]);

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'TRANSACTION_CREATE',
            entityType: 'Transaction',
            entityId: debitTransaction.id,
            changes: {
              type: 'TRANSFER',
              amount: data.amount,
              fromAccountId: data.fromAccountId,
              toAccountId: data.toAccountId,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return {
          debitTransaction,
          creditTransaction,
          reference,
        };
      });

      logger.info('Transfer transaction created successfully', {
        reference: result.reference,
      });

      return result;
    } catch (error) {
      logger.error('Error creating transfer transaction:', error);
      throw error;
    }
  }

  /**
   * Get transaction by ID
   */
  async getTransactionById(id: string) {
    try {
      const transaction = await this.prisma.transaction.findUnique({
        where: { id },
        include: {
          account: {
            include: {
              member: true,
            },
          },
        },
      });

      if (!transaction) {
        throw new Error('Transaction not found');
      }

      return transaction;
    } catch (error) {
      logger.error('Error getting transaction:', error);
      throw error;
    }
  }

  /**
   * Query transactions with filters and pagination
   */
  async queryTransactions(params: TransactionQueryParams) {
    try {
      const {
        accountId,
        type,
        status,
        startDate,
        endDate,
        minAmount,
        maxAmount,
        search,
        page = 1,
        limit = 20,
        sortBy = 'createdAt',
        sortOrder = 'desc',
      } = params;

      // Build where clause
      const where: Prisma.TransactionWhereInput = {
        ...(accountId && { accountId }),
        ...(type && { type }),
        ...(status && { status }),
        ...(startDate && {
          createdAt: {
            gte: startDate,
          },
        }),
        ...(endDate && {
          createdAt: {
            ...((startDate && { gte: startDate }) || {}),
            lte: endDate,
          },
        }),
        ...(minAmount && {
          amount: {
            gte: minAmount,
          },
        }),
        ...(maxAmount && {
          amount: {
            ...((minAmount && { gte: minAmount }) || {}),
            lte: maxAmount,
          },
        }),
        ...(search && {
          OR: [
            { reference: { contains: search, mode: 'insensitive' } },
            { description: { contains: search, mode: 'insensitive' } },
          ],
        }),
      };

      // Get total count
      const total = await this.prisma.transaction.count({ where });

      // Get transactions
      const transactions = await this.prisma.transaction.findMany({
        where,
        include: {
          account: {
            include: {
              member: true,
            },
          },
        },
        orderBy: {
          [sortBy]: sortOrder,
        },
        skip: (page - 1) * limit,
        take: limit,
      });

      return {
        data: transactions,
        pagination: {
          page,
          limit,
          total,
          totalPages: Math.ceil(total / limit),
        },
      };
    } catch (error) {
      logger.error('Error querying transactions:', error);
      throw error;
    }
  }

  /**
   * Get transaction summary
   */
  async getTransactionSummary(accountId?: string, startDate?: Date, endDate?: Date) {
    try {
      const where: Prisma.TransactionWhereInput = {
        ...(accountId && { accountId }),
        status: 'COMPLETED',
        ...(startDate && {
          createdAt: {
            gte: startDate,
          },
        }),
        ...(endDate && {
          createdAt: {
            ...((startDate && { gte: startDate }) || {}),
            lte: endDate,
          },
        }),
      };

      const [totalDebit, totalCredit, transactionCount] = await Promise.all([
        this.prisma.transaction.aggregate({
          where: {
            ...where,
            type: 'DEBIT',
          },
          _sum: {
            amount: true,
          },
        }),
        this.prisma.transaction.aggregate({
          where: {
            ...where,
            type: 'CREDIT',
          },
          _sum: {
            amount: true,
          },
        }),
        this.prisma.transaction.count({ where }),
      ]);

      return {
        totalDebit: Number(totalDebit._sum.amount || 0),
        totalCredit: Number(totalCredit._sum.amount || 0),
        netAmount: Number(totalDebit._sum.amount || 0) - Number(totalCredit._sum.amount || 0),
        transactionCount,
      };
    } catch (error) {
      logger.error('Error getting transaction summary:', error);
      throw error;
    }
  }

  /**
   * Reverse transaction
   */
  async reverseTransaction(data: ReverseTransactionData) {
    try {
      logger.info('Reversing transaction', {
        transactionId: data.transactionId,
      });

      // Get original transaction
      const originalTransaction = await this.prisma.transaction.findUnique({
        where: { id: data.transactionId },
        include: {
          account: true,
        },
      });

      if (!originalTransaction) {
        throw new Error('Transaction not found');
      }

      if (originalTransaction.status === 'REVERSED') {
        throw new Error('Transaction already reversed');
      }

      if (originalTransaction.status !== 'COMPLETED') {
        throw new Error('Only completed transactions can be reversed');
      }

      // Create reversal transaction
      const result = await this.prisma.$transaction(async (tx) => {
        // Create reversal transaction
        const reversalTransaction = await tx.transaction.create({
          data: {
            accountId: originalTransaction.accountId,
            type: originalTransaction.type === 'DEBIT' ? 'CREDIT' : 'DEBIT',
            amount: originalTransaction.amount,
            description: `Reversal: ${originalTransaction.description}`,
            reference: this.generateReference('REV'),
            status: 'COMPLETED',
            metadata: {
              reversalReason: data.reason,
              originalTransactionId: data.transactionId,
              originalReference: originalTransaction.reference,
            } as Prisma.JsonObject,
            createdBy: data.userId,
          },
        });

        // Update original transaction status
        await tx.transaction.update({
          where: { id: data.transactionId },
          data: {
            status: 'REVERSED',
            metadata: {
              ...(originalTransaction.metadata as object),
              reversedAt: new Date().toISOString(),
              reversedBy: data.userId,
              reversalTransactionId: reversalTransaction.id,
            } as Prisma.JsonObject,
          },
        });

        // Update account balance
        const balanceChange = originalTransaction.type === 'DEBIT' 
          ? -Number(originalTransaction.amount) 
          : Number(originalTransaction.amount);

        await tx.account.update({
          where: { id: originalTransaction.accountId },
          data: {
            balance: {
              increment: balanceChange,
            },
            updatedAt: new Date(),
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'TRANSACTION_REVERSE',
            entityType: 'Transaction',
            entityId: data.transactionId,
            changes: {
              reason: data.reason,
              reversalTransactionId: reversalTransaction.id,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return reversalTransaction;
      });

      logger.info('Transaction reversed successfully', {
        transactionId: data.transactionId,
        reversalTransactionId: result.id,
      });

      return result;
    } catch (error) {
      logger.error('Error reversing transaction:', error);
      throw error;
    }
  }

  /**
   * Generate transaction reference
   */
  private generateReference(prefix: string): string {
    const timestamp = Date.now();
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    return `${prefix}-${timestamp}-${random}`;
  }

  /**
   * Validate transaction rules
   */
  async validateTransactionRules(
    accountId: string,
    amount: number,
    type: 'DEBIT' | 'CREDIT'
  ): Promise<{ isValid: boolean; errors: string[] }> {
    const errors: string[] = [];

    // Get account
    const account = await this.prisma.account.findUnique({
      where: { id: accountId },
    });

    if (!account) {
      errors.push('Account not found');
      return { isValid: false, errors };
    }

    // Check account status
    if (account.status !== 'ACTIVE') {
      errors.push('Account is not active');
    }

    // Check amount
    if (amount <= 0) {
      errors.push('Amount must be greater than zero');
    }

    // Check balance for withdrawals
    if (type === 'CREDIT' && Number(account.balance) < amount) {
      errors.push('Insufficient balance');
    }

    // Check daily transaction limit (example: 1,000,000)
    const DAILY_LIMIT = 1000000;
    if (amount > DAILY_LIMIT) {
      errors.push(`Amount exceeds daily limit of ${DAILY_LIMIT}`);
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }
}

export default TransactionService;
