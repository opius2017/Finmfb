/**
 * Transaction Matching Engine
 * Implements various matching algorithms for bank reconciliation
 */

import type {
  BankTransaction,
  InternalTransaction,
  ReconciliationMatch,
  MatchingRule,
  MatchSuggestion,
  MatchType,
} from '../types/reconciliation.types';

class MatchingEngine {
  private matchingRules: MatchingRule[] = [];
  private learningData: Map<string, number> = new Map();

  /**
   * Find matches for bank transactions
   */
  async findMatches(
    bankTransactions: BankTransaction[],
    internalTransactions: InternalTransaction[]
  ): Promise<{
    matches: ReconciliationMatch[];
    unmatchedBank: BankTransaction[];
    unmatchedInternal: InternalTransaction[];
  }> {
    const matches: ReconciliationMatch[] = [];
    const matchedBankIds = new Set<string>();
    const matchedInternalIds = new Set<string>();

    // Try exact matching first
    for (const bankTx of bankTransactions) {
      if (matchedBankIds.has(bankTx.id)) continue;

      const exactMatch = this.exactMatch(bankTx, internalTransactions, matchedInternalIds);
      if (exactMatch) {
        matches.push(this.createMatch(bankTx, exactMatch, 'exact', 100));
        matchedBankIds.add(bankTx.id);
        matchedInternalIds.add(exactMatch.id);
      }
    }

    // Try rule-based matching
    for (const bankTx of bankTransactions) {
      if (matchedBankIds.has(bankTx.id)) continue;

      const ruleMatch = this.ruleBasedMatch(bankTx, internalTransactions, matchedInternalIds);
      if (ruleMatch) {
        matches.push(this.createMatch(bankTx, ruleMatch.transaction, 'rule-based', ruleMatch.confidence));
        matchedBankIds.add(bankTx.id);
        matchedInternalIds.add(ruleMatch.transaction.id);
      }
    }

    // Try fuzzy matching for remaining
    for (const bankTx of bankTransactions) {
      if (matchedBankIds.has(bankTx.id)) continue;

      const fuzzyMatches = this.fuzzyMatch(bankTx, internalTransactions, matchedInternalIds, 0.8);
      if (fuzzyMatches.length > 0) {
        const bestMatch = fuzzyMatches[0];
        matches.push(this.createMatch(bankTx, bestMatch.transaction, 'fuzzy', bestMatch.confidence));
        matchedBankIds.add(bankTx.id);
        matchedInternalIds.add(bestMatch.transaction.id);
      }
    }

    // Collect unmatched
    const unmatchedBank = bankTransactions.filter(tx => !matchedBankIds.has(tx.id));
    const unmatchedInternal = internalTransactions.filter(tx => !matchedInternalIds.has(tx.id));

    return { matches, unmatchedBank, unmatchedInternal };
  }

  /**
   * Exact matching algorithm
   */
  exactMatch(
    bankTx: BankTransaction,
    internalTransactions: InternalTransaction[],
    excludeIds: Set<string>
  ): InternalTransaction | null {
    const bankAmount = (bankTx.debit || 0) || (bankTx.credit || 0);
    
    for (const internalTx of internalTransactions) {
      if (excludeIds.has(internalTx.id)) continue;

      // Match on amount and date (within 1 day)
      const amountMatches = Math.abs(Math.abs(internalTx.amount) - bankAmount) < 0.01;
      const dateMatches = this.datesMatch(bankTx.date, internalTx.date, 1);

      if (amountMatches && dateMatches) {
        // Check reference if available
        if (bankTx.reference && internalTx.reference) {
          if (bankTx.reference === internalTx.reference) {
            return internalTx;
          }
        } else {
          return internalTx;
        }
      }
    }

    return null;
  }

  /**
   * Fuzzy matching algorithm using Levenshtein distance
   */
  fuzzyMatch(
    bankTx: BankTransaction,
    internalTransactions: InternalTransaction[],
    excludeIds: Set<string>,
    threshold: number = 0.75
  ): Array<{ transaction: InternalTransaction; confidence: number }> {
    const bankAmount = (bankTx.debit || 0) || (bankTx.credit || 0);
    const matches: Array<{ transaction: InternalTransaction; confidence: number }> = [];

    for (const internalTx of internalTransactions) {
      if (excludeIds.has(internalTx.id)) continue;

      let confidence = 0;
      let factors = 0;

      // Amount similarity (40% weight)
      const amountDiff = Math.abs(Math.abs(internalTx.amount) - bankAmount);
      const amountSimilarity = 1 - Math.min(amountDiff / bankAmount, 1);
      if (amountSimilarity > 0.95) {
        confidence += amountSimilarity * 0.4;
        factors++;
      }

      // Date similarity (30% weight)
      const daysDiff = Math.abs(
        (bankTx.date.getTime() - internalTx.date.getTime()) / (1000 * 60 * 60 * 24)
      );
      const dateSimilarity = Math.max(0, 1 - daysDiff / 7); // Within 7 days
      if (dateSimilarity > 0.5) {
        confidence += dateSimilarity * 0.3;
        factors++;
      }

      // Description similarity (30% weight)
      const descSimilarity = this.calculateStringSimilarity(
        bankTx.description,
        internalTx.description
      );
      if (descSimilarity > 0.5) {
        confidence += descSimilarity * 0.3;
        factors++;
      }

      // Normalize confidence
      if (factors > 0) {
        confidence = confidence / (factors * 0.33); // Normalize to 0-1
        
        if (confidence >= threshold) {
          matches.push({
            transaction: internalTx,
            confidence: Math.round(confidence * 100),
          });
        }
      }
    }

    // Sort by confidence descending
    return matches.sort((a, b) => b.confidence - a.confidence);
  }

  /**
   * Rule-based matching
   */
  ruleBasedMatch(
    bankTx: BankTransaction,
    internalTransactions: InternalTransaction[],
    excludeIds: Set<string>
  ): { transaction: InternalTransaction; confidence: number } | null {
    const enabledRules = this.matchingRules
      .filter(rule => rule.enabled)
      .sort((a, b) => b.priority - a.priority);

    for (const rule of enabledRules) {
      for (const internalTx of internalTransactions) {
        if (excludeIds.has(internalTx.id)) continue;

        if (this.evaluateRule(rule, bankTx, internalTx)) {
          return {
            transaction: internalTx,
            confidence: 90, // Rule-based matches have high confidence
          };
        }
      }
    }

    return null;
  }

  /**
   * Get match suggestions for a bank transaction
   */
  getSuggestions(
    bankTx: BankTransaction,
    internalTransactions: InternalTransaction[],
    limit: number = 5
  ): MatchSuggestion[] {
    const suggestions: MatchSuggestion[] = [];

    // Try exact match
    const exactMatch = this.exactMatch(bankTx, internalTransactions, new Set());
    if (exactMatch) {
      suggestions.push({
        bankTransaction: bankTx,
        internalTransaction: exactMatch,
        confidence: 100,
        matchType: 'exact',
        reasons: ['Exact amount and date match'],
      });
    }

    // Try fuzzy matches
    const fuzzyMatches = this.fuzzyMatch(bankTx, internalTransactions, new Set(), 0.6);
    for (const match of fuzzyMatches.slice(0, limit - suggestions.length)) {
      const reasons: string[] = [];
      
      const bankAmount = (bankTx.debit || 0) || (bankTx.credit || 0);
      if (Math.abs(Math.abs(match.transaction.amount) - bankAmount) < 0.01) {
        reasons.push('Amount matches');
      }
      
      if (this.datesMatch(bankTx.date, match.transaction.date, 2)) {
        reasons.push('Date is close');
      }
      
      if (this.calculateStringSimilarity(bankTx.description, match.transaction.description) > 0.7) {
        reasons.push('Description is similar');
      }

      suggestions.push({
        bankTransaction: bankTx,
        internalTransaction: match.transaction,
        confidence: match.confidence,
        matchType: 'fuzzy',
        reasons,
      });
    }

    return suggestions;
  }

  /**
   * Learn from manual match
   */
  learnFromMatch(bankTx: BankTransaction, internalTx: InternalTransaction): void {
    // Create a pattern key
    const pattern = this.createPattern(bankTx, internalTx);
    const currentCount = this.learningData.get(pattern) || 0;
    this.learningData.set(pattern, currentCount + 1);

    // If pattern appears frequently, create a rule
    if (currentCount + 1 >= 3) {
      this.createRuleFromPattern(pattern, bankTx, internalTx);
    }
  }

  /**
   * Add matching rule
   */
  addRule(rule: MatchingRule): void {
    this.matchingRules.push(rule);
  }

  /**
   * Remove matching rule
   */
  removeRule(ruleId: string): void {
    this.matchingRules = this.matchingRules.filter(r => r.id !== ruleId);
  }

  /**
   * Get all rules
   */
  getRules(): MatchingRule[] {
    return this.matchingRules;
  }

  // Private helper methods

  private createMatch(
    bankTx: BankTransaction,
    internalTx: InternalTransaction,
    matchType: MatchType,
    confidence: number
  ): ReconciliationMatch {
    return {
      id: `match-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      bankTransaction: bankTx,
      internalTransaction: internalTx,
      matchType,
      confidence,
      matchedBy: 'system',
      matchedAt: new Date(),
    };
  }

  private datesMatch(date1: Date, date2: Date, toleranceDays: number): boolean {
    const diffDays = Math.abs(
      (date1.getTime() - date2.getTime()) / (1000 * 60 * 60 * 24)
    );
    return diffDays <= toleranceDays;
  }

  private calculateStringSimilarity(str1: string, str2: string): number {
    const s1 = str1.toLowerCase().trim();
    const s2 = str2.toLowerCase().trim();

    if (s1 === s2) return 1;
    if (s1.length === 0 || s2.length === 0) return 0;

    // Levenshtein distance
    const matrix: number[][] = [];

    for (let i = 0; i <= s2.length; i++) {
      matrix[i] = [i];
    }

    for (let j = 0; j <= s1.length; j++) {
      matrix[0][j] = j;
    }

    for (let i = 1; i <= s2.length; i++) {
      for (let j = 1; j <= s1.length; j++) {
        if (s2.charAt(i - 1) === s1.charAt(j - 1)) {
          matrix[i][j] = matrix[i - 1][j - 1];
        } else {
          matrix[i][j] = Math.min(
            matrix[i - 1][j - 1] + 1,
            matrix[i][j - 1] + 1,
            matrix[i - 1][j] + 1
          );
        }
      }
    }

    const maxLength = Math.max(s1.length, s2.length);
    return 1 - matrix[s2.length][s1.length] / maxLength;
  }

  private evaluateRule(
    rule: MatchingRule,
    bankTx: BankTransaction,
    internalTx: InternalTransaction
  ): boolean {
    return rule.conditions.every(condition => {
      switch (condition.field) {
        case 'amount':
          const bankAmount = (bankTx.debit || 0) || (bankTx.credit || 0);
          const tolerance = condition.tolerance || 0;
          return Math.abs(Math.abs(internalTx.amount) - bankAmount) <= tolerance;
        
        case 'date':
          if (condition.operator === 'equals') {
            return this.datesMatch(bankTx.date, internalTx.date, 0);
          } else if (condition.operator === 'range') {
            return this.datesMatch(bankTx.date, internalTx.date, condition.value);
          }
          return false;
        
        case 'description':
          return this.evaluateStringCondition(
            bankTx.description,
            internalTx.description,
            condition.operator,
            condition.value
          );
        
        case 'reference':
          if (!bankTx.reference || !internalTx.reference) return false;
          return this.evaluateStringCondition(
            bankTx.reference,
            internalTx.reference,
            condition.operator,
            condition.value
          );
        
        default:
          return false;
      }
    });
  }

  private evaluateStringCondition(
    str1: string,
    str2: string,
    operator: string,
    value: any
  ): boolean {
    const s1 = str1.toLowerCase();
    const s2 = str2.toLowerCase();

    switch (operator) {
      case 'equals':
        return s1 === s2;
      case 'contains':
        return s1.includes(value.toLowerCase()) || s2.includes(value.toLowerCase());
      case 'startsWith':
        return s1.startsWith(value.toLowerCase()) || s2.startsWith(value.toLowerCase());
      case 'endsWith':
        return s1.endsWith(value.toLowerCase()) || s2.endsWith(value.toLowerCase());
      default:
        return false;
    }
  }

  private createPattern(bankTx: BankTransaction, internalTx: InternalTransaction): string {
    const bankAmount = (bankTx.debit || 0) || (bankTx.credit || 0);
    return `${bankAmount}-${bankTx.description.substring(0, 20)}`;
  }

  private createRuleFromPattern(
    pattern: string,
    bankTx: BankTransaction,
    internalTx: InternalTransaction
  ): void {
    const rule: MatchingRule = {
      id: `rule-${Date.now()}`,
      name: `Auto-learned: ${pattern}`,
      description: 'Automatically created from matching pattern',
      conditions: [
        {
          field: 'amount',
          operator: 'equals',
          value: (bankTx.debit || 0) || (bankTx.credit || 0),
          tolerance: 0.01,
        },
      ],
      priority: 50,
      enabled: true,
      createdBy: 'system',
      createdAt: new Date(),
    };

    this.addRule(rule);
  }
}

// Export singleton instance
export const matchingEngine = new MatchingEngine();
