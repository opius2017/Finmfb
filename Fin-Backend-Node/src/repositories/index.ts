// Export all repositories
export { BaseRepository } from './BaseRepository';
export { UserRepository } from './UserRepository';
export { MemberRepository } from './MemberRepository';
export { AccountRepository } from './AccountRepository';
export { LoanRepository } from './LoanRepository';

// Export types
export type {
  IRepository,
  FilterOptions,
  PaginationOptions,
  PaginatedResult,
} from './BaseRepository';

// Repository factory
import { UserRepository } from './UserRepository';
import { MemberRepository } from './MemberRepository';
import { AccountRepository } from './AccountRepository';
import { LoanRepository } from './LoanRepository';

export class RepositoryFactory {
  private static instances: Map<string, any> = new Map();

  static getUserRepository(): UserRepository {
    if (!this.instances.has('user')) {
      this.instances.set('user', new UserRepository());
    }
    return this.instances.get('user');
  }

  static getMemberRepository(): MemberRepository {
    if (!this.instances.has('member')) {
      this.instances.set('member', new MemberRepository());
    }
    return this.instances.get('member');
  }

  static getAccountRepository(): AccountRepository {
    if (!this.instances.has('account')) {
      this.instances.set('account', new AccountRepository());
    }
    return this.instances.get('account');
  }

  static getLoanRepository(): LoanRepository {
    if (!this.instances.has('loan')) {
      this.instances.set('loan', new LoanRepository());
    }
    return this.instances.get('loan');
  }

  static clearInstances(): void {
    this.instances.clear();
  }
}

export default RepositoryFactory;
