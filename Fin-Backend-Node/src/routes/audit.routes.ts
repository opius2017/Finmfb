import { Router } from 'express';
import { AuditController } from '@controllers/AuditController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const auditController = new AuditController();

// All routes require authentication and admin/auditor role
router.use(authenticate);

// Query audit logs
router.get(
  '/logs',
  authorize('audit', 'read'),
  auditController.queryLogs.bind(auditController)
);

// Get audit log by ID
router.get(
  '/logs/:id',
  authorize('audit', 'read'),
  auditController.getLogById.bind(auditController)
);

// Get audit statistics
router.get(
  '/stats',
  authorize('audit', 'read'),
  auditController.getStats.bind(auditController)
);

// Export audit logs
router.post(
  '/export',
  authorize('audit', 'read'),
  auditController.exportLogs.bind(auditController)
);

// Apply retention policy
router.post(
  '/retention',
  authorize('audit', 'delete'),
  auditController.applyRetentionPolicy.bind(auditController)
);

export default router;
