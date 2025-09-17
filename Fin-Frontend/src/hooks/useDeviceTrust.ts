import { useEffect, useState, useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../store/store';
import { 
  useGetTrustedDevicesQuery, 
  useAddTrustedDeviceMutation,
  useRevokeTrustedDeviceMutation, 
  useRevokeAllTrustedDevicesExceptCurrentMutation
} from '../services/authApi';
import { setTrustedDevices, removeTrustedDevice } from '../store/slices/authSlice';
import { toast } from 'react-hot-toast';

interface DeviceInfo {
  name: string;
  type: string;
  browser: string;
  operatingSystem: string;
  ipAddress?: string;
}

export function useDeviceTrust() {
  const [isLoading, setIsLoading] = useState(false);
  const [currentDeviceId, setCurrentDeviceId] = useState<string | null>(null);
  
  const dispatch = useDispatch();
  const { trustedDevices } = useSelector((state: RootState) => state.auth);
  
  const { 
    data: devicesData,
    isLoading: isLoadingDevices, 
    refetch 
  } = useGetTrustedDevicesQuery();
  
  const [addTrustedDeviceApi] = useAddTrustedDeviceMutation();
  const [revokeTrustedDeviceApi] = useRevokeTrustedDeviceMutation();
  const [revokeAllDevicesExceptCurrentApi] = useRevokeAllTrustedDevicesExceptCurrentMutation();
  
  // Load trusted devices on mount and update
  useEffect(() => {
    if (devicesData?.success && devicesData.data) {
      dispatch(setTrustedDevices(devicesData.data));
      
      // Find current device
      const current = devicesData.data.find(device => device.isCurrent);
      if (current) {
        setCurrentDeviceId(current.id);
      }
    }
  }, [devicesData, dispatch]);
  
  // Get information about the current device
  const collectDeviceInfo = useCallback((): DeviceInfo => {
    const userAgent = navigator.userAgent;
    let deviceType = 'desktop';
    let browser = 'Unknown';
    let operatingSystem = 'Unknown';
    
    // Detect device type
    if (/mobile/i.test(userAgent)) {
      deviceType = 'mobile';
    } else if (/tablet/i.test(userAgent)) {
      deviceType = 'tablet';
    }
    
    // Detect browser
    if (/chrome/i.test(userAgent)) {
      browser = 'Chrome';
    } else if (/firefox/i.test(userAgent)) {
      browser = 'Firefox';
    } else if (/safari/i.test(userAgent) && !/chrome/i.test(userAgent)) {
      browser = 'Safari';
    } else if (/edge/i.test(userAgent)) {
      browser = 'Edge';
    } else if (/opera/i.test(userAgent) || /opr/i.test(userAgent)) {
      browser = 'Opera';
    }
    
    // Detect OS
    if (/windows/i.test(userAgent)) {
      operatingSystem = 'Windows';
    } else if (/macintosh/i.test(userAgent)) {
      operatingSystem = 'macOS';
    } else if (/linux/i.test(userAgent)) {
      operatingSystem = 'Linux';
    } else if (/android/i.test(userAgent)) {
      operatingSystem = 'Android';
    } else if (/iphone|ipad|ipod/i.test(userAgent)) {
      operatingSystem = 'iOS';
    }
    
    const deviceName = `${browser} on ${operatingSystem}`;
    
    return {
      name: deviceName,
      type: deviceType,
      browser,
      operatingSystem
    };
  }, []);
  
  // Add the current device as trusted
  const trustCurrentDevice = useCallback(async (deviceName?: string): Promise<boolean> => {
    try {
      setIsLoading(true);
      
      const deviceInfo = collectDeviceInfo();
      
      const response = await addTrustedDeviceApi({
        name: deviceName || deviceInfo.name,
        type: deviceInfo.type,
        browser: deviceInfo.browser,
        operatingSystem: deviceInfo.operatingSystem
      }).unwrap();
      
      if (response.success && response.data) {
        // Store the new device ID
        setCurrentDeviceId(response.data);
        
        // Refresh the device list
        await refetch();
        
        toast.success('Device added to trusted devices');
        return true;
      } else {
        toast.error(response.message || 'Failed to add device to trusted devices');
        return false;
      }
    } catch (error) {
      console.error('Error adding trusted device:', error);
      toast.error('An error occurred while adding trusted device');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [addTrustedDeviceApi, collectDeviceInfo, refetch]);
  
  // Remove a device from trusted devices
  const removeDevice = useCallback(async (deviceId: string): Promise<boolean> => {
    try {
      setIsLoading(true);
      
      const response = await revokeTrustedDeviceApi(deviceId).unwrap();
      
      if (response.success && response.data) {
        // Update local state
        dispatch(removeTrustedDevice(deviceId));
        
        toast.success('Device removed from trusted devices');
        return true;
      } else {
        toast.error(response.message || 'Failed to remove device from trusted devices');
        return false;
      }
    } catch (error) {
      console.error('Error revoking trusted device:', error);
      toast.error('An error occurred while removing trusted device');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [revokeTrustedDeviceApi, dispatch]);
  
  // Revoke all devices except the current one
  const removeAllOtherDevices = useCallback(async (): Promise<boolean> => {
    if (!currentDeviceId) {
      toast.error('Current device not identified');
      return false;
    }
    
    try {
      setIsLoading(true);
      
      const response = await revokeAllDevicesExceptCurrentApi({
        currentDeviceId
      }).unwrap();
      
      if (response.success && response.data) {
        // Refresh the device list
        await refetch();
        
        toast.success('All other devices removed from trusted devices');
        return true;
      } else {
        toast.error(response.message || 'Failed to remove other devices');
        return false;
      }
    } catch (error) {
      console.error('Error revoking all trusted devices:', error);
      toast.error('An error occurred while removing other devices');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [currentDeviceId, revokeAllDevicesExceptCurrentApi, refetch]);
  
  // Format a device name for display
  const formatDeviceName = useCallback((device: any): string => {
    return `${device.deviceName || 'Unknown'} (${device.operatingSystem || 'Unknown OS'} - ${device.browser || 'Unknown Browser'})`;
  }, []);
  
  return {
    trustedDevices,
    currentDeviceId,
    isLoading: isLoading || isLoadingDevices,
    collectDeviceInfo,
    trustCurrentDevice,
    removeDevice,
    removeAllOtherDevices,
    formatDeviceName,
    refreshDevices: refetch
  };
}

export default useDeviceTrust;