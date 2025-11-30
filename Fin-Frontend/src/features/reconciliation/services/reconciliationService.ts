/**
 * Reconciliation Service
 * Manages reconciliation sessions and persistence
 */

import type {
  ReconciliationSession,
  BankStatement,
  InternalTransaction,
  AdjustmentEntry,
  ReconciliationReport,
} from '../types/reconciliation.types';

const STORAGE_KEY = 'reconciliation-sessions';

class ReconciliationService {
  /**
   * Create a new reconciliation session
   */
  async createSession(
    statement: BankStatement,
    internalTransactions: InternalTransaction[]
  ): Promise<ReconciliationSession> {
    const session: ReconciliationSession = {
      id: `session-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      accountId: statement.accountId,
      accountName: statement.accountName,
      statementId: statement.id,
      statementDate: statement.statementDate,
      openingBalance: statement.openingBalance,
      closingBalance: statement.closingBalance,
      bookBalance: this.calculateBookBalance(internalTransactions),
      difference: 0,
      status: 'in-progress',
      bankTransactions: statement.transactions,
      internalTransactions,
      matches: [],
      unmatchedBank: statement.transactions,
      unmatchedInternal: internalTransactions,
      adjustments: [],
      reconciledBy: 'current-user', // Replace with actual user
      reconciledAt: new Date(),
    };

    session.difference = this.calculateDifference(session);

    await this.saveSession(session);
    return session;
  }

  /**
   * Get all sessions
   */
  async getSessions(): Promise<ReconciliationSession[]> {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (!stored) return [];

      const sessions = JSON.parse(stored);
      return sessions.map((s: any) => ({
        ...s,
        statementDate: new Date(s.statementDate),
        reconciledAt: new Date(s.reconciledAt),
        approvedAt: s.approvedAt ? new Date(s.approvedAt) : undefined,
        bankTransactions: s.bankTransactions.map((tx: any) => ({
          ...tx,
          date: new Date(tx.date),
        })),
        internalTransactions: s.internalTransactions.map((tx: any) => ({
          ...tx,
          date: new Date(tx.date),
        })),
        unmatchedBank: s.unmatchedBank.map((tx: any) => ({
          ...tx,
          date: new Date(tx.date),
        })),
        unmatchedInternal: s.unmatchedInternal.map((tx: any) => ({
          ...tx,
          date: new Date(tx.date),
        })),
        matches: s.matches.map((m: any) => ({
          ...m,
          matchedAt: new Date(m.matchedAt),
          bankTransaction: {
            ...m.bankTransaction,
            date: new Date(m.bankTransaction.date),
          },
          internalTransaction: {
            ...m.internalTransaction,
            date: new Date(m.internalTransaction.date),
          },
        })),
        adjustments: s.adjustments.map((a: any) => ({
          ...a,
          createdAt: new Date(a.createdAt),
          approvedAt: a.approvedAt ? new Date(a.approvedAt) : undefined,
        })),
      }));
    } catch (error) {
      console.error('[ReconciliationService] Error loading sessions:', error);
      return [];
    }
  }

  /**
   * Get session by ID
   */
  async getSession(sessionId: string): Promise<ReconciliationSession | null> {
    const sessions = await this.getSessions();
    return sessions.find(s => s.id === sessionId) || null;
  }

  /**
   * Save session
   */
  async saveSession(session: ReconciliationSession): Promise<void> {
    try {
      const sessions = await this.getSessions();
      const index = sessions.findIndex(s => s.id === session.id);

      if (index >= 0) {
        sessions[index] = session;
      } else {
        sessions.push(session);
      }

      localStorage.setItem(STORAGE_KEY, JSON.stringify(sessions));
    } catch (error) {
      console.error('[ReconciliationService] Error saving session:', error);
      throw error;
    }
  }

  /**
   * Delete session
   */
  async deleteSession(sessionId: string): Promise<void> {
    try {
      const sessions = await this.getSessions();
      const filtered = sessions.filter(s => s.id !== sessionId);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
    } catch (error) {
      console.error('[ReconciliationService] Error deleting session:', error);
      throw error;
    }
  }

  /**
   * Complete reconciliation
   */
  async completeReconciliation(sessionId: string): Promise<ReconciliationSession> {
    const session = await this.getSession(sessionId);
    if (!session) throw new Error('Session not found');

    session.status = 'completed';
    await this.saveSession(session);
    return session;
  }

  /**
   * Approve reconciliation
   */
  async approveReconciliation(
    sessionId: string,
    approvedBy: string
  ): Promise<ReconciliationSession> {
    const session = await this.getSession(sessionId);
    if (!session) throw new Error('Session not found');

    session.status = 'approved';
    session.approvedBy = approvedBy;
    session.approvedAt = new Date();
    await this.saveSession(session);
    return session;
  }

  /**
   * Add adjustment entry
   */
  async addAdjustment(
    sessionId: string,
    adjustment: Omit<AdjustmentEntry, 'id' | 'createdAt'>
  ): Promise<ReconciliationSession> {
    const session = await this.getSession(sessionId);
    if (!session) throw new Error('Session not found');

    const newAdjustment: AdjustmentEntry = {
      ...adjustment,
      id: `adj-${Date.now()}`,
      createdAt: new Date(),
    };

    session.adjustments.push(newAdjustment);
    session.difference = this.calculateDifference(session);
    await this.saveSession(session);
    return session;
  }

  /**
   * Remove adjustment entry
   */
  async removeAdjustment(
    sessionId: string,
    adjustmentId: string
  ): Promise<ReconciliationSession> {
    const session = await this.getSession(sessionId);
    if (!session) throw new Error('Session not found');

    session.adjustments = session.adjustments.filter(a => a.id !== adjustmentId);
    session.difference = this.calculateDifference(session);
    await this.saveSession(session);
    return session;
  }

  /**
   * Generate reconciliation report
   */
  async generateReport(sessionId: string): Promise<ReconciliationReport> {
    const session = await this.getSession(sessionId);
    if (!session) throw new Error('Session not found');

    const totalAdjustments = session.adjustments.reduce(
      (sum, adj) => sum + adj.amount,
      0
    );

    return {
      sessionId: session.id,
      accountName: session.accountName,
      statementDate: session.statementDate,
      openingBalance: session.openingBalance,
      closingBalance: session.closingBalance,
      bookBalance: session.bookBalance,
      difference: session.difference,
      matchedCount: session.matches.length,
      unmatchedBankCount: session.unmatchedBank.length,
      unmatchedInternalCount: session.unmatchedInternal.length,
      adjustmentCount: session.adjustments.length,
      totalAdjustments,
      status: session.status,
      generatedAt: new Date(),
      generatedBy: 'current-user',
    };
  }

  /**
   * Export session to JSON
   */
  async exportSession(sessionId: string): Promise<string> {
    const session = await this.getSession(sessionId);
    if (!session) throw new Error('Session not found');

    return JSON.stringify(session, null, 2);
  }

  /**
   * Calculate book balance from internal transactions
   */
  private calculateBookBalance(transactions: InternalTransaction[]): number {
    return transactions.reduce((balance, tx) => {
      return balance + (tx.type === 'credit' ? tx.amount : -tx.amount);
    }, 0);
  }

  /**
   * Calculate difference between bank and book
   */
  private calculateDifference(session: ReconciliationSession): number {
    const matchedAmount = session.matches.reduce((sum, match) => {
      const bankAmount = (match.bankTransaction.debit || 0) || (match.bankTransaction.credit || 0);
      return sum + bankAmount;
    }, 0);

    const adjustmentAmount = session.adjustments.reduce(
      (sum, adj) => sum + adj.amount,
      0
    );

    return session.closingBalance - session.bookBalance - adjustmentAmount;
  }

  /**
   * Get reconciliation statistics
   */
  async getStatistics(): Promise<{
    totalSessions: number;
    completedSessions: number;
    approvedSessions: number;
    inProgressSessions: number;
    totalMatches: number;
    averageMatchRate: number;
  }> {
    const sessions = await this.getSessions();

    const totalSessions = sessions.length;
    const completedSessions = sessions.filter(s => s.status === 'completed').length;
    const approvedSessions = sessions.filter(s => s.status === 'approved').length;
    const inProgressSessions = sessions.filter(s => s.status === 'in-progress').length;

    const totalMatches = sessions.reduce((sum, s) => sum + s.matches.length, 0);
    const totalTransactions = sessions.reduce(
      (sum, s) => sum + s.bankTransactions.length,
      0
    );

    const averageMatchRate = totalTransactions > 0
      ? (totalMatches / totalTransactions) * 100
      : 0;

    return {
      totalSessions,
      completedSessions,
      approvedSessions,
      inProgressSessions,
      totalMatches,
      averageMatchRate: Math.round(averageMatchRate * 100) / 100,
    };
  }
}

// Export singleton instance
export const reconciliationService = new ReconciliationService();
