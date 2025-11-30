import React, { useState, useEffect } from 'react';
import { Plus, Star, TrendingUp, FileText, Mail, Download, BarChart } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Vendor, VendorAgingReport, VendorStatistics } from '../types/vendor.types';
import { vendorService } from '../services/vendorService';

type ViewMode = 'list' | 'aging' | 'performance' | 'details';

export const VendorManagement: React.FC = () => {
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [agingReport, setAgingReport] = useState<VendorAgingReport | null>(null);
  const [statistics, setStatistics] = useState<VendorStatistics | null>(null);
  const [selectedVendor, setSelectedVendor] = useState<Vendor | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadVendors();
    loadStatistics();
  }, []);

  const loadVendors = async () => {
    setLoading(true);
    try {
      const result = await vendorService.getVendors({ status: 'active' });
      setVendors(result.vendors);
    } catch (error) {
      console.error('Failed to load vendors:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadAgingReport = async () => {
    setLoading(true);
    try {
      const report = await vendorService.getAgingReport();
      setAgingReport(report);
      setViewMode('aging');
    } catch (error) {
      console.error('Failed to load aging report:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadStatistics = async () => {
    try {
      const stats = await vendorService.getStatistics();
      setStatistics(stats);
    } catch (error) {
      console.error('Failed to load statistics:', error);
    }
  };

  const handleSendStatement = async (vendorId: string) => {
    try {
      const statement = await vendorService.getVendorStatement(
        vendorId,
        new Date(new Date().getFullYear(), 0, 1),
        new Date()
      );
      await vendorService.sendStatement(vendorId, statement);
      alert('Statement sent successfully');
    } catch (error) {
      console.error('Failed to send statement:', error);
    }
  };

  const getRatingStars = (rating: number) => {
    return Array.from({ length: 5 }, (_, i) => (
      <Star
        key={i}
        className={`w-4 h-4 ${
          i < rating ? 'fill-warning-500 text-warning-500' : 'text-neutral-300'
        }`}
      />
    ));
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active':
        return 'bg-success-100 text-success-800';
      case 'inactive':
        return 'bg-neutral-100 text-neutral-800';
      case 'suspended':
        return 'bg-warning-100 text-warning-800';
      case 'blacklisted':
        return 'bg-error-100 text-error-800';
      default:
        return 'bg-neutral-100 text-neutral-800';
    }
  };

  if (viewMode === 'aging' && agingReport) {
    return (
      <div className="p-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setViewMode('list')}
          >
            ← Back to Vendors
          </Button>
        </div>

        <Card className="p-6">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-xl font-bold">Vendor Aging Report</h2>
              <p className="text-sm text-neutral-600 mt-1">
                As of {new Date(agingReport.asOfDate).toLocaleDateString()}
              </p>
            </div>
            <Button variant="outline" size="sm">
              <Download className="w-4 h-4 mr-2" />
              Export
            </Button>
          </div>

          {/* Aging Summary */}
          <div className="grid grid-cols-5 gap-4 mb-6">
            <div className="p-4 bg-success-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">Current</div>
              <div className="text-lg font-semibold">
                ₦{agingReport.totals.current.toLocaleString()}
              </div>
            </div>
            <div className="p-4 bg-primary-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">1-30 Days</div>
              <div className="text-lg font-semibold">
                ₦{agingReport.totals.days1to30.toLocaleString()}
              </div>
            </div>
            <div className="p-4 bg-warning-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">31-60 Days</div>
              <div className="text-lg font-semibold">
                ₦{agingReport.totals.days31to60.toLocaleString()}
              </div>
            </div>
            <div className="p-4 bg-error-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">61-90 Days</div>
              <div className="text-lg font-semibold">
                ₦{agingReport.totals.days61to90.toLocaleString()}
              </div>
            </div>
            <div className="p-4 bg-error-100 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">Over 90 Days</div>
              <div className="text-lg font-semibold">
                ₦{agingReport.totals.over90.toLocaleString()}
              </div>
            </div>
          </div>

          {/* Aging Details */}
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Vendor
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Current
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    1-30 Days
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    31-60 Days
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    61-90 Days
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Over 90
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Total
                  </th>
                </tr>
              </thead>
              <tbody>
                {agingReport.vendors.map((entry) => (
                  <tr key={entry.vendorId} className="border-b border-neutral-100 hover:bg-neutral-50">
                    <td className="py-3 px-4">{entry.vendorName}</td>
                    <td className="py-3 px-4 text-right">
                      ₦{entry.current.toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-right">
                      ₦{entry.days1to30.toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-right">
                      ₦{entry.days31to60.toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-right">
                      ₦{entry.days61to90.toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-right text-error-600">
                      ₦{entry.over90.toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-right font-semibold">
                      ₦{entry.total.toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Vendor Management</h1>
          <p className="text-neutral-600 mt-1">
            Manage vendors and track performance
          </p>
        </div>
        <div className="flex space-x-3">
          <Button variant="outline" onClick={loadAgingReport}>
            <BarChart className="w-4 h-4 mr-2" />
            Aging Report
          </Button>
          <Button variant="primary">
            <Plus className="w-4 h-4 mr-2" />
            Add Vendor
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      {statistics && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Vendors</div>
            <div className="text-2xl font-bold">{statistics.totalVendors}</div>
            <div className="text-xs text-success-600 mt-1">
              {statistics.activeVendors} active
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Total Spend</div>
            <div className="text-2xl font-bold">
              ₦{(statistics.totalSpend / 1000000).toFixed(1)}M
            </div>
            <div className="text-xs text-neutral-600 mt-1">This year</div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Avg Payment Days</div>
            <div className="text-2xl font-bold">
              {statistics.averagePaymentDays}
            </div>
            <div className="text-xs text-neutral-600 mt-1">Days</div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-neutral-600 mb-1">Top Vendor</div>
            <div className="text-lg font-bold truncate">
              {statistics.topVendors[0]?.vendorName || 'N/A'}
            </div>
            <div className="text-xs text-neutral-600 mt-1">
              ₦{(statistics.topVendors[0]?.totalSpend || 0).toLocaleString()}
            </div>
          </Card>
        </div>
      )}

      {/* Vendors List */}
      <Card className="p-6">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-neutral-200">
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Vendor
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Category
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Contact
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Rating
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Total Spend
                </th>
                <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                  Status
                </th>
                <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {vendors.map((vendor) => (
                <tr key={vendor.id} className="border-b border-neutral-100 hover:bg-neutral-50">
                  <td className="py-3 px-4">
                    <div className="font-medium">{vendor.name}</div>
                    <div className="text-sm text-neutral-600">{vendor.vendorCode}</div>
                  </td>
                  <td className="py-3 px-4">{vendor.category}</td>
                  <td className="py-3 px-4">
                    <div className="text-sm">{vendor.contactPerson}</div>
                    <div className="text-xs text-neutral-600">{vendor.email}</div>
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center space-x-1">
                      {getRatingStars(vendor.rating.overall)}
                    </div>
                    <div className="text-xs text-neutral-600 mt-1">
                      {vendor.rating.reviewCount} reviews
                    </div>
                  </td>
                  <td className="py-3 px-4 text-right font-semibold">
                    ₦{vendor.performance.totalSpend.toLocaleString()}
                  </td>
                  <td className="py-3 px-4">
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(vendor.status)}`}>
                      {vendor.status}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-right">
                    <div className="flex justify-end space-x-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleSendStatement(vendor.id)}
                      >
                        <Mail className="w-4 h-4" />
                      </Button>
                      <Button variant="ghost" size="sm">
                        <FileText className="w-4 h-4" />
                      </Button>
                      <Button variant="ghost" size="sm">
                        <TrendingUp className="w-4 h-4" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};
