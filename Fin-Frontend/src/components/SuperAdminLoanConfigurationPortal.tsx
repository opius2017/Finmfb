import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface LoanConfiguration {
  id: number;
  configKey: string;
  configValue: string;
  valueType: string;
  label: string;
  description: string;
  category: string;
  minValue: string;
  maxValue: string;
  lastModifiedBy: string;
  lastModifiedDate: string;
  isLocked: boolean;
  lockReason: string;
  effectiveDate: string;
  previousValue: string;
  requiresBoardApproval: boolean;
  approvalStatus: string;
}

/**
 * Super Admin Loan Configuration Portal
 * Manage system-wide loan parameters: interest rates, deduction limits, multipliers, thresholds
 * Only accessible by authorized Super Admins
 */
export const SuperAdminLoanConfigurationPortal: React.FC = () => {
  const [configurations, setConfigurations] = useState<LoanConfiguration[]>([]);
  const [selectedConfig, setSelectedConfig] = useState<LoanConfiguration | null>(null);
  const [filterCategory, setFilterCategory] = useState<string>('');
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [showForm, setShowForm] = useState<boolean>(false);
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [formData, setFormData] = useState({
    configKey: '',
    configValue: '',
    valueType: 'Decimal',
    label: '',
    description: '',
    category: 'Interest',
    minValue: '',
    maxValue: '',
    requiresBoardApproval: false,
    reason: ''
  });
  const [submitting, setSubmitting] = useState<boolean>(false);

  useEffect(() => {
    fetchConfigurations();
  }, [pageNumber, pageSize, filterCategory]);

  const fetchConfigurations = async () => {
    setLoading(true);
    setError('');
    try {
      const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
      });
      if (filterCategory) params.append('category', filterCategory);

      const response = await axios.get(`/api/v1/super-admin/loan-configurations?${params}`);
      setConfigurations(response.data.items || []);
      setTotalCount(response.data.totalCount || 0);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to fetch configurations');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitConfiguration = async () => {
    setSubmitting(true);
    setError('');
    try {
      if (isEditing && selectedConfig) {
        await axios.put(`/api/v1/super-admin/loan-configurations/${selectedConfig.id}`, {
          configValue: formData.configValue,
          reason: formData.reason,
          requiresBoardApproval: formData.requiresBoardApproval
        });
        alert('Configuration updated successfully');
      } else {
        await axios.post('/api/v1/super-admin/loan-configurations', formData);
        alert('Configuration created successfully');
      }
      resetForm();
      fetchConfigurations();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to save configuration');
    } finally {
      setSubmitting(false);
    }
  };

  const resetForm = () => {
    setFormData({
      configKey: '',
      configValue: '',
      valueType: 'Decimal',
      label: '',
      description: '',
      category: 'Interest',
      minValue: '',
      maxValue: '',
      requiresBoardApproval: false,
      reason: ''
    });
    setShowForm(false);
    setIsEditing(false);
    setSelectedConfig(null);
  };

  const handleEditClick = (config: LoanConfiguration) => {
    setSelectedConfig(config);
    setFormData({
      ...formData,
      configValue: config.configValue,
      reason: ''
    });
    setIsEditing(true);
    setShowForm(true);
  };

  const getCategoryColor = (category: string): string => {
    const colors: Record<string, string> = {
      'Interest': 'bg-blue-100 text-blue-800',
      'Deduction': 'bg-green-100 text-green-800',
      'Multiplier': 'bg-purple-100 text-purple-800',
      'Thresholds': 'bg-yellow-100 text-yellow-800',
      'Compliance': 'bg-red-100 text-red-800'
    };
    return colors[category] || 'bg-gray-100 text-gray-800';
  };

  const getApprovalStatusColor = (status: string): string => {
    switch (status) {
      case 'Approved': return 'bg-green-100 text-green-800';
      case 'Pending': return 'bg-yellow-100 text-yellow-800';
      case 'Rejected': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg p-6 max-w-6xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-800 mb-2">Super Admin Loan Configuration Portal</h2>
      <p className="text-gray-600 mb-6">Manage system-wide loan parameters and compliance settings</p>

      {/* Error Message */}
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      {/* Warning Banner */}
      <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
        <p className="text-yellow-800 font-semibold">⚠️ Critical System Configuration</p>
        <p className="text-yellow-700 text-sm">Changes made here affect all loan processing. Board approval required for critical changes.</p>
      </div>

      {!showForm ? (
        <>
          {/* Filters and Controls */}
          <div className="flex justify-between items-center mb-6">
            <div className="flex gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Category</label>
                <select
                  value={filterCategory}
                  onChange={(e) => {
                    setFilterCategory(e.target.value);
                    setPageNumber(1);
                  }}
                  className="px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value="">All Categories</option>
                  <option value="Interest">Interest</option>
                  <option value="Deduction">Deduction</option>
                  <option value="Multiplier">Multiplier</option>
                  <option value="Thresholds">Thresholds</option>
                  <option value="Compliance">Compliance</option>
                </select>
              </div>
              <div className="flex items-end">
                <button
                  onClick={() => fetchConfigurations()}
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Refresh
                </button>
              </div>
            </div>
            <button
              onClick={() => {
                resetForm();
                setShowForm(true);
              }}
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 font-semibold"
            >
              + Create Configuration
            </button>
          </div>

          {/* Configurations Table */}
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse border border-gray-300">
              <thead className="bg-gray-100">
                <tr>
                  <th className="border border-gray-300 px-4 py-2 text-left">Config Key</th>
                  <th className="border border-gray-300 px-4 py-2 text-left">Label</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Value</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Category</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Status</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Last Modified</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Action</th>
                </tr>
              </thead>
              <tbody>
                {configurations.map((config) => (
                  <tr key={config.id} className={config.isLocked ? 'bg-gray-50 opacity-75' : 'hover:bg-gray-50'}>
                    <td className="border border-gray-300 px-4 py-2 font-mono font-semibold">{config.configKey}</td>
                    <td className="border border-gray-300 px-4 py-2">{config.label}</td>
                    <td className="border border-gray-300 px-4 py-2 text-center font-bold text-blue-600">
                      {config.valueType === 'Decimal' && config.configKey.includes('Rate') ? `${config.configValue}%` : config.configValue}
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center">
                      <span className={`px-2 py-1 rounded text-xs font-semibold ${getCategoryColor(config.category)}`}>
                        {config.category}
                      </span>
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center">
                      <span className={`px-2 py-1 rounded text-xs font-semibold ${getApprovalStatusColor(config.approvalStatus)}`}>
                        {config.approvalStatus}
                      </span>
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center text-sm text-gray-600">
                      <div>{config.lastModifiedBy}</div>
                      <div>{new Date(config.lastModifiedDate).toLocaleDateString()}</div>
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center">
                      <div className="space-y-2">
                        <button
                          onClick={() => handleEditClick(config)}
                          disabled={config.isLocked}
                          className="block w-full bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700 text-sm disabled:bg-gray-400"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => alert(`Previous value: ${config.previousValue}`)}
                          className="block w-full bg-gray-600 text-white px-3 py-1 rounded hover:bg-gray-700 text-sm"
                        >
                          History
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="mt-4 flex justify-between items-center">
            <div className="text-sm text-gray-600">
              Showing {((pageNumber - 1) * pageSize) + 1} to {Math.min(pageNumber * pageSize, totalCount)} of {totalCount} total
            </div>
            <div className="space-x-2">
              <button
                onClick={() => setPageNumber(Math.max(1, pageNumber - 1))}
                disabled={pageNumber === 1}
                className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Previous
              </button>
              <button
                onClick={() => setPageNumber(pageNumber + 1)}
                disabled={pageNumber * pageSize >= totalCount}
                className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </div>
        </>
      ) : (
        <>
          {/* Configuration Form */}
          <button
            onClick={resetForm}
            className="mb-4 text-blue-600 hover:text-blue-800 font-semibold"
          >
            ← Back to Configurations
          </button>

          <div className="bg-gray-50 p-6 rounded-lg max-w-2xl">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">
              {isEditing ? 'Edit Configuration' : 'Create New Configuration'}
            </h3>

            {!isEditing && (
              <>
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Configuration Key *</label>
                  <input
                    type="text"
                    value={formData.configKey}
                    onChange={(e) => setFormData({ ...formData, configKey: e.target.value.toUpperCase() })}
                    placeholder="E.g., GLOBAL_INTEREST_RATE"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>

                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Category *</label>
                  <select
                    value={formData.category}
                    onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  >
                    <option value="Interest">Interest</option>
                    <option value="Deduction">Deduction</option>
                    <option value="Multiplier">Multiplier</option>
                    <option value="Thresholds">Thresholds</option>
                    <option value="Compliance">Compliance</option>
                  </select>
                </div>

                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Label *</label>
                  <input
                    type="text"
                    value={formData.label}
                    onChange={(e) => setFormData({ ...formData, label: e.target.value })}
                    placeholder="E.g., Global Interest Rate"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>

                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Description</label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    rows={2}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4 mb-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Value Type *</label>
                    <select
                      value={formData.valueType}
                      onChange={(e) => setFormData({ ...formData, valueType: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    >
                      <option value="Decimal">Decimal</option>
                      <option value="Integer">Integer</option>
                      <option value="Boolean">Boolean</option>
                      <option value="String">String</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Min Value</label>
                    <input
                      type="text"
                      value={formData.minValue}
                      onChange={(e) => setFormData({ ...formData, minValue: e.target.value })}
                      placeholder="E.g., 0"
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4 mb-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Max Value</label>
                    <input
                      type="text"
                      value={formData.maxValue}
                      onChange={(e) => setFormData({ ...formData, maxValue: e.target.value })}
                      placeholder="E.g., 100"
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                  <div>
                    <label className="flex items-center text-sm font-medium text-gray-700">
                      <input
                        type="checkbox"
                        checked={formData.requiresBoardApproval}
                        onChange={(e) => setFormData({ ...formData, requiresBoardApproval: e.target.checked })}
                        className="mr-2"
                      />
                      Requires Board Approval
                    </label>
                  </div>
                </div>
              </>
            )}

            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">Value *</label>
              <input
                type="text"
                value={formData.configValue}
                onChange={(e) => setFormData({ ...formData, configValue: e.target.value })}
                placeholder="Enter configuration value"
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
              />
            </div>

            {isEditing && (
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">Change Reason *</label>
                <textarea
                  value={formData.reason}
                  onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                  rows={2}
                  placeholder="Document reason for configuration change..."
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                />
              </div>
            )}

            <div className="flex gap-4">
              <button
                onClick={handleSubmitConfiguration}
                disabled={submitting}
                className="flex-1 bg-green-600 text-white py-2 rounded-md hover:bg-green-700 disabled:bg-gray-400 font-semibold"
              >
                {submitting ? 'Saving...' : 'Save Configuration'}
              </button>
              <button
                onClick={resetForm}
                className="flex-1 bg-gray-400 text-white py-2 rounded-md hover:bg-gray-500 font-semibold"
              >
                Cancel
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default SuperAdminLoanConfigurationPortal;
