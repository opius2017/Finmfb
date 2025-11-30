import { Request, Response } from 'express';
import RegulatoryReportingService from '../services/RegulatoryReportingService';
import ComplianceService from '../services/ComplianceService';
import { ReportType, ReportStatus, ComplianceStatus } from '../types/regulatory.types';
import { logger } from '../utils/logger';

export class RegulatoryController {
  /**
   * Generate CBN Prudential Return
   * POST /api/regulatory/reports/cbn-prudential
   */
  async generateCBNPrudentialReturn(req: Request, res: Response): Promise<void> {
    try {
      const { periodStart, periodEnd } = req.body;
      const userId = (req as any).user.id;

      if (!periodStart || !periodEnd) {
        res.status(400).json({
          success: false,
          message: 'Period start and end dates are required'
        });
        return;
      }

      const report = await RegulatoryReportingService.generateCBNPrudentialReturn(
        new Date(periodStart),
        new Date(periodEnd),
        userId
      );

      res.status(201).json({
        success: true,
        message: 'CBN Prudential Return generated successfully',
        data: report
      });
    } catch (error) {
      logger.error('Error in generateCBNPrudentialReturn', error);
      res.status(500).json({
        success: false,
        message: 'Failed to generate CBN Prudential Return',
        error: (error as Error).message
      });
    }
  }

  /**
   * Generate CBN Capital Adequacy Report
   * POST /api/regulatory/reports/cbn-capital-adequacy
   */
  async generateCBNCapitalAdequacyReport(req: Request, res: Response): Promise<void> {
    try {
      const { periodStart, periodEnd } = req.body;
      const userId = (req as any).user.id;

      if (!periodStart || !periodEnd) {
        res.status(400).json({
          success: false,
          message: 'Period start and end dates are required'
        });
        return;
      }

      const report = await RegulatoryReportingService.generateCBNCapitalAdequacyReport(
        new Date(periodStart),
        new Date(periodEnd),
        userId
      );

      res.status(201).json({
        success: true,
        message: 'CBN Capital Adequacy Report generated successfully',
        data: report
      });
    } catch (error) {
      logger.error('Error in generateCBNCapitalAdequacyReport', error);
      res.status(500).json({
        success: false,
        message: 'Failed to generate CBN Capital Adequacy Report',
        error: (error as Error).message
      });
    }
  }

  /**
   * Generate FIRS VAT Return
   * POST /api/regulatory/reports/firs-vat
   */
  async generateFIRSVATReturn(req: Request, res: Response): Promise<void> {
    try {
      const { periodStart, periodEnd } = req.body;
      const userId = (req as any).user.id;

      if (!periodStart || !periodEnd) {
        res.status(400).json({
          success: false,
          message: 'Period start and end dates are required'
        });
        return;
      }

      const report = await RegulatoryReportingService.generateFIRSVATReturn(
        new Date(periodStart),
        new Date(periodEnd),
        userId
      );

      res.status(201).json({
        success: true,
        message: 'FIRS VAT Return generated successfully',
        data: report
      });
    } catch (error) {
      logger.error('Error in generateFIRSVATReturn', error);
      res.status(500).json({
        success: false,
        message: 'Failed to generate FIRS VAT Return',
        error: (error as Error).message
      });
    }
  }

  /**
   * Generate IFRS 9 ECL Report
   * POST /api/regulatory/reports/ifrs9-ecl
   */
  async generateIFRS9ECLReport(req: Request, res: Response): Promise<void> {
    try {
      const { assessmentDate } = req.body;
      const userId = (req as any).user.id;

      const date = assessmentDate ? new Date(assessmentDate) : new Date();

      const report = await RegulatoryReportingService.generateIFRS9ECLReport(date, userId);

      res.status(201).json({
        success: true,
        message: 'IFRS 9 ECL Report generated successfully',
        data: report
      });
    } catch (error) {
      logger.error('Error in generateIFRS9ECLReport', error);
      res.status(500).json({
        success: false,
        message: 'Failed to generate IFRS 9 ECL Report',
        error: (error as Error).message
      });
    }
  }

  /**
   * Get all regulatory reports
   * GET /api/regulatory/reports
   */
  async getAllReports(req: Request, res: Response): Promise<void> {
    try {
      const { reportType, status, fiscalYear } = req.query;

      const reports = await RegulatoryReportingService.getAllReports({
        reportType: reportType as ReportType,
        status: status as ReportStatus,
        fiscalYear: fiscalYear ? parseInt(fiscalYear as string) : undefined
      });

      res.status(200).json({
        success: true,
        data: reports,
        count: reports.length
      });
    } catch (error) {
      logger.error('Error in getAllReports', error);
      res.status(500).json({
        success: false,
        message: 'Failed to retrieve reports',
        error: (error as Error).message
      });
    }
  }

  /**
   * Get report by ID
   * GET /api/regulatory/reports/:id
   */
  async getReportById(req: Request, res: Response): Promise<void> {
    try {
      const { id } = req.params;

      const report = await RegulatoryReportingService.getReportById(id);

      if (!report) {
        res.status(404).json({
          success: false,
          message: 'Report not found'
        });
        return;
      }

      res.status(200).json({
        success: true,
        data: report
      });
    } catch (error) {
      logger.error('Error in getReportById', error);
      res.status(500).json({
        success: false,
        message: 'Failed to retrieve report',
        error: (error as Error).message
      });
    }
  }

  /**
   * Update report status
   * PATCH /api/regulatory/reports/:id/status
   */
  async updateReportStatus(req: Request, res: Response): Promise<void> {
    try {
      const { id } = req.params;
      const { status, submissionReference } = req.body;

      if (!status) {
        res.status(400).json({
          success: false,
          message: 'Status is required'
        });
        return;
      }

      await RegulatoryReportingService.updateReportStatus(id, status, submissionReference);

      res.status(200).json({
        success: true,
        message: 'Report status updated successfully'
      });
    } catch (error) {
      logger.error('Error in updateReportStatus', error);
      res.status(500).json({
        success: false,
        message: 'Failed to update report status',
        error: (error as Error).message
      });
    }
  }

  /**
   * Get all compliance checklist items
   * GET /api/regulatory/compliance/checklist
   */
  async getAllChecklistItems(req: Request, res: Response): Promise<void> {
    try {
      const { category, status, priority, responsiblePerson } = req.query;

      const items = await ComplianceService.getAllChecklistItems({
        category: category as any,
        status: status as any,
        priority: priority as any,
        responsiblePerson: responsiblePerson as string
      });

      res.status(200).json({
        success: true,
        data: items,
        count: items.length
      });
    } catch (error) {
      logger.error('Error in getAllChecklistItems', error);
      res.status(500).json({
        success: false,
        message: 'Failed to retrieve checklist items',
        error: (error as Error).message
      });
    }
  }

  /**
   * Create compliance checklist item
   * POST /api/regulatory/compliance/checklist
   */
  async createChecklistItem(req: Request, res: Response): Promise<void> {
    try {
      const item = await ComplianceService.createChecklistItem(req.body);

      res.status(201).json({
        success: true,
        message: 'Checklist item created successfully',
        data: item
      });
    } catch (error) {
      logger.error('Error in createChecklistItem', error);
      res.status(500).json({
        success: false,
        message: 'Failed to create checklist item',
        error: (error as Error).message
      });
    }
  }

  /**
   * Update checklist item status
   * PATCH /api/regulatory/compliance/checklist/:id/status
   */
  async updateChecklistStatus(req: Request, res: Response): Promise<void> {
    try {
      const { id } = req.params;
      const { status, notes } = req.body;
      const userId = (req as any).user.id;

      if (!status) {
        res.status(400).json({
          success: false,
          message: 'Status is required'
        });
        return;
      }

      await ComplianceService.updateChecklistStatus(
        id,
        status,
        status === ComplianceStatus.COMPLETED ? userId : undefined,
        notes
      );

      res.status(200).json({
        success: true,
        message: 'Checklist item status updated successfully'
      });
    } catch (error) {
      logger.error('Error in updateChecklistStatus', error);
      res.status(500).json({
        success: false,
        message: 'Failed to update checklist item status',
        error: (error as Error).message
      });
    }
  }

  /**
   * Get overdue checklist items
   * GET /api/regulatory/compliance/checklist/overdue
   */
  async getOverdueItems(req: Request, res: Response): Promise<void> {
    try {
      const items = await ComplianceService.getOverdueItems();

      res.status(200).json({
        success: true,
        data: items,
        count: items.length
      });
    } catch (error) {
      logger.error('Error in getOverdueItems', error);
      res.status(500).json({
        success: false,
        message: 'Failed to retrieve overdue items',
        error: (error as Error).message
      });
    }
  }

  /**
   * Get all regulatory alerts
   * GET /api/regulatory/alerts
   */
  async getAllAlerts(req: Request, res: Response): Promise<void> {
    try {
      const { alertType, severity, isAcknowledged } = req.query;

      const alerts = await ComplianceService.getAllAlerts({
        alertType: alertType as any,
        severity: severity as any,
        isAcknowledged: isAcknowledged === 'true' ? true : isAcknowledged === 'false' ? false : undefined
      });

      res.status(200).json({
        success: true,
        data: alerts,
        count: alerts.length
      });
    } catch (error) {
      logger.error('Error in getAllAlerts', error);
      res.status(500).json({
        success: false,
        message: 'Failed to retrieve alerts',
        error: (error as Error).message
      });
    }
  }

  /**
   * Acknowledge alert
   * PATCH /api/regulatory/alerts/:id/acknowledge
   */
  async acknowledgeAlert(req: Request, res: Response): Promise<void> {
    try {
      const { id } = req.params;
      const { resolutionNotes } = req.body;
      const userId = (req as any).user.id;

      await ComplianceService.acknowledgeAlert(id, userId, resolutionNotes);

      res.status(200).json({
        success: true,
        message: 'Alert acknowledged successfully'
      });
    } catch (error) {
      logger.error('Error in acknowledgeAlert', error);
      res.status(500).json({
        success: false,
        message: 'Failed to acknowledge alert',
        error: (error as Error).message
      });
    }
  }

  /**
   * Get compliance dashboard
   * GET /api/regulatory/compliance/dashboard
   */
  async getComplianceDashboard(req: Request, res: Response): Promise<void> {
    try {
      const dashboard = await ComplianceService.getComplianceDashboard();

      res.status(200).json({
        success: true,
        data: dashboard
      });
    } catch (error) {
      logger.error('Error in getComplianceDashboard', error);
      res.status(500).json({
        success: false,
        message: 'Failed to retrieve compliance dashboard',
        error: (error as Error).message
      });
    }
  }

  /**
   * Create recurring checklists
   * POST /api/regulatory/compliance/checklist/recurring
   */
  async createRecurringChecklists(req: Request, res: Response): Promise<void> {
    try {
      await ComplianceService.createRecurringChecklists();

      res.status(201).json({
        success: true,
        message: 'Recurring checklists created successfully'
      });
    } catch (error) {
      logger.error('Error in createRecurringChecklists', error);
      res.status(500).json({
        success: false,
        message: 'Failed to create recurring checklists',
        error: (error as Error).message
      });
    }
  }
}

export default new RegulatoryController();
