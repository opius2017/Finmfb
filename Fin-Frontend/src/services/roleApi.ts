import { api } from './api';

interface Role {
  id: string;
  roleName: string;
  displayName: string;
  description?: string;
  level: string;
  defaultModule?: string;
  defaultDashboard?: string;
  permissionCount: number;
  isSystemRole: boolean;
  createdAt: string;
}

interface Permission {
  id: string;
  permissionName: string;
  displayName: string;
  module: string;
  resource: string;
  action: string;
  isGranted: boolean;
  conditions?: string;
}

interface UserPermissions {
  permissions: Permission[];
  defaultModule: string;
  defaultDashboard: string;
  roles: string[];
}

interface RolesResponse {
  success: boolean;
  data: Role[];
}

interface PermissionsResponse {
  success: boolean;
  data: Permission[];
}

interface UserPermissionsResponse {
  success: boolean;
  data: UserPermissions;
}

export const roleApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getRoles: builder.query<RolesResponse, void>({
      query: () => '/role',
      providesTags: ['Role'],
    }),
    getRolePermissions: builder.query<PermissionsResponse, string>({
      query: (roleId) => `/role/${roleId}/permissions`,
      providesTags: ['Permission'],
    }),
    getUserPermissions: builder.query<UserPermissionsResponse, void>({
      query: () => '/role/user-permissions',
      providesTags: ['Permission'],
    }),
  }),
  overrideExisting: false,
});

export const { 
  useGetRolesQuery, 
  useGetRolePermissionsQuery, 
  useGetUserPermissionsQuery 
} = roleApi;