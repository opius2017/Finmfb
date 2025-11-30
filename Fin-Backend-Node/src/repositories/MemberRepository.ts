import { Member, KYCStatus, MemberStatus } from '@prisma/client';
import { BaseRepository } from './BaseRepository';

export class MemberRepository extends BaseRepository<Member> {
  constructor() {
    super('member');
  }

  /**
   * Find member by member number
   */
  async findByMemberNumber(memberNumber: string): Promise<Member | null> {
    return this.model.findUnique({
      where: { memberNumber },
      include: {
        branch: true,
        accounts: true,
        loans: true,
      },
    });
  }

  /**
   * Find member by email
   */
  async findByEmail(email: string): Promise<Member | null> {
    return this.model.findUnique({
      where: { email },
    });
  }

  /**
   * Find members by branch
   */
  async findByBranch(branchId: string): Promise<Member[]> {
    return this.model.findMany({
      where: { branchId },
      include: {
        accounts: true,
      },
    });
  }

  /**
   * Find members by status
   */
  async findByStatus(status: MemberStatus): Promise<Member[]> {
    return this.model.findMany({
      where: { status },
    });
  }

  /**
   * Find members by KYC status
   */
  async findByKYCStatus(kycStatus: KYCStatus): Promise<Member[]> {
    return this.model.findMany({
      where: { kycStatus },
    });
  }

  /**
   * Update KYC status
   */
  async updateKYCStatus(id: string, kycStatus: KYCStatus): Promise<Member> {
    return this.model.update({
      where: { id },
      data: { kycStatus },
    });
  }

  /**
   * Update member status
   */
  async updateStatus(id: string, status: MemberStatus): Promise<Member> {
    return this.model.update({
      where: { id },
      data: { status },
    });
  }

  /**
   * Search members
   */
  async search(query: string): Promise<Member[]> {
    return this.model.findMany({
      where: {
        OR: [
          { firstName: { contains: query, mode: 'insensitive' } },
          { lastName: { contains: query, mode: 'insensitive' } },
          { email: { contains: query, mode: 'insensitive' } },
          { memberNumber: { contains: query, mode: 'insensitive' } },
          { phone: { contains: query } },
        ],
      },
      take: 20,
    });
  }

  /**
   * Get member with full details
   */
  async findByIdWithDetails(id: string): Promise<Member | null> {
    return this.model.findUnique({
      where: { id },
      include: {
        branch: true,
        accounts: {
          include: {
            transactions: {
              take: 10,
              orderBy: { createdAt: 'desc' },
            },
          },
        },
        loans: {
          include: {
            product: true,
            schedules: true,
          },
        },
        documents: true,
      },
    });
  }
}

export default MemberRepository;
