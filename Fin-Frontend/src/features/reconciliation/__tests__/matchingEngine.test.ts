import { matchingEngine } from '../services/matchingEngine';
import type { BankTransaction, InternalTransaction, MatchingRule } from '../types/reconciliation.types';

describe('Matching Engine', () => {
  const bankTx: BankTransaction = {
    id: 'bank-1',
    date: new Date('2024-01-15'),
    description: 'Payment from Customer ABC',
    reference: 'REF123',
    credit: 5000,
    balance: 15000,
  };

  const internalTx: InternalTransaction = {
    id: 'internal-1',
    date: new Date('2024-01-15'),
    description: 'Payment from Customer ABC',
    reference: 'REF123',
    amount: 5000,
    type: 'credit',
    accountId: 'acc-1',
    accountName: 'Cash Account',
  };

  describe('Exact Matching', () => {
    it('should match transactions with same amount, date, and reference', () => {
      const result = matchingEngine.exactMatch(bankTx, [internalTx], new Set());
      
      expect(result).not.toBeNull();
      expect(result?.id).toBe('internal-1');
    });

    it('should match transactions with same amount and date (no reference)', () => {
      const bankTxNoRef = { ...bankTx, reference: undefined };
      const internalTxNoRef = { ...internalTx, reference: undefined };
      
      const result = matchingEngine.exactMatch(bankTxNoRef, [internalTxNoRef], new Set());
      
      expect(result).not.toBeNull();
    });

    it('should not match if amount differs', () => {
      const differentAmount = { ...internalTx, amount: 6000 };
      
      const result = matchingEngine.exactMatch(bankTx, [differentAmount], new Set());
      
      expect(result).toBeNull();
    });

    it('should not match if date differs by more than 1 day', () => {
      const differentDate = { ...internalTx, date: new Date('2024-01-18') };
      
      const result = matchingEngine.exactMatch(bankTx, [differentDate], new Set());
      
      expect(result).toBeNull();
    });

    it('should match if date differs by 1 day', () => {
      const nextDay = { ...internalTx, date: new Date('2024-01-16') };
      
      const result = matchingEngine.exactMatch(bankTx, [nextDay], new Set());
      
      expect(result).not.toBeNull();
    });

    it('should exclude already matched transactions', () => {
      const excludeIds = new Set(['internal-1']);
      
      const result = matchingEngine.exactMatch(bankTx, [internalTx], excludeIds);
      
      expect(result).toBeNull();
    });
  });

  describe('Fuzzy Matching', () => {
    it('should find fuzzy matches with high confidence', () => {
      const similarTx = {
        ...internalTx,
        description: 'Payment from Customer ABC Ltd',
      };
      
      const matches = matchingEngine.fuzzyMatch(bankTx, [similarTx], new Set(), 0.75);
      
      expect(matches.length).toBeGreaterThan(0);
      expect(matches[0].confidence).toBeGreaterThan(75);
    });

    it('should not match if confidence is below threshold', () => {
      const differentTx = {
        ...internalTx,
        description: 'Completely different transaction',
        amount: 10000,
        date: new Date('2024-02-01'),
      };
      
      const matches = matchingEngine.fuzzyMatch(bankTx, [differentTx], new Set(), 0.75);
      
      expect(matches.length).toBe(0);
    });

    it('should sort matches by confidence descending', () => {
      const tx1 = { ...internalTx, id: 'tx1', description: 'Payment from Customer ABC' };
      const tx2 = { ...internalTx, id: 'tx2', description: 'Payment Customer ABC', amount: 4900 };
      const tx3 = { ...internalTx, id: 'tx3', description: 'Customer ABC payment', amount: 4800 };
      
      const matches = matchingEngine.fuzzyMatch(bankTx, [tx1, tx2, tx3], new Set(), 0.5);
      
      expect(matches.length).toBeGreaterThan(0);
      // First match should have highest confidence
      for (let i = 1; i < matches.length; i++) {
        expect(matches[i - 1].confidence).toBeGreaterThanOrEqual(matches[i].confidence);
      }
    });

    it('should exclude already matched transactions', () => {
      const excludeIds = new Set(['internal-1']);
      
      const matches = matchingEngine.fuzzyMatch(bankTx, [internalTx], excludeIds, 0.75);
      
      expect(matches.length).toBe(0);
    });
  });

  describe('Rule-Based Matching', () => {
    beforeEach(() => {
      // Clear existing rules
      matchingEngine.getRules().forEach(rule => {
        matchingEngine.removeRule(rule.id);
      });
    });

    it('should match using custom rules', () => {
      const rule: MatchingRule = {
        id: 'rule-1',
        name: 'Test Rule',
        description: 'Match by amount',
        conditions: [
          {
            field: 'amount',
            operator: 'equals',
            value: 5000,
            tolerance: 0.01,
          },
        ],
        priority: 100,
        enabled: true,
        createdBy: 'test',
        createdAt: new Date(),
      };

      matchingEngine.addRule(rule);
      
      const result = matchingEngine.ruleBasedMatch(bankTx, [internalTx], new Set());
      
      expect(result).not.toBeNull();
      expect(result?.confidence).toBe(90);
    });

    it('should not match if rule is disabled', () => {
      const rule: MatchingRule = {
        id: 'rule-2',
        name: 'Disabled Rule',
        conditions: [
          {
            field: 'amount',
            operator: 'equals',
            value: 5000,
            tolerance: 0.01,
          },
        ],
        priority: 100,
        enabled: false,
        createdBy: 'test',
        createdAt: new Date(),
      };

      matchingEngine.addRule(rule);
      
      const result = matchingEngine.ruleBasedMatch(bankTx, [internalTx], new Set());
      
      expect(result).toBeNull();
    });

    it('should apply rules in priority order', () => {
      const lowPriorityRule: MatchingRule = {
        id: 'rule-low',
        name: 'Low Priority',
        conditions: [{ field: 'amount', operator: 'equals', value: 5000, tolerance: 0.01 }],
        priority: 50,
        enabled: true,
        createdBy: 'test',
        createdAt: new Date(),
      };

      const highPriorityRule: MatchingRule = {
        id: 'rule-high',
        name: 'High Priority',
        conditions: [{ field: 'amount', operator: 'equals', value: 5000, tolerance: 0.01 }],
        priority: 100,
        enabled: true,
        createdBy: 'test',
        createdAt: new Date(),
      };

      matchingEngine.addRule(lowPriorityRule);
      matchingEngine.addRule(highPriorityRule);
      
      const result = matchingEngine.ruleBasedMatch(bankTx, [internalTx], new Set());
      
      expect(result).not.toBeNull();
    });
  });

  describe('Find Matches', () => {
    it('should find all matches automatically', async () => {
      const bankTransactions: BankTransaction[] = [
        { id: 'b1', date: new Date('2024-01-15'), description: 'Payment 1', credit: 1000 },
        { id: 'b2', date: new Date('2024-01-16'), description: 'Payment 2', credit: 2000 },
        { id: 'b3', date: new Date('2024-01-17'), description: 'Payment 3', credit: 3000 },
      ];

      const internalTransactions: InternalTransaction[] = [
        { id: 'i1', date: new Date('2024-01-15'), description: 'Payment 1', amount: 1000, type: 'credit', accountId: 'acc1', accountName: 'Cash' },
        { id: 'i2', date: new Date('2024-01-16'), description: 'Payment 2', amount: 2000, type: 'credit', accountId: 'acc1', accountName: 'Cash' },
        { id: 'i3', date: new Date('2024-01-17'), description: 'Payment 3', amount: 3000, type: 'credit', accountId: 'acc1', accountName: 'Cash' },
      ];

      const result = await matchingEngine.findMatches(bankTransactions, internalTransactions);

      expect(result.matches.length).toBe(3);
      expect(result.unmatchedBank.length).toBe(0);
      expect(result.unmatchedInternal.length).toBe(0);
    });

    it('should handle partial matches', async () => {
      const bankTransactions: BankTransaction[] = [
        { id: 'b1', date: new Date('2024-01-15'), description: 'Payment 1', credit: 1000 },
        { id: 'b2', date: new Date('2024-01-16'), description: 'Payment 2', credit: 2000 },
      ];

      const internalTransactions: InternalTransaction[] = [
        { id: 'i1', date: new Date('2024-01-15'), description: 'Payment 1', amount: 1000, type: 'credit', accountId: 'acc1', accountName: 'Cash' },
      ];

      const result = await matchingEngine.findMatches(bankTransactions, internalTransactions);

      expect(result.matches.length).toBe(1);
      expect(result.unmatchedBank.length).toBe(1);
      expect(result.unmatchedInternal.length).toBe(0);
    });
  });

  describe('Get Suggestions', () => {
    it('should return match suggestions', () => {
      const suggestions = matchingEngine.getSuggestions(bankTx, [internalTx], 5);

      expect(suggestions.length).toBeGreaterThan(0);
      expect(suggestions[0].confidence).toBeGreaterThan(0);
      expect(suggestions[0].reasons.length).toBeGreaterThan(0);
    });

    it('should limit suggestions to specified count', () => {
      const manyTransactions = Array.from({ length: 10 }, (_, i) => ({
        ...internalTx,
        id: `tx-${i}`,
        description: `Payment ${i}`,
      }));

      const suggestions = matchingEngine.getSuggestions(bankTx, manyTransactions, 3);

      expect(suggestions.length).toBeLessThanOrEqual(3);
    });

    it('should include reasons for suggestions', () => {
      const suggestions = matchingEngine.getSuggestions(bankTx, [internalTx], 5);

      expect(suggestions.length).toBeGreaterThan(0);
      expect(suggestions[0].reasons).toBeDefined();
      expect(Array.isArray(suggestions[0].reasons)).toBe(true);
    });
  });

  describe('Learning from Matches', () => {
    it('should learn from manual matches', () => {
      const initialRuleCount = matchingEngine.getRules().length;

      // Learn from same pattern 3 times
      for (let i = 0; i < 3; i++) {
        matchingEngine.learnFromMatch(bankTx, internalTx);
      }

      const finalRuleCount = matchingEngine.getRules().length;
      expect(finalRuleCount).toBeGreaterThan(initialRuleCount);
    });
  });

  describe('Rule Management', () => {
    it('should add and retrieve rules', () => {
      const rule: MatchingRule = {
        id: 'test-rule',
        name: 'Test Rule',
        conditions: [],
        priority: 100,
        enabled: true,
        createdBy: 'test',
        createdAt: new Date(),
      };

      matchingEngine.addRule(rule);
      const rules = matchingEngine.getRules();

      expect(rules.some(r => r.id === 'test-rule')).toBe(true);
    });

    it('should remove rules', () => {
      const rule: MatchingRule = {
        id: 'remove-rule',
        name: 'Remove Rule',
        conditions: [],
        priority: 100,
        enabled: true,
        createdBy: 'test',
        createdAt: new Date(),
      };

      matchingEngine.addRule(rule);
      matchingEngine.removeRule('remove-rule');
      const rules = matchingEngine.getRules();

      expect(rules.some(r => r.id === 'remove-rule')).toBe(false);
    });
  });
});
