import express, { Application, Request, Response } from 'express';
import helmet from 'helmet';
import cors from 'cors';
import compression from 'compression';
import { config } from '@config/index';
import { errorHandler } from '@middleware/errorHandler';
import { requestLogger } from '@middleware/requestLogger';
import { correlationId } from '@middleware/correlationId';
import { auditLogger } from '@middleware/auditLogger';
import { performanceMonitor, memoryMonitor, requestTimeout } from '@middleware/performanceMonitor';
import { securityHeaders, xssProtection } from '@middleware/security';

// Create Express application
const app: Application = express();

// Security middleware
app.use(helmet());
app.use(securityHeaders());
app.use(xssProtection());

// CORS configuration
app.use(
  cors({
    origin: config.CORS_ORIGINS,
    credentials: true,
    methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization', 'X-Correlation-ID'],
  })
);

// Body parsing middleware
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// Compression middleware
app.use(compression());

// Request correlation ID
app.use(correlationId);

// Request logging
app.use(requestLogger);

// Performance monitoring
app.use(performanceMonitor());
app.use(requestTimeout(30000)); // 30 second timeout

// Memory monitoring (background)
memoryMonitor();

// Audit logging
app.use(auditLogger());

// Health check endpoint
app.get('/health', (_req: Request, res: Response) => {
  res.status(200).json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    environment: config.NODE_ENV,
  });
});

// Readiness check endpoint
app.get('/ready', async (_req: Request, res: Response): Promise<void> => {
  try {
    const { checkDatabaseHealth } = await import('@config/database');
    const dbHealthy = await checkDatabaseHealth();
    
    if (!dbHealthy) {
      res.status(503).json({
        status: 'not_ready',
        reason: 'Database connection failed',
        timestamp: new Date().toISOString(),
      });
      return;
    }

    // TODO: Add Redis health check
    
    res.status(200).json({
      status: 'ready',
      timestamp: new Date().toISOString(),
      checks: {
        database: 'healthy',
      },
    });
  } catch (error) {
    res.status(503).json({
      status: 'not_ready',
      reason: 'Health check failed',
      timestamp: new Date().toISOString(),
    });
  }
});

// API version endpoint
app.get(`/api/${config.API_VERSION}`, (_req: Request, res: Response) => {
  res.status(200).json({
    message: 'FinTech API',
    version: config.API_VERSION,
    timestamp: new Date().toISOString(),
  });
});

// Swagger documentation
import swaggerUi from 'swagger-ui-express';
import { swaggerSpec } from '@config/swagger';

app.use('/api/docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec, {
  customCss: '.swagger-ui .topbar { display: none }',
  customSiteTitle: 'FinTech API Documentation',
}));

// Swagger JSON endpoint
app.get('/api/docs.json', (_req: Request, res: Response) => {
  res.setHeader('Content-Type', 'application/json');
  res.send(swaggerSpec);
});

// Register API routes
import authRoutes from './routes/auth.routes';
import passwordRoutes from './routes/password.routes';
import memberRoutes from './routes/member.routes';
import accountRoutes from './routes/account.routes';
import kycRoutes from './routes/kyc.routes';
import transactionRoutes from './routes/transaction.routes';
import loanRoutes from './routes/loan.routes';
import budgetRoutes from './routes/budget.routes';
import workflowRoutes from './routes/workflow.routes';
import documentRoutes from './routes/document.routes';
import auditRoutes from './routes/audit.routes';
import notificationRoutes from './routes/notification.routes';
import metricsRoutes from './routes/metrics.routes';
import regulatoryRoutes from './routes/regulatory.routes';

app.use(`/api/${config.API_VERSION}/auth`, authRoutes);
app.use(`/api/${config.API_VERSION}/password`, passwordRoutes);
app.use(`/api/${config.API_VERSION}/members`, memberRoutes);
app.use(`/api/${config.API_VERSION}/accounts`, accountRoutes);
app.use(`/api/${config.API_VERSION}/kyc`, kycRoutes);
app.use(`/api/${config.API_VERSION}/transactions`, transactionRoutes);
app.use(`/api/${config.API_VERSION}/loans`, loanRoutes);
app.use(`/api/${config.API_VERSION}/budgets`, budgetRoutes);
app.use(`/api/${config.API_VERSION}/workflows`, workflowRoutes);
app.use(`/api/${config.API_VERSION}/documents`, documentRoutes);
app.use(`/api/${config.API_VERSION}/audit`, auditRoutes);
app.use(`/api/${config.API_VERSION}/notifications`, notificationRoutes);
app.use(`/api/${config.API_VERSION}/metrics`, metricsRoutes);
app.use(`/api/${config.API_VERSION}/regulatory`, regulatoryRoutes);

// 404 handler
app.use((_req: Request, res: Response) => {
  res.status(404).json({
    error: {
      code: 'NOT_FOUND',
      message: 'The requested resource was not found',
      timestamp: new Date().toISOString(),
    },
  });
});

// Global error handler (must be last)
app.use(errorHandler);

export default app;
