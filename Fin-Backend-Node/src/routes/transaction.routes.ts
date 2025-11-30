import { Router } from 'express';
import { TransactionController } from '@controllers/TransactionController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const transactionController = new TransactionController();

// All routes require authentication
router.use(authenticate);

// Create deposit
router.post(
  '/deposit',
  authorize('transactions', 'create'),
  transactionController.createDeposit.bind(transactionController)
);

// Create withdrawal
router.post(
  '/withdrawal',
  authorize('transactions', 'create'),
  transactionController.createWithdrawal.bind(transactionController)
);

// Create transfer
router.post(
  '/transfer',
  authorize('transactions', 'create'),
  transactionController.createTransfer.bind(transactionController)
);

// List transactions
router.get(
  '/',
  authorize('transactions', 'read'),
  transactionController.list.bind(transactionController)
);

// Get transaction by ID
router.get(
  '/:id',
  authorize('transactions', 'read'),
  transactionController.getById.bind(transactionController)
);

// Reverse transaction
router.post(
  '/:id/reverse',
  authorize('transactions', 'delete'),
  transactionController.reverseTransaction.bind(transactionController)
);

export default router;
