import React, { useState, useEffect } from 'react';
import { RefreshCw } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { serviceWorkerService } from '../services/serviceWorkerService';

export const UpdateNotification: React.FC = () => {
  const [showUpdate, setShowUpdate] = useState(false);

  useEffect(() => {
    const handleUpdate = () => {
      setShowUpdate(true);
    };

    window.addEventListener('sw-update-available', handleUpdate);

    return () => {
      window.removeEventListener('sw-update-available', handleUpdate);
    };
  }, []);

  const handleUpdate = async () => {
    await serviceWorkerService.skipWaiting();
    window.location.reload();
  };

  if (!showUpdate) return null;

  return (
    <div className="fixed top-4 left-4 right-4 md:left-auto md:right-4 md:w-96 z-50">
      <Card className="p-4 shadow-lg bg-primary-50 border-primary-200">
        <div className="flex items-start space-x-3">
          <div className="w-10 h-10 bg-primary-100 rounded-lg flex items-center justify-center flex-shrink-0">
            <RefreshCw className="w-5 h-5 text-primary-600" />
          </div>
          <div className="flex-1">
            <h3 className="font-semibold text-primary-900 mb-1">Update Available</h3>
            <p className="text-sm text-primary-700 mb-3">
              A new version of Soar-Fin+ is available. Update now to get the latest features.
            </p>
            <Button variant="primary" size="sm" onClick={handleUpdate} fullWidth>
              Update Now
            </Button>
          </div>
        </div>
      </Card>
    </div>
  );
};
