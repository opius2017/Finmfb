import { Router } from 'express';
import { BankController } from '@controllers/BankController';
import { authenticate } from '@middleware/auth';

const router = Router();
const bankController = new BankController();

// Bank connection management
router.post('/connections', authenticate, bankController.createBankConnection);
router.get('/connections', authenticate, bankController.listBankConnections);
router.get('/connections/:id', authenticate, bankController.getBankConnection);
router.post('/connections/:id/test', authenticate, bankController.testBankConnection);

// Bank reconciliation
router.post('/transactions/import', authenticate, bankController.importBankTransactions);
router.get('/transactions/unmatched/:bankConnectionId', authenticate, bankController.getUnmatchedBankTransactions);
router.post('/transactions/match', authenticate, bankController.matchTransactions);
router.post('/transactions/auto-match', authenticate, bankController.autoMatchTransactions);
router.get('/reconciliation/summary', authenticate, bankController.getReconciliationSummary);

export default router;
