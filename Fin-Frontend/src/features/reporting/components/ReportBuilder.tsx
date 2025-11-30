import React, { useState, useEffect } from 'react';
import { Plus, Play, Save, Download, Settings } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { Report, ReportField, ReportFilter, ReportData } from '../types/report.types';
import { reportService } from '../services/reportService';

export const ReportBuilder: React.FC = () => {
  const [report, setReport] = useState<Partial<Report>>({
    name: '',
    type: 'tabular',
    category: 'custom',
    fields: [],
    filters: [],
    groupings: [],
    sortings: [],
  });
  const [dataSources, setDataSources] = useState<any[]>([]);
  const [availableFields, setAvailableFields] = useState<any[]>([]);
  const [reportData, setReportData] = useState<ReportData | null>(null);
  const [step, setStep] = useState<'setup' | 'fields' | 'filters' | 'preview'>('setup');

  useEffect(() => {
    loadDataSources();
  }, []);

  const loadDataSources = async () => {
    try {
      const sources = await reportService.getDataSources();
      setDataSources(sources);
    } catch (error) {
      console.error('Failed to load data sources:', error);
    }
  };

  const handleDataSourceSelect = async (dataSourceId: string) => {
    try {
      const fields = await reportService.getFields(dataSourceId);
      setAvailableFields(fields);
      setReport({
        ...report,
        dataSource: dataSources.find(ds => ds.id === dataSourceId),
      });
    } catch (error) {
      console.error('Failed to load fields:', error);
    }
  };

  const handleAddField = (field: any) => {
    const newField: ReportField = {
      id: `field-${Date.now()}`,
      name: field.name,
      label: field.label || field.name,
      dataType: field.dataType,
      source: field.source,
      visible: true,
    };
    setReport({
      ...report,
      fields: [...(report.fields || []), newField],
    });
  };

  const handleRemoveField = (fieldId: string) => {
    setReport({
      ...report,
      fields: report.fields?.filter(f => f.id !== fieldId),
    });
  };

  const handleAddFilter = () => {
    const newFilter: ReportFilter = {
      id: `filter-${Date.now()}`,
      field: '',
      operator: 'equals',
      value: '',
      dataType: 'string',
    };
    setReport({
      ...report,
      filters: [...(report.filters || []), newFilter],
    });
  };

  const handleRunReport = async () => {
    try {
      const data = await reportService.executeReport(report.id || 'preview', {});
      setReportData(data);
      setStep('preview');
    } catch (error) {
      console.error('Failed to run report:', error);
    }
  };

  const handleSaveReport = async () => {
    try {
      if (report.id) {
        await reportService.updateReport(report.id, report);
      } else {
        await reportService.createReport(report);
      }
      alert('Report saved successfully');
    } catch (error) {
      console.error('Failed to save report:', error);
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Report Builder</h1>
        <div className="flex space-x-3">
          <Button variant="outline" onClick={handleRunReport}>
            <Play className="w-4 h-4 mr-2" />
            Run Report
          </Button>
          <Button variant="primary" onClick={handleSaveReport}>
            <Save className="w-4 h-4 mr-2" />
            Save Report
          </Button>
        </div>
      </div>

      {/* Step Navigation */}
      <div className="flex space-x-2 mb-6">
        {['setup', 'fields', 'filters', 'preview'].map((s) => (
          <button
            key={s}
            onClick={() => setStep(s as any)}
            className={`px-4 py-2 rounded-lg capitalize ${
              step === s
                ? 'bg-primary-600 text-white'
                : 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200'
            }`}
          >
            {s}
          </button>
        ))}
      </div>

      {/* Setup Step */}
      {step === 'setup' && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Report Setup</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Report Name</label>
              <Input
                value={report.name}
                onChange={(e) => setReport({ ...report, name: e.target.value })}
                placeholder="Enter report name"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Data Source</label>
              <select
                value={report.dataSource?.id || ''}
                onChange={(e) => handleDataSourceSelect(e.target.value)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
              >
                <option value="">Select data source</option>
                {dataSources.map((ds) => (
                  <option key={ds.id} value={ds.id}>
                    {ds.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </Card>
      )}

      {/* Fields Step */}
      {step === 'fields' && (
        <div className="grid grid-cols-2 gap-6">
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">Available Fields</h2>
            <div className="space-y-2">
              {availableFields.map((field) => (
                <button
                  key={field.name}
                  onClick={() => handleAddField(field)}
                  className="w-full p-3 text-left border border-neutral-200 rounded-lg hover:bg-primary-50 hover:border-primary-300"
                >
                  <div className="font-medium">{field.label || field.name}</div>
                  <div className="text-xs text-neutral-600">{field.dataType}</div>
                </button>
              ))}
            </div>
          </Card>

          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">Selected Fields</h2>
            <div className="space-y-2">
              {report.fields?.map((field) => (
                <div
                  key={field.id}
                  className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
                >
                  <div>
                    <div className="font-medium">{field.label}</div>
                    <div className="text-xs text-neutral-600">{field.dataType}</div>
                  </div>
                  <button
                    onClick={() => handleRemoveField(field.id)}
                    className="text-error-600 hover:text-error-700"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
          </Card>
        </div>
      )}

      {/* Filters Step */}
      {step === 'filters' && (
        <Card className="p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold">Filters</h2>
            <Button variant="outline" size="sm" onClick={handleAddFilter}>
              <Plus className="w-4 h-4 mr-2" />
              Add Filter
            </Button>
          </div>
          <div className="space-y-3">
            {report.filters?.map((filter, index) => (
              <div key={filter.id} className="flex items-center space-x-3 p-3 bg-neutral-50 rounded-lg">
                <select
                  value={filter.field}
                  onChange={(e) => {
                    const newFilters = [...(report.filters || [])];
                    newFilters[index].field = e.target.value;
                    setReport({ ...report, filters: newFilters });
                  }}
                  className="flex-1 px-3 py-2 border border-neutral-300 rounded-lg"
                >
                  <option value="">Select field</option>
                  {report.fields?.map((f) => (
                    <option key={f.id} value={f.name}>
                      {f.label}
                    </option>
                  ))}
                </select>
                <select
                  value={filter.operator}
                  onChange={(e) => {
                    const newFilters = [...(report.filters || [])];
                    newFilters[index].operator = e.target.value as any;
                    setReport({ ...report, filters: newFilters });
                  }}
                  className="px-3 py-2 border border-neutral-300 rounded-lg"
                >
                  <option value="equals">Equals</option>
                  <option value="not_equals">Not Equals</option>
                  <option value="greater_than">Greater Than</option>
                  <option value="less_than">Less Than</option>
                  <option value="contains">Contains</option>
                </select>
                <Input
                  value={filter.value}
                  onChange={(e) => {
                    const newFilters = [...(report.filters || [])];
                    newFilters[index].value = e.target.value;
                    setReport({ ...report, filters: newFilters });
                  }}
                  placeholder="Value"
                />
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Preview Step */}
      {step === 'preview' && reportData && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Report Preview</h2>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  {reportData.columns.map((col) => (
                    <th key={col.name} className="text-left py-3 px-4 font-semibold">
                      {col.label}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {reportData.rows.slice(0, 50).map((row, idx) => (
                  <tr key={idx} className="border-b border-neutral-100">
                    {reportData.columns.map((col) => (
                      <td key={col.name} className="py-3 px-4">
                        {row[col.name]}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-4 text-sm text-neutral-600">
            Showing {Math.min(50, reportData.totalRows)} of {reportData.totalRows} rows
          </div>
        </Card>
      )}
    </div>
  );
};
