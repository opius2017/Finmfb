import { Router } from 'express';
import { BudgetController } from '@controllers/BudgetController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const budgetController = new BudgetController();

// All routes require authentication
router.use(authenticate);

// Create budget
router.post(
  '/',
  authorize('budgets', 'create'),
  budgetController.create.bind(budgetController)
);

// List budgets
router.get(
  '/',
  authorize('budgets', 'read'),
  budgetController.list.bind(budgetController)
);

// Get budget by ID
router.get(
  '/:id',
  authorize('budgets', 'read'),
  budgetController.getById.bind(budgetController)
);

// Update budget
router.put(
  '/:id',
  authorize('budgets', 'update'),
  budgetController.update.bind(budgetController)
);

// Approve budget
router.post(
  '/:id/approve',
  authorize('budgets', 'approve'),
  budgetController.approve.bind(budgetController)
);

// Record actual expense
router.post(
  '/:id/actuals',
  authorize('budgets', 'update'),
  budgetController.recordActual.bind(budgetController)
);

// Get variance analysis
router.get(
  '/:id/variance',
  authorize('budgets', 'read'),
  budgetController.getVariance.bind(budgetController)
);

// Delete budget
router.delete(
  '/:id',
  authorize('budgets', 'delete'),
  budgetController.delete.bind(budgetController)
);

export default router;
