import { Request, Response } from 'express';
import { z } from 'zod';
import { ReportingService } from '@services/ReportingService';
import { AnalyticsService } from '@services/AnalyticsService';
import { asyncHandler } from '@utils/asyncHandler';

// Validation schemas
const reportParamsSchema = z.object({
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
  branchId: z.string().uuid().optional(),
  format: z.enum(['json', 'pdf', 'excel']).optional(),
});

const trendAnalysisSchema = z.object({
  metric: z.string(),
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
  interval: z.enum(['daily', 'weekly', 'monthly']),
  branchId: z.string().uuid().optional(),
});

export class ReportController {
  private reportingService = new ReportingService();
  private analyticsService = new AnalyticsService();

  /**
   * Generate balance sheet
   * POST /api/v1/reports/balance-sheet
   */
  generateBalanceSheet = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const params = reportParamsSchema.parse(req.body);

    const balanceSheet = await this.reportingService.generateBalanceSheet({
      startDate: new Date(params.startDate),
      endDate: new Date(params.endDate),
      branchId: params.branchId,
      format: params.format,
    });

    res.status(200).json({
      success: true,
      data: balanceSheet,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Generate income statement
   * POST /api/v1/reports/income-statement
   */
  generateIncomeStatement = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const params = reportParamsSchema.parse(req.body);

    const incomeStatement = await this.reportingService.generateIncomeStatement({
      startDate: new Date(params.startDate),
      endDate: new Date(params.endDate),
      branchId: params.branchId,
      format: params.format,
    });

    res.status(200).json({
      success: true,
      data: incomeStatement,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Generate cash flow statement
   * POST /api/v1/reports/cash-flow-statement
   */
  generateCashFlowStatement = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const params = reportParamsSchema.parse(req.body);

    const cashFlowStatement = await this.reportingService.generateCashFlowStatement({
      startDate: new Date(params.startDate),
      endDate: new Date(params.endDate),
      branchId: params.branchId,
      format: params.format,
    });

    res.status(200).json({
      success: true,
      data: cashFlowStatement,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Generate trial balance
   * POST /api/v1/reports/trial-balance
   */
  generateTrialBalance = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const params = reportParamsSchema.parse(req.body);

    const trialBalance = await this.reportingService.generateTrialBalance({
      startDate: new Date(params.startDate),
      endDate: new Date(params.endDate),
      branchId: params.branchId,
      format: params.format,
    });

    res.status(200).json({
      success: true,
      data: trialBalance,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Generate comprehensive financial report
   * POST /api/v1/reports/financial
   */
  generateFinancialReport = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const params = reportParamsSchema.parse(req.body);

    const report = await this.reportingService.generateFinancialReport({
      startDate: new Date(params.startDate),
      endDate: new Date(params.endDate),
      branchId: params.branchId,
      format: params.format,
    });

    res.status(200).json({
      success: true,
      data: report,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Get dashboard metrics
   * GET /api/v1/analytics/dashboard
   */
  getDashboardMetrics = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { startDate, endDate, branchId } = req.query;

    const metrics = await this.analyticsService.getDashboardMetrics({
      startDate: startDate ? new Date(startDate as string) : new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
      endDate: endDate ? new Date(endDate as string) : new Date(),
      branchId: branchId as string | undefined,
    });

    res.status(200).json({
      success: true,
      data: metrics,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Get KPIs
   * GET /api/v1/analytics/kpis
   */
  getKPIs = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const { startDate, endDate, branchId } = req.query;

    const kpis = await this.analyticsService.getKPIs({
      startDate: startDate ? new Date(startDate as string) : new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
      endDate: endDate ? new Date(endDate as string) : new Date(),
      branchId: branchId as string | undefined,
    });

    res.status(200).json({
      success: true,
      data: kpis,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });

  /**
   * Get trend analysis
   * POST /api/v1/analytics/trends
   */
  getTrendAnalysis = asyncHandler(async (req: Request, res: Response): Promise<void> => {
    const params = trendAnalysisSchema.parse(req.body);

    const trends = await this.analyticsService.getTrendAnalysis(
      params.metric,
      new Date(params.startDate),
      new Date(params.endDate),
      params.interval,
      params.branchId
    );

    res.status(200).json({
      success: true,
      data: trends,
      timestamp: new Date(),
      correlationId: req.correlationId,
    });
  });
}

export default ReportController;
