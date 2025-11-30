import { Request, Response } from 'express';
import { z } from 'zod';
import { BankConnectionService } from '@services/BankConnectionService';
import { BankReconciliationService } from '@services/BankReconciliationService';
import { asyncHandler } from '@utils/asyncHandler';

// Validation schemas
const createBankConnectionSchema = z.object({
  bankName: z.string().min(2),
  accountNumber: z.string().min(5),
  accountName: z.string().min(2),
  bankCode: z.string().optional(),
  branchId: z.string().uuid().optional(),
  credentials: z.record(z.any()).optional(),
  metadata: z.record(z.any()).optional(),
});

const importTransactionsSchema = z.object({
  bankConnectionId: z.string().uuid(),
  transactions: z.array(z.object({
    transactionDate: z.string().datetime(),
    description: z.string(),
    reference: z.string(),
    debit: z.number().optional(),
    credit: z.number().optional(),
    balance: z.number(),
  })),
});

const matchTransactionsSchema = z.object({
  bankTransactionId: z.string().uuid(),
  systemTransactionId: z.string().uuid(),
});

export class BankController {
  private connectionService = new BankConnectionService();
  private reconciliationService = new BankReconciliationService();

  // Bank Connection endpoints
  createBankConnection = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = req.user?.id;
    if (!userId) {
      res.status(401).json({ success: false, message: 'Not authenticated', timestamp: new Date() });
      return;
    }

    const data = createBankConnectionSchema.parse(req.body);
    const connection = await this.connectionService.createBankConnection({ ...data, createdBy: userId });

    res.status(201).json({
      success: true,
      data: connection,
      message: 'Bank connection created successfully',
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  getBankConnection = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { id } = req.params;
    const connection = await this.connectionService.getBankConnectionById(id);

    res.status(200).json({
      success: true,
      data: connection,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  listBankConnections = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { branchId } = req.query;
    const connections = await this.connectionService.listBankConnections(branchId as string | undefined);

    res.status(200).json({
      success: true,
      data: connections,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  testBankConnection = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { id } = req.params;
    const result = await this.connectionService.testBankConnection(id);

    res.status(200).json({
      success: true,
      data: result,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  // Reconciliation endpoints
  importBankTransactions = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = req.user?.id;
    if (!userId) {
      res.status(401).json({ success: false, message: 'Not authenticated', timestamp: new Date() });
      return;
    }

    const data = importTransactionsSchema.parse(req.body);
    const imported = await this.reconciliationService.importBankTransactions({
      ...data,
      transactions: data.transactions.map(t => ({
        ...t,
        transactionDate: new Date(t.transactionDate),
      })),
      userId,
    });

    res.status(201).json({
      success: true,
      data: imported,
      message: 'Bank transactions imported successfully',
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  getUnmatchedBankTransactions = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { bankConnectionId } = req.params;
    const { startDate, endDate } = req.query;

    const transactions = await this.reconciliationService.getUnmatchedBankTransactions(
      bankConnectionId,
      startDate ? new Date(startDate as string) : undefined,
      endDate ? new Date(endDate as string) : undefined
    );

    res.status(200).json({
      success: true,
      data: transactions,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  matchTransactions = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const userId = req.user?.id;
    if (!userId) {
      res.status(401).json({ success: false, message: 'Not authenticated', timestamp: new Date() });
      return;
    }

    const data = matchTransactionsSchema.parse(req.body);
    await this.reconciliationService.matchTransactions({ ...data, userId });

    res.status(200).json({
      success: true,
      message: 'Transactions matched successfully',
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  autoMatchTransactions = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { bankConnectionId, accountId } = req.body;
    const matches = await this.reconciliationService.autoMatchTransactions(bankConnectionId, accountId);

    res.status(200).json({
      success: true,
      data: matches,
      message: `Found ${matches.length} potential matches`,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  getReconciliationSummary = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { bankConnectionId, accountId, startDate, endDate } = req.query;

    const summary = await this.reconciliationService.generateReconciliationSummary(
      bankConnectionId as string,
      accountId as string,
      new Date(startDate as string),
      new Date(endDate as string)
    );

    res.status(200).json({
      success: true,
      data: summary,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });
}

export default BankController;
