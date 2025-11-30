import { Router } from 'express';
import { DocumentController } from '@controllers/DocumentController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const documentController = new DocumentController();

// All routes require authentication
router.use(authenticate);

// Upload document
router.post(
  '/',
  authorize('documents', 'create'),
  documentController.uploadMiddleware,
  documentController.uploadDocument.bind(documentController)
);

// Search documents
router.get(
  '/search',
  authorize('documents', 'read'),
  documentController.searchDocuments.bind(documentController)
);

// Get document
router.get(
  '/:id',
  authorize('documents', 'read'),
  documentController.getDocument.bind(documentController)
);

// Download document
router.get(
  '/:id/download',
  authorize('documents', 'read'),
  documentController.downloadDocument.bind(documentController)
);

// Update document
router.patch(
  '/:id',
  authorize('documents', 'update'),
  documentController.updateDocument.bind(documentController)
);

// Upload new version
router.post(
  '/:id/versions',
  authorize('documents', 'update'),
  documentController.uploadMiddleware,
  documentController.uploadNewVersion.bind(documentController)
);

// Get versions
router.get(
  '/:id/versions',
  authorize('documents', 'read'),
  documentController.getVersions.bind(documentController)
);

// Download specific version
router.get(
  '/:id/versions/:versionNumber/download',
  authorize('documents', 'read'),
  documentController.downloadVersion.bind(documentController)
);

// Delete document
router.delete(
  '/:id',
  authorize('documents', 'delete'),
  documentController.deleteDocument.bind(documentController)
);

export default router;
