import { CacheService } from './CacheService';
import { RepositoryFactory } from '@repositories/index';
import { getPrismaClient } from '@config/database';

export class CachedDataService {
  private cacheService = new CacheService();
  private prisma = getPrismaClient();

  // Cache TTLs (in seconds)
  private readonly TTL = {
    SESSION: 86400, // 24 hours
    PERMISSIONS: 3600, // 1 hour
    REFERENCE_DATA: 3600, // 1 hour
    DASHBOARD: 300, // 5 minutes
    ACCOUNT_BALANCE: 60, // 1 minute
    USER_PROFILE: 1800, // 30 minutes
  };

  /**
   * Cache user sessions
   */
  async cacheUserSession(userId: string, sessionData: any): Promise<void> {
    const key = this.cacheService.buildKey('session', userId);
    await this.cacheService.set(key, sessionData, this.TTL.SESSION);
  }

  async getUserSession(userId: string): Promise<any | null> {
    const key = this.cacheService.buildKey('session', userId);
    return this.cacheService.get(key);
  }

  async invalidateUserSession(userId: string): Promise<void> {
    const key = this.cacheService.buildKey('session', userId);
    await this.cacheService.delete(key);
  }

  /**
   * Cache user permissions
   */
  async cacheUserPermissions(userId: string): Promise<any> {
    const key = this.cacheService.buildKey('permissions', userId);
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        const userRepo = RepositoryFactory.getUserRepository();
        const user = await userRepo.findByIdWithPermissions(userId);
        return user?.role?.permissions || [];
      },
      this.TTL.PERMISSIONS
    );
  }

  async invalidateUserPermissions(userId: string): Promise<void> {
    const key = this.cacheService.buildKey('permissions', userId);
    await this.cacheService.delete(key);
  }

  /**
   * Cache roles
   */
  async cacheRoles(): Promise<any[]> {
    const key = this.cacheService.buildKey('reference', 'roles');
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        return this.prisma.role.findMany({
          include: { permissions: true },
        });
      },
      this.TTL.REFERENCE_DATA
    );
  }

  async invalidateRoles(): Promise<void> {
    const key = this.cacheService.buildKey('reference', 'roles');
    await this.cacheService.delete(key);
  }

  /**
   * Cache configurations
   */
  async cacheConfiguration(category?: string): Promise<any[]> {
    const key = category
      ? this.cacheService.buildKey('config', category)
      : this.cacheService.buildKey('config', 'all');
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        return this.prisma.configuration.findMany({
          where: category ? { category } : undefined,
        });
      },
      this.TTL.REFERENCE_DATA
    );
  }

  async invalidateConfiguration(category?: string): Promise<void> {
    if (category) {
      const key = this.cacheService.buildKey('config', category);
      await this.cacheService.delete(key);
    } else {
      await this.cacheService.invalidatePattern('config:*');
    }
  }

  /**
   * Cache dashboard metrics
   */
  async cacheDashboardMetrics(userId: string): Promise<any> {
    const key = this.cacheService.buildKey('dashboard', userId);
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        // Calculate dashboard metrics
        const [memberCount, accountCount, loanCount, transactionCount] = await Promise.all([
          this.prisma.member.count({ where: { status: 'ACTIVE' } }),
          this.prisma.account.count({ where: { status: 'ACTIVE' } }),
          this.prisma.loan.count({ where: { status: { in: ['DISBURSED', 'ACTIVE'] } } }),
          this.prisma.transaction.count({
            where: {
              createdAt: {
                gte: new Date(Date.now() - 24 * 60 * 60 * 1000), // Last 24 hours
              },
            },
          }),
        ]);

        return {
          members: memberCount,
          accounts: accountCount,
          loans: loanCount,
          transactions: transactionCount,
          timestamp: new Date(),
        };
      },
      this.TTL.DASHBOARD
    );
  }

  async invalidateDashboardMetrics(userId?: string): Promise<void> {
    if (userId) {
      const key = this.cacheService.buildKey('dashboard', userId);
      await this.cacheService.delete(key);
    } else {
      await this.cacheService.invalidatePattern('dashboard:*');
    }
  }

  /**
   * Cache account balance
   */
  async cacheAccountBalance(accountId: string): Promise<any> {
    const key = this.cacheService.buildKey('balance', accountId);
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        const account = await this.prisma.account.findUnique({
          where: { id: accountId },
          select: {
            balance: true,
            availableBalance: true,
            currency: true,
          },
        });
        return account;
      },
      this.TTL.ACCOUNT_BALANCE
    );
  }

  async invalidateAccountBalance(accountId: string): Promise<void> {
    const key = this.cacheService.buildKey('balance', accountId);
    await this.cacheService.delete(key);
  }

  /**
   * Cache user profile
   */
  async cacheUserProfile(userId: string): Promise<any> {
    const key = this.cacheService.buildKey('profile', userId);
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        const userRepo = RepositoryFactory.getUserRepository();
        const user = await userRepo.findByIdWithPermissions(userId);
        
        if (!user) return null;

        // Remove sensitive data
        const { passwordHash, mfaSecret, ...profile } = user;
        return profile;
      },
      this.TTL.USER_PROFILE
    );
  }

  async invalidateUserProfile(userId: string): Promise<void> {
    const key = this.cacheService.buildKey('profile', userId);
    await this.cacheService.delete(key);
  }

  /**
   * Cache loan products
   */
  async cacheLoanProducts(): Promise<any[]> {
    const key = this.cacheService.buildKey('reference', 'loan-products');
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        return this.prisma.loanProduct.findMany({
          where: { isActive: true },
        });
      },
      this.TTL.REFERENCE_DATA
    );
  }

  async invalidateLoanProducts(): Promise<void> {
    const key = this.cacheService.buildKey('reference', 'loan-products');
    await this.cacheService.delete(key);
  }

  /**
   * Cache transaction types
   */
  async cacheTransactionTypes(): Promise<any[]> {
    const key = this.cacheService.buildKey('reference', 'transaction-types');
    
    return this.cacheService.getOrSet(
      key,
      async () => {
        return this.prisma.transactionType.findMany({
          where: { isActive: true },
        });
      },
      this.TTL.REFERENCE_DATA
    );
  }

  async invalidateTransactionTypes(): Promise<void> {
    const key = this.cacheService.buildKey('reference', 'transaction-types');
    await this.cacheService.delete(key);
  }

  /**
   * Invalidate all reference data
   */
  async invalidateAllReferenceData(): Promise<void> {
    await this.cacheService.invalidatePattern('reference:*');
  }

  /**
   * Invalidate all user-related cache
   */
  async invalidateUserCache(userId: string): Promise<void> {
    await Promise.all([
      this.invalidateUserSession(userId),
      this.invalidateUserPermissions(userId),
      this.invalidateUserProfile(userId),
      this.invalidateDashboardMetrics(userId),
    ]);
  }
}

export default CachedDataService;
