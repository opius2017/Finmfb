// Common types and interfaces

export interface PaginationParams {
  page: number;
  limit: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
    hasNext: boolean;
    hasPrev: boolean;
  };
}

export interface RequestContext {
  correlationId: string;
  userId?: string;
  userRole?: string;
  ipAddress: string;
  timestamp: Date;
}

export interface Filter<T> {
  where?: Partial<T>;
  orderBy?: {
    [K in keyof T]?: 'asc' | 'desc';
  };
  skip?: number;
  take?: number;
}

export interface CreateInput<T> {
  data: Partial<T>;
}

export interface UpdateInput<T> {
  data: Partial<T>;
}

export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  message?: string;
  timestamp: Date;
  correlationId?: string;
}

export interface ErrorResponse {
  success: false;
  error: {
    code: string;
    message: string;
    details?: unknown;
  };
  timestamp: Date;
  correlationId?: string;
}

// Authentication types
export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface JWTPayload {
  userId: string;
  email: string;
  roleId: string;
  iat?: number;
  exp?: number;
}

export interface RefreshTokenPayload {
  userId: string;
  sessionId: string;
  iat?: number;
  exp?: number;
}

// Permission types
export interface Permission {
  resource: string;
  action: 'create' | 'read' | 'update' | 'delete' | 'approve';
  conditions?: Record<string, unknown>;
}

// Audit types
export interface AuditLogData {
  userId?: string;
  action: string;
  entityType: string;
  entityId?: string;
  changes?: Record<string, unknown>;
  ipAddress?: string;
  userAgent?: string;
  correlationId?: string;
}
