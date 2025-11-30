import React, { useState, useEffect } from 'react';
import { ChevronRight, ChevronLeft, Check, FileText, Copy, Sparkles } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { Budget, BudgetTemplate, BudgetCopyOptions } from '../types/budget.types';
import { budgetService } from '../services/budgetService';

type WizardStep = 'method' | 'details' | 'template' | 'copy' | 'review';

interface BudgetWizardProps {
  onComplete: (budget: Budget) => void;
  onCancel: () => void;
}

export const BudgetWizard: React.FC<BudgetWizardProps> = ({ onComplete, onCancel }) => {
  const [step, setStep] = useState<WizardStep>('method');
  const [creationMethod, setCreationMethod] = useState<'blank' | 'template' | 'copy' | null>(null);
  const [templates, setTemplates] = useState<BudgetTemplate[]>([]);
  const [selectedTemplate, setSelectedTemplate] = useState<BudgetTemplate | null>(null);
  const [priorBudgets, setPriorBudgets] = useState<Budget[]>([]);
  const [selectedBudget, setSelectedBudget] = useState<Budget | null>(null);
  const [loading, setLoading] = useState(false);

  const [budgetDetails, setBudgetDetails] = useState({
    name: '',
    fiscalYear: new Date().getFullYear(),
    type: 'operating' as const,
  });

  const [copyOptions, setCopyOptions] = useState<Partial<BudgetCopyOptions>>({
    adjustmentType: 'percentage',
    adjustmentValue: 0,
    copyActuals: false,
    copyNotes: true,
  });

  useEffect(() => {
    if (step === 'template') {
      loadTemplates();
    } else if (step === 'copy') {
      loadPriorBudgets();
    }
  }, [step]);

  const loadTemplates = async () => {
    setLoading(true);
    try {
      const templateList = await budgetService.getTemplates();
      setTemplates(templateList);
    } catch (error) {
      console.error('Failed to load templates:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadPriorBudgets = async () => {
    setLoading(true);
    try {
      const result = await budgetService.getBudgets({
        status: 'approved',
        fiscalYear: budgetDetails.fiscalYear - 1,
      });
      setPriorBudgets(result.budgets);
    } catch (error) {
      console.error('Failed to load prior budgets:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleMethodSelect = (method: 'blank' | 'template' | 'copy') => {
    setCreationMethod(method);
    if (method === 'blank') {
      setStep('details');
    } else if (method === 'template') {
      setStep('template');
    } else {
      setStep('copy');
    }
  };

  const handleCreateBudget = async () => {
    setLoading(true);
    try {
      let budget: Budget;

      if (creationMethod === 'blank') {
        budget = await budgetService.createBudget({
          name: budgetDetails.name,
          fiscalYear: budgetDetails.fiscalYear,
          type: budgetDetails.type,
          status: 'draft',
          lines: [],
        });
      } else if (creationMethod === 'template' && selectedTemplate) {
        budget = await budgetService.createFromTemplate(
          selectedTemplate.id,
          budgetDetails.fiscalYear,
          budgetDetails.name
        );
      } else if (creationMethod === 'copy' && selectedBudget) {
        budget = await budgetService.copyFromPriorYear({
          sourceBudgetId: selectedBudget.id,
          targetYear: budgetDetails.fiscalYear,
          adjustmentType: copyOptions.adjustmentType!,
          adjustmentValue: copyOptions.adjustmentValue!,
          copyActuals: copyOptions.copyActuals!,
          copyNotes: copyOptions.copyNotes!,
        });
      } else {
        throw new Error('Invalid creation method');
      }

      onComplete(budget);
    } catch (error) {
      console.error('Failed to create budget:', error);
    } finally {
      setLoading(false);
    }
  };

  const renderMethodSelection = () => (
    <Card className="p-6">
      <h2 className="text-xl font-bold mb-6">Create New Budget</h2>
      <p className="text-neutral-600 mb-6">Choose how you want to create your budget</p>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <button
          onClick={() => handleMethodSelect('blank')}
          className="flex flex-col items-center justify-center p-6 border-2 border-dashed border-neutral-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
        >
          <FileText className="w-12 h-12 text-primary-600 mb-3" />
          <h3 className="font-semibold mb-1">Start from Scratch</h3>
          <p className="text-sm text-neutral-600 text-center">
            Create a blank budget and add accounts manually
          </p>
        </button>

        <button
          onClick={() => handleMethodSelect('template')}
          className="flex flex-col items-center justify-center p-6 border-2 border-dashed border-neutral-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
        >
          <Sparkles className="w-12 h-12 text-primary-600 mb-3" />
          <h3 className="font-semibold mb-1">Use Template</h3>
          <p className="text-sm text-neutral-600 text-center">
            Start with industry-standard budget templates
          </p>
        </button>

        <button
          onClick={() => handleMethodSelect('copy')}
          className="flex flex-col items-center justify-center p-6 border-2 border-dashed border-neutral-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
        >
          <Copy className="w-12 h-12 text-primary-600 mb-3" />
          <h3 className="font-semibold mb-1">Copy from Prior Year</h3>
          <p className="text-sm text-neutral-600 text-center">
            Copy and adjust last year's budget
          </p>
        </button>
      </div>

      <div className="mt-6 flex justify-end">
        <Button variant="outline" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </Card>
  );

  const renderDetails = () => (
    <Card className="p-6">
      <h2 className="text-xl font-bold mb-6">Budget Details</h2>

      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Budget Name *
          </label>
          <Input
            value={budgetDetails.name}
            onChange={(e) => setBudgetDetails({ ...budgetDetails, name: e.target.value })}
            placeholder="e.g., FY2025 Operating Budget"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Fiscal Year *
          </label>
          <Input
            type="number"
            value={budgetDetails.fiscalYear}
            onChange={(e) =>
              setBudgetDetails({ ...budgetDetails, fiscalYear: parseInt(e.target.value) })
            }
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-1">
            Budget Type *
          </label>
          <select
            value={budgetDetails.type}
            onChange={(e) =>
              setBudgetDetails({ ...budgetDetails, type: e.target.value as any })
            }
            className="w-full px-3 py-2 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
          >
            <option value="operating">Operating Budget</option>
            <option value="capital">Capital Budget</option>
            <option value="cash-flow">Cash Flow Budget</option>
            <option value="project">Project Budget</option>
            <option value="departmental">Departmental Budget</option>
          </select>
        </div>
      </div>

      <div className="mt-6 flex justify-between">
        <Button variant="outline" onClick={() => setStep('method')}>
          <ChevronLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
        <Button
          variant="primary"
          onClick={handleCreateBudget}
          disabled={!budgetDetails.name || loading}
        >
          Create Budget
          <ChevronRight className="w-4 h-4 ml-2" />
        </Button>
      </div>
    </Card>
  );

  const renderTemplateSelection = () => (
    <Card className="p-6">
      <h2 className="text-xl font-bold mb-6">Select Template</h2>

      <div className="space-y-3 mb-6">
        {templates.map((template) => (
          <button
            key={template.id}
            onClick={() => setSelectedTemplate(template)}
            className={`w-full p-4 border-2 rounded-lg text-left transition-colors ${
              selectedTemplate?.id === template.id
                ? 'border-primary-500 bg-primary-50'
                : 'border-neutral-200 hover:border-primary-300'
            }`}
          >
            <div className="flex items-center justify-between">
              <div>
                <h3 className="font-semibold">{template.name}</h3>
                <p className="text-sm text-neutral-600 mt-1">{template.description}</p>
                <div className="flex items-center space-x-2 mt-2">
                  <span className="text-xs px-2 py-1 bg-neutral-100 rounded">
                    {template.industry}
                  </span>
                  <span className="text-xs px-2 py-1 bg-neutral-100 rounded">
                    {template.accounts.length} accounts
                  </span>
                </div>
              </div>
              {selectedTemplate?.id === template.id && (
                <Check className="w-6 h-6 text-primary-600" />
              )}
            </div>
          </button>
        ))}
      </div>

      <div className="flex justify-between">
        <Button variant="outline" onClick={() => setStep('method')}>
          <ChevronLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
        <Button
          variant="primary"
          onClick={() => setStep('details')}
          disabled={!selectedTemplate}
        >
          Continue
          <ChevronRight className="w-4 h-4 ml-2" />
        </Button>
      </div>
    </Card>
  );

  const renderCopyOptions = () => (
    <Card className="p-6">
      <h2 className="text-xl font-bold mb-6">Copy from Prior Year</h2>

      <div className="space-y-4 mb-6">
        <div>
          <label className="block text-sm font-medium text-neutral-700 mb-2">
            Select Budget to Copy
          </label>
          <div className="space-y-2">
            {priorBudgets.map((budget) => (
              <button
                key={budget.id}
                onClick={() => setSelectedBudget(budget)}
                className={`w-full p-3 border-2 rounded-lg text-left transition-colors ${
                  selectedBudget?.id === budget.id
                    ? 'border-primary-500 bg-primary-50'
                    : 'border-neutral-200 hover:border-primary-300'
                }`}
              >
                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-semibold">{budget.name}</div>
                    <div className="text-sm text-neutral-600">
                      ₦{budget.totalBudget.toLocaleString()}
                    </div>
                  </div>
                  {selectedBudget?.id === budget.id && (
                    <Check className="w-5 h-5 text-primary-600" />
                  )}
                </div>
              </button>
            ))}
          </div>
        </div>

        {selectedBudget && (
          <>
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                Adjustment Type
              </label>
              <select
                value={copyOptions.adjustmentType}
                onChange={(e) =>
                  setCopyOptions({ ...copyOptions, adjustmentType: e.target.value as any })
                }
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
              >
                <option value="none">No Adjustment</option>
                <option value="percentage">Percentage Increase/Decrease</option>
                <option value="fixed">Fixed Amount Increase/Decrease</option>
              </select>
            </div>

            {copyOptions.adjustmentType !== 'none' && (
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  Adjustment Value
                </label>
                <Input
                  type="number"
                  value={copyOptions.adjustmentValue}
                  onChange={(e) =>
                    setCopyOptions({
                      ...copyOptions,
                      adjustmentValue: parseFloat(e.target.value),
                    })
                  }
                  placeholder={
                    copyOptions.adjustmentType === 'percentage' ? 'e.g., 5 for 5%' : 'Amount'
                  }
                />
              </div>
            )}

            <div className="space-y-2">
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={copyOptions.copyActuals}
                  onChange={(e) =>
                    setCopyOptions({ ...copyOptions, copyActuals: e.target.checked })
                  }
                  className="rounded"
                />
                <span className="text-sm">Copy actual amounts as budget</span>
              </label>
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={copyOptions.copyNotes}
                  onChange={(e) =>
                    setCopyOptions({ ...copyOptions, copyNotes: e.target.checked })
                  }
                  className="rounded"
                />
                <span className="text-sm">Copy notes and comments</span>
              </label>
            </div>
          </>
        )}
      </div>

      <div className="flex justify-between">
        <Button variant="outline" onClick={() => setStep('method')}>
          <ChevronLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
        <Button
          variant="primary"
          onClick={() => setStep('details')}
          disabled={!selectedBudget}
        >
          Continue
          <ChevronRight className="w-4 h-4 ml-2" />
        </Button>
      </div>
    </Card>
  );

  if (step === 'method') return renderMethodSelection();
  if (step === 'template') return renderTemplateSelection();
  if (step === 'copy') return renderCopyOptions();
  if (step === 'details') return renderDetails();

  return null;
};
