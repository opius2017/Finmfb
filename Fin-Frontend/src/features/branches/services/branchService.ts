import {
  Branch,
  BranchPerformance,
  BranchComparison,
  BranchFinancials,
  BranchDashboard,
  BranchActivity,
  BranchUser,
  BranchAccess,
} from '../types/branch.types';

export class BranchService {
  private apiEndpoint = '/api/branches';

  async getBranches(status?: string): Promise<Branch[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    const response = await fetch(`${this.apiEndpoint}?${params}`);
    if (!response.ok) throw new Error('Failed to fetch branches');
    return response.json();
  }

  async getBranch(branchId: string): Promise<Branch> {
    const response = await fetch(`${this.apiEndpoint}/${branchId}`);
    if (!response.ok) throw new Error('Failed to fetch branch');
    return response.json();
  }

  async createBranch(branch: Partial<Branch>): Promise<Branch> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(branch),
    });
    if (!response.ok) throw new Error('Failed to create branch');
    return response.json();
  }

  async updateBranch(branchId: string, branch: Partial<Branch>): Promise<Branch> {
    const response = await fetch(`${this.apiEndpoint}/${branchId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(branch),
    });
    if (!response.ok) throw new Error('Failed to update branch');
    return response.json();
  }

  async deleteBranch(branchId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${branchId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete branch');
  }

  async getBranchPerformance(branchId: string, from: Date, to: Date): Promise<BranchPerformance> {
    const params = new URLSearchParams({
      from: from.toISOString(),
      to: to.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/${branchId}/performance?${params}`);
    if (!response.ok) throw new Error('Failed to fetch branch performance');
    return response.json();
  }

  async compareBranches(branchIds: string[], from: Date, to: Date): Promise<BranchComparison> {
    const params = new URLSearchParams({
      branches: branchIds.join(','),
      from: from.toISOString(),
      to: to.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/compare?${params}`);
    if (!response.ok) throw new Error('Failed to compare branches');
    return response.json();
  }

  async getBranchFinancials(branchId: string, from: Date, to: Date): Promise<BranchFinancials> {
    const params = new URLSearchParams({
      from: from.toISOString(),
      to: to.toISOString(),
    });

    const response = await fetch(`${this.apiEndpoint}/${branchId}/financials?${params}`);
    if (!response.ok) throw new Error('Failed to fetch branch financials');
    return response.json();
  }

  async getDashboard(): Promise<BranchDashboard> {
    const response = await fetch(`${this.apiEndpoint}/dashboard`);
    if (!response.ok) throw new Error('Failed to fetch dashboard');
    return response.json();
  }

  async getBranchActivity(branchId?: string, limit: number = 50): Promise<BranchActivity[]> {
    const params = new URLSearchParams({ limit: limit.toString() });
    if (branchId) params.append('branchId', branchId);

    const response = await fetch(`${this.apiEndpoint}/activity?${params}`);
    if (!response.ok) throw new Error('Failed to fetch branch activity');
    return response.json();
  }

  async getBranchUsers(branchId: string): Promise<BranchUser[]> {
    const response = await fetch(`${this.apiEndpoint}/${branchId}/users`);
    if (!response.ok) throw new Error('Failed to fetch branch users');
    return response.json();
  }

  async assignUserToBranch(userId: string, branchId: string, role: string): Promise<BranchUser> {
    const response = await fetch(`${this.apiEndpoint}/${branchId}/users`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userId, role }),
    });
    if (!response.ok) throw new Error('Failed to assign user to branch');
    return response.json();
  }

  async removeUserFromBranch(branchId: string, userId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${branchId}/users/${userId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to remove user from branch');
  }

  async getUserBranchAccess(userId: string): Promise<BranchAccess> {
    const response = await fetch(`${this.apiEndpoint}/access/${userId}`);
    if (!response.ok) throw new Error('Failed to fetch user branch access');
    return response.json();
  }

  async updateUserBranchAccess(userId: string, access: Partial<BranchAccess>): Promise<BranchAccess> {
    const response = await fetch(`${this.apiEndpoint}/access/${userId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(access),
    });
    if (!response.ok) throw new Error('Failed to update user branch access');
    return response.json();
  }

  async setDefaultBranch(branchId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/default`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ branchId }),
    });
    if (!response.ok) throw new Error('Failed to set default branch');
  }

  // Utility methods
  calculateProfitMargin(revenue: number, profit: number): number {
    return revenue === 0 ? 0 : (profit / revenue) * 100;
  }

  calculateGrowth(current: number, previous: number): number {
    return previous === 0 ? 0 : ((current - previous) / previous) * 100;
  }

  calculateWorkingCapital(currentAssets: number, currentLiabilities: number): number {
    return currentAssets - currentLiabilities;
  }

  calculateCurrentRatio(currentAssets: number, currentLiabilities: number): number {
    return currentLiabilities === 0 ? 0 : currentAssets / currentLiabilities;
  }

  rankBranches(branches: BranchPerformance[], metric: keyof BranchPerformance): BranchPerformance[] {
    return [...branches].sort((a, b) => {
      const aValue = typeof a[metric] === 'number' ? a[metric] : 0;
      const bValue = typeof b[metric] === 'number' ? b[metric] : 0;
      return (bValue as number) - (aValue as number);
    });
  }

  formatCurrency(amount: number, currency: string = 'NGN'): string {
    const symbol = currency === 'NGN' ? '₦' : currency;
    return `${symbol}${amount.toLocaleString()}`;
  }

  getBranchHierarchy(branches: Branch[]): Branch[] {
    const branchMap = new Map(branches.map(b => [b.id, { ...b, children: [] as Branch[] }]));
    const roots: Branch[] = [];

    branches.forEach(branch => {
      const node = branchMap.get(branch.id);
      if (branch.parentBranchId) {
        const parent = branchMap.get(branch.parentBranchId);
        if (parent) {
          (parent as any).children.push(node);
        }
      } else {
        roots.push(node as Branch);
      }
    });

    return roots;
  }
}

export const branchService = new BranchService();
