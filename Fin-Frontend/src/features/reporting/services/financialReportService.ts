import {
  TrialBalance,
  GeneralLedger,
  ProfitAndLoss,
  BalanceSheet,
  CashFlowStatement,
  DateRange,
} from '../types/financial.types';

export class FinancialReportService {
  private apiEndpoint = '/api/reports/financial';

  async getTrialBalance(asOfDate: Date): Promise<TrialBalance> {
    const response = await fetch(
      `${this.apiEndpoint}/trial-balance?asOfDate=${asOfDate.toISOString()}`
    );
    if (!response.ok) throw new Error('Failed to fetch trial balance');
    return response.json();
  }

  async getGeneralLedger(accountId: string, period: DateRange): Promise<GeneralLedger> {
    const response = await fetch(
      `${this.apiEndpoint}/general-ledger?accountId=${accountId}&from=${period.from.toISOString()}&to=${period.to.toISOString()}`
    );
    if (!response.ok) throw new Error('Failed to fetch general ledger');
    return response.json();
  }

  async getProfitAndLoss(period: DateRange, comparative?: DateRange): Promise<ProfitAndLoss> {
    const params = new URLSearchParams({
      from: period.from.toISOString(),
      to: period.to.toISOString(),
    });
    if (comparative) {
      params.append('comparativeFrom', comparative.from.toISOString());
      params.append('comparativeTo', comparative.to.toISOString());
    }
    
    const response = await fetch(`${this.apiEndpoint}/profit-loss?${params}`);
    if (!response.ok) throw new Error('Failed to fetch P&L');
    return response.json();
  }

  async getBalanceSheet(asOfDate: Date, comparative?: Date): Promise<BalanceSheet> {
    const params = new URLSearchParams({ asOfDate: asOfDate.toISOString() });
    if (comparative) {
      params.append('comparative', comparative.toISOString());
    }
    
    const response = await fetch(`${this.apiEndpoint}/balance-sheet?${params}`);
    if (!response.ok) throw new Error('Failed to fetch balance sheet');
    return response.json();
  }

  async getCashFlowStatement(
    period: DateRange,
    method: 'direct' | 'indirect'
  ): Promise<CashFlowStatement> {
    const response = await fetch(
      `${this.apiEndpoint}/cash-flow?from=${period.from.toISOString()}&to=${period.to.toISOString()}&method=${method}`
    );
    if (!response.ok) throw new Error('Failed to fetch cash flow statement');
    return response.json();
  }

  async exportFinancialReport(
    reportType: string,
    params: any,
    format: 'pdf' | 'excel'
  ): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/${reportType}/export`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...params, format }),
    });
    if (!response.ok) throw new Error('Failed to export report');
    return response.blob();
  }
}

export const financialReportService = new FinancialReportService();
