import React, { useState, useEffect } from 'react';
import { clsx } from 'clsx';
import { Check, X, Link2, AlertCircle } from 'lucide-react';
import { Button, Card, toastService } from '@/design-system';
import { matchingEngine } from '../services/matchingEngine';
import type {
  BankTransaction,
  InternalTransaction,
  ReconciliationMatch,
  ReconciliationSession,
  MatchSuggestion,
} from '../types/reconciliation.types';

export interface ReconciliationWorkspaceProps {
  session: ReconciliationSession;
  onSessionUpdate: (session: ReconciliationSession) => void;
}

export const ReconciliationWorkspace: React.FC<ReconciliationWorkspaceProps> = ({
  session,
  onSessionUpdate,
}) => {
  const [selectedBank, setSelectedBank] = useState<BankTransaction | null>(null);
  const [selectedInternal, setSelectedInternal] = useState<InternalTransaction | null>(null);
  const [suggestions, setSuggestions] = useState<MatchSuggestion[]>([]);
  const [isMatching, setIsMatching] = useState(false);

  useEffect(() => {
    if (selectedBank) {
      const matchSuggestions = matchingEngine.getSuggestions(
        selectedBank,
        session.unmatchedInternal,
        5
      );
      setSuggestions(matchSuggestions);
    } else {
      setSuggestions([]);
    }
  }, [selectedBank, session.unmatchedInternal]);

  const handleBankSelect = (tx: BankTransaction) => {
    setSelectedBank(tx.id === selectedBank?.id ? null : tx);
    setSelectedInternal(null);
  };

  const handleInternalSelect = (tx: InternalTransaction) => {
    setSelectedInternal(tx.id === selectedInternal?.id ? null : tx);
  };

  const handleManualMatch = () => {
    if (!selectedBank || !selectedInternal) {
      toastService.warning('Please select both bank and internal transactions');
      return;
    }

    const match: ReconciliationMatch = {
      id: `match-${Date.now()}`,
      bankTransaction: selectedBank,
      internalTransaction: selectedInternal,
      matchType: 'manual',
      confidence: 100,
      matchedBy: 'user',
      matchedAt: new Date(),
    };

    // Learn from this match
    matchingEngine.learnFromMatch(selectedBank, selectedInternal);

    // Update session
    const updatedSession: ReconciliationSession = {
      ...session,
      matches: [...session.matches, match],
      unmatchedBank: session.unmatchedBank.filter(tx => tx.id !== selectedBank.id),
      unmatchedInternal: session.unmatchedInternal.filter(tx => tx.id !== selectedInternal.id),
    };

    onSessionUpdate(updatedSession);
    setSelectedBank(null);
    setSelectedInternal(null);
    toastService.success('Transactions matched successfully');
  };

  const handleSuggestionMatch = (suggestion: MatchSuggestion) => {
    const match: ReconciliationMatch = {
      id: `match-${Date.now()}`,
      bankTransaction: suggestion.bankTransaction,
      internalTransaction: suggestion.internalTransaction,
      matchType: suggestion.matchType,
      confidence: suggestion.confidence,
      matchedBy: 'user',
      matchedAt: new Date(),
    };

    const updatedSession: ReconciliationSession = {
      ...session,
      matches: [...session.matches, match],
      unmatchedBank: session.unmatchedBank.filter(tx => tx.id === suggestion.bankTransaction.id),
      unmatchedInternal: session.unmatchedInternal.filter(tx => tx.id === suggestion.internalTransaction.id),
    };

    onSessionUpdate(updatedSession);
    setSelectedBank(null);
    setSuggestions([]);
    toastService.success('Transactions matched successfully');
  };

  const handleUnmatch = (matchId: string) => {
    const match = session.matches.find(m => m.id === matchId);
    if (!match) return;

    const updatedSession: ReconciliationSession = {
      ...session,
      matches: session.matches.filter(m => m.id !== matchId),
      unmatchedBank: [...session.unmatchedBank, match.bankTransaction],
      unmatchedInternal: [...session.unmatchedInternal, match.internalTransaction],
    };

    onSessionUpdate(updatedSession);
    toastService.success('Match removed');
  };

  const handleAutoMatch = async () => {
    setIsMatching(true);
    try {
      const result = await matchingEngine.findMatches(
        session.unmatchedBank,
        session.unmatchedInternal
      );

      const updatedSession: ReconciliationSession = {
        ...session,
        matches: [...session.matches, ...result.matches],
        unmatchedBank: result.unmatchedBank,
        unmatchedInternal: result.unmatchedInternal,
      };

      onSessionUpdate(updatedSession);
      toastService.success(`Matched ${result.matches.length} transactions automatically`);
    } catch (error) {
      toastService.error('Auto-matching failed');
    } finally {
      setIsMatching(false);
    }
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
    }).format(amount);
  };

  const formatDate = (date: Date) => {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-neutral-900 dark:text-neutral-100">
            Reconciliation Workspace
          </h2>
          <p className="text-sm text-neutral-600 dark:text-neutral-400 mt-1">
            {session.matches.length} matched • {session.unmatchedBank.length} unmatched bank • {session.unmatchedInternal.length} unmatched internal
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={handleAutoMatch}
            loading={isMatching}
            disabled={isMatching || session.unmatchedBank.length === 0}
          >
            Auto-Match
          </Button>
          <Button
            variant="primary"
            onClick={handleManualMatch}
            disabled={!selectedBank || !selectedInternal}
            icon={<Link2 className="h-4 w-4" />}
          >
            Match Selected
          </Button>
        </div>
      </div>

      {/* Split Screen */}
      <div className="grid grid-cols-2 gap-4">
        {/* Bank Transactions */}
        <Card title="Bank Statement" className="h-[600px] flex flex-col">
          <div className="flex-1 overflow-y-auto space-y-2">
            {session.unmatchedBank.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-center py-8">
                <Check className="h-12 w-12 text-success-500 mb-2" />
                <p className="text-neutral-600 dark:text-neutral-400">
                  All bank transactions matched
                </p>
              </div>
            ) : (
              session.unmatchedBank.map(tx => (
                <TransactionCard
                  key={tx.id}
                  type="bank"
                  transaction={tx}
                  isSelected={selectedBank?.id === tx.id}
                  onClick={() => handleBankSelect(tx)}
                  formatAmount={formatAmount}
                  formatDate={formatDate}
                />
              ))
            )}
          </div>
        </Card>

        {/* Internal Transactions */}
        <Card title="Internal Records" className="h-[600px] flex flex-col">
          <div className="flex-1 overflow-y-auto space-y-2">
            {session.unmatchedInternal.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-center py-8">
                <Check className="h-12 w-12 text-success-500 mb-2" />
                <p className="text-neutral-600 dark:text-neutral-400">
                  All internal transactions matched
                </p>
              </div>
            ) : (
              session.unmatchedInternal.map(tx => (
                <TransactionCard
                  key={tx.id}
                  type="internal"
                  transaction={tx}
                  isSelected={selectedInternal?.id === tx.id}
                  onClick={() => handleInternalSelect(tx)}
                  formatAmount={formatAmount}
                  formatDate={formatDate}
                />
              ))
            )}
          </div>
        </Card>
      </div>

      {/* Suggestions */}
      {suggestions.length > 0 && (
        <Card title="Match Suggestions" subtitle={`${suggestions.length} potential matches found`}>
          <div className="space-y-2">
            {suggestions.map((suggestion, index) => (
              <div
                key={index}
                className={clsx(
                  'flex items-center justify-between p-3 rounded-lg border',
                  'hover:bg-neutral-50 dark:hover:bg-neutral-800 transition-colors',
                  'border-neutral-200 dark:border-neutral-700'
                )}
              >
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <span className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
                      {suggestion.internalTransaction.description}
                    </span>
                    <span className={clsx(
                      'px-2 py-0.5 text-xs font-medium rounded-full',
                      suggestion.confidence >= 90
                        ? 'bg-success-100 text-success-700 dark:bg-success-900/20 dark:text-success-400'
                        : suggestion.confidence >= 75
                        ? 'bg-warning-100 text-warning-700 dark:bg-warning-900/20 dark:text-warning-400'
                        : 'bg-neutral-100 text-neutral-700 dark:bg-neutral-800 dark:text-neutral-400'
                    )}>
                      {suggestion.confidence}% match
                    </span>
                  </div>
                  <div className="flex items-center gap-4 text-xs text-neutral-600 dark:text-neutral-400">
                    <span>{formatDate(suggestion.internalTransaction.date)}</span>
                    <span>{formatAmount(Math.abs(suggestion.internalTransaction.amount))}</span>
                    <span className="text-neutral-500">•</span>
                    <span>{suggestion.reasons.join(', ')}</span>
                  </div>
                </div>
                <Button
                  size="sm"
                  variant="primary"
                  onClick={() => handleSuggestionMatch(suggestion)}
                >
                  Match
                </Button>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Matched Transactions */}
      {session.matches.length > 0 && (
        <Card title="Matched Transactions" subtitle={`${session.matches.length} transactions matched`}>
          <div className="space-y-2">
            {session.matches.map(match => (
              <div
                key={match.id}
                className="flex items-center justify-between p-3 rounded-lg bg-success-50 dark:bg-success-900/10 border border-success-200 dark:border-success-800"
              >
                <div className="flex-1 grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
                      {match.bankTransaction.description}
                    </p>
                    <p className="text-xs text-neutral-600 dark:text-neutral-400">
                      Bank • {formatDate(match.bankTransaction.date)}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
                      {match.internalTransaction.description}
                    </p>
                    <p className="text-xs text-neutral-600 dark:text-neutral-400">
                      Internal • {formatDate(match.internalTransaction.date)}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm font-semibold text-success-700 dark:text-success-400">
                    {formatAmount((match.bankTransaction.debit || 0) || (match.bankTransaction.credit || 0))}
                  </span>
                  <Button
                    size="xs"
                    variant="ghost"
                    onClick={() => handleUnmatch(match.id)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
};

interface TransactionCardProps {
  type: 'bank' | 'internal';
  transaction: BankTransaction | InternalTransaction;
  isSelected: boolean;
  onClick: () => void;
  formatAmount: (amount: number) => string;
  formatDate: (date: Date) => string;
}

const TransactionCard: React.FC<TransactionCardProps> = ({
  type,
  transaction,
  isSelected,
  onClick,
  formatAmount,
  formatDate,
}) => {
  const amount = type === 'bank'
    ? ((transaction as BankTransaction).debit || 0) || ((transaction as BankTransaction).credit || 0)
    : Math.abs((transaction as InternalTransaction).amount);

  return (
    <div
      onClick={onClick}
      className={clsx(
        'p-3 rounded-lg border cursor-pointer transition-all',
        isSelected
          ? 'border-primary-500 bg-primary-50 dark:bg-primary-900/20 ring-2 ring-primary-500'
          : 'border-neutral-200 dark:border-neutral-700 hover:border-primary-300 dark:hover:border-primary-700'
      )}
    >
      <div className="flex items-start justify-between mb-2">
        <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100 flex-1">
          {transaction.description}
        </p>
        <span className="text-sm font-semibold text-neutral-900 dark:text-neutral-100 ml-2">
          {formatAmount(amount)}
        </span>
      </div>
      <div className="flex items-center gap-3 text-xs text-neutral-600 dark:text-neutral-400">
        <span>{formatDate(transaction.date)}</span>
        {transaction.reference && (
          <>
            <span>•</span>
            <span>Ref: {transaction.reference}</span>
          </>
        )}
      </div>
    </div>
  );
};
