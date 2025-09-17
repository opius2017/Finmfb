import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  ShieldCheck,
  Settings,
  Bell,
  Key,
  Clock,
  ChevronRight,
  Smartphone,
  Mail,
  AlertOctagon,
  Shield,
  RefreshCw
} from 'lucide-react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import DeviceTrustManagement from './DeviceTrustManagement';

interface SecurityActivity {
  id: string;
  eventType: 'login' | 'password_change' | 'mfa_update' | 'device_added' | 'device_removed';
  timestamp: string;
  ipAddress: string;
  location: string;
  deviceInfo: string;
  status: 'success' | 'failed';
}

interface SecuritySettingsPageProps {
  user: {
    id: string;
    email: string;
    isMfaEnabled: boolean;
    lastLogin: string;
    emailNotificationsEnabled: boolean;
    loginNotificationsEnabled: boolean;
    unusualActivityNotificationsEnabled: boolean;
  };
  onUpdateSecuritySettings: (settings: any) => Promise<any>;
  onDisableMfa: () => Promise<any>;
  onGetSecurityActivity: () => Promise<SecurityActivity[]>;
}

const SecuritySettingsPage: React.FC<SecuritySettingsPageProps> = ({
  user,
  onUpdateSecuritySettings,
  onDisableMfa,
  onGetSecurityActivity
}) => {
  const [isLoading, setIsLoading] = useState(true);
  const [securityActivity, setSecurityActivity] = useState<SecurityActivity[]>([]);
  const [showDeviceManagement, setShowDeviceManagement] = useState(false);
  const [settings, setSettings] = useState({
    emailNotificationsEnabled: user.emailNotificationsEnabled,
    loginNotificationsEnabled: user.loginNotificationsEnabled,
    unusualActivityNotificationsEnabled: user.unusualActivityNotificationsEnabled
  });
  
  useEffect(() => {
    fetchSecurityActivity();
  }, []);
  
  const fetchSecurityActivity = async () => {
    setIsLoading(true);
    try {
      const activity = await onGetSecurityActivity();
      setSecurityActivity(activity);
    } catch (error) {
      console.error('Error fetching security activity:', error);
      toast.error('Failed to load security activity');
    } finally {
      setIsLoading(false);
    }
  };
  
  const handleSettingChange = async (setting: keyof typeof settings) => {
    const newSettings = {
      ...settings,
      [setting]: !settings[setting]
    };
    
    setSettings(newSettings);
    
    try {
      await onUpdateSecuritySettings(newSettings);
      toast.success('Security settings updated');
    } catch (error) {
      // Revert the setting if the update fails
      setSettings(settings);
      console.error('Error updating security settings:', error);
      toast.error('Failed to update security settings');
    }
  };
  
  const handleDisableMfa = async () => {
    if (!window.confirm('Are you sure you want to disable two-factor authentication? This will reduce the security of your account.')) {
      return;
    }
    
    try {
      await onDisableMfa();
      toast.success('Two-factor authentication has been disabled');
    } catch (error) {
      console.error('Error disabling MFA:', error);
      toast.error('Failed to disable two-factor authentication');
    }
  };
  
  const getActivityIcon = (eventType: string) => {
    switch (eventType) {
      case 'login':
        return <Key className="w-4 h-4 text-blue-500" />;
      case 'password_change':
        return <Settings className="w-4 h-4 text-amber-500" />;
      case 'mfa_update':
        return <Shield className="w-4 h-4 text-emerald-500" />;
      case 'device_added':
        return <Smartphone className="w-4 h-4 text-purple-500" />;
      case 'device_removed':
        return <AlertOctagon className="w-4 h-4 text-red-500" />;
      default:
        return <Clock className="w-4 h-4 text-gray-500" />;
    }
  };
  
  const getActivityDescription = (activity: SecurityActivity) => {
    switch (activity.eventType) {
      case 'login':
        return `Account login ${activity.status === 'failed' ? '(failed attempt)' : ''}`;
      case 'password_change':
        return 'Password changed';
      case 'mfa_update':
        return 'Two-factor authentication settings updated';
      case 'device_added':
        return 'New device added to trusted devices';
      case 'device_removed':
        return 'Device removed from trusted devices';
      default:
        return 'Unknown activity';
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
  
  // Mock security activity data for demonstration
  const mockSecurityActivity: SecurityActivity[] = [
    {
      id: '1',
      eventType: 'login',
      timestamp: new Date().toISOString(),
      ipAddress: '102.89.23.156',
      location: 'Lagos, Nigeria',
      deviceInfo: 'Chrome on Windows',
      status: 'success'
    },
    {
      id: '2',
      eventType: 'mfa_update',
      timestamp: new Date(Date.now() - 3600000).toISOString(), // 1 hour ago
      ipAddress: '102.89.23.156',
      location: 'Lagos, Nigeria',
      deviceInfo: 'Chrome on Windows',
      status: 'success'
    },
    {
      id: '3',
      eventType: 'login',
      timestamp: new Date(Date.now() - 86400000).toISOString(), // 1 day ago
      ipAddress: '197.210.53.8',
      location: 'Abuja, Nigeria',
      deviceInfo: 'Safari on iPhone',
      status: 'failed'
    },
    {
      id: '4',
      eventType: 'password_change',
      timestamp: new Date(Date.now() - 604800000).toISOString(), // 1 week ago
      ipAddress: '102.89.23.156',
      location: 'Lagos, Nigeria',
      deviceInfo: 'Chrome on Windows',
      status: 'success'
    }
  ];
  
  return (
    <div className="container mx-auto px-4 py-8 max-w-5xl">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="mb-8"
      >
        <h1 className="text-3xl font-bold text-gray-800 flex items-center">
          <ShieldCheck className="w-8 h-8 mr-3 text-emerald-600" />
          Security Settings
        </h1>
        <p className="text-gray-600 mt-2">
          Manage your account security settings and preferences
        </p>
      </motion.div>
      
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="bg-white rounded-xl shadow-lg p-6"
          >
            <h2 className="text-xl font-semibold text-gray-800 mb-4 flex items-center">
              <Key className="w-5 h-5 mr-2 text-emerald-600" />
              Account Security
            </h2>
            
            <div className="space-y-4">
              <div className="flex items-center justify-between p-4 border border-gray-100 rounded-lg hover:bg-gray-50 transition-colors">
                <div>
                  <h3 className="font-medium text-gray-800">Password</h3>
                  <p className="text-gray-600 text-sm">
                    Last changed {formatDate(mockSecurityActivity[3].timestamp)}
                  </p>
                </div>
                <Link
                  to="/change-password"
                  className="text-emerald-600 hover:text-emerald-700 font-medium flex items-center"
                >
                  Change
                  <ChevronRight className="w-4 h-4 ml-1" />
                </Link>
              </div>
              
              <div className="flex items-center justify-between p-4 border border-gray-100 rounded-lg hover:bg-gray-50 transition-colors">
                <div>
                  <h3 className="font-medium text-gray-800">Two-Factor Authentication</h3>
                  <p className="text-gray-600 text-sm">
                    {user.isMfaEnabled ? 'Enabled' : 'Disabled'} - Adds an extra layer of security to your account
                  </p>
                </div>
                {user.isMfaEnabled ? (
                  <div className="flex space-x-3">
                    <button
                      type="button"
                      onClick={handleDisableMfa}
                      className="text-red-600 hover:text-red-700 font-medium"
                    >
                      Disable
                    </button>
                    <Link
                      to="/mfa-setup"
                      className="text-emerald-600 hover:text-emerald-700 font-medium flex items-center"
                    >
                      Reconfigure
                      <ChevronRight className="w-4 h-4 ml-1" />
                    </Link>
                  </div>
                ) : (
                  <Link
                    to="/mfa-setup"
                    className="text-emerald-600 hover:text-emerald-700 font-medium flex items-center"
                  >
                    Enable
                    <ChevronRight className="w-4 h-4 ml-1" />
                  </Link>
                )}
              </div>
              
              <div className="flex items-center justify-between p-4 border border-gray-100 rounded-lg hover:bg-gray-50 transition-colors">
                <div>
                  <h3 className="font-medium text-gray-800">Trusted Devices</h3>
                  <p className="text-gray-600 text-sm">
                    Manage devices that can access your account
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => setShowDeviceManagement(!showDeviceManagement)}
                  className="text-emerald-600 hover:text-emerald-700 font-medium flex items-center"
                >
                  {showDeviceManagement ? 'Hide' : 'Manage'}
                  <ChevronRight className="w-4 h-4 ml-1" />
                </button>
              </div>
              
              {showDeviceManagement && (
                <motion.div
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  className="mt-4"
                >
                  <DeviceTrustManagement
                    onRevoke={(deviceId) => Promise.resolve(deviceId)}
                    onRevokeAll={() => Promise.resolve()}
                    onRefresh={() => Promise.resolve()}
                  />
                </motion.div>
              )}
            </div>
          </motion.div>
          
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="bg-white rounded-xl shadow-lg p-6"
          >
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-800 flex items-center">
                <Clock className="w-5 h-5 mr-2 text-emerald-600" />
                Recent Security Activity
              </h2>
              <button
                type="button"
                onClick={fetchSecurityActivity}
                className="text-emerald-600 hover:text-emerald-700 p-2 rounded-full hover:bg-emerald-50 transition-colors"
                disabled={isLoading}
              >
                <RefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
              </button>
            </div>
            
            <div className="space-y-3">
              {isLoading ? (
                <div className="flex items-center justify-center py-8">
                  <RefreshCw className="w-8 h-8 text-emerald-500 animate-spin" />
                  <span className="ml-3 text-gray-600">Loading activity...</span>
                </div>
              ) : mockSecurityActivity.length === 0 ? (
                <div className="text-center py-8 text-gray-500">
                  <p>No security activity found</p>
                </div>
              ) : (
                mockSecurityActivity.map((activity) => (
                  <div
                    key={activity.id}
                    className={`p-3 rounded-lg ${
                      activity.status === 'failed' ? 'bg-red-50' : 'bg-gray-50'
                    }`}
                  >
                    <div className="flex items-start">
                      <div className={`p-2 rounded-full mr-3 ${
                        activity.status === 'failed' ? 'bg-red-100' : 'bg-gray-100'
                      }`}>
                        {getActivityIcon(activity.eventType)}
                      </div>
                      <div className="flex-grow">
                        <div className="flex items-center justify-between">
                          <h3 className={`font-medium ${
                            activity.status === 'failed' ? 'text-red-800' : 'text-gray-800'
                          }`}>
                            {getActivityDescription(activity)}
                          </h3>
                          <span className="text-xs text-gray-500">
                            {formatDate(activity.timestamp)}
                          </span>
                        </div>
                        <p className="text-sm text-gray-600">
                          {activity.deviceInfo} • {activity.location}
                        </p>
                        <p className="text-xs text-gray-500">
                          IP: {activity.ipAddress}
                        </p>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
            
            <div className="mt-4 text-center">
              <Link
                to="/security-activity"
                className="text-emerald-600 hover:text-emerald-700 font-medium"
              >
                View all activity
              </Link>
            </div>
          </motion.div>
        </div>
        
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="lg:col-span-1"
        >
          <div className="bg-white rounded-xl shadow-lg p-6 sticky top-6">
            <h2 className="text-xl font-semibold text-gray-800 mb-4 flex items-center">
              <Bell className="w-5 h-5 mr-2 text-emerald-600" />
              Security Notifications
            </h2>
            
            <div className="space-y-4">
              <div className="flex items-center justify-between py-3 border-b border-gray-100">
                <div>
                  <h3 className="font-medium text-gray-800">Email Notifications</h3>
                  <p className="text-gray-600 text-sm">
                    Receive security alerts via email
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => handleSettingChange('emailNotificationsEnabled')}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    settings.emailNotificationsEnabled ? 'bg-emerald-600' : 'bg-gray-200'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      settings.emailNotificationsEnabled ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
              </div>
              
              <div className="flex items-center justify-between py-3 border-b border-gray-100">
                <div>
                  <h3 className="font-medium text-gray-800">Login Notifications</h3>
                  <p className="text-gray-600 text-sm">
                    Get notified when there's a new login to your account
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => handleSettingChange('loginNotificationsEnabled')}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    settings.loginNotificationsEnabled ? 'bg-emerald-600' : 'bg-gray-200'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      settings.loginNotificationsEnabled ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
              </div>
              
              <div className="flex items-center justify-between py-3">
                <div>
                  <h3 className="font-medium text-gray-800">Unusual Activity</h3>
                  <p className="text-gray-600 text-sm">
                    Get alerts about suspicious activity on your account
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => handleSettingChange('unusualActivityNotificationsEnabled')}
                  className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                    settings.unusualActivityNotificationsEnabled ? 'bg-emerald-600' : 'bg-gray-200'
                  }`}
                >
                  <span
                    className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                      settings.unusualActivityNotificationsEnabled ? 'translate-x-6' : 'translate-x-1'
                    }`}
                  />
                </button>
              </div>
            </div>
            
            <div className="mt-6 bg-blue-50 border border-blue-100 rounded-lg p-4">
              <div className="flex">
                <Mail className="w-5 h-5 text-blue-500 mr-2 flex-shrink-0" />
                <div>
                  <h3 className="font-medium text-blue-800 mb-1">Notification Email</h3>
                  <p className="text-blue-700 text-sm">{user.email}</p>
                  <Link
                    to="/profile"
                    className="text-blue-600 hover:text-blue-700 text-sm font-medium inline-block mt-1"
                  >
                    Update email address
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
};

export default SecuritySettingsPage;