import React, { useState, useEffect } from 'react';
import { Repeat, Plus, Play, Pause, Edit, Trash2, Calendar } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { RecurringTemplate } from '../types/recurring.types';
import { recurringService } from '../services/recurringService';

export const RecurringTemplates: React.FC = () => {
  const [templates, setTemplates] = useState<RecurringTemplate[]>([]);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<string>('all');

  useEffect(() => {
    loadTemplates();
  }, [filter]);

  const loadTemplates = async () => {
    setLoading(true);
    try {
      const status = filter === 'all' ? undefined : filter;
      const data = await recurringService.getTemplates(status);
      setTemplates(data);
    } catch (error) {
      console.error('Failed to load templates:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleActivate = async (templateId: string) => {
    try {
      await recurringService.activateTemplate(templateId);
      await loadTemplates();
    } catch (error) {
      console.error('Failed to activate template:', error);
    }
  };

  const handlePause = async (templateId: string) => {
    try {
      await recurringService.pauseTemplate(templateId);
      await loadTemplates();
    } catch (error) {
      console.error('Failed to pause template:', error);
    }
  };

  const handleExecuteNow = async (templateId: string) => {
    if (!confirm('Execute this template now?')) return;
    
    try {
      await recurringService.executeNow(templateId);
      alert('Template executed successfully');
    } catch (error) {
      console.error('Failed to execute template:', error);
    }
  };

  const handleDelete = async (templateId: string) => {
    if (!confirm('Are you sure you want to delete this template?')) return;
    
    try {
      await recurringService.deleteTemplate(templateId);
      await loadTemplates();
    } catch (error) {
      console.error('Failed to delete template:', error);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'bg-success-100 text-success-800';
      case 'paused': return 'bg-warning-100 text-warning-800';
      case 'inactive': return 'bg-neutral-100 text-neutral-800';
      case 'expired': return 'bg-error-100 text-error-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Recurring Transactions</h1>
        <Button variant="primary">
          <Plus className="w-4 h-4 mr-2" />
          Create Template
        </Button>
      </div>

      {/* Filter Tabs */}
      <div className="flex space-x-2 mb-6">
        {['all', 'active', 'paused', 'inactive'].map((status) => (
          <button
            key={status}
            onClick={() => setFilter(status)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              filter === status
                ? 'bg-primary-600 text-white'
                : 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200'
            }`}
          >
            {status.charAt(0).toUpperCase() + status.slice(1)}
          </button>
        ))}
      </div>

      {/* Templates List */}
      <div className="space-y-4">
        {loading ? (
          <Card className="p-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          </Card>
        ) : templates.length === 0 ? (
          <Card className="p-8">
            <div className="text-center text-neutral-600">
              <Repeat className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No recurring templates found</p>
            </div>
          </Card>
        ) : (
          templates.map((template) => (
            <Card key={template.id} className="p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <Repeat className="w-5 h-5 text-primary-600" />
                    <h3 className="text-lg font-semibold">{template.name}</h3>
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(template.status)}`}>
                      {template.status}
                    </span>
                  </div>
                  
                  <p className="text-sm text-neutral-600 mb-3">{template.description}</p>
                  
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                    <div>
                      <div className="text-neutral-600">Type</div>
                      <div className="font-medium">{template.transactionType}</div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Frequency</div>
                      <div className="font-medium">
                        {recurringService.formatFrequency(template.frequency)}
                      </div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Amount</div>
                      <div className="font-medium">
                        {template.amount.type === 'fixed' 
                          ? `₦${template.amount.fixedAmount?.toLocaleString()}`
                          : 'Variable'}
                      </div>
                    </div>
                    <div>
                      <div className="text-neutral-600">Next Run</div>
                      <div className="font-medium flex items-center">
                        <Calendar className="w-4 h-4 mr-1" />
                        {new Date(template.nextRunDate).toLocaleDateString()}
                      </div>
                    </div>
                  </div>

                  {template.lastRunDate && (
                    <div className="mt-3 text-xs text-neutral-500">
                      Last executed: {new Date(template.lastRunDate).toLocaleString()}
                    </div>
                  )}
                </div>

                <div className="flex items-center space-x-2 ml-4">
                  {template.status === 'active' ? (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handlePause(template.id)}
                      title="Pause"
                    >
                      <Pause className="w-4 h-4" />
                    </Button>
                  ) : (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleActivate(template.id)}
                      title="Activate"
                    >
                      <Play className="w-4 h-4" />
                    </Button>
                  )}
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleExecuteNow(template.id)}
                    title="Execute Now"
                  >
                    <Play className="w-4 h-4 text-success-600" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    title="Edit"
                  >
                    <Edit className="w-4 h-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDelete(template.id)}
                    title="Delete"
                  >
                    <Trash2 className="w-4 h-4 text-error-600" />
                  </Button>
                </div>
              </div>
            </Card>
          ))
        )}
      </div>
    </div>
  );
};
