import { Router } from 'express';
import { MemberController } from '@controllers/MemberController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const memberController = new MemberController();

// All routes require authentication
router.use(authenticate);

// Create member
router.post(
  '/',
  authorize('members', 'create'),
  memberController.create.bind(memberController)
);

// List members
router.get(
  '/',
  authorize('members', 'read'),
  memberController.list.bind(memberController)
);

// Get member by ID
router.get(
  '/:id',
  authorize('members', 'read'),
  memberController.getById.bind(memberController)
);

// Update member
router.put(
  '/:id',
  authorize('members', 'update'),
  memberController.update.bind(memberController)
);

// Update member status
router.patch(
  '/:id/status',
  authorize('members', 'update'),
  memberController.updateStatus.bind(memberController)
);

// Delete member
router.delete(
  '/:id',
  authorize('members', 'delete'),
  memberController.delete.bind(memberController)
);

export default router;
