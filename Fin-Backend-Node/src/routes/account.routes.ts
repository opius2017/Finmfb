import { Router } from 'express';
import { AccountController } from '@controllers/AccountController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const accountController = new AccountController();

// All routes require authentication
router.use(authenticate);

// Create account
router.post(
  '/',
  authorize('accounts', 'create'),
  accountController.create.bind(accountController)
);

// List accounts
router.get(
  '/',
  authorize('accounts', 'read'),
  accountController.list.bind(accountController)
);

// Get account by ID
router.get(
  '/:id',
  authorize('accounts', 'read'),
  accountController.getById.bind(accountController)
);

// Get account by account number
router.get(
  '/number/:accountNumber',
  authorize('accounts', 'read'),
  accountController.getByAccountNumber.bind(accountController)
);

// Get account balance
router.get(
  '/:id/balance',
  authorize('accounts', 'read'),
  accountController.getBalance.bind(accountController)
);

// Generate account statement
router.get(
  '/:id/statement',
  authorize('accounts', 'read'),
  accountController.generateStatement.bind(accountController)
);

// Update account
router.put(
  '/:id',
  authorize('accounts', 'update'),
  accountController.update.bind(accountController)
);

// Close account
router.post(
  '/:id/close',
  authorize('accounts', 'delete'),
  accountController.close.bind(accountController)
);

export default router;
