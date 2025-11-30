import { PrismaClient } from '@prisma/client';
import { getPrismaClient } from '@config/database';

/**
 * Base repository interface for CRUD operations
 */
export interface IRepository<T> {
  findById(id: string): Promise<T | null>;
  findMany(filter?: FilterOptions<T>): Promise<T[]>;
  findOne(filter: FilterOptions<T>): Promise<T | null>;
  create(data: Partial<T>): Promise<T>;
  update(id: string, data: Partial<T>): Promise<T>;
  delete(id: string): Promise<void>;
  count(filter?: FilterOptions<T>): Promise<number>;
}

/**
 * Filter options for queries
 */
export interface FilterOptions<T> {
  where?: Partial<T> | any;
  orderBy?: any;
  skip?: number;
  take?: number;
  include?: any;
  select?: any;
}

/**
 * Pagination options
 */
export interface PaginationOptions {
  page: number;
  limit: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Paginated result
 */
export interface PaginatedResult<T> {
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

/**
 * Base repository implementation
 */
export abstract class BaseRepository<T> implements IRepository<T> {
  protected prisma: PrismaClient;
  protected modelName: string;

  constructor(modelName: string) {
    this.prisma = getPrismaClient();
    this.modelName = modelName;
  }

  /**
   * Get the Prisma model delegate
   */
  protected get model(): any {
    return (this.prisma as any)[this.modelName];
  }

  /**
   * Find entity by ID
   */
  async findById(id: string): Promise<T | null> {
    return this.model.findUnique({
      where: { id },
    });
  }

  /**
   * Find multiple entities
   */
  async findMany(filter?: FilterOptions<T>): Promise<T[]> {
    return this.model.findMany(filter);
  }

  /**
   * Find single entity
   */
  async findOne(filter: FilterOptions<T>): Promise<T | null> {
    return this.model.findFirst(filter);
  }

  /**
   * Create new entity
   */
  async create(data: Partial<T>): Promise<T> {
    return this.model.create({
      data,
    });
  }

  /**
   * Update entity
   */
  async update(id: string, data: Partial<T>): Promise<T> {
    return this.model.update({
      where: { id },
      data,
    });
  }

  /**
   * Delete entity
   */
  async delete(id: string): Promise<void> {
    await this.model.delete({
      where: { id },
    });
  }

  /**
   * Count entities
   */
  async count(filter?: FilterOptions<T>): Promise<number> {
    return this.model.count(filter);
  }

  /**
   * Find with pagination
   */
  async findPaginated(
    filter: FilterOptions<T>,
    pagination: PaginationOptions
  ): Promise<PaginatedResult<T>> {
    const { page, limit, sortBy, sortOrder } = pagination;
    const skip = (page - 1) * limit;

    const orderBy = sortBy ? { [sortBy]: sortOrder || 'asc' } : undefined;

    const [data, total] = await Promise.all([
      this.model.findMany({
        ...filter,
        skip,
        take: limit,
        orderBy,
      }),
      this.model.count({ where: filter.where }),
    ]);

    const totalPages = Math.ceil(total / limit);

    return {
      data,
      pagination: {
        page,
        limit,
        total,
        totalPages,
        hasNext: page < totalPages,
        hasPrev: page > 1,
      },
    };
  }

  /**
   * Check if entity exists
   */
  async exists(id: string): Promise<boolean> {
    const count = await this.model.count({
      where: { id },
    });
    return count > 0;
  }

  /**
   * Soft delete (if model has isActive field)
   */
  async softDelete(id: string): Promise<T> {
    return this.model.update({
      where: { id },
      data: { isActive: false },
    });
  }

  /**
   * Restore soft deleted entity
   */
  async restore(id: string): Promise<T> {
    return this.model.update({
      where: { id },
      data: { isActive: true },
    });
  }
}

export default BaseRepository;
