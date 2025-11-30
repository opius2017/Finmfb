import { Router } from 'express';
import { NotificationController } from '@controllers/NotificationController';
import { authenticate } from '@middleware/authenticate';
import { authorize } from '@middleware/authorize';

const router = Router();
const notificationController = new NotificationController();

// All routes require authentication
router.use(authenticate);

// Send notification (admin only)
router.post(
  '/send',
  authorize('notifications', 'create'),
  notificationController.sendNotification.bind(notificationController)
);

// Get user preferences
router.get(
  '/preferences',
  notificationController.getPreferences.bind(notificationController)
);

// Update user preferences
router.put(
  '/preferences',
  notificationController.updatePreferences.bind(notificationController)
);

// Get notification history
router.get(
  '/history',
  notificationController.getHistory.bind(notificationController)
);

// Send test notification (admin only)
router.post(
  '/test',
  authorize('notifications', 'create'),
  notificationController.sendTestNotification.bind(notificationController)
);

export default router;
