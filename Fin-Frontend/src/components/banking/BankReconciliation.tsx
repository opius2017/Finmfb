import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import axios from 'axios';

interface BankAccount {
  id: string;
  accountName: string;
  accountNumber: string;
  currentBalance: number;
}

interface ReconciliationForm {
  bankAccountId: string;
  reconciliationDate: string;
  statementStartDate: string;
  statementEndDate: string;
  statementOpeningBalance: number;
  statementClosingBalance: number;
  notes?: string;
}

interface Reconciliation {
  id: string;
  bankAccountName: string;
  reconciliationDate: string;
  statementClosingBalance: number;
  bookClosingBalance: number;
  variance: number;
  status: string;
}

const BankReconciliation: React.FC = () => {
  const [selectedAccount, setSelectedAccount] = useState<string>('');
  const [showForm, setShowForm] = useState(false);
  const queryClient = useQueryClient();

  const { register, handleSubmit, formState: { errors }, reset } = useForm<ReconciliationForm>();

  // Fetch bank accounts
  const { data: bankAccounts, isLoading: loadingAccounts } = useQuery<BankAccount[]>({
    queryKey: ['bankAccounts'],
    queryFn: async () => {
      const response = await axios.get('/api/v1/bank-accounts');
      return response.data;
    }
  });

  // Fetch reconciliations
  const { data: reconciliations, isLoading: loadingReconciliations } = useQuery<Reconciliation[]>({
    queryKey: ['reconciliations', selectedAccount],
    queryFn: async () => {
      if (!selectedAccount) return [];
      const response = await axios.get(`/api/v1/bank-reconciliation/bank-account/${selectedAccount}`);
      return response.data;
    },
    enabled: !!selectedAccount
  });

  // Create reconciliation mutation
  const createReconciliation = useMutation({
    mutationFn: async (data: ReconciliationForm) => {
      const response = await axios.post('/api/v1/bank-reconciliation', data);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Bank reconciliation created successfully');
      queryClient.invalidateQueries({ queryKey: ['reconciliations'] });
      setShowForm(false);
      reset();
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to create reconciliation');
    }
  });

  const onSubmit = (data: ReconciliationForm) => {
    createReconciliation.mutate(data);
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN'
    }).format(amount);
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      Draft: 'bg-gray-100 text-gray-800',
      InProgress: 'bg-blue-100 text-blue-800',
      Completed: 'bg-green-100 text-green-800',
      Approved: 'bg-emerald-100 text-emerald-800',
      Rejected: 'bg-red-100 text-red-800'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Bank Reconciliation</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          className="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors"
        >
          {showForm ? 'Cancel' : 'New Reconciliation'}
        </button>
      </div>

      {/* Bank Account Selector */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Select Bank Account
        </label>
        <select
          value={selectedAccount}
          onChange={(e) => setSelectedAccount(e.target.value)}
          className="w-full md:w-96 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
        >
          <option value="">-- Select Bank Account --</option>
          {bankAccounts?.map((account) => (
            <option key={account.id} value={account.id}>
              {account.accountName} - {account.accountNumber}
            </option>
          ))}
        </select>
      </div>

      {/* Create Reconciliation Form */}
      {showForm && (
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Create New Reconciliation</h2>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Bank Account *
                </label>
                <select
                  {...register('bankAccountId', { required: 'Bank account is required' })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                >
                  <option value="">Select account</option>
                  {bankAccounts?.map((account) => (
                    <option key={account.id} value={account.id}>
                      {account.accountName}
                    </option>
                  ))}
                </select>
                {errors.bankAccountId && (
                  <p className="text-red-500 text-sm mt-1">{errors.bankAccountId.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Reconciliation Date *
                </label>
                <input
                  type="date"
                  {...register('reconciliationDate', { required: 'Date is required' })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                />
                {errors.reconciliationDate && (
                  <p className="text-red-500 text-sm mt-1">{errors.reconciliationDate.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Statement Start Date *
                </label>
                <input
                  type="date"
                  {...register('statementStartDate', { required: 'Start date is required' })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                />
                {errors.statementStartDate && (
                  <p className="text-red-500 text-sm mt-1">{errors.statementStartDate.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Statement End Date *
                </label>
                <input
                  type="date"
                  {...register('statementEndDate', { required: 'End date is required' })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                />
                {errors.statementEndDate && (
                  <p className="text-red-500 text-sm mt-1">{errors.statementEndDate.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Statement Opening Balance *
                </label>
                <input
                  type="number"
                  step="0.01"
                  {...register('statementOpeningBalance', { 
                    required: 'Opening balance is required',
                    valueAsNumber: true 
                  })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                />
                {errors.statementOpeningBalance && (
                  <p className="text-red-500 text-sm mt-1">{errors.statementOpeningBalance.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Statement Closing Balance *
                </label>
                <input
                  type="number"
                  step="0.01"
                  {...register('statementClosingBalance', { 
                    required: 'Closing balance is required',
                    valueAsNumber: true 
                  })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                />
                {errors.statementClosingBalance && (
                  <p className="text-red-500 text-sm mt-1">{errors.statementClosingBalance.message}</p>
                )}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Notes
              </label>
              <textarea
                {...register('notes')}
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500"
                placeholder="Add any notes about this reconciliation..."
              />
            </div>

            <div className="flex justify-end space-x-3">
              <button
                type="button"
                onClick={() => setShowForm(false)}
                className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={createReconciliation.isPending}
                className="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors disabled:opacity-50"
              >
                {createReconciliation.isPending ? 'Creating...' : 'Create Reconciliation'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Reconciliations List */}
      {selectedAccount && (
        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold">Reconciliation History</h2>
          </div>
          
          {loadingReconciliations ? (
            <div className="p-8 text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto"></div>
              <p className="mt-4 text-gray-600">Loading reconciliations...</p>
            </div>
          ) : reconciliations && reconciliations.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Date
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Bank Account
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Statement Balance
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Book Balance
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Variance
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {reconciliations.map((recon) => (
                    <tr key={recon.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {new Date(recon.reconciliationDate).toLocaleDateString()}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {recon.bankAccountName}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(recon.statementClosingBalance)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900">
                        {formatCurrency(recon.bookClosingBalance)}
                      </td>
                      <td className={`px-6 py-4 whitespace-nowrap text-sm text-right font-medium ${
                        Math.abs(recon.variance) < 0.01 ? 'text-green-600' : 'text-red-600'
                      }`}>
                        {formatCurrency(recon.variance)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(recon.status)}`}>
                          {recon.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                          onClick={() => window.location.href = `/bank-reconciliation/${recon.id}`}
                          className="text-emerald-600 hover:text-emerald-900"
                        >
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="p-8 text-center text-gray-500">
              No reconciliations found for this account.
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default BankReconciliation;
