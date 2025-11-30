import app from './app';
import { config } from '@config/index';
import { logger } from '@utils/logger';
import { connectDatabase, disconnectDatabase } from '@config/database';
import { connectRedis, disconnectRedis } from '@config/redis';

// Handle uncaught exceptions
process.on('uncaughtException', (error: Error) => {
  logger.error('Uncaught Exception:', error);
  process.exit(1);
});

// Handle unhandled promise rejections
process.on('unhandledRejection', (reason: unknown) => {
  logger.error('Unhandled Rejection:', reason);
  process.exit(1);
});

// Initialize and start server
const startServer = async (): Promise<void> => {
  try {
    // Connect to database
    await connectDatabase();
    logger.info('✅ Database connection established');

    // Connect to Redis
    await connectRedis();
    logger.info('✅ Redis connection established');

    // Initialize scheduled tasks
    const { ScheduledTaskExecutor } = await import('@services/ScheduledTaskExecutor');
    const scheduledTaskExecutor = new ScheduledTaskExecutor();
    scheduledTaskExecutor.initialize();
    logger.info('✅ Scheduled tasks initialized');

    // Start HTTP server
    const server = app.listen(config.PORT, () => {
      logger.info(`🚀 Server running on port ${config.PORT}`);
      logger.info(`📝 Environment: ${config.NODE_ENV}`);
      logger.info(`🔗 API Version: ${config.API_VERSION}`);
      logger.info(`🌐 Health check: http://localhost:${config.PORT}/health`);
      logger.info(`📚 API Docs: http://localhost:${config.PORT}/api/docs`);
    });

    // Graceful shutdown
    const gracefulShutdown = async (signal: string): Promise<void> => {
      logger.info(`${signal} received. Starting graceful shutdown...`);
      
      server.close(async () => {
        logger.info('HTTP server closed');
        
        // Stop scheduled tasks
        scheduledTaskExecutor.stopAll();
        logger.info('Scheduled tasks stopped');
        
        // Close database connections
        await disconnectDatabase();
        
        // Close Redis connections
        await disconnectRedis();
        
        logger.info('Graceful shutdown completed');
        process.exit(0);
      });

      // Force shutdown after 30 seconds
      setTimeout(() => {
        logger.error('Forced shutdown after timeout');
        process.exit(1);
      }, 30000);
    };

    // Listen for termination signals
    process.on('SIGTERM', () => void gracefulShutdown('SIGTERM'));
    process.on('SIGINT', () => void gracefulShutdown('SIGINT'));
  } catch (error) {
    logger.error('Failed to start server:', error);
    process.exit(1);
  }
};

// Start the server
void startServer();

export default startServer;
