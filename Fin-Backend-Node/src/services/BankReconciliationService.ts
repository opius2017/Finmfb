import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';

/**
 * Bank transaction import data
 */
export interface ImportBankTransactionsData {
  bankConnectionId: string;
  transactions: {
    transactionDate: Date;
    description: string;
    reference: string;
    debit?: number;
    credit?: number;
    balance: number;
  }[];
  userId: string;
}

/**
 * Reconciliation match data
 */
export interface ReconciliationMatchData {
  bankTransactionId: string;
  systemTransactionId: string;
  userId: string;
}

/**
 * Reconciliation summary
 */
export interface ReconciliationSummary {
  bankConnectionId: string;
  period: {
    startDate: Date;
    endDate: Date;
  };
  bankTransactions: {
    total: number;
    matched: number;
    unmatched: number;
    totalAmount: number;
  };
  systemTransactions: {
    total: number;
    matched: number;
    unmatched: number;
    totalAmount: number;
  };
  variance: number;
}

export class BankReconciliationService {
  private prisma = getPrismaClient();

  /**
   * Import bank transactions
   */
  async importBankTransactions(data: ImportBankTransactionsData) {
    try {
      logger.info('Importing bank transactions', {
        bankConnectionId: data.bankConnectionId,
        count: data.transactions.length,
      });

      const imported = await this.prisma.$transaction(async (tx) => {
        const results = [];

        for (const txn of data.transactions) {
          // Check for duplicates
          const existing = await tx.bankTransaction.findFirst({
            where: {
              bankConnectionId: data.bankConnectionId,
              reference: txn.reference,
              transactionDate: txn.transactionDate,
            },
          });

          if (existing) {
            logger.warn('Duplicate bank transaction skipped', {
              reference: txn.reference,
            });
            continue;
          }

          // Create bank transaction
          const bankTxn = await tx.bankTransaction.create({
            data: {
              bankConnectionId: data.bankConnectionId,
              transactionDate: txn.transactionDate,
              description: txn.description,
              reference: txn.reference,
              debit: txn.debit || 0,
              credit: txn.credit || 0,
              balance: txn.balance,
              status: 'UNMATCHED',
            },
          });

          results.push(bankTxn);
        }

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'BANK_TRANSACTIONS_IMPORT',
            entityType: 'BankTransaction',
            entityId: data.bankConnectionId,
            changes: {
              imported: results.length,
              skipped: data.transactions.length - results.length,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return results;
      });

      logger.info('Bank transactions imported successfully', {
        imported: imported.length,
      });

      return imported;
    } catch (error) {
      logger.error('Error importing bank transactions:', error);
      throw error;
    }
  }

  /**
   * Get unmatched bank transactions
   */
  async getUnmatchedBankTransactions(bankConnectionId: string, startDate?: Date, endDate?: Date) {
    try {
      const transactions = await this.prisma.bankTransaction.findMany({
        where: {
          bankConnectionId,
          status: 'UNMATCHED',
          ...(startDate && {
            transactionDate: {
              gte: startDate,
            },
          }),
          ...(endDate && {
            transactionDate: {
              ...((startDate && { gte: startDate }) || {}),
              lte: endDate,
            },
          }),
        },
        orderBy: {
          transactionDate: 'desc',
        },
      });

      return transactions;
    } catch (error) {
      logger.error('Error getting unmatched bank transactions:', error);
      throw error;
    }
  }

  /**
   * Get unmatched system transactions
   */
  async getUnmatchedSystemTransactions(accountId: string, startDate?: Date, endDate?: Date) {
    try {
      const transactions = await this.prisma.transaction.findMany({
        where: {
          accountId,
          status: 'COMPLETED',
          reconciliationStatus: 'UNMATCHED',
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
        },
        orderBy: {
          createdAt: 'desc',
        },
      });

      return transactions;
    } catch (error) {
      logger.error('Error getting unmatched system transactions:', error);
      throw error;
    }
  }

  /**
   * Match bank transaction with system transaction
   */
  async matchTransactions(data: ReconciliationMatchData) {
    try {
      logger.info('Matching transactions', {
        bankTransactionId: data.bankTransactionId,
        systemTransactionId: data.systemTransactionId,
      });

      await this.prisma.$transaction(async (tx) => {
        // Update bank transaction
        await tx.bankTransaction.update({
          where: { id: data.bankTransactionId },
          data: {
            status: 'MATCHED',
            matchedTransactionId: data.systemTransactionId,
            matchedAt: new Date(),
            matchedBy: data.userId,
          },
        });

        // Update system transaction
        await tx.transaction.update({
          where: { id: data.systemTransactionId },
          data: {
            reconciliationStatus: 'MATCHED',
            reconciledAt: new Date(),
            reconciledBy: data.userId,
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'TRANSACTION_MATCH',
            entityType: 'BankTransaction',
            entityId: data.bankTransactionId,
            changes: {
              systemTransactionId: data.systemTransactionId,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });
      });

      logger.info('Transactions matched successfully');
    } catch (error) {
      logger.error('Error matching transactions:', error);
      throw error;
    }
  }

  /**
   * Auto-match transactions
   */
  async autoMatchTransactions(bankConnectionId: string, accountId: string) {
    try {
      logger.info('Auto-matching transactions', {
        bankConnectionId,
        accountId,
      });

      const [bankTransactions, systemTransactions] = await Promise.all([
        this.getUnmatchedBankTransactions(bankConnectionId),
        this.getUnmatchedSystemTransactions(accountId),
      ]);

      const matches = [];

      for (const bankTxn of bankTransactions) {
        // Try to find matching system transaction
        const match = systemTransactions.find(sysTxn => {
          // Match by amount and date (within 2 days)
          const amountMatch = 
            (bankTxn.debit > 0 && Math.abs(bankTxn.debit - Number(sysTxn.amount)) < 0.01) ||
            (bankTxn.credit > 0 && Math.abs(bankTxn.credit - Number(sysTxn.amount)) < 0.01);

          const dateMatch = Math.abs(
            bankTxn.transactionDate.getTime() - sysTxn.createdAt.getTime()
          ) < 2 * 24 * 60 * 60 * 1000; // 2 days

          // Match by reference if available
          const referenceMatch = bankTxn.reference && sysTxn.reference &&
            bankTxn.reference.toLowerCase().includes(sysTxn.reference.toLowerCase());

          return amountMatch && (dateMatch || referenceMatch);
        });

        if (match) {
          matches.push({
            bankTransactionId: bankTxn.id,
            systemTransactionId: match.id,
          });
        }
      }

      logger.info('Auto-match found matches', {
        count: matches.length,
      });

      return matches;
    } catch (error) {
      logger.error('Error auto-matching transactions:', error);
      throw error;
    }
  }

  /**
   * Generate reconciliation summary
   */
  async generateReconciliationSummary(
    bankConnectionId: string,
    accountId: string,
    startDate: Date,
    endDate: Date
  ): Promise<ReconciliationSummary> {
    try {
      logger.info('Generating reconciliation summary');

      const [bankStats, systemStats] = await Promise.all([
        this.prisma.bankTransaction.aggregate({
          where: {
            bankConnectionId,
            transactionDate: {
              gte: startDate,
              lte: endDate,
            },
          },
          _count: true,
          _sum: {
            debit: true,
            credit: true,
          },
        }),
        this.prisma.transaction.aggregate({
          where: {
            accountId,
            createdAt: {
              gte: startDate,
              lte: endDate,
            },
            status: 'COMPLETED',
          },
          _count: true,
          _sum: {
            amount: true,
          },
        }),
      ]);

      const [bankMatched, systemMatched] = await Promise.all([
        this.prisma.bankTransaction.count({
          where: {
            bankConnectionId,
            status: 'MATCHED',
            transactionDate: {
              gte: startDate,
              lte: endDate,
            },
          },
        }),
        this.prisma.transaction.count({
          where: {
            accountId,
            reconciliationStatus: 'MATCHED',
            createdAt: {
              gte: startDate,
              lte: endDate,
            },
          },
        }),
      ]);

      const bankTotal = bankStats._count;
      const bankAmount = Number(bankStats._sum.debit || 0) + Number(bankStats._sum.credit || 0);
      const systemTotal = systemStats._count;
      const systemAmount = Number(systemStats._sum.amount || 0);

      return {
        bankConnectionId,
        period: {
          startDate,
          endDate,
        },
        bankTransactions: {
          total: bankTotal,
          matched: bankMatched,
          unmatched: bankTotal - bankMatched,
          totalAmount: bankAmount,
        },
        systemTransactions: {
          total: systemTotal,
          matched: systemMatched,
          unmatched: systemTotal - systemMatched,
          totalAmount: systemAmount,
        },
        variance: Math.abs(bankAmount - systemAmount),
      };
    } catch (error) {
      logger.error('Error generating reconciliation summary:', error);
      throw error;
    }
  }

  /**
   * Unmatch transaction
   */
  async unmatchTransaction(bankTransactionId: string, userId: string) {
    try {
      logger.info('Unmatching transaction', {
        bankTransactionId,
      });

      await this.prisma.$transaction(async (tx) => {
        const bankTxn = await tx.bankTransaction.findUnique({
          where: { id: bankTransactionId },
        });

        if (!bankTxn || !bankTxn.matchedTransactionId) {
          throw new Error('Bank transaction not found or not matched');
        }

        // Update bank transaction
        await tx.bankTransaction.update({
          where: { id: bankTransactionId },
          data: {
            status: 'UNMATCHED',
            matchedTransactionId: null,
            matchedAt: null,
            matchedBy: null,
          },
        });

        // Update system transaction
        await tx.transaction.update({
          where: { id: bankTxn.matchedTransactionId },
          data: {
            reconciliationStatus: 'UNMATCHED',
            reconciledAt: null,
            reconciledBy: null,
          },
        });

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId,
            action: 'TRANSACTION_UNMATCH',
            entityType: 'BankTransaction',
            entityId: bankTransactionId,
            changes: {} as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });
      });

      logger.info('Transaction unmatched successfully');
    } catch (error) {
      logger.error('Error unmatching transaction:', error);
      throw error;
    }
  }
}

export default BankReconciliationService;
