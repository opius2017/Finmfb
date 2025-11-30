import { Router } from 'express';
import { MetricsController } from '@controllers/MetricsController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const metricsController = new MetricsController();

// Health check (public)
router.get(
  '/health',
  metricsController.getHealthStatus.bind(metricsController)
);

// All other routes require authentication
router.use(authenticate);

// Get performance metrics
router.get(
  '/performance',
  authorize('metrics', 'read'),
  metricsController.getPerformanceMetrics.bind(metricsController)
);

// Get query statistics
router.get(
  '/queries',
  authorize('metrics', 'read'),
  metricsController.getQueryStats.bind(metricsController)
);

// Get system information
router.get(
  '/system',
  authorize('metrics', 'read'),
  metricsController.getSystemInfo.bind(metricsController)
);

// Reset metrics
router.post(
  '/reset',
  authorize('metrics', 'delete'),
  metricsController.resetMetrics.bind(metricsController)
);

export default router;
