import dotenv from 'dotenv';
import { z } from 'zod';

// Load environment variables
dotenv.config();

// Environment variable schema
const envSchema = z.object({
  // Application
  NODE_ENV: z.enum(['development', 'staging', 'production', 'test']).default('development'),
  PORT: z.string().transform(Number).default('3000'),
  API_VERSION: z.string().default('v1'),

  // Database
  DATABASE_URL: z.string(),
  DATABASE_POOL_MIN: z.string().transform(Number).default('10'),
  DATABASE_POOL_MAX: z.string().transform(Number).default('50'),

  // Redis
  REDIS_HOST: z.string().default('localhost'),
  REDIS_PORT: z.string().transform(Number).default('6379'),
  REDIS_PASSWORD: z.string().optional(),
  REDIS_DB: z.string().transform(Number).default('0'),

  // JWT
  JWT_SECRET: z.string().min(32),
  JWT_EXPIRES_IN: z.string().default('15m'),
  JWT_REFRESH_SECRET: z.string().min(32),
  JWT_REFRESH_EXPIRES_IN: z.string().default('7d'),

  // CORS
  CORS_ORIGINS: z.string().transform((val) => val.split(',')),

  // Rate Limiting
  RATE_LIMIT_WINDOW_MS: z.string().transform(Number).default('900000'),
  RATE_LIMIT_MAX_REQUESTS: z.string().transform(Number).default('100'),

  // Encryption
  ENCRYPTION_KEY: z.string().length(32),
  ENCRYPTION_ALGORITHM: z.string().default('aes-256-gcm'),

  // Email
  SMTP_HOST: z.string(),
  SMTP_PORT: z.string().transform(Number),
  SMTP_USER: z.string(),
  SMTP_PASSWORD: z.string(),
  SMTP_FROM: z.string().email(),

  // File Storage
  S3_ENDPOINT: z.string().url(),
  S3_REGION: z.string(),
  S3_BUCKET: z.string(),
  S3_ACCESS_KEY_ID: z.string(),
  S3_SECRET_ACCESS_KEY: z.string(),

  // Monitoring
  LOG_LEVEL: z.enum(['error', 'warn', 'info', 'debug']).default('info'),
  ENABLE_METRICS: z.string().transform((val) => val === 'true').default('true'),
  ENABLE_TRACING: z.string().transform((val) => val === 'true').default('true'),

  // External Services
  PAYSTACK_SECRET_KEY: z.string().optional(),
  FLUTTERWAVE_SECRET_KEY: z.string().optional(),
  NIBSS_API_KEY: z.string().optional(),
  NIBSS_API_URL: z.string().url().optional(),

  // Security
  MFA_ISSUER: z.string().default('FinTech'),
  PASSWORD_MIN_LENGTH: z.string().transform(Number).default('8'),
  PASSWORD_REQUIRE_UPPERCASE: z.string().transform((val) => val === 'true').default('true'),
  PASSWORD_REQUIRE_LOWERCASE: z.string().transform((val) => val === 'true').default('true'),
  PASSWORD_REQUIRE_NUMBERS: z.string().transform((val) => val === 'true').default('true'),
  PASSWORD_REQUIRE_SPECIAL: z.string().transform((val) => val === 'true').default('true'),
  MAX_LOGIN_ATTEMPTS: z.string().transform(Number).default('5'),
  LOCKOUT_DURATION_MINUTES: z.string().transform(Number).default('30'),

  // Background Jobs
  QUEUE_CONCURRENCY: z.string().transform(Number).default('5'),
  JOB_RETRY_ATTEMPTS: z.string().transform(Number).default('3'),
  JOB_RETRY_DELAY: z.string().transform(Number).default('5000'),
});

// Validate and parse environment variables
const parseEnv = (): z.infer<typeof envSchema> => {
  try {
    return envSchema.parse(process.env);
  } catch (error) {
    if (error instanceof z.ZodError) {
      const missingVars = error.errors.map((err) => err.path.join('.')).join(', ');
      throw new Error(`Missing or invalid environment variables: ${missingVars}`);
    }
    throw error;
  }
};

export const config = parseEnv();

export const isDevelopment = config.NODE_ENV === 'development';
export const isProduction = config.NODE_ENV === 'production';
export const isTest = config.NODE_ENV === 'test';
