import { Router } from 'express';
import { KYCController } from '@controllers/KYCController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const kycController = new KYCController();

// All routes require authentication
router.use(authenticate);

// List pending KYC
router.get(
  '/pending',
  authorize('kyc', 'read'),
  kycController.listPendingKYC.bind(kycController)
);

// Upload KYC document
router.post(
  '/members/:memberId/documents',
  authorize('kyc', 'create'),
  kycController.uploadDocument.bind(kycController)
);

// Get KYC status
router.get(
  '/members/:memberId/status',
  authorize('kyc', 'read'),
  kycController.getKYCStatus.bind(kycController)
);

// Verify KYC
router.post(
  '/members/:memberId/verify',
  authorize('kyc', 'update'),
  kycController.verifyKYC.bind(kycController)
);

// List KYC documents
router.get(
  '/members/:memberId/documents',
  authorize('kyc', 'read'),
  kycController.listDocuments.bind(kycController)
);

// Delete KYC document
router.delete(
  '/members/:memberId/documents/:documentId',
  authorize('kyc', 'delete'),
  kycController.deleteDocument.bind(kycController)
);

export default router;
