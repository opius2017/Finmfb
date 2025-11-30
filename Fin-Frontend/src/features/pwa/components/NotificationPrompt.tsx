import React, { useState, useEffect } from 'react';
import { Bell, X } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { notificationService } from '../services/notificationService';

export const NotificationPrompt: React.FC = () => {
  const [showPrompt, setShowPrompt] = useState(false);

  useEffect(() => {
    const checkPermission = async () => {
      if (!notificationService.isSupported()) return;

      const permission = notificationService.getPermission();
      const dismissed = localStorage.getItem('notification-prompt-dismissed');

      if (permission === 'default' && !dismissed) {
        setTimeout(() => setShowPrompt(true), 5000);
      }
    };

    checkPermission();
  }, []);

  const handleEnable = async () => {
    const granted = await notificationService.requestPermission();
    if (granted) {
      await notificationService.subscribe();
    }
    setShowPrompt(false);
  };

  const handleDismiss = () => {
    setShowPrompt(false);
    localStorage.setItem('notification-prompt-dismissed', 'true');
  };

  if (!showPrompt) return null;

  return (
    <div className="fixed bottom-4 left-4 right-4 md:left-auto md:right-4 md:w-96 z-50">
      <Card className="p-4 shadow-lg">
        <div className="flex items-start justify-between mb-3">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center">
              <Bell className="w-6 h-6 text-primary-600" />
            </div>
            <div>
              <h3 className="font-semibold">Enable Notifications</h3>
              <p className="text-sm text-neutral-600">Stay updated</p>
            </div>
          </div>
          <button
            onClick={handleDismiss}
            className="text-neutral-400 hover:text-neutral-600"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <p className="text-sm text-neutral-600 mb-4">
          Get notified about important updates, payment reminders, and account activity.
        </p>

        <div className="flex space-x-2">
          <Button variant="outline" size="sm" onClick={handleDismiss} fullWidth>
            Not Now
          </Button>
          <Button variant="primary" size="sm" onClick={handleEnable} fullWidth>
            Enable
          </Button>
        </div>
      </Card>
    </div>
  );
};
