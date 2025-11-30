import { Router } from 'express';
import { LoanController } from '@controllers/LoanController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const loanController = new LoanController();

// All routes require authentication
router.use(authenticate);

// Submit loan application
router.post(
  '/apply',
  authorize('loans', 'create'),
  loanController.submitApplication.bind(loanController)
);

// Check loan eligibility
router.get(
  '/eligibility/:memberId',
  authorize('loans', 'read'),
  loanController.checkEligibility.bind(loanController)
);

// Approve loan
router.post(
  '/:id/approve',
  authorize('loans', 'approve'),
  loanController.approveLoan.bind(loanController)
);

// Reject loan
router.post(
  '/:id/reject',
  authorize('loans', 'approve'),
  loanController.rejectLoan.bind(loanController)
);

// Disburse loan
router.post(
  '/:id/disburse',
  authorize('loans', 'approve'),
  loanController.disburseLoan.bind(loanController)
);

// Record loan payment
router.post(
  '/:id/payments',
  authorize('loans', 'create'),
  loanController.recordPayment.bind(loanController)
);

// Get loan details
router.get(
  '/:id',
  authorize('loans', 'read'),
  loanController.getLoanDetails.bind(loanController)
);

// Get portfolio summary
router.get(
  '/portfolio/summary',
  authorize('loans', 'read'),
  loanController.getPortfolioSummary.bind(loanController)
);

// Get aging report
router.get(
  '/reports/aging',
  authorize('loans', 'read'),
  loanController.getAgingReport.bind(loanController)
);

export default router;
