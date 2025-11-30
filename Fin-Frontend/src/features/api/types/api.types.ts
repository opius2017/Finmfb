// REST API Types
export interface APIKey {
  id: string;
  name: string;
  key: string;
  secret?: string;
  status: APIKeyStatus;
  permissions: APIPermission[];
  rateLimit: RateLimit;
  ipWhitelist: string[];
  expiresAt?: Date;
  lastUsedAt?: Date;
  createdBy: string;
  createdAt: Date;
}

export type APIKeyStatus = 'active' | 'inactive' | 'revoked' | 'expired';

export interface APIPermission {
  resource: string;
  actions: APIAction[];
  scope?: string;
}

export type APIAction = 'read' | 'write' | 'delete' | 'execute';

export interface RateLimit {
  requestsPerMinute: number;
  requestsPerHour: number;
  requestsPerDay: number;
  burstLimit: number;
}

export interface APIVersion {
  version: string;
  status: VersionStatus;
  releaseDate: Date;
  deprecationDate?: Date;
  endOfLifeDate?: Date;
  changelog: string[];
  endpoints: APIEndpoint[];
}

export type VersionStatus = 'current' | 'supported' | 'deprecated' | 'retired';

export interface APIEndpoint {
  path: string;
  method: HTTPMethod;
  description: string;
  parameters: APIParameter[];
  requestBody?: RequestBodySchema;
  responses: APIResponse[];
  authentication: boolean;
  rateLimit?: RateLimit;
  deprecated?: boolean;
}

export type HTTPMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface APIParameter {
  name: string;
  in: 'path' | 'query' | 'header';
  type: string;
  required: boolean;
  description: string;
  example?: any;
}

export interface RequestBodySchema {
  contentType: string;
  schema: any;
  example?: any;
}

export interface APIResponse {
  statusCode: number;
  description: string;
  schema?: any;
  example?: any;
}

export interface APIUsageStats {
  apiKeyId: string;
  apiKeyName: string;
  period: DateRange;
  totalRequests: number;
  successfulRequests: number;
  failedRequests: number;
  averageResponseTime: number;
  topEndpoints: EndpointUsage[];
  errorBreakdown: ErrorStats[];
}

export interface DateRange {
  from: Date;
  to: Date;
}

export interface EndpointUsage {
  endpoint: string;
  method: string;
  count: number;
  averageResponseTime: number;
  errorRate: number;
}

export interface ErrorStats {
  statusCode: number;
  count: number;
  percentage: number;
}

export interface APILog {
  id: string;
  apiKeyId: string;
  endpoint: string;
  method: string;
  statusCode: number;
  responseTime: number;
  requestSize: number;
  responseSize: number;
  ipAddress: string;
  userAgent: string;
  timestamp: Date;
  error?: string;
}

export interface OpenAPISpec {
  openapi: string;
  info: APIInfo;
  servers: APIServer[];
  paths: Record<string, any>;
  components: APIComponents;
  security: SecurityRequirement[];
}

export interface APIInfo {
  title: string;
  version: string;
  description: string;
  contact?: ContactInfo;
  license?: LicenseInfo;
}

export interface ContactInfo {
  name: string;
  email: string;
  url?: string;
}

export interface LicenseInfo {
  name: string;
  url?: string;
}

export interface APIServer {
  url: string;
  description: string;
  variables?: Record<string, ServerVariable>;
}

export interface ServerVariable {
  default: string;
  enum?: string[];
  description?: string;
}

export interface APIComponents {
  schemas: Record<string, any>;
  securitySchemes: Record<string, SecurityScheme>;
  responses?: Record<string, any>;
  parameters?: Record<string, any>;
}

export interface SecurityScheme {
  type: 'apiKey' | 'http' | 'oauth2' | 'openIdConnect';
  name?: string;
  in?: 'query' | 'header' | 'cookie';
  scheme?: string;
  bearerFormat?: string;
}

export interface SecurityRequirement {
  [key: string]: string[];
}

export interface APISandbox {
  id: string;
  name: string;
  description: string;
  baseUrl: string;
  apiKeys: string[];
  testData: boolean;
  resetSchedule?: string;
  createdAt: Date;
}

export interface APITestRequest {
  endpoint: string;
  method: HTTPMethod;
  headers: Record<string, string>;
  queryParams?: Record<string, string>;
  body?: any;
}

export interface APITestResponse {
  statusCode: number;
  headers: Record<string, string>;
  body: any;
  responseTime: number;
  timestamp: Date;
}
