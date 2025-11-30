// Jest setup file for global test configuration

// Set test environment variables
process.env.NODE_ENV = 'test';
process.env.JWT_SECRET = 'test-jwt-secret-key-for-testing-only-32-chars';
process.env.JWT_REFRESH_SECRET = 'test-refresh-secret-key-for-testing-only-32';
process.env.ENCRYPTION_KEY = 'test-encryption-key-32-chars!!';
process.env.DATABASE_URL = 'postgresql://test:test@localhost:5432/fintech_test?schema=public';
process.env.REDIS_HOST = 'localhost';
process.env.REDIS_PORT = '6379';
process.env.LOG_LEVEL = 'error';

// Increase test timeout for integration tests
jest.setTimeout(10000);

// Mock console methods to reduce noise in tests
global.console = {
  ...console,
  log: jest.fn(),
  debug: jest.fn(),
  info: jest.fn(),
  warn: jest.fn(),
  error: jest.fn(),
};

// Global test utilities
export const mockRequest = (overrides = {}): any => ({
  body: {},
  params: {},
  query: {},
  headers: {},
  correlationId: 'test-correlation-id',
  ...overrides,
});

export const mockResponse = (): any => {
  const res: any = {};
  res.status = jest.fn().mockReturnValue(res);
  res.json = jest.fn().mockReturnValue(res);
  res.send = jest.fn().mockReturnValue(res);
  res.setHeader = jest.fn().mockReturnValue(res);
  return res;
};

export const mockNext = (): any => jest.fn();
