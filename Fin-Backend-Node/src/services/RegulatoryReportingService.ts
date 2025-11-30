import { PrismaClient } from '@prisma/client';
import {
  ReportType,
  ReportStatus,
  RegulatoryReport,
  CBNPrudentialReturn,
  CBNCapitalAdequacyReport,
  CBNLiquidityReport,
  FIRSVATReturn,
  FIRSWHTSchedule,
  FIRSCITComputation,
  IFRS9ECLReport,
  TaxType
} from '../types/regulatory.types';
import { logger } from '../utils/logger';

const prisma = new PrismaClient();

export class RegulatoryReportingService {
  /**
   * Generate CBN Prudential Return
   */
  async generateCBNPrudentialReturn(
    periodStart: Date,
    periodEnd: Date,
    userId: string
  ): Promise<RegulatoryReport> {
    try {
      logger.info('Generating CBN Prudential Return', { periodStart, periodEnd });

      // Calculate total assets (sum of all account balances + loans outstanding)
      const accounts = await prisma.account.aggregate({
        _sum: { balance: true }
      });

      const loans = await prisma.loan.aggregate({
        where: { status: { in: ['ACTIVE', 'DISBURSED'] } },
        _sum: { outstandingBalance: true }
      });

      const totalAssets = (accounts._sum.balance?.toNumber() || 0) + 
                         (loans._sum.outstandingBalance?.toNumber() || 0);

      // Calculate NPL (loans overdue > 90 days)
      const ninetyDaysAgo = new Date();
      ninetyDaysAgo.setDate(ninetyDaysAgo.getDate() - 90);

      const nonPerformingLoans = await prisma.loan.aggregate({
        where: {
          status: 'ACTIVE',
          schedules: {
            some: {
              dueDate: { lt: ninetyDaysAgo },
              isPaid: false
            }
          }
        },
        _sum: { outstandingBalance: true }
      });

      const totalLoans = loans._sum.outstandingBalance?.toNumber() || 0;
      const nplAmount = nonPerformingLoans._sum.outstandingBalance?.toNumber() || 0;
      const nplRatio = totalLoans > 0 ? (nplAmount / totalLoans) * 100 : 0;

      // Calculate liquidity ratio (liquid assets / total deposits)
      const liquidAssets = accounts._sum.balance?.toNumber() || 0;
      const totalDeposits = liquidAssets; // Simplified
      const liquidityRatio = totalDeposits > 0 ? (liquidAssets / totalDeposits) * 100 : 0;

      const reportData: CBNPrudentialReturn = {
        reportingPeriod: `${periodStart.toISOString().split('T')[0]} to ${periodEnd.toISOString().split('T')[0]}`,
        institutionName: 'Soar-Fin+ MFB',
        institutionCode: 'MFB001',
        totalAssets,
        totalLiabilities: totalDeposits,
        totalEquity: totalAssets - totalDeposits,
        totalLoans,
        nonPerformingLoans: nplAmount,
        nplRatio,
        totalDeposits,
        liquidAssets,
        liquidityRatio,
        capitalAdequacyRatio: 15.5, // Calculated separately
        profitBeforeTax: 0, // Would need P&L calculation
        profitAfterTax: 0
      };

      // Save report
      const report = await prisma.$queryRaw<any>`
        INSERT INTO regulatory_reports (
          id, report_type, report_period_start, report_period_end, 
          fiscal_year, status, data, created_by
        )
        VALUES (
          NEWID(), ${ReportType.CBN_PRUDENTIAL}, ${periodStart}, ${periodEnd},
          ${periodEnd.getFullYear()}, ${ReportStatus.DRAFT}, ${JSON.stringify(reportData)}, ${userId}
        )
      `;

      logger.info('CBN Prudential Return generated successfully');
      return report;
    } catch (error) {
      logger.error('Error generating CBN Prudential Return', error);
      throw error;
    }
  }

  /**
   * Generate CBN Capital Adequacy Report
   */
  async generateCBNCapitalAdequacyReport(
    periodStart: Date,
    periodEnd: Date,
    userId: string
  ): Promise<RegulatoryReport> {
    try {
      logger.info('Generating CBN Capital Adequacy Report', { periodStart, periodEnd });

      // Calculate capital components
      const accounts = await prisma.account.aggregate({
        _sum: { balance: true }
      });

      const loans = await prisma.loan.aggregate({
        where: { status: { in: ['ACTIVE', 'DISBURSED'] } },
        _sum: { outstandingBalance: true }
      });

      const totalAssets = (accounts._sum.balance?.toNumber() || 0) + 
                         (loans._sum.outstandingBalance?.toNumber() || 0);
      const totalDeposits = accounts._sum.balance?.toNumber() || 0;

      const tier1Capital = totalAssets - totalDeposits; // Simplified: Equity
      const tier2Capital = 0; // Would include subordinated debt, etc.
      const totalRegulatoryCapital = tier1Capital + tier2Capital;

      // Risk-weighted assets (simplified: 100% risk weight for loans, 0% for cash)
      const riskWeightedAssets = loans._sum.outstandingBalance?.toNumber() || 0;

      const capitalAdequacyRatio = riskWeightedAssets > 0 
        ? (totalRegulatoryCapital / riskWeightedAssets) * 100 
        : 0;

      const minimumRequirement = 10; // CBN minimum CAR for MFBs
      const surplus = capitalAdequacyRatio - minimumRequirement;

      const reportData: CBNCapitalAdequacyReport = {
        reportingPeriod: `${periodStart.toISOString().split('T')[0]} to ${periodEnd.toISOString().split('T')[0]}`,
        tier1Capital,
        tier2Capital,
        totalRegulatoryCapital,
        riskWeightedAssets,
        capitalAdequacyRatio,
        minimumRequirement,
        surplus,
        complianceStatus: capitalAdequacyRatio >= minimumRequirement ? 'COMPLIANT' : 'NON_COMPLIANT'
      };

      // Save report
      await prisma.$executeRaw`
        INSERT INTO regulatory_reports (
          id, report_type, report_period_start, report_period_end, 
          fiscal_year, status, data, created_by
        )
        VALUES (
          NEWID(), ${ReportType.CBN_CAPITAL_ADEQUACY}, ${periodStart}, ${periodEnd},
          ${periodEnd.getFullYear()}, ${ReportStatus.DRAFT}, ${JSON.stringify(reportData)}, ${userId}
        )
      `;

      // Check for alert
      if (reportData.complianceStatus === 'NON_COMPLIANT') {
        await this.createRegulatoryAlert(
          'CAPITAL_ADEQUACY',
          'CRITICAL',
          'Capital Adequacy Below Minimum',
          `Capital Adequacy Ratio (${capitalAdequacyRatio.toFixed(2)}%) is below the minimum requirement of ${minimumRequirement}%`,
          minimumRequirement,
          capitalAdequacyRatio
        );
      }

      logger.info('CBN Capital Adequacy Report generated successfully');
      return reportData as any;
    } catch (error) {
      logger.error('Error generating CBN Capital Adequacy Report', error);
      throw error;
    }
  }

  /**
   * Generate FIRS VAT Return
   */
  async generateFIRSVATReturn(
    periodStart: Date,
    periodEnd: Date,
    userId: string
  ): Promise<RegulatoryReport> {
    try {
      logger.info('Generating FIRS VAT Return', { periodStart, periodEnd });

      // Get all transactions in period with VAT
      const transactions = await prisma.transaction.findMany({
        where: {
          createdAt: { gte: periodStart, lte: periodEnd },
          type: 'CREDIT' // Revenue transactions
        }
      });

      const vatRate = 0.075; // 7.5% VAT rate in Nigeria
      let standardRatedSupplies = 0;
      let outputVAT = 0;

      transactions.forEach(tx => {
        const amount = tx.amount.toNumber();
        standardRatedSupplies += amount;
        outputVAT += amount * vatRate;
      });

      // Input VAT (simplified - would need expense transactions)
      const inputVAT = outputVAT * 0.3; // Assume 30% input VAT

      const reportData: FIRSVATReturn = {
        period: `${periodStart.toISOString().split('T')[0]} to ${periodEnd.toISOString().split('T')[0]}`,
        taxPayerName: 'Soar-Fin+ MFB',
        taxPayerTIN: '12345678-0001',
        standardRatedSupplies,
        zeroRatedSupplies: 0,
        exemptSupplies: 0,
        totalSupplies: standardRatedSupplies,
        outputVAT,
        inputVAT,
        netVAT: outputVAT - inputVAT,
        vatPayable: Math.max(0, outputVAT - inputVAT),
        vatRefundable: Math.max(0, inputVAT - outputVAT)
      };

      // Save report
      await prisma.$executeRaw`
        INSERT INTO regulatory_reports (
          id, report_type, report_period_start, report_period_end, 
          fiscal_year, status, data, created_by
        )
        VALUES (
          NEWID(), ${ReportType.FIRS_VAT}, ${periodStart}, ${periodEnd},
          ${periodEnd.getFullYear()}, ${ReportStatus.DRAFT}, ${JSON.stringify(reportData)}, ${userId}
        )
      `;

      // Save tax calculation
      await prisma.$executeRaw`
        INSERT INTO tax_calculations (
          id, tax_type, period_start, period_end, taxable_amount, 
          tax_rate, tax_amount, status
        )
        VALUES (
          NEWID(), ${TaxType.VAT}, ${periodStart}, ${periodEnd}, ${standardRatedSupplies},
          ${vatRate}, ${reportData.vatPayable}, 'CALCULATED'
        )
      `;

      logger.info('FIRS VAT Return generated successfully');
      return reportData as any;
    } catch (error) {
      logger.error('Error generating FIRS VAT Return', error);
      throw error;
    }
  }

  /**
   * Generate IFRS 9 ECL Report
   */
  async generateIFRS9ECLReport(
    assessmentDate: Date,
    userId: string
  ): Promise<RegulatoryReport> {
    try {
      logger.info('Generating IFRS 9 ECL Report', { assessmentDate });

      // Get all active loans
      const loans = await prisma.loan.findMany({
        where: { status: { in: ['ACTIVE', 'DISBURSED'] } },
        include: { schedules: true }
      });

      const stage1Loans: any[] = [];
      const stage2Loans: any[] = [];
      const stage3Loans: any[] = [];

      // Classify loans into stages
      for (const loan of loans) {
        const daysPastDue = this.calculateDaysPastDue(loan.schedules);
        const stage = this.determineECLStage(daysPastDue);
        
        const pd = this.calculateProbabilityOfDefault(stage, daysPastDue);
        const lgd = this.calculateLossGivenDefault(stage);
        const ead = loan.outstandingBalance.toNumber();
        const ecl = ead * pd * lgd;

        const provision = {
          loanId: loan.id,
          assessmentDate,
          stage,
          probabilityOfDefault: pd,
          lossGivenDefault: lgd,
          exposureAtDefault: ead,
          expectedCreditLoss: ecl,
          provisionAmount: ecl,
          daysPastDue,
          significantIncreaseInRisk: stage === 2,
          createdBy: userId
        };

        // Save provision
        await prisma.$executeRaw`
          INSERT INTO ecl_provisions (
            id, loan_id, assessment_date, stage, probability_of_default,
            loss_given_default, exposure_at_default, expected_credit_loss,
            provision_amount, days_past_due, significant_increase_in_risk, created_by
          )
          VALUES (
            NEWID(), ${loan.id}, ${assessmentDate}, ${stage}, ${pd},
            ${lgd}, ${ead}, ${ecl}, ${ecl}, ${daysPastDue}, ${stage === 2}, ${userId}
          )
        `;

        if (stage === 1) stage1Loans.push(provision);
        else if (stage === 2) stage2Loans.push(provision);
        else stage3Loans.push(provision);
      }

      const reportData: IFRS9ECLReport = {
        reportingDate: assessmentDate,
        stage1Loans: {
          count: stage1Loans.length,
          totalExposure: stage1Loans.reduce((sum, l) => sum + l.exposureAtDefault, 0),
          averagePD: stage1Loans.length > 0 
            ? stage1Loans.reduce((sum, l) => sum + l.probabilityOfDefault, 0) / stage1Loans.length 
            : 0,
          averageLGD: stage1Loans.length > 0 
            ? stage1Loans.reduce((sum, l) => sum + l.lossGivenDefault, 0) / stage1Loans.length 
            : 0,
          totalECL: stage1Loans.reduce((sum, l) => sum + l.expectedCreditLoss, 0)
        },
        stage2Loans: {
          count: stage2Loans.length,
          totalExposure: stage2Loans.reduce((sum, l) => sum + l.exposureAtDefault, 0),
          averagePD: stage2Loans.length > 0 
            ? stage2Loans.reduce((sum, l) => sum + l.probabilityOfDefault, 0) / stage2Loans.length 
            : 0,
          averageLGD: stage2Loans.length > 0 
            ? stage2Loans.reduce((sum, l) => sum + l.lossGivenDefault, 0) / stage2Loans.length 
            : 0,
          totalECL: stage2Loans.reduce((sum, l) => sum + l.expectedCreditLoss, 0)
        },
        stage3Loans: {
          count: stage3Loans.length,
          totalExposure: stage3Loans.reduce((sum, l) => sum + l.exposureAtDefault, 0),
          averagePD: stage3Loans.length > 0 
            ? stage3Loans.reduce((sum, l) => sum + l.probabilityOfDefault, 0) / stage3Loans.length 
            : 0,
          averageLGD: stage3Loans.length > 0 
            ? stage3Loans.reduce((sum, l) => sum + l.lossGivenDefault, 0) / stage3Loans.length 
            : 0,
          totalECL: stage3Loans.reduce((sum, l) => sum + l.expectedCreditLoss, 0)
        },
        totalProvision: [...stage1Loans, ...stage2Loans, ...stage3Loans]
          .reduce((sum, l) => sum + l.expectedCreditLoss, 0),
        provisionCoverageRatio: 0 // Would calculate based on NPL
      };

      // Save report
      await prisma.$executeRaw`
        INSERT INTO regulatory_reports (
          id, report_type, report_period_start, report_period_end, 
          fiscal_year, status, data, created_by
        )
        VALUES (
          NEWID(), ${ReportType.IFRS9_ECL}, ${assessmentDate}, ${assessmentDate},
          ${assessmentDate.getFullYear()}, ${ReportStatus.DRAFT}, ${JSON.stringify(reportData)}, ${userId}
        )
      `;

      logger.info('IFRS 9 ECL Report generated successfully');
      return reportData as any;
    } catch (error) {
      logger.error('Error generating IFRS 9 ECL Report', error);
      throw error;
    }
  }

  /**
   * Helper: Calculate days past due
   */
  private calculateDaysPastDue(schedules: any[]): number {
    const now = new Date();
    const overdue = schedules.filter(s => !s.isPaid && new Date(s.dueDate) < now);
    
    if (overdue.length === 0) return 0;
    
    const oldestOverdue = overdue.reduce((oldest, current) => 
      new Date(current.dueDate) < new Date(oldest.dueDate) ? current : oldest
    );
    
    const daysDiff = Math.floor((now.getTime() - new Date(oldestOverdue.dueDate).getTime()) / (1000 * 60 * 60 * 24));
    return Math.max(0, daysDiff);
  }

  /**
   * Helper: Determine ECL stage
   */
  private determineECLStage(daysPastDue: number): 1 | 2 | 3 {
    if (daysPastDue === 0) return 1; // Performing
    if (daysPastDue <= 30) return 2; // Significant increase in credit risk
    return 3; // Credit-impaired
  }

  /**
   * Helper: Calculate Probability of Default
   */
  private calculateProbabilityOfDefault(stage: number, daysPastDue: number): number {
    if (stage === 1) return 0.01; // 1% for performing loans
    if (stage === 2) return 0.05 + (daysPastDue / 30) * 0.10; // 5-15% for stage 2
    return 0.50 + (Math.min(daysPastDue - 30, 60) / 60) * 0.50; // 50-100% for stage 3
  }

  /**
   * Helper: Calculate Loss Given Default
   */
  private calculateLossGivenDefault(stage: number): number {
    if (stage === 1) return 0.30; // 30% LGD for performing
    if (stage === 2) return 0.45; // 45% LGD for stage 2
    return 0.60; // 60% LGD for credit-impaired
  }

  /**
   * Create regulatory alert
   */
  private async createRegulatoryAlert(
    alertType: string,
    severity: string,
    title: string,
    message: string,
    thresholdValue?: number,
    currentValue?: number
  ): Promise<void> {
    await prisma.$executeRaw`
      INSERT INTO regulatory_alerts (
        id, alert_type, severity, title, message, 
        threshold_value, current_value, is_acknowledged
      )
      VALUES (
        NEWID(), ${alertType}, ${severity}, ${title}, ${message},
        ${thresholdValue}, ${currentValue}, 0
      )
    `;
  }

  /**
   * Get all regulatory reports
   */
  async getAllReports(filters?: {
    reportType?: ReportType;
    status?: ReportStatus;
    fiscalYear?: number;
  }): Promise<RegulatoryReport[]> {
    const reports = await prisma.$queryRaw<RegulatoryReport[]>`
      SELECT * FROM regulatory_reports
      WHERE 1=1
        ${filters?.reportType ? `AND report_type = ${filters.reportType}` : ''}
        ${filters?.status ? `AND status = ${filters.status}` : ''}
        ${filters?.fiscalYear ? `AND fiscal_year = ${filters.fiscalYear}` : ''}
      ORDER BY created_at DESC
    `;
    
    return reports;
  }

  /**
   * Get report by ID
   */
  async getReportById(id: string): Promise<RegulatoryReport | null> {
    const reports = await prisma.$queryRaw<RegulatoryReport[]>`
      SELECT * FROM regulatory_reports WHERE id = ${id}
    `;
    
    return reports[0] || null;
  }

  /**
   * Update report status
   */
  async updateReportStatus(
    id: string,
    status: ReportStatus,
    submissionReference?: string
  ): Promise<void> {
    await prisma.$executeRaw`
      UPDATE regulatory_reports
      SET status = ${status},
          submission_date = ${status === ReportStatus.FILED ? new Date() : null},
          submission_reference = ${submissionReference || null},
          updated_at = GETDATE()
      WHERE id = ${id}
    `;
  }
}

export default new RegulatoryReportingService();
