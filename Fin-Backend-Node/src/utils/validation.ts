import { z } from 'zod';
import { config } from '@config/index';

/**
 * Email validation schema
 */
export const emailSchema = z.string().email('Invalid email address');

/**
 * Password validation schema based on configuration
 */
export const passwordSchema = z
  .string()
  .min(config.PASSWORD_MIN_LENGTH, `Password must be at least ${config.PASSWORD_MIN_LENGTH} characters`)
  .refine(
    (password) => {
      if (config.PASSWORD_REQUIRE_UPPERCASE && !/[A-Z]/.test(password)) {
        return false;
      }
      if (config.PASSWORD_REQUIRE_LOWERCASE && !/[a-z]/.test(password)) {
        return false;
      }
      if (config.PASSWORD_REQUIRE_NUMBERS && !/\d/.test(password)) {
        return false;
      }
      if (config.PASSWORD_REQUIRE_SPECIAL && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
        return false;
      }
      return true;
    },
    {
      message: 'Password must meet complexity requirements',
    }
  );

/**
 * UUID validation schema
 */
export const uuidSchema = z.string().uuid('Invalid UUID format');

/**
 * Pagination validation schema
 */
export const paginationSchema = z.object({
  page: z.coerce.number().int().positive().default(1),
  limit: z.coerce.number().int().positive().max(100).default(10),
  sortBy: z.string().optional(),
  sortOrder: z.enum(['asc', 'desc']).optional().default('asc'),
});

/**
 * Date range validation schema
 */
export const dateRangeSchema = z.object({
  startDate: z.coerce.date(),
  endDate: z.coerce.date(),
}).refine(
  (data) => data.startDate <= data.endDate,
  {
    message: 'Start date must be before or equal to end date',
  }
);

/**
 * Validate request body against schema
 */
export const validateBody = <T extends z.ZodType>(schema: T) => {
  return (data: unknown): z.infer<T> => {
    return schema.parse(data);
  };
};

/**
 * Validate request query parameters against schema
 */
export const validateQuery = <T extends z.ZodType>(schema: T) => {
  return (data: unknown): z.infer<T> => {
    return schema.parse(data);
  };
};

/**
 * Validate request params against schema
 */
export const validateParams = <T extends z.ZodType>(schema: T) => {
  return (data: unknown): z.infer<T> => {
    return schema.parse(data);
  };
};
