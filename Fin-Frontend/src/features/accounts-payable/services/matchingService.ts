import {
  ThreeWayMatchResult,
  MatchVariance,
  MatchingTolerance,
  PurchaseOrder,
  GoodsReceipt,
  MatchException,
} from '../types/matching.types';
import { VendorInvoice } from '../types/invoice.types';

export class MatchingService {
  private apiEndpoint = '/api/accounts-payable/matching';

  private defaultTolerances: MatchingTolerance = {
    quantityVariance: 5, // 5%
    priceVariance: 2, // 2%
    totalVariance: 1000, // ₦1,000
    autoApproveThreshold: 1, // 1%
  };

  /**
   * Perform three-way match between PO, GRN, and Invoice
   */
  async performThreeWayMatch(
    invoice: VendorInvoice,
    purchaseOrder: PurchaseOrder,
    goodsReceipt: GoodsReceipt,
    tolerances?: MatchingTolerance
  ): Promise<ThreeWayMatchResult> {
    const response = await fetch(`${this.apiEndpoint}/three-way`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        invoiceId: invoice.id,
        purchaseOrderId: purchaseOrder.id,
        goodsReceiptId: goodsReceipt.id,
        tolerances: tolerances || this.defaultTolerances,
      }),
    });

    if (!response.ok) {
      throw new Error('Failed to perform three-way match');
    }

    return response.json();
  }

  /**
   * Calculate variances between documents
   */
  calculateVariances(
    invoice: VendorInvoice,
    purchaseOrder: PurchaseOrder,
    goodsReceipt: GoodsReceipt,
    tolerances: MatchingTolerance
  ): MatchVariance[] {
    const variances: MatchVariance[] = [];

    // Total amount variance
    const totalVariance = invoice.total - purchaseOrder.total;
    const totalVariancePercentage = (totalVariance / purchaseOrder.total) * 100;
    
    variances.push({
      type: 'total',
      field: 'Total Amount',
      poValue: purchaseOrder.total,
      grnValue: goodsReceipt.lines.reduce((sum, line) => sum + line.amount, 0),
      invoiceValue: invoice.total,
      variance: totalVariance,
      variancePercentage: totalVariancePercentage,
      withinTolerance: Math.abs(totalVariance) <= tolerances.totalVariance,
      toleranceLimit: tolerances.totalVariance,
      severity: this.getVarianceSeverity(totalVariancePercentage, tolerances.autoApproveThreshold),
    });

    // Line-level variances
    invoice.lines.forEach((invoiceLine) => {
      const poLine = purchaseOrder.lines.find(
        (pl) => pl.itemCode === invoiceLine.description || pl.description === invoiceLine.description
      );
      
      const grnLine = goodsReceipt.lines.find(
        (gl) => gl.itemCode === invoiceLine.description || gl.description === invoiceLine.description
      );

      if (poLine && grnLine) {
        // Quantity variance
        const qtyVariance = invoiceLine.quantity - grnLine.acceptedQuantity;
        const qtyVariancePercentage = (qtyVariance / grnLine.acceptedQuantity) * 100;

        if (Math.abs(qtyVariancePercentage) > 0.01) {
          variances.push({
            type: 'quantity',
            field: `${invoiceLine.description} - Quantity`,
            poValue: poLine.quantity,
            grnValue: grnLine.acceptedQuantity,
            invoiceValue: invoiceLine.quantity,
            variance: qtyVariance,
            variancePercentage: qtyVariancePercentage,
            withinTolerance: Math.abs(qtyVariancePercentage) <= tolerances.quantityVariance,
            toleranceLimit: tolerances.quantityVariance,
            severity: this.getVarianceSeverity(qtyVariancePercentage, tolerances.quantityVariance),
          });
        }

        // Price variance
        const priceVariance = invoiceLine.unitPrice - poLine.unitPrice;
        const priceVariancePercentage = (priceVariance / poLine.unitPrice) * 100;

        if (Math.abs(priceVariancePercentage) > 0.01) {
          variances.push({
            type: 'price',
            field: `${invoiceLine.description} - Unit Price`,
            poValue: poLine.unitPrice,
            grnValue: poLine.unitPrice,
            invoiceValue: invoiceLine.unitPrice,
            variance: priceVariance,
            variancePercentage: priceVariancePercentage,
            withinTolerance: Math.abs(priceVariancePercentage) <= tolerances.priceVariance,
            toleranceLimit: tolerances.priceVariance,
            severity: this.getVarianceSeverity(priceVariancePercentage, tolerances.priceVariance),
          });
        }
      }
    });

    return variances;
  }

  /**
   * Determine variance severity
   */
  private getVarianceSeverity(
    variancePercentage: number,
    threshold: number
  ): 'info' | 'warning' | 'error' {
    const absVariance = Math.abs(variancePercentage);
    
    if (absVariance <= threshold) {
      return 'info';
    } else if (absVariance <= threshold * 2) {
      return 'warning';
    } else {
      return 'error';
    }
  }

  /**
   * Get matching recommendations
   */
  getRecommendations(variances: MatchVariance[]): string[] {
    const recommendations: string[] = [];

    const errorVariances = variances.filter((v) => v.severity === 'error');
    const warningVariances = variances.filter((v) => v.severity === 'warning');

    if (errorVariances.length > 0) {
      recommendations.push(
        `${errorVariances.length} critical variance(s) detected. Manual review required.`
      );
    }

    if (warningVariances.length > 0) {
      recommendations.push(
        `${warningVariances.length} variance(s) exceed normal tolerance. Consider vendor communication.`
      );
    }

    const priceVariances = variances.filter((v) => v.type === 'price' && !v.withinTolerance);
    if (priceVariances.length > 0) {
      recommendations.push('Price differences detected. Verify with purchase order and vendor quote.');
    }

    const quantityVariances = variances.filter((v) => v.type === 'quantity' && !v.withinTolerance);
    if (quantityVariances.length > 0) {
      recommendations.push('Quantity discrepancies found. Check goods receipt notes for partial deliveries.');
    }

    if (recommendations.length === 0) {
      recommendations.push('All variances are within acceptable tolerance. Invoice can be approved.');
    }

    return recommendations;
  }

  /**
   * Find purchase orders for vendor
   */
  async findPurchaseOrders(vendorId: string, status?: string): Promise<PurchaseOrder[]> {
    const params = new URLSearchParams({ vendorId });
    if (status) params.append('status', status);

    const response = await fetch(`/api/purchase-orders?${params}`);

    if (!response.ok) {
      throw new Error('Failed to fetch purchase orders');
    }

    return response.json();
  }

  /**
   * Find goods receipts for purchase order
   */
  async findGoodsReceipts(purchaseOrderId: string): Promise<GoodsReceipt[]> {
    const response = await fetch(`/api/goods-receipts?purchaseOrderId=${purchaseOrderId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch goods receipts');
    }

    return response.json();
  }

  /**
   * Create match exception
   */
  async createException(
    invoiceId: string,
    variances: MatchVariance[],
    description: string
  ): Promise<MatchException> {
    const response = await fetch(`${this.apiEndpoint}/exceptions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        invoiceId,
        variances,
        description,
      }),
    });

    if (!response.ok) {
      throw new Error('Failed to create match exception');
    }

    return response.json();
  }

  /**
   * Override match and approve
   */
  async overrideMatch(
    invoiceId: string,
    reason: string,
    approvedBy: string
  ): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/override`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        invoiceId,
        reason,
        approvedBy,
      }),
    });

    if (!response.ok) {
      throw new Error('Failed to override match');
    }
  }

  /**
   * Get matching history for invoice
   */
  async getMatchingHistory(invoiceId: string): Promise<any[]> {
    const response = await fetch(`${this.apiEndpoint}/history/${invoiceId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch matching history');
    }

    return response.json();
  }

  /**
   * Update matching tolerances
   */
  async updateTolerances(tolerances: MatchingTolerance): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/tolerances`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(tolerances),
    });

    if (!response.ok) {
      throw new Error('Failed to update tolerances');
    }
  }

  /**
   * Get current tolerances
   */
  async getTolerances(): Promise<MatchingTolerance> {
    const response = await fetch(`${this.apiEndpoint}/tolerances`);

    if (!response.ok) {
      return this.defaultTolerances;
    }

    return response.json();
  }
}

export const matchingService = new MatchingService();
