import { PrismaClient } from '@prisma/client';
import {
  ComplianceChecklist,
  ComplianceCategory,
  ComplianceFrequency,
  ComplianceStatus,
  CompliancePriority,
  RegulatoryAlert,
  AlertType,
  AlertSeverity
} from '../types/regulatory.types';
import { logger } from '../utils/logger';

const prisma = new PrismaClient();

export class ComplianceService {
  /**
   * Create compliance checklist item
   */
  async createChecklistItem(data: {
    title: string;
    description?: string;
    category: ComplianceCategory;
    frequency: ComplianceFrequency;
    dueDate: Date;
    priority?: CompliancePriority;
    responsiblePerson?: string;
  }): Promise<ComplianceChecklist> {
    try {
      logger.info('Creating compliance checklist item', data);

      await prisma.$executeRaw`
        INSERT INTO compliance_checklists (
          id, title, description, category, frequency, due_date, 
          priority, responsible_person, status
        )
        VALUES (
          NEWID(), ${data.title}, ${data.description || null}, ${data.category}, 
          ${data.frequency}, ${data.dueDate}, ${data.priority || CompliancePriority.MEDIUM},
          ${data.responsiblePerson || null}, ${ComplianceStatus.PENDING}
        )
      `;

      logger.info('Compliance checklist item created successfully');
      return data as any;
    } catch (error) {
      logger.error('Error creating compliance checklist item', error);
      throw error;
    }
  }

  /**
   * Get all checklist items
   */
  async getAllChecklistItems(filters?: {
    category?: ComplianceCategory;
    status?: ComplianceStatus;
    priority?: CompliancePriority;
    responsiblePerson?: string;
  }): Promise<ComplianceChecklist[]> {
    const items = await prisma.$queryRaw<ComplianceChecklist[]>`
      SELECT * FROM compliance_checklists
      WHERE 1=1
        ${filters?.category ? `AND category = ${filters.category}` : ''}
        ${filters?.status ? `AND status = ${filters.status}` : ''}
        ${filters?.priority ? `AND priority = ${filters.priority}` : ''}
        ${filters?.responsiblePerson ? `AND responsible_person = ${filters.responsiblePerson}` : ''}
      ORDER BY due_date ASC, priority DESC
    `;

    return items;
  }

  /**
   * Get overdue checklist items
   */
  async getOverdueItems(): Promise<ComplianceChecklist[]> {
    const now = new Date();
    
    const items = await prisma.$queryRaw<ComplianceChecklist[]>`
      SELECT * FROM compliance_checklists
      WHERE due_date < ${now}
        AND status != ${ComplianceStatus.COMPLETED}
      ORDER BY due_date ASC
    `;

    // Update status to OVERDUE
    for (const item of items) {
      await this.updateChecklistStatus(item.id, ComplianceStatus.OVERDUE);
    }

    return items;
  }

  /**
   * Update checklist item status
   */
  async updateChecklistStatus(
    id: string,
    status: ComplianceStatus,
    completedBy?: string,
    notes?: string
  ): Promise<void> {
    await prisma.$executeRaw`
      UPDATE compliance_checklists
      SET status = ${status},
          completed_at = ${status === ComplianceStatus.COMPLETED ? new Date() : null},
          completed_by = ${completedBy || null},
          notes = ${notes || null},
          updated_at = GETDATE()
      WHERE id = ${id}
    `;
  }

  /**
   * Create recurring checklist items
   */
  async createRecurringChecklists(): Promise<void> {
    try {
      logger.info('Creating recurring compliance checklists');

      const now = new Date();
      const currentYear = now.getFullYear();
      const currentMonth = now.getMonth();

      // CBN Monthly Returns
      const cbnMonthlyDue = new Date(currentYear, currentMonth + 1, 15); // 15th of next month
      await this.createChecklistItem({
        title: 'CBN Monthly Prudential Return',
        description: 'Submit monthly prudential return to CBN',
        category: ComplianceCategory.CBN,
        frequency: ComplianceFrequency.MONTHLY,
        dueDate: cbnMonthlyDue,
        priority: CompliancePriority.HIGH
      });

      // FIRS Monthly VAT
      const firsVATDue = new Date(currentYear, currentMonth + 1, 21); // 21st of next month
      await this.createChecklistItem({
        title: 'FIRS VAT Return',
        description: 'File monthly VAT return with FIRS',
        category: ComplianceCategory.FIRS,
        frequency: ComplianceFrequency.MONTHLY,
        dueDate: firsVATDue,
        priority: CompliancePriority.HIGH
      });

      // FIRS Monthly WHT
      const firsWHTDue = new Date(currentYear, currentMonth + 1, 21); // 21st of next month
      await this.createChecklistItem({
        title: 'FIRS WHT Schedule',
        description: 'Submit monthly withholding tax schedule to FIRS',
        category: ComplianceCategory.FIRS,
        frequency: ComplianceFrequency.MONTHLY,
        dueDate: firsWHTDue,
        priority: CompliancePriority.HIGH
      });

      // Quarterly IFRS 9 ECL Assessment
      if (currentMonth % 3 === 0) {
        const eclDue = new Date(currentYear, currentMonth + 1, 10);
        await this.createChecklistItem({
          title: 'IFRS 9 ECL Assessment',
          description: 'Perform quarterly Expected Credit Loss assessment',
          category: ComplianceCategory.IFRS,
          frequency: ComplianceFrequency.QUARTERLY,
          dueDate: eclDue,
          priority: CompliancePriority.CRITICAL
        });
      }

      // Annual Audit
      if (currentMonth === 0) { // January
        const auditDue = new Date(currentYear, 3, 30); // April 30
        await this.createChecklistItem({
          title: 'Annual Financial Audit',
          description: 'Complete annual financial audit and submit to regulators',
          category: ComplianceCategory.INTERNAL,
          frequency: ComplianceFrequency.ANNUALLY,
          dueDate: auditDue,
          priority: CompliancePriority.CRITICAL
        });
      }

      logger.info('Recurring compliance checklists created successfully');
    } catch (error) {
      logger.error('Error creating recurring checklists', error);
      throw error;
    }
  }

  /**
   * Get all regulatory alerts
   */
  async getAllAlerts(filters?: {
    alertType?: AlertType;
    severity?: AlertSeverity;
    isAcknowledged?: boolean;
  }): Promise<RegulatoryAlert[]> {
    const alerts = await prisma.$queryRaw<RegulatoryAlert[]>`
      SELECT * FROM regulatory_alerts
      WHERE 1=1
        ${filters?.alertType ? `AND alert_type = ${filters.alertType}` : ''}
        ${filters?.severity ? `AND severity = ${filters.severity}` : ''}
        ${filters?.isAcknowledged !== undefined ? `AND is_acknowledged = ${filters.isAcknowledged ? 1 : 0}` : ''}
      ORDER BY created_at DESC
    `;

    return alerts;
  }

  /**
   * Acknowledge alert
   */
  async acknowledgeAlert(
    id: string,
    acknowledgedBy: string,
    resolutionNotes?: string
  ): Promise<void> {
    await prisma.$executeRaw`
      UPDATE regulatory_alerts
      SET is_acknowledged = 1,
          acknowledged_by = ${acknowledgedBy},
          acknowledged_at = GETDATE(),
          resolution_notes = ${resolutionNotes || null},
          resolved_at = ${resolutionNotes ? new Date() : null}
      WHERE id = ${id}
    `;
  }

  /**
   * Create regulatory alert
   */
  async createAlert(data: {
    alertType: AlertType;
    severity: AlertSeverity;
    title: string;
    message: string;
    thresholdValue?: number;
    currentValue?: number;
    entityType?: string;
    entityId?: string;
  }): Promise<RegulatoryAlert> {
    await prisma.$executeRaw`
      INSERT INTO regulatory_alerts (
        id, alert_type, severity, title, message, 
        threshold_value, current_value, entity_type, entity_id, is_acknowledged
      )
      VALUES (
        NEWID(), ${data.alertType}, ${data.severity}, ${data.title}, ${data.message},
        ${data.thresholdValue || null}, ${data.currentValue || null}, 
        ${data.entityType || null}, ${data.entityId || null}, 0
      )
    `;

    return data as any;
  }

  /**
   * Check compliance deadlines and create alerts
   */
  async checkComplianceDeadlines(): Promise<void> {
    try {
      logger.info('Checking compliance deadlines');

      const now = new Date();
      const threeDaysFromNow = new Date(now.getTime() + 3 * 24 * 60 * 60 * 1000);

      // Get items due within 3 days
      const upcomingItems = await prisma.$queryRaw<ComplianceChecklist[]>`
        SELECT * FROM compliance_checklists
        WHERE due_date BETWEEN ${now} AND ${threeDaysFromNow}
          AND status = ${ComplianceStatus.PENDING}
      `;

      for (const item of upcomingItems) {
        const daysUntilDue = Math.ceil((item.dueDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
        
        await this.createAlert({
          alertType: AlertType.COMPLIANCE_DEADLINE,
          severity: daysUntilDue <= 1 ? AlertSeverity.CRITICAL : AlertSeverity.WARNING,
          title: `Compliance Deadline Approaching: ${item.title}`,
          message: `${item.title} is due in ${daysUntilDue} day(s). Please ensure timely completion.`,
          entityType: 'COMPLIANCE_CHECKLIST',
          entityId: item.id
        });
      }

      logger.info('Compliance deadline check completed');
    } catch (error) {
      logger.error('Error checking compliance deadlines', error);
      throw error;
    }
  }

  /**
   * Get compliance dashboard summary
   */
  async getComplianceDashboard(): Promise<{
    totalItems: number;
    pendingItems: number;
    overdueItems: number;
    completedThisMonth: number;
    criticalAlerts: number;
    upcomingDeadlines: ComplianceChecklist[];
  }> {
    const now = new Date();
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    const sevenDaysFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

    const [totalItems] = await prisma.$queryRaw<[{ count: number }]>`
      SELECT COUNT(*) as count FROM compliance_checklists
    `;

    const [pendingItems] = await prisma.$queryRaw<[{ count: number }]>`
      SELECT COUNT(*) as count FROM compliance_checklists
      WHERE status = ${ComplianceStatus.PENDING}
    `;

    const [overdueItems] = await prisma.$queryRaw<[{ count: number }]>`
      SELECT COUNT(*) as count FROM compliance_checklists
      WHERE status = ${ComplianceStatus.OVERDUE}
    `;

    const [completedThisMonth] = await prisma.$queryRaw<[{ count: number }]>`
      SELECT COUNT(*) as count FROM compliance_checklists
      WHERE status = ${ComplianceStatus.COMPLETED}
        AND completed_at >= ${startOfMonth}
    `;

    const [criticalAlerts] = await prisma.$queryRaw<[{ count: number }]>`
      SELECT COUNT(*) as count FROM regulatory_alerts
      WHERE severity = ${AlertSeverity.CRITICAL}
        AND is_acknowledged = 0
    `;

    const upcomingDeadlines = await prisma.$queryRaw<ComplianceChecklist[]>`
      SELECT * FROM compliance_checklists
      WHERE due_date BETWEEN ${now} AND ${sevenDaysFromNow}
        AND status != ${ComplianceStatus.COMPLETED}
      ORDER BY due_date ASC
    `;

    return {
      totalItems: totalItems.count,
      pendingItems: pendingItems.count,
      overdueItems: overdueItems.count,
      completedThisMonth: completedThisMonth.count,
      criticalAlerts: criticalAlerts.count,
      upcomingDeadlines
    };
  }
}

export default new ComplianceService();
