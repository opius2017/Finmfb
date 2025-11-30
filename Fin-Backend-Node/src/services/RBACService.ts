import { Permission as PermissionModel } from '@prisma/client';
import { RepositoryFactory } from '@repositories/index';
import { Permission } from '@/types';

export class RBACService {
  private userRepository = RepositoryFactory.getUserRepository();

  /**
   * Check if user has permission
   */
  async checkPermission(userId: string, permission: Permission): Promise<boolean> {
    const user = await this.userRepository.findByIdWithPermissions(userId);
    if (!user || !user.role) {
      return false;
    }

    // Check if user's role has the required permission
    const hasPermission = user.role.permissions.some(
      (p: PermissionModel) =>
        p.resource === permission.resource && p.action === permission.action
    );

    return hasPermission;
  }

  /**
   * Check if user has any of the permissions
   */
  async checkAnyPermission(
    userId: string,
    permissions: Permission[]
  ): Promise<boolean> {
    for (const permission of permissions) {
      const hasPermission = await this.checkPermission(userId, permission);
      if (hasPermission) {
        return true;
      }
    }
    return false;
  }

  /**
   * Check if user has all permissions
   */
  async checkAllPermissions(
    userId: string,
    permissions: Permission[]
  ): Promise<boolean> {
    for (const permission of permissions) {
      const hasPermission = await this.checkPermission(userId, permission);
      if (!hasPermission) {
        return false;
      }
    }
    return true;
  }

  /**
   * Get user roles
   */
  async getUserRoles(userId: string): Promise<string[]> {
    const user = await this.userRepository.findByIdWithPermissions(userId);
    if (!user || !user.role) {
      return [];
    }
    return [user.role.name];
  }

  /**
   * Get user permissions
   */
  async getUserPermissions(userId: string): Promise<PermissionModel[]> {
    const user = await this.userRepository.findByIdWithPermissions(userId);
    if (!user || !user.role) {
      return [];
    }
    return user.role.permissions;
  }

  /**
   * Check if user has role
   */
  async hasRole(userId: string, roleName: string): Promise<boolean> {
    const roles = await this.getUserRoles(userId);
    return roles.includes(roleName);
  }

  /**
   * Check if user has any of the roles
   */
  async hasAnyRole(userId: string, roleNames: string[]): Promise<boolean> {
    const roles = await this.getUserRoles(userId);
    return roleNames.some((role) => roles.includes(role));
  }
}

export default RBACService;
