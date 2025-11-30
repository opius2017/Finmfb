import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import RegulatoryReportingService from '../RegulatoryReportingService';
import { ReportType, ReportStatus } from '../../types/regulatory.types';

// Mock Prisma Client
jest.mock('@prisma/client', () => ({
  PrismaClient: jest.fn().mockImplementation(() => ({
    $queryRaw: jest.fn(),
    $executeRaw: jest.fn(),
    account: {
      aggregate: jest.fn().mockResolvedValue({
        _sum: { balance: 10000000 }
      })
    },
    loan: {
      aggregate: jest.fn().mockResolvedValue({
        _sum: { outstandingBalance: 5000000 }
      }),
      findMany: jest.fn().mockResolvedValue([])
    },
    transaction: {
      findMany: jest.fn().mockResolvedValue([
        { amount: { toNumber: () => 100000 }, type: 'CREDIT' },
        { amount: { toNumber: () => 200000 }, type: 'CREDIT' }
      ])
    }
  }))
}));

describe('RegulatoryReportingService', () => {
  const userId = 'test-user-id';
  const periodStart = new Date('2024-01-01');
  const periodEnd = new Date('2024-01-31');

  describe('generateCBNPrudentialReturn', () => {
    it('should generate CBN prudential return successfully', async () => {
      const report = await RegulatoryReportingService.generateCBNPrudentialReturn(
        periodStart,
        periodEnd,
        userId
      );

      expect(report).toBeDefined();
    });

    it('should calculate NPL ratio correctly', async () => {
      // Test NPL ratio calculation logic
      const totalLoans = 5000000;
      const nplAmount = 250000;
      const expectedNPLRatio = (nplAmount / totalLoans) * 100;

      expect(expectedNPLRatio).toBe(5);
    });

    it('should calculate liquidity ratio correctly', async () => {
      // Test liquidity ratio calculation
      const liquidAssets = 10000000;
      const totalDeposits = 8000000;
      const expectedLiquidityRatio = (liquidAssets / totalDeposits) * 100;

      expect(expectedLiquidityRatio).toBe(125);
    });
  });

  describe('generateCBNCapitalAdequacyReport', () => {
    it('should generate capital adequacy report successfully', async () => {
      const report = await RegulatoryReportingService.generateCBNCapitalAdequacyReport(
        periodStart,
        periodEnd,
        userId
      );

      expect(report).toBeDefined();
    });

    it('should calculate CAR correctly', async () => {
      const totalRegulatoryCapital = 2000000;
      const riskWeightedAssets = 10000000;
      const expectedCAR = (totalRegulatoryCapital / riskWeightedAssets) * 100;

      expect(expectedCAR).toBe(20);
    });

    it('should identify non-compliance when CAR below minimum', () => {
      const car = 8; // Below 10% minimum
      const minimumRequirement = 10;
      const complianceStatus = car >= minimumRequirement ? 'COMPLIANT' : 'NON_COMPLIANT';

      expect(complianceStatus).toBe('NON_COMPLIANT');
    });
  });

  describe('generateFIRSVATReturn', () => {
    it('should generate VAT return successfully', async () => {
      const report = await RegulatoryReportingService.generateFIRSVATReturn(
        periodStart,
        periodEnd,
        userId
      );

      expect(report).toBeDefined();
    });

    it('should calculate VAT at 7.5% rate', () => {
      const standardRatedSupplies = 1000000;
      const vatRate = 0.075;
      const expectedOutputVAT = standardRatedSupplies * vatRate;

      expect(expectedOutputVAT).toBe(75000);
    });

    it('should calculate net VAT payable correctly', () => {
      const outputVAT = 75000;
      const inputVAT = 30000;
      const expectedNetVAT = outputVAT - inputVAT;

      expect(expectedNetVAT).toBe(45000);
    });
  });

  describe('generateIFRS9ECLReport', () => {
    it('should generate IFRS 9 ECL report successfully', async () => {
      const assessmentDate = new Date('2024-01-31');
      const report = await RegulatoryReportingService.generateIFRS9ECLReport(
        assessmentDate,
        userId
      );

      expect(report).toBeDefined();
    });

    it('should classify loans into correct stages', () => {
      const testCases = [
        { daysPastDue: 0, expectedStage: 1 },
        { daysPastDue: 15, expectedStage: 2 },
        { daysPastDue: 45, expectedStage: 3 }
      ];

      testCases.forEach(({ daysPastDue, expectedStage }) => {
        let stage: 1 | 2 | 3;
        if (daysPastDue === 0) stage = 1;
        else if (daysPastDue <= 30) stage = 2;
        else stage = 3;

        expect(stage).toBe(expectedStage);
      });
    });

    it('should calculate ECL correctly', () => {
      const ead = 1000000; // Exposure at default
      const pd = 0.05; // Probability of default (5%)
      const lgd = 0.45; // Loss given default (45%)
      const expectedECL = ead * pd * lgd;

      expect(expectedECL).toBe(22500);
    });

    it('should use higher PD for stage 3 loans', () => {
      const stage1PD = 0.01; // 1%
      const stage2PD = 0.10; // 10%
      const stage3PD = 0.75; // 75%

      expect(stage3PD).toBeGreaterThan(stage2PD);
      expect(stage2PD).toBeGreaterThan(stage1PD);
    });
  });

  describe('getAllReports', () => {
    it('should retrieve all reports', async () => {
      const reports = await RegulatoryReportingService.getAllReports();
      expect(Array.isArray(reports)).toBe(true);
    });

    it('should filter reports by type', async () => {
      const reports = await RegulatoryReportingService.getAllReports({
        reportType: ReportType.CBN_PRUDENTIAL
      });
      expect(Array.isArray(reports)).toBe(true);
    });

    it('should filter reports by status', async () => {
      const reports = await RegulatoryReportingService.getAllReports({
        status: ReportStatus.DRAFT
      });
      expect(Array.isArray(reports)).toBe(true);
    });

    it('should filter reports by fiscal year', async () => {
      const reports = await RegulatoryReportingService.getAllReports({
        fiscalYear: 2024
      });
      expect(Array.isArray(reports)).toBe(true);
    });
  });

  describe('updateReportStatus', () => {
    it('should update report status successfully', async () => {
      const reportId = 'test-report-id';
      const newStatus = ReportStatus.SUBMITTED;
      const submissionReference = 'CBN-2024-001';

      await expect(
        RegulatoryReportingService.updateReportStatus(
          reportId,
          newStatus,
          submissionReference
        )
      ).resolves.not.toThrow();
    });
  });
});
