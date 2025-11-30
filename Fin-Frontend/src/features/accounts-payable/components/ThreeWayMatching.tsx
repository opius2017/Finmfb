import React, { useState, useEffect } from 'react';
import { AlertTriangle, CheckCircle, XCircle, FileText, Package, Receipt } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { VendorInvoice } from '../types/invoice.types';
import { PurchaseOrder, GoodsReceipt, ThreeWayMatchResult, MatchVariance } from '../types/matching.types';
import { matchingService } from '../services/matchingService';

interface ThreeWayMatchingProps {
  invoice: VendorInvoice;
  onMatchComplete: (result: ThreeWayMatchResult) => void;
  onCancel: () => void;
}

export const ThreeWayMatching: React.FC<ThreeWayMatchingProps> = ({
  invoice,
  onMatchComplete,
  onCancel,
}) => {
  const [purchaseOrders, setPurchaseOrders] = useState<PurchaseOrder[]>([]);
  const [selectedPO, setSelectedPO] = useState<PurchaseOrder | null>(null);
  const [goodsReceipts, setGoodsReceipts] = useState<GoodsReceipt[]>([]);
  const [selectedGRN, setSelectedGRN] = useState<GoodsReceipt | null>(null);
  const [matchResult, setMatchResult] = useState<ThreeWayMatchResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [step, setStep] = useState<'select-po' | 'select-grn' | 'review-match'>('select-po');

  useEffect(() => {
    if (invoice.vendorId) {
      loadPurchaseOrders();
    }
  }, [invoice.vendorId]);

  const loadPurchaseOrders = async () => {
    try {
      const pos = await matchingService.findPurchaseOrders(invoice.vendorId, 'approved');
      setPurchaseOrders(pos);
    } catch (error) {
      console.error('Failed to load purchase orders:', error);
    }
  };

  const handlePOSelect = async (po: PurchaseOrder) => {
    setSelectedPO(po);
    setLoading(true);
    
    try {
      const grns = await matchingService.findGoodsReceipts(po.id);
      setGoodsReceipts(grns);
      setStep('select-grn');
    } catch (error) {
      console.error('Failed to load goods receipts:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleGRNSelect = async (grn: GoodsReceipt) => {
    setSelectedGRN(grn);
    setLoading(true);

    try {
      if (selectedPO) {
        const result = await matchingService.performThreeWayMatch(
          invoice,
          selectedPO,
          grn
        );
        setMatchResult(result);
        setStep('review-match');
      }
    } catch (error) {
      console.error('Failed to perform matching:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = () => {
    if (matchResult) {
      onMatchComplete(matchResult);
    }
  };

  const handleOverride = async () => {
    const reason = prompt('Please provide a reason for overriding the match:');
    if (reason && matchResult) {
      try {
        await matchingService.overrideMatch(invoice.id, reason, 'current-user');
        onMatchComplete(matchResult);
      } catch (error) {
        console.error('Failed to override match:', error);
      }
    }
  };

  const getVarianceColor = (variance: MatchVariance) => {
    if (variance.severity === 'error') return 'text-error-600 bg-error-50';
    if (variance.severity === 'warning') return 'text-warning-600 bg-warning-50';
    return 'text-success-600 bg-success-50';
  };

  const getVarianceIcon = (variance: MatchVariance) => {
    if (variance.severity === 'error') return <XCircle className="w-5 h-5" />;
    if (variance.severity === 'warning') return <AlertTriangle className="w-5 h-5" />;
    return <CheckCircle className="w-5 h-5" />;
  };

  if (step === 'select-po') {
    return (
      <Card className="p-6">
        <h2 className="text-xl font-bold mb-6">Select Purchase Order</h2>

        {purchaseOrders.length === 0 ? (
          <div className="text-center py-12">
            <FileText className="w-16 h-16 text-neutral-300 mx-auto mb-4" />
            <p className="text-neutral-600">No purchase orders found for this vendor</p>
          </div>
        ) : (
          <div className="space-y-3">
            {purchaseOrders.map((po) => (
              <button
                key={po.id}
                onClick={() => handlePOSelect(po)}
                className="w-full p-4 border border-neutral-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors text-left"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-semibold">{po.poNumber}</div>
                    <div className="text-sm text-neutral-600 mt-1">
                      Date: {new Date(po.orderDate).toLocaleDateString()}
                    </div>
                    <div className="text-sm text-neutral-600">
                      {po.lines.length} line item(s)
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="font-semibold">₦{po.total.toLocaleString()}</div>
                    <span className="inline-block px-2 py-1 text-xs font-semibold rounded-full bg-success-100 text-success-800 mt-1">
                      {po.status}
                    </span>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}

        <div className="mt-6 flex justify-end">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
        </div>
      </Card>
    );
  }

  if (step === 'select-grn') {
    return (
      <Card className="p-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setStep('select-po')}
          >
            ← Back to PO Selection
          </Button>
        </div>

        <h2 className="text-xl font-bold mb-2">Select Goods Receipt</h2>
        <p className="text-neutral-600 mb-6">
          Purchase Order: {selectedPO?.poNumber}
        </p>

        {goodsReceipts.length === 0 ? (
          <div className="text-center py-12">
            <Package className="w-16 h-16 text-neutral-300 mx-auto mb-4" />
            <p className="text-neutral-600">No goods receipts found for this purchase order</p>
          </div>
        ) : (
          <div className="space-y-3">
            {goodsReceipts.map((grn) => (
              <button
                key={grn.id}
                onClick={() => handleGRNSelect(grn)}
                className="w-full p-4 border border-neutral-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors text-left"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-semibold">{grn.grnNumber}</div>
                    <div className="text-sm text-neutral-600 mt-1">
                      Received: {new Date(grn.receiptDate).toLocaleDateString()}
                    </div>
                    <div className="text-sm text-neutral-600">
                      By: {grn.receivedBy}
                    </div>
                  </div>
                  <div>
                    <span className="inline-block px-2 py-1 text-xs font-semibold rounded-full bg-success-100 text-success-800">
                      {grn.status}
                    </span>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}

        <div className="mt-6 flex justify-end">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
        </div>
      </Card>
    );
  }

  if (step === 'review-match' && matchResult) {
    return (
      <Card className="p-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setStep('select-grn')}
          >
            ← Back to GRN Selection
          </Button>
        </div>

        <h2 className="text-xl font-bold mb-6">Three-Way Match Results</h2>

        {/* Match Status */}
        <div className={`p-4 rounded-lg mb-6 ${
          matchResult.matchStatus === 'matched' ? 'bg-success-50 border border-success-200' :
          matchResult.matchStatus === 'partial-match' ? 'bg-warning-50 border border-warning-200' :
          'bg-error-50 border border-error-200'
        }`}>
          <div className="flex items-center space-x-3">
            {matchResult.matchStatus === 'matched' ? (
              <CheckCircle className="w-6 h-6 text-success-600" />
            ) : matchResult.matchStatus === 'partial-match' ? (
              <AlertTriangle className="w-6 h-6 text-warning-600" />
            ) : (
              <XCircle className="w-6 h-6 text-error-600" />
            )}
            <div>
              <div className="font-semibold">
                {matchResult.matchStatus === 'matched' && 'Match Successful'}
                {matchResult.matchStatus === 'partial-match' && 'Partial Match'}
                {matchResult.matchStatus === 'exception' && 'Match Exception'}
                {matchResult.matchStatus === 'not-matched' && 'No Match'}
              </div>
              <div className="text-sm mt-1">
                Match Score: {(matchResult.overallScore * 100).toFixed(1)}%
              </div>
            </div>
          </div>
        </div>

        {/* Document Summary */}
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="p-4 bg-neutral-50 rounded-lg">
            <div className="flex items-center space-x-2 mb-2">
              <FileText className="w-5 h-5 text-primary-600" />
              <span className="font-semibold">Purchase Order</span>
            </div>
            <div className="text-sm text-neutral-600">{selectedPO?.poNumber}</div>
            <div className="text-lg font-semibold mt-2">
              ₦{selectedPO?.total.toLocaleString()}
            </div>
          </div>

          <div className="p-4 bg-neutral-50 rounded-lg">
            <div className="flex items-center space-x-2 mb-2">
              <Package className="w-5 h-5 text-primary-600" />
              <span className="font-semibold">Goods Receipt</span>
            </div>
            <div className="text-sm text-neutral-600">{selectedGRN?.grnNumber}</div>
            <div className="text-lg font-semibold mt-2">
              ₦{selectedGRN?.lines.reduce((sum, line) => sum + line.amount, 0).toLocaleString()}
            </div>
          </div>

          <div className="p-4 bg-neutral-50 rounded-lg">
            <div className="flex items-center space-x-2 mb-2">
              <Receipt className="w-5 h-5 text-primary-600" />
              <span className="font-semibold">Invoice</span>
            </div>
            <div className="text-sm text-neutral-600">{invoice.invoiceNumber}</div>
            <div className="text-lg font-semibold mt-2">
              ₦{invoice.total.toLocaleString()}
            </div>
          </div>
        </div>

        {/* Variances */}
        {matchResult.variances.length > 0 && (
          <div className="mb-6">
            <h3 className="font-semibold mb-3">Variances Detected</h3>
            <div className="space-y-2">
              {matchResult.variances.map((variance, index) => (
                <div
                  key={index}
                  className={`p-3 rounded-lg flex items-start space-x-3 ${getVarianceColor(variance)}`}
                >
                  {getVarianceIcon(variance)}
                  <div className="flex-1">
                    <div className="font-medium">{variance.field}</div>
                    <div className="text-sm mt-1">
                      PO: {variance.poValue.toLocaleString()} | 
                      GRN: {variance.grnValue.toLocaleString()} | 
                      Invoice: {variance.invoiceValue.toLocaleString()}
                    </div>
                    <div className="text-sm mt-1">
                      Variance: {variance.variance.toLocaleString()} 
                      ({variance.variancePercentage.toFixed(2)}%)
                      {variance.withinTolerance ? ' - Within Tolerance' : ' - Exceeds Tolerance'}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Recommendations */}
        {matchResult.recommendations.length > 0 && (
          <div className="mb-6">
            <h3 className="font-semibold mb-3">Recommendations</h3>
            <ul className="space-y-2">
              {matchResult.recommendations.map((rec, index) => (
                <li key={index} className="flex items-start space-x-2">
                  <span className="text-primary-600 mt-1">•</span>
                  <span className="text-sm">{rec}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Actions */}
        <div className="flex justify-end space-x-3">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          {matchResult.requiresReview && (
            <Button variant="secondary" onClick={handleOverride}>
              Override & Approve
            </Button>
          )}
          {matchResult.canAutoApprove && (
            <Button variant="primary" onClick={handleApprove}>
              <CheckCircle className="w-4 h-4 mr-2" />
              Approve Match
            </Button>
          )}
        </div>
      </Card>
    );
  }

  return null;
};
