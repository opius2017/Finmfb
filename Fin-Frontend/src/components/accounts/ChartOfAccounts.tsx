import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { ChevronRight, ChevronDown, Plus, Edit2, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';
import { useGetAccountsQuery, useCreateAccountMutation, useUpdateAccountMutation, useDeleteAccountMutation } from '../../services/accountsApi';
import AccountModal from './AccountModal';

import { Account } from '../../services/accountsApi';

const AccountRow: React.FC<{
  account: Account;
  expanded: Record<string, boolean>;
  onToggle: (id: string) => void;
  onEdit: (account: Account) => void;
  onDelete: (id: string) => void;
}> = ({ account, expanded, onToggle, onEdit, onDelete }) => {
  const hasChildren = account.children && account.children.length > 0;
  const isExpanded = expanded[account.id];

  return (
    <>
      <motion.div
        initial={false}
        animate={{ backgroundColor: isExpanded ? 'rgb(243 244 246)' : 'transparent' }}
        className="group flex items-center p-2 hover:bg-gray-50 rounded-lg cursor-pointer"
        style={{ paddingLeft: `${account.level * 1.5}rem` }}
      >
        <button
          onClick={() => hasChildren && onToggle(account.id)}
          className={`p-1 rounded-md ${hasChildren ? 'hover:bg-gray-200' : 'invisible'}`}
        >
          {hasChildren && (isExpanded ? <ChevronDown size={16} /> : <ChevronRight size={16} />)}
        </button>

        <span className="flex-1 ml-2">
          <span className="font-mono text-sm text-gray-500">{account.code}</span>
          <span className="ml-3 text-gray-900">{account.name}</span>
        </span>

        <span className={`px-2 py-1 rounded text-sm ${getAccountTypeStyle(account.type)}`}>
          {account.type}
        </span>

        <span className="ml-4 font-mono text-sm text-gray-700">
          {formatCurrency(account.balance)}
        </span>

        <div className="ml-4 opacity-0 group-hover:opacity-100 flex items-center space-x-2">
          <button
            onClick={() => onEdit(account)}
            className="p-1 text-gray-500 hover:text-emerald-600 rounded-md hover:bg-emerald-50"
          >
            <Edit2 size={16} />
          </button>
          <button
            onClick={() => onDelete(account.id)}
            className="p-1 text-gray-500 hover:text-red-600 rounded-md hover:bg-red-50"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </motion.div>

      {hasChildren && isExpanded && (
        <div>
          {account.children?.map((child: Account) => (
            <AccountRow
              key={child.id}
              account={child}
              expanded={expanded}
              onToggle={onToggle}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </>
  );
};

const getAccountTypeStyle = (type: Account['type']) => {
  switch (type) {
    case 'asset':
      return 'bg-blue-100 text-blue-800';
    case 'liability':
      return 'bg-red-100 text-red-800';
    case 'equity':
      return 'bg-purple-100 text-purple-800';
    case 'revenue':
      return 'bg-green-100 text-green-800';
    case 'expense':
      return 'bg-orange-100 text-orange-800';
  }
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('en-NG', {
    style: 'currency',
    currency: 'NGN',
  }).format(amount);
};

const ChartOfAccounts: React.FC = () => {
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});
  const [editingAccount, setEditingAccount] = useState<Account | null>(null);
  const [showNewAccountModal, setShowNewAccountModal] = useState(false);

  const { data: accounts, isLoading } = useGetAccountsQuery();
  const [createAccount] = useCreateAccountMutation();
  const [updateAccount] = useUpdateAccountMutation();
  const [deleteAccount] = useDeleteAccountMutation();

  const handleToggle = (id: string) => {
    setExpanded((prev) => ({ ...prev, [id]: !prev[id] }));
  };

  const handleEdit = (account: Account) => {
    setEditingAccount(account);
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteAccount(id).unwrap();
      toast.success('Account deleted successfully');
    } catch (error) {
      toast.error('Failed to delete account');
    }
  };

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse space-y-4">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="h-12 bg-gray-100 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Chart of Accounts</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage your financial accounts and track balances
          </p>
        </div>

        <button
          onClick={() => setShowNewAccountModal(true)}
          className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2"
        >
          <Plus size={18} className="mr-2" />
          New Account
        </button>
      </div>

      <div className="bg-white rounded-lg shadow divide-y divide-gray-200">
        <div className="p-4 bg-gray-50 rounded-t-lg grid grid-cols-12 text-sm font-medium text-gray-500">
          <div className="col-span-6">Account</div>
          <div className="col-span-2">Type</div>
          <div className="col-span-3">Balance</div>
          <div className="col-span-1">Actions</div>
        </div>

        {accounts?.map((account: Account) => (
          <AccountRow
            key={account.id}
            account={account}
            expanded={expanded}
            onToggle={handleToggle}
            onEdit={handleEdit}
            onDelete={handleDelete}
          />
        ))}
      </div>

      <div className="flex space-x-2 mt-4">
        <button
          onClick={async () => {
            try {
              await fetch(`/api/chartofaccounts/template/cbn?tenantId=default`, { method: 'POST' });
              toast.success('CBN template initialized');
            } catch (error) {
              toast.error('Failed to initialize CBN template');
            }
          }}
          className="px-3 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Initialize CBN Template
        </button>
        <button
          onClick={async () => {
            try {
              await fetch(`/api/chartofaccounts/template/ndic?tenantId=default`, { method: 'POST' });
              toast.success('NDIC template initialized');
            } catch (error) {
              toast.error('Failed to initialize NDIC template');
            }
          }}
          className="px-3 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
        >
          Initialize NDIC Template
        </button>
        <button
          onClick={async () => {
            try {
              await fetch(`/api/chartofaccounts/template/ifrs?tenantId=default`, { method: 'POST' });
              toast.success('IFRS template initialized');
            } catch (error) {
              toast.error('Failed to initialize IFRS template');
            }
          }}
          className="px-3 py-2 bg-orange-600 text-white rounded hover:bg-orange-700"
        >
          Initialize IFRS Template
        </button>
      </div>

      <AccountModal
        isOpen={showNewAccountModal}
        onClose={() => setShowNewAccountModal(false)}
        accounts={accounts}
        onSubmit={async (data) => {
          try {
            await createAccount(data).unwrap();
            toast.success('Account created successfully');
          } catch (error) {
            toast.error('Failed to create account');
          }
        }}
      />

      <AccountModal
        isOpen={!!editingAccount}
        onClose={() => setEditingAccount(null)}
        accounts={accounts}
        editingAccount={editingAccount}
        title="Edit Account"
        onSubmit={async (data) => {
          try {
            await updateAccount({
              id: editingAccount?.id!,
              ...data,
            }).unwrap();
            toast.success('Account updated successfully');
          } catch (error) {
            toast.error('Failed to update account');
          }
        }}
      />
    </div>
  );
};

export default ChartOfAccounts;