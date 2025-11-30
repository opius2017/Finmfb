import { Role, Permission, AccessCheck, AccessResult, UserRole } from '../types/rbac.types';

export class RBACService {
  private apiEndpoint = '/api/security/rbac';
  private cachedPermissions: Permission[] = [];

  async getRoles(): Promise<Role[]> {
    const response = await fetch(`${this.apiEndpoint}/roles`);
    if (!response.ok) throw new Error('Failed to fetch roles');
    return response.json();
  }

  async createRole(role: Partial<Role>): Promise<Role> {
    const response = await fetch(`${this.apiEndpoint}/roles`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(role),
    });
    if (!response.ok) throw new Error('Failed to create role');
    return response.json();
  }

  async updateRole(roleId: string, role: Partial<Role>): Promise<Role> {
    const response = await fetch(`${this.apiEndpoint}/roles/${roleId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(role),
    });
    if (!response.ok) throw new Error('Failed to update role');
    return response.json();
  }

  async deleteRole(roleId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/roles/${roleId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete role');
  }

  async assignRole(userId: string, roleId: string): Promise<UserRole> {
    const response = await fetch(`${this.apiEndpoint}/users/${userId}/roles`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ roleId }),
    });
    if (!response.ok) throw new Error('Failed to assign role');
    return response.json();
  }

  async removeRole(userId: string, roleId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/users/${userId}/roles/${roleId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to remove role');
  }

  async checkAccess(check: AccessCheck): Promise<AccessResult> {
    const response = await fetch(`${this.apiEndpoint}/check-access`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(check),
    });
    if (!response.ok) throw new Error('Failed to check access');
    return response.json();
  }

  async getUserPermissions(userId: string): Promise<Permission[]> {
    const response = await fetch(`${this.apiEndpoint}/users/${userId}/permissions`);
    if (!response.ok) throw new Error('Failed to fetch permissions');
    const permissions = await response.json();
    this.cachedPermissions = permissions;
    return permissions;
  }

  hasPermission(resource: string, action: string): boolean {
    return this.cachedPermissions.some(
      p => (p.resource === resource || p.resource === '*') && 
           (p.action === action || p.action === 'all')
    );
  }

  canAccess(resource: string, action: string, scope?: string): boolean {
    return this.cachedPermissions.some(
      p => (p.resource === resource || p.resource === '*') && 
           (p.action === action || p.action === 'all') &&
           (!scope || p.scope === scope || p.scope === 'all')
    );
  }
}

export const rbacService = new RBACService();
