import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Laptop, 
  Smartphone, 
  Trash2, 
  Shield, 
  AlertCircle,
  Calendar,
  MapPin,
  Search,
  RefreshCw
} from 'lucide-react';
import toast from 'react-hot-toast';

interface TrustedDevice {
  id: string;
  deviceName: string;
  deviceType: 'mobile' | 'desktop' | 'tablet' | 'other';
  browser: string;
  operatingSystem: string;
  lastUsed: string; // ISO date string
  location: string;
  ipAddress: string;
  isCurrent: boolean;
}

interface DeviceTrustManagementProps {
  onRevoke: (deviceId: string) => Promise<any>;
  onRevokeAll: () => Promise<any>;
  onRefresh: () => Promise<any>;
}

const DeviceTrustManagement: React.FC<DeviceTrustManagementProps> = ({
  onRevoke,
  onRevokeAll,
  onRefresh
}) => {
  const [devices, setDevices] = useState<TrustedDevice[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showConfirmation, setShowConfirmation] = useState<string | null>(null);
  
  // Mock data for demonstration - replace with actual API call
  useEffect(() => {
    // Simulate API fetch
    setTimeout(() => {
      setDevices([
        {
          id: '1',
          deviceName: 'Chrome on Windows',
          deviceType: 'desktop',
          browser: 'Chrome',
          operatingSystem: 'Windows 11',
          lastUsed: new Date().toISOString(),
          location: 'Lagos, Nigeria',
          ipAddress: '192.168.1.1',
          isCurrent: true
        },
        {
          id: '2',
          deviceName: 'Safari on iPhone',
          deviceType: 'mobile',
          browser: 'Safari',
          operatingSystem: 'iOS 16',
          lastUsed: new Date(Date.now() - 86400000).toISOString(), // 1 day ago
          location: 'Abuja, Nigeria',
          ipAddress: '192.168.1.2',
          isCurrent: false
        },
        {
          id: '3',
          deviceName: 'Firefox on MacBook',
          deviceType: 'desktop',
          browser: 'Firefox',
          operatingSystem: 'macOS',
          lastUsed: new Date(Date.now() - 172800000).toISOString(), // 2 days ago
          location: 'Lagos, Nigeria',
          ipAddress: '192.168.1.3',
          isCurrent: false
        }
      ]);
      setIsLoading(false);
    }, 1000);
  }, []);
  
  const handleRefresh = async () => {
    setIsLoading(true);
    try {
      await onRefresh();
      toast.success('Device list refreshed');
    } catch (error) {
      toast.error('Failed to refresh device list');
    } finally {
      setIsLoading(false);
    }
  };
  
  const handleRevoke = async (deviceId: string) => {
    try {
      await onRevoke(deviceId);
      setDevices(devices.filter(device => device.id !== deviceId));
      toast.success('Device removed from trusted devices');
    } catch (error) {
      toast.error('Failed to revoke device access');
    }
    setShowConfirmation(null);
  };
  
  const handleRevokeAll = async () => {
    try {
      await onRevokeAll();
      setDevices(devices.filter(device => device.isCurrent));
      toast.success('All other devices have been revoked');
    } catch (error) {
      toast.error('Failed to revoke all devices');
    }
    setShowConfirmation(null);
  };
  
  const getDeviceIcon = (deviceType: string) => {
    switch (deviceType) {
      case 'mobile':
        return <Smartphone className="w-5 h-5" />;
      case 'desktop':
      case 'tablet':
      default:
        return <Laptop className="w-5 h-5" />;
    }
  };
  
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  };
  
  const filteredDevices = devices.filter(device => 
    device.deviceName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    device.browser.toLowerCase().includes(searchTerm.toLowerCase()) ||
    device.operatingSystem.toLowerCase().includes(searchTerm.toLowerCase()) ||
    device.location.toLowerCase().includes(searchTerm.toLowerCase())
  );
  
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      className="bg-white rounded-xl shadow-lg p-6"
    >
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-semibold text-gray-800">Trusted Devices</h2>
          <p className="text-gray-600 mt-1">
            Manage devices that have access to your account
          </p>
        </div>
        <button
          type="button"
          onClick={handleRefresh}
          className="text-emerald-600 hover:text-emerald-700 p-2 rounded-full hover:bg-emerald-50 transition-colors"
          disabled={isLoading}
        >
          <RefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
        </button>
      </div>
      
      <div className="mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            placeholder="Search devices..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
          />
        </div>
      </div>
      
      <div className="space-y-4 mb-6">
        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <RefreshCw className="w-8 h-8 text-emerald-500 animate-spin" />
            <span className="ml-3 text-gray-600">Loading devices...</span>
          </div>
        ) : filteredDevices.length === 0 ? (
          <div className="text-center py-8 text-gray-500">
            <AlertCircle className="w-12 h-12 mx-auto text-gray-400 mb-3" />
            <p>No devices found matching your search</p>
          </div>
        ) : (
          filteredDevices.map((device) => (
            <div
              key={device.id}
              className={`border ${device.isCurrent ? 'border-emerald-200 bg-emerald-50' : 'border-gray-200'} rounded-lg p-4`}
            >
              <div className="flex items-start justify-between">
                <div className="flex items-start">
                  <div className={`rounded-full p-2 mr-3 ${device.isCurrent ? 'bg-emerald-100 text-emerald-600' : 'bg-gray-100 text-gray-600'}`}>
                    {getDeviceIcon(device.deviceType)}
                  </div>
                  <div>
                    <div className="flex items-center">
                      <h3 className="font-medium text-gray-800">{device.deviceName}</h3>
                      {device.isCurrent && (
                        <span className="ml-2 text-xs bg-emerald-100 text-emerald-800 px-2 py-0.5 rounded-full font-medium">
                          Current
                        </span>
                      )}
                    </div>
                    <p className="text-sm text-gray-600">{device.operatingSystem}</p>
                    <div className="flex flex-wrap mt-2 text-xs text-gray-500 gap-2">
                      <div className="flex items-center">
                        <Calendar className="w-3 h-3 mr-1" />
                        <span>{formatDate(device.lastUsed)}</span>
                      </div>
                      <div className="flex items-center">
                        <MapPin className="w-3 h-3 mr-1" />
                        <span>{device.location}</span>
                      </div>
                    </div>
                  </div>
                </div>
                {!device.isCurrent && (
                  <button
                    type="button"
                    onClick={() => setShowConfirmation(device.id)}
                    className="text-red-500 hover:text-red-700 p-1 rounded hover:bg-red-50 transition-colors"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                )}
              </div>
              
              {showConfirmation === device.id && (
                <div className="mt-3 p-3 bg-red-50 rounded-lg border border-red-100">
                  <p className="text-sm text-red-800 mb-2">
                    Are you sure you want to remove this device?
                  </p>
                  <div className="flex space-x-2">
                    <button
                      type="button"
                      onClick={() => handleRevoke(device.id)}
                      className="px-3 py-1 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 transition-colors"
                    >
                      Yes, Remove
                    </button>
                    <button
                      type="button"
                      onClick={() => setShowConfirmation(null)}
                      className="px-3 py-1 bg-gray-200 text-gray-800 text-sm rounded-md hover:bg-gray-300 transition-colors"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}
            </div>
          ))
        )}
      </div>
      
      <div className="border-t border-gray-200 pt-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center text-gray-600">
            <Shield className="w-5 h-5 mr-2 text-emerald-600" />
            <span className="text-sm">Removing devices will force re-authentication</span>
          </div>
          
          <button
            type="button"
            onClick={() => setShowConfirmation('all')}
            className="text-sm text-red-600 hover:text-red-700 font-medium"
            disabled={devices.filter(d => !d.isCurrent).length === 0}
          >
            Revoke All Other Devices
          </button>
        </div>
        
        {showConfirmation === 'all' && (
          <div className="mt-3 p-4 bg-red-50 rounded-lg border border-red-100">
            <div className="flex items-start">
              <AlertCircle className="w-5 h-5 text-red-600 mr-2 mt-0.5" />
              <div>
                <p className="text-sm text-red-800 font-medium">
                  Revoke access from all other devices?
                </p>
                <p className="text-xs text-red-700 mt-1">
                  This will sign out all devices except your current one. They will need to sign in again and may require MFA verification.
                </p>
                <div className="flex space-x-2 mt-3">
                  <button
                    type="button"
                    onClick={handleRevokeAll}
                    className="px-3 py-1.5 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 transition-colors"
                  >
                    Yes, Revoke All
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowConfirmation(null)}
                    className="px-3 py-1.5 bg-gray-200 text-gray-800 text-sm rounded-md hover:bg-gray-300 transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </motion.div>
  );
};

export default DeviceTrustManagement;