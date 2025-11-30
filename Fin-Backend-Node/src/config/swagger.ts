import swaggerJsdoc from 'swagger-jsdoc';
import { config } from './index';

const swaggerDefinition = {
  openapi: '3.0.0',
  info: {
    title: 'FinTech API',
    version: '1.0.0',
    description: 'Enterprise Backend Infrastructure for MSME FinTech Solution',
    contact: {
      name: 'API Support',
      email: 'support@fintech.com',
    },
    license: {
      name: 'MIT',
      url: 'https://opensource.org/licenses/MIT',
    },
  },
  servers: [
    {
      url: `http://localhost:${config.PORT}/api/${config.API_VERSION}`,
      description: 'Development server',
    },
    {
      url: `https://api.fintech.com/api/${config.API_VERSION}`,
      description: 'Production server',
    },
  ],
  components: {
    securitySchemes: {
      bearerAuth: {
        type: 'http',
        scheme: 'bearer',
        bearerFormat: 'JWT',
        description: 'Enter your JWT token',
      },
    },
    schemas: {
      Error: {
        type: 'object',
        properties: {
          success: {
            type: 'boolean',
            example: false,
          },
          error: {
            type: 'object',
            properties: {
              code: {
                type: 'string',
                example: 'BAD_REQUEST',
              },
              message: {
                type: 'string',
                example: 'Invalid request',
              },
              details: {
                type: 'object',
              },
            },
          },
          timestamp: {
            type: 'string',
            format: 'date-time',
          },
          correlationId: {
            type: 'string',
            format: 'uuid',
          },
        },
      },
      Success: {
        type: 'object',
        properties: {
          success: {
            type: 'boolean',
            example: true,
          },
          data: {
            type: 'object',
          },
          message: {
            type: 'string',
          },
          timestamp: {
            type: 'string',
            format: 'date-time',
          },
          correlationId: {
            type: 'string',
            format: 'uuid',
          },
        },
      },
      PaginatedResponse: {
        type: 'object',
        properties: {
          success: {
            type: 'boolean',
            example: true,
          },
          data: {
            type: 'array',
            items: {
              type: 'object',
            },
          },
          pagination: {
            type: 'object',
            properties: {
              page: {
                type: 'integer',
                example: 1,
              },
              limit: {
                type: 'integer',
                example: 10,
              },
              total: {
                type: 'integer',
                example: 100,
              },
              totalPages: {
                type: 'integer',
                example: 10,
              },
              hasNext: {
                type: 'boolean',
                example: true,
              },
              hasPrev: {
                type: 'boolean',
                example: false,
              },
            },
          },
          timestamp: {
            type: 'string',
            format: 'date-time',
          },
        },
      },
      User: {
        type: 'object',
        properties: {
          id: {
            type: 'string',
            format: 'uuid',
          },
          email: {
            type: 'string',
            format: 'email',
          },
          firstName: {
            type: 'string',
          },
          lastName: {
            type: 'string',
          },
          roleId: {
            type: 'string',
            format: 'uuid',
          },
          mfaEnabled: {
            type: 'boolean',
          },
          isActive: {
            type: 'boolean',
          },
          createdAt: {
            type: 'string',
            format: 'date-time',
          },
          updatedAt: {
            type: 'string',
            format: 'date-time',
          },
        },
      },
      AuthTokens: {
        type: 'object',
        properties: {
          accessToken: {
            type: 'string',
            description: 'JWT access token',
          },
          refreshToken: {
            type: 'string',
            description: 'JWT refresh token',
          },
          expiresIn: {
            type: 'integer',
            description: 'Token expiration time in seconds',
            example: 900,
          },
        },
      },
    },
    responses: {
      UnauthorizedError: {
        description: 'Authentication required',
        content: {
          'application/json': {
            schema: {
              $ref: '#/components/schemas/Error',
            },
            example: {
              success: false,
              error: {
                code: 'UNAUTHORIZED',
                message: 'Authentication required',
              },
              timestamp: '2024-01-01T00:00:00.000Z',
            },
          },
        },
      },
      ForbiddenError: {
        description: 'Insufficient permissions',
        content: {
          'application/json': {
            schema: {
              $ref: '#/components/schemas/Error',
            },
            example: {
              success: false,
              error: {
                code: 'FORBIDDEN',
                message: 'Insufficient permissions',
              },
              timestamp: '2024-01-01T00:00:00.000Z',
            },
          },
        },
      },
      NotFoundError: {
        description: 'Resource not found',
        content: {
          'application/json': {
            schema: {
              $ref: '#/components/schemas/Error',
            },
            example: {
              success: false,
              error: {
                code: 'NOT_FOUND',
                message: 'Resource not found',
              },
              timestamp: '2024-01-01T00:00:00.000Z',
            },
          },
        },
      },
      ValidationError: {
        description: 'Validation error',
        content: {
          'application/json': {
            schema: {
              $ref: '#/components/schemas/Error',
            },
            example: {
              success: false,
              error: {
                code: 'VALIDATION_ERROR',
                message: 'Validation failed',
                details: [
                  {
                    path: 'email',
                    message: 'Invalid email format',
                  },
                ],
              },
              timestamp: '2024-01-01T00:00:00.000Z',
            },
          },
        },
      },
      RateLimitError: {
        description: 'Rate limit exceeded',
        content: {
          'application/json': {
            schema: {
              $ref: '#/components/schemas/Error',
            },
            example: {
              success: false,
              error: {
                code: 'RATE_LIMIT_EXCEEDED',
                message: 'Too many requests, please try again later',
                retryAfter: 900,
              },
              timestamp: '2024-01-01T00:00:00.000Z',
            },
          },
        },
      },
    },
  },
  tags: [
    {
      name: 'Authentication',
      description: 'User authentication and session management',
    },
    {
      name: 'Password',
      description: 'Password management and reset',
    },
    {
      name: 'Members',
      description: 'Member management operations',
    },
    {
      name: 'Accounts',
      description: 'Account management operations',
    },
    {
      name: 'Transactions',
      description: 'Transaction processing',
    },
    {
      name: 'Loans',
      description: 'Loan management',
    },
    {
      name: 'Budgets',
      description: 'Budget planning and tracking',
    },
    {
      name: 'Documents',
      description: 'Document management',
    },
    {
      name: 'Reports',
      description: 'Reporting and analytics',
    },
  ],
};

const options = {
  swaggerDefinition,
  apis: ['./src/routes/*.ts', './src/controllers/*.ts'],
};

export const swaggerSpec = swaggerJsdoc(options);

export default swaggerSpec;
