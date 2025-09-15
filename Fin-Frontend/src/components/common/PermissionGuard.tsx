import React from 'react';
import { useGetUserPermissionsQuery } from '../../services/roleApi';

interface PermissionGuardProps {
  permission: string;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

const PermissionGuard: React.FC<PermissionGuardProps> = ({ 
  permission, 
  children, 
  fallback = null 
}) => {
  const { data: userPermissions, isLoading } = useGetUserPermissionsQuery();

  if (isLoading) {
    return <div className="animate-pulse bg-gray-200 h-8 rounded"></div>;
  }

  const hasPermission = userPermissions?.data?.permissions?.some(
    p => p.permissionName === permission && p.isGranted
  ) || false;

  if (!hasPermission) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};

export default PermissionGuard;