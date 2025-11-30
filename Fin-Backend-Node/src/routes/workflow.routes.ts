import { Router } from 'express';
import { WorkflowController } from '@controllers/WorkflowController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const workflowController = new WorkflowController();

// All routes require authentication
router.use(authenticate);

// Start workflow
router.post(
  '/start',
  authorize('workflows', 'create'),
  workflowController.startWorkflow.bind(workflowController)
);

// Get pending approvals
router.get(
  '/pending',
  authenticate,
  workflowController.getPendingApprovals.bind(workflowController)
);

// Get workflow status
router.get(
  '/:id/status',
  authorize('workflows', 'read'),
  workflowController.getStatus.bind(workflowController)
);

// Approve step
router.post(
  '/:id/approve',
  authorize('workflows', 'approve'),
  workflowController.approveStep.bind(workflowController)
);

// Reject step
router.post(
  '/:id/reject',
  authorize('workflows', 'approve'),
  workflowController.rejectStep.bind(workflowController)
);

export default router;
