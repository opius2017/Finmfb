import { Router } from 'express';
import { ReportController } from '@controllers/ReportController';
import { authenticate } from '@middleware/auth';

const router = Router();
const reportController = new ReportController();

// Financial reports
router.post('/balance-sheet', authenticate, reportController.generateBalanceSheet);
router.post('/income-statement', authenticate, reportController.generateIncomeStatement);
router.post('/cash-flow-statement', authenticate, reportController.generateCashFlowStatement);
router.post('/trial-balance', authenticate, reportController.generateTrialBalance);
router.post('/financial', authenticate, reportController.generateFinancialReport);

// Analytics
router.get('/analytics/dashboard', authenticate, reportController.getDashboardMetrics);
router.get('/analytics/kpis', authenticate, reportController.getKPIs);
router.post('/analytics/trends', authenticate, reportController.getTrendAnalysis);

export default router;
