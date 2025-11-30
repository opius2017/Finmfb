import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { LoanService } from '@services/LoanService';

const loanApplicationSchema = z.object({
  memberId: z.string().uuid(),
  loanProductId: z.string().uuid(),
  requestedAmount: z.number().positive(),
  termMonths: z.number().int().positive(),
  purpose: z.string().min(10),
  collateralDescription: z.string().optional(),
  guarantors: z.array(z.object({
    memberId: z.string().uuid(),
    guaranteedAmount: z.number().positive(),
  })).min(1),
});

const loanApprovalSchema = z.object({
  approvedAmount: z.number().positive(),
  comment: z.string().optional(),
});

const loanDisbursementSchema = z.object({
  disbursementAccountId: z.string().uuid(),
  notes: z.string().optional(),
});

const loanPaymentSchema = z.object({
  amount: z.number().positive(),
  paymentMethod: z.enum(['CASH', 'TRANSFER', 'CHEQUE', 'CARD']),
  reference: z.string().optional(),
  notes: z.string().optional(),
});

export class LoanController {
  private loanService = new LoanService();

  /**
   * @swagger
   * /api/v1/loans/apply:
   *   post:
   *     summary: Submit loan application
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - memberId
   *               - loanProductId
   *               - requestedAmount
   *               - termMonths
   *               - purpose
   *               - guarantors
   *             properties:
   *               memberId:
   *                 type: string
   *               loanProductId:
   *                 type: string
   *               requestedAmount:
   *                 type: number
   *               termMonths:
   *                 type: integer
   *               purpose:
   *                 type: string
   *               collateralDescription:
   *                 type: string
   *               guarantors:
   *                 type: array
   *                 items:
   *                   type: object
   *                   properties:
   *                     memberId:
   *                       type: string
   *                     guaranteedAmount:
   *                       type: number
   *     responses:
   *       201:
   *         description: Loan application submitted successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async submitApplication(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = loanApplicationSchema.parse(req.body);
      const userId = req.user?.id;

      const loan = await this.loanService.submitLoanApplication({
        ...validatedData,
        userId: userId!,
      });

      res.status(201).json({
        success: true,
        data: loan,
        message: 'Loan application submitted successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/eligibility/{memberId}:
   *   get:
   *     summary: Check loan eligibility
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: memberId
   *         required: true
   *         schema:
   *           type: string
   *       - in: query
   *         name: requestedAmount
   *         required: true
   *         schema:
   *           type: number
   *     responses:
   *       200:
   *         description: Eligibility check completed
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async checkEligibility(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { memberId } = req.params;
      const requestedAmount = parseFloat(req.query.requestedAmount as string);

      const eligibility = await this.loanService.checkLoanEligibility(memberId, requestedAmount);

      res.json({
        success: true,
        data: eligibility,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/{id}/approve:
   *   post:
   *     summary: Approve loan application
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - approvedAmount
   *             properties:
   *               approvedAmount:
   *                 type: number
   *               comment:
   *                 type: string
   *     responses:
   *       200:
   *         description: Loan approved successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async approveLoan(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = loanApprovalSchema.parse(req.body);
      const approverId = req.user?.id;

      const loan = await this.loanService.approveLoan({
        loanId: id,
        approvedAmount: validatedData.approvedAmount,
        approverId: approverId!,
        comment: validatedData.comment,
      });

      res.json({
        success: true,
        data: loan,
        message: 'Loan approved successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/{id}/reject:
   *   post:
   *     summary: Reject loan application
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - reason
   *             properties:
   *               reason:
   *                 type: string
   *     responses:
   *       200:
   *         description: Loan rejected successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async rejectLoan(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { reason } = req.body;
      const rejectedBy = req.user?.id;

      const loan = await this.loanService.rejectLoan(id, reason, rejectedBy!);

      res.json({
        success: true,
        data: loan,
        message: 'Loan rejected successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/{id}/disburse:
   *   post:
   *     summary: Disburse approved loan
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - disbursementAccountId
   *             properties:
   *               disbursementAccountId:
   *                 type: string
   *               notes:
   *                 type: string
   *     responses:
   *       200:
   *         description: Loan disbursed successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async disburseLoan(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = loanDisbursementSchema.parse(req.body);
      const disbursedBy = req.user?.id;

      const loan = await this.loanService.disburseLoan({
        loanId: id,
        disbursementAccountId: validatedData.disbursementAccountId,
        disbursedBy: disbursedBy!,
        notes: validatedData.notes,
      });

      res.json({
        success: true,
        data: loan,
        message: 'Loan disbursed successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/{id}/payments:
   *   post:
   *     summary: Record loan payment
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - amount
   *               - paymentMethod
   *             properties:
   *               amount:
   *                 type: number
   *               paymentMethod:
   *                 type: string
   *                 enum: [CASH, TRANSFER, CHEQUE, CARD]
   *               reference:
   *                 type: string
   *               notes:
   *                 type: string
   *     responses:
   *       201:
   *         description: Payment recorded successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async recordPayment(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = loanPaymentSchema.parse(req.body);
      const paidBy = req.user?.id;

      const result = await this.loanService.recordLoanPayment({
        loanId: id,
        amount: validatedData.amount,
        paymentMethod: validatedData.paymentMethod,
        reference: validatedData.reference,
        notes: validatedData.notes,
        paidBy: paidBy!,
      });

      res.status(201).json({
        success: true,
        data: result,
        message: 'Payment recorded successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/{id}:
   *   get:
   *     summary: Get loan details
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Loan details retrieved successfully
   *       404:
   *         description: Loan not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getLoanDetails(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const loan = await this.loanService.getLoanDetails(id);

      res.json({
        success: true,
        data: loan,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/portfolio/summary:
   *   get:
   *     summary: Get loan portfolio summary
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Portfolio summary retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getPortfolioSummary(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const summary = await this.loanService.getLoanPortfolioSummary();

      res.json({
        success: true,
        data: summary,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/loans/reports/aging:
   *   get:
   *     summary: Get loan aging report
   *     tags: [Loans]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Aging report retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getAgingReport(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const report = await this.loanService.getLoanAgingReport();

      res.json({
        success: true,
        data: report,
      });
    } catch (error) {
      next(error);
    }
  }
}

export default LoanController;
