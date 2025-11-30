import { Router } from 'express';
import RegulatoryController from '../controllers/RegulatoryController';
import { authenticate } from '../middleware/authenticate';
import { authorize } from '../middleware/authorize';

const router = Router();

// All routes require authentication
router.use(authenticate);

// Regulatory Reports Routes
router.post(
  '/reports/cbn-prudential',
  authorize('regulatory:create'),
  RegulatoryController.generateCBNPrudentialReturn.bind(RegulatoryController)
);

router.post(
  '/reports/cbn-capital-adequacy',
  authorize('regulatory:create'),
  RegulatoryController.generateCBNCapitalAdequacyReport.bind(RegulatoryController)
);

router.post(
  '/reports/firs-vat',
  authorize('regulatory:create'),
  RegulatoryController.generateFIRSVATReturn.bind(RegulatoryController)
);

router.post(
  '/reports/ifrs9-ecl',
  authorize('regulatory:create'),
  RegulatoryController.generateIFRS9ECLReport.bind(RegulatoryController)
);

router.get(
  '/reports',
  authorize('regulatory:read'),
  RegulatoryController.getAllReports.bind(RegulatoryController)
);

router.get(
  '/reports/:id',
  authorize('regulatory:read'),
  RegulatoryController.getReportById.bind(RegulatoryController)
);

router.patch(
  '/reports/:id/status',
  authorize('regulatory:update'),
  RegulatoryController.updateReportStatus.bind(RegulatoryController)
);

// Compliance Checklist Routes
router.get(
  '/compliance/checklist',
  authorize('compliance:read'),
  RegulatoryController.getAllChecklistItems.bind(RegulatoryController)
);

router.post(
  '/compliance/checklist',
  authorize('compliance:create'),
  RegulatoryController.createChecklistItem.bind(RegulatoryController)
);

router.patch(
  '/compliance/checklist/:id/status',
  authorize('compliance:update'),
  RegulatoryController.updateChecklistStatus.bind(RegulatoryController)
);

router.get(
  '/compliance/checklist/overdue',
  authorize('compliance:read'),
  RegulatoryController.getOverdueItems.bind(RegulatoryController)
);

router.post(
  '/compliance/checklist/recurring',
  authorize('compliance:create'),
  RegulatoryController.createRecurringChecklists.bind(RegulatoryController)
);

router.get(
  '/compliance/dashboard',
  authorize('compliance:read'),
  RegulatoryController.getComplianceDashboard.bind(RegulatoryController)
);

// Regulatory Alerts Routes
router.get(
  '/alerts',
  authorize('regulatory:read'),
  RegulatoryController.getAllAlerts.bind(RegulatoryController)
);

router.patch(
  '/alerts/:id/acknowledge',
  authorize('regulatory:update'),
  RegulatoryController.acknowledgeAlert.bind(RegulatoryController)
);

export default router;
