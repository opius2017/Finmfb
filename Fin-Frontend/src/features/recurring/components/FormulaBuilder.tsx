import React, { useState } from 'react';
import { Calculator, Plus, TestTube, CheckCircle, XCircle } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { Input } from '../../../design-system/components/Input';
import { AmountVariable } from '../types/recurring.types';
import { recurringService } from '../services/recurringService';

interface FormulaBuilderProps {
  formula: string;
  variables: AmountVariable[];
  onChange: (formula: string, variables: AmountVariable[]) => void;
}

export const FormulaBuilder: React.FC<FormulaBuilderProps> = ({
  formula,
  variables,
  onChange,
}) => {
  const [testValues, setTestValues] = useState<Record<string, number>>({});
  const [testResult, setTestResult] = useState<number | null>(null);
  const [testError, setTestError] = useState<string | null>(null);
  const [validationResult, setValidationResult] = useState<{ valid: boolean; error?: string } | null>(null);

  const commonOperators = [
    { label: '+', value: '+', description: 'Addition' },
    { label: '-', value: '-', description: 'Subtraction' },
    { label: '×', value: '*', description: 'Multiplication' },
    { label: '÷', value: '/', description: 'Division' },
    { label: '(', value: '(', description: 'Open parenthesis' },
    { label: ')', value: ')', description: 'Close parenthesis' },
  ];

  const commonFunctions = [
    { label: 'MAX', value: 'Math.max(', description: 'Maximum value' },
    { label: 'MIN', value: 'Math.min(', description: 'Minimum value' },
    { label: 'ROUND', value: 'Math.round(', description: 'Round to nearest integer' },
    { label: 'ABS', value: 'Math.abs(', description: 'Absolute value' },
  ];

  const handleAddVariable = () => {
    const newVariable: AmountVariable = {
      name: `var${variables.length + 1}`,
      source: 'manual',
      defaultValue: 0,
    };
    onChange(formula, [...variables, newVariable]);
  };

  const handleUpdateVariable = (index: number, updates: Partial<AmountVariable>) => {
    const updated = [...variables];
    updated[index] = { ...updated[index], ...updates };
    onChange(formula, updated);
  };

  const handleRemoveVariable = (index: number) => {
    const updated = variables.filter((_, i) => i !== index);
    onChange(formula, updated);
  };

  const handleInsert = (text: string) => {
    onChange(formula + text, variables);
  };

  const handleValidate = () => {
    const result = recurringService.validateFormula(formula);
    setValidationResult(result);
  };

  const handleTest = async () => {
    setTestError(null);
    setTestResult(null);

    try {
      const result = await recurringService.testFormula(formula, testValues);
      setTestResult(result);
    } catch (error) {
      setTestError(error instanceof Error ? error.message : 'Test failed');
    }
  };

  return (
    <div className="space-y-6">
      {/* Formula Input */}
      <Card className="p-6">
        <div className="flex items-center space-x-3 mb-4">
          <Calculator className="w-6 h-6 text-primary-600" />
          <h3 className="text-lg font-semibold">Formula Builder</h3>
        </div>

        <div className="mb-4">
          <label className="block text-sm font-medium text-neutral-700 mb-2">
            Formula Expression
          </label>
          <textarea
            value={formula}
            onChange={(e) => onChange(e.target.value, variables)}
            className="w-full px-3 py-2 border border-neutral-300 rounded-lg font-mono text-sm"
            rows={3}
            placeholder="e.g., baseAmount * (1 + inflationRate / 100)"
          />
          <p className="text-xs text-neutral-600 mt-1">
            Use variable names in your formula. Example: amount * 1.05 or (revenue - costs) * 0.1
          </p>
        </div>

        {/* Quick Insert Buttons */}
        <div className="space-y-3">
          <div>
            <div className="text-sm font-medium text-neutral-700 mb-2">Operators</div>
            <div className="flex flex-wrap gap-2">
              {commonOperators.map((op) => (
                <button
                  key={op.value}
                  onClick={() => handleInsert(op.value)}
                  className="px-3 py-1 bg-neutral-100 hover:bg-neutral-200 rounded text-sm font-medium"
                  title={op.description}
                >
                  {op.label}
                </button>
              ))}
            </div>
          </div>

          <div>
            <div className="text-sm font-medium text-neutral-700 mb-2">Functions</div>
            <div className="flex flex-wrap gap-2">
              {commonFunctions.map((func) => (
                <button
                  key={func.value}
                  onClick={() => handleInsert(func.value)}
                  className="px-3 py-1 bg-primary-100 hover:bg-primary-200 text-primary-700 rounded text-sm font-medium"
                  title={func.description}
                >
                  {func.label}
                </button>
              ))}
            </div>
          </div>

          <div>
            <div className="text-sm font-medium text-neutral-700 mb-2">Variables</div>
            <div className="flex flex-wrap gap-2">
              {variables.map((variable) => (
                <button
                  key={variable.name}
                  onClick={() => handleInsert(variable.name)}
                  className="px-3 py-1 bg-success-100 hover:bg-success-200 text-success-700 rounded text-sm font-medium"
                >
                  {variable.name}
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="flex space-x-3 mt-4">
          <Button variant="outline" onClick={handleValidate}>
            Validate Formula
          </Button>
        </div>

        {validationResult && (
          <div className={`mt-4 p-3 rounded-lg flex items-start space-x-2 ${
            validationResult.valid 
              ? 'bg-success-50 text-success-700' 
              : 'bg-error-50 text-error-700'
          }`}>
            {validationResult.valid ? (
              <CheckCircle className="w-5 h-5 flex-shrink-0 mt-0.5" />
            ) : (
              <XCircle className="w-5 h-5 flex-shrink-0 mt-0.5" />
            )}
            <div>
              <div className="font-medium">
                {validationResult.valid ? 'Formula is valid' : 'Formula has errors'}
              </div>
              {validationResult.error && (
                <div className="text-sm mt-1">{validationResult.error}</div>
              )}
            </div>
          </div>
        )}
      </Card>

      {/* Variables Configuration */}
      <Card className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Variables</h3>
          <Button variant="outline" size="sm" onClick={handleAddVariable}>
            <Plus className="w-4 h-4 mr-2" />
            Add Variable
          </Button>
        </div>

        <div className="space-y-4">
          {variables.map((variable, index) => (
            <div key={index} className="p-4 border border-neutral-200 rounded-lg">
              <div className="grid grid-cols-2 gap-4 mb-3">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">
                    Variable Name
                  </label>
                  <Input
                    value={variable.name}
                    onChange={(e) => handleUpdateVariable(index, { name: e.target.value })}
                    placeholder="e.g., baseAmount"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">
                    Source
                  </label>
                  <select
                    value={variable.source}
                    onChange={(e) => handleUpdateVariable(index, { source: e.target.value as any })}
                    className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
                  >
                    <option value="manual">Manual Entry</option>
                    <option value="database">Database Query</option>
                    <option value="api">API Call</option>
                    <option value="calculation">Calculation</option>
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-neutral-700 mb-1">
                    Default Value
                  </label>
                  <Input
                    type="number"
                    value={variable.defaultValue || 0}
                    onChange={(e) => handleUpdateVariable(index, { defaultValue: parseFloat(e.target.value) })}
                  />
                </div>
                {variable.source === 'database' && (
                  <div>
                    <label className="block text-sm font-medium text-neutral-700 mb-1">
                      Query
                    </label>
                    <Input
                      value={variable.query || ''}
                      onChange={(e) => handleUpdateVariable(index, { query: e.target.value })}
                      placeholder="SQL query or field path"
                    />
                  </div>
                )}
              </div>

              <div className="mt-3 flex justify-end">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleRemoveVariable(index)}
                >
                  Remove
                </Button>
              </div>
            </div>
          ))}

          {variables.length === 0 && (
            <div className="text-center py-8 text-neutral-600">
              <p className="text-sm">No variables defined. Add variables to use in your formula.</p>
            </div>
          )}
        </div>
      </Card>

      {/* Formula Testing */}
      <Card className="p-6">
        <div className="flex items-center space-x-3 mb-4">
          <TestTube className="w-6 h-6 text-primary-600" />
          <h3 className="text-lg font-semibold">Test Formula</h3>
        </div>

        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            {variables.map((variable) => (
              <div key={variable.name}>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  {variable.name}
                </label>
                <Input
                  type="number"
                  value={testValues[variable.name] || variable.defaultValue || 0}
                  onChange={(e) => setTestValues({
                    ...testValues,
                    [variable.name]: parseFloat(e.target.value) || 0,
                  })}
                />
              </div>
            ))}
          </div>

          <Button variant="primary" onClick={handleTest}>
            <TestTube className="w-4 h-4 mr-2" />
            Test Formula
          </Button>

          {testResult !== null && (
            <div className="p-4 bg-success-50 border border-success-200 rounded-lg">
              <div className="text-sm font-medium text-success-900 mb-1">Result</div>
              <div className="text-2xl font-bold text-success-700">
                ₦{testResult.toLocaleString()}
              </div>
            </div>
          )}

          {testError && (
            <div className="p-4 bg-error-50 border border-error-200 rounded-lg">
              <div className="text-sm font-medium text-error-900 mb-1">Error</div>
              <div className="text-sm text-error-700">{testError}</div>
            </div>
          )}
        </div>
      </Card>
    </div>
  );
};
