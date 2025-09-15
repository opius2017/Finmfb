import React from 'react';
import { useGetUserPermissionsQuery } from '../../services/roleApi';

interface RoleBasedComponentProps {
  allowedRoles: string[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

const RoleBasedComponent: React.FC<RoleBasedComponentProps> = ({ 
  allowedRoles, 
  children, 
  fallback = null 
}) => {
  const { data: userPermissions, isLoading } = useGetUserPermissionsQuery();

  if (isLoading) {
    return <div className="animate-pulse bg-gray-200 h-8 rounded"></div>;
  }

  const hasRole = userPermissions?.data?.roles?.some(
    role => allowedRoles.includes(role)
  ) || false;

  if (!hasRole) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};

export default RoleBasedComponent;