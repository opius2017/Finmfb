import { Account, AccountType, AccountStatus } from '@prisma/client';
import { BaseRepository } from './BaseRepository';

export class AccountRepository extends BaseRepository<Account> {
  constructor() {
    super('account');
  }

  /**
   * Find account by account number
   */
  async findByAccountNumber(accountNumber: string): Promise<Account | null> {
    return this.model.findUnique({
      where: { accountNumber },
      include: {
        member: true,
        branch: true,
      },
    });
  }

  /**
   * Find accounts by member
   */
  async findByMember(memberId: string): Promise<Account[]> {
    return this.model.findMany({
      where: { memberId },
      include: {
        transactions: {
          take: 10,
          orderBy: { createdAt: 'desc' },
        },
      },
    });
  }

  /**
   * Find accounts by type
   */
  async findByType(accountType: AccountType): Promise<Account[]> {
    return this.model.findMany({
      where: { accountType },
    });
  }

  /**
   * Find accounts by status
   */
  async findByStatus(status: AccountStatus): Promise<Account[]> {
    return this.model.findMany({
      where: { status },
    });
  }

  /**
   * Update account balance
   */
  async updateBalance(
    id: string,
    balance: number,
    availableBalance: number
  ): Promise<Account> {
    return this.model.update({
      where: { id },
      data: {
        balance,
        availableBalance,
      },
    });
  }

  /**
   * Update account status
   */
  async updateStatus(id: string, status: AccountStatus): Promise<Account> {
    return this.model.update({
      where: { id },
      data: { status },
    });
  }

  /**
   * Close account
   */
  async closeAccount(id: string): Promise<Account> {
    return this.model.update({
      where: { id },
      data: {
        status: AccountStatus.CLOSED,
        closedAt: new Date(),
      },
    });
  }

  /**
   * Get account with transaction history
   */
  async findByIdWithTransactions(
    id: string,
    limit: number = 50
  ): Promise<Account | null> {
    return this.model.findUnique({
      where: { id },
      include: {
        member: true,
        transactions: {
          take: limit,
          orderBy: { createdAt: 'desc' },
        },
        balanceHistory: {
          take: 30,
          orderBy: { date: 'desc' },
        },
      },
    });
  }

  /**
   * Get total balance by account type
   */
  async getTotalBalanceByType(accountType: AccountType): Promise<number> {
    const result = await this.model.aggregate({
      where: {
        accountType,
        status: AccountStatus.ACTIVE,
      },
      _sum: {
        balance: true,
      },
    });

    return result._sum.balance?.toNumber() || 0;
  }

  /**
   * Get dormant accounts
   */
  async findDormantAccounts(daysInactive: number = 180): Promise<Account[]> {
    const cutoffDate = new Date();
    cutoffDate.setDate(cutoffDate.getDate() - daysInactive);

    return this.model.findMany({
      where: {
        status: AccountStatus.ACTIVE,
        updatedAt: {
          lt: cutoffDate,
        },
      },
    });
  }
}

export default AccountRepository;
