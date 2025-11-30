// Role-Based Access Control Types
export interface Role {
  id: string;
  name: string;
  description: string;
  permissions: Permission[];
  isSystem: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface Permission {
  id: string;
  resource: string;
  action: Action;
  scope: Scope;
  conditions?: Condition[];
}

export type Action = 'create' | 'read' | 'update' | 'delete' | 'approve' | 'export' | 'all';

export type Scope = 'own' | 'department' | 'branch' | 'organization' | 'all';

export interface Condition {
  field: string;
  operator: 'equals' | 'not_equals' | 'in' | 'not_in' | 'greater_than' | 'less_than';
  value: any;
}

export interface UserRole {
  userId: string;
  roleId: string;
  roleName: string;
  assignedBy: string;
  assignedAt: Date;
  expiresAt?: Date;
}

export interface FieldLevelAccess {
  resource: string;
  field: string;
  access: 'read' | 'write' | 'none';
}

export interface AccessCheck {
  resource: string;
  action: Action;
  context?: Record<string, any>;
}

export interface AccessResult {
  allowed: boolean;
  reason?: string;
  fieldRestrictions?: FieldLevelAccess[];
}
