# Admin UI & Frontend CRUD Patterns - Complete Reference

## 1. React Admin Dashboard Layout

```typescript
// src/features/admin/layout/AdminLayout.tsx
import { useState } from 'react';
import { useAuth } from '@/services/authService';
import Sidebar from './Sidebar';
import TopBar from './TopBar';

export const AdminLayout = ({ children }: { children: React.ReactNode }) => {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const { user } = useAuth();

  return (
    <div className="flex h-screen bg-gray-100">
      <Sidebar isOpen={sidebarOpen} />
      <div className="flex-1 flex flex-col">
        <TopBar onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} user={user} />
        <main className="flex-1 overflow-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
};
```

```typescript
// src/features/admin/layout/Sidebar.tsx
import { useAuth } from '@/services/authService';
import { Link } from 'react-router-dom';

export const Sidebar = ({ isOpen }: { isOpen: boolean }) => {
  const { hasRole } = useAuth();

  return (
    <aside className={`${isOpen ? 'w-64' : 'w-20'} bg-gray-900 text-white transition-all duration-300`}>
      <nav className="p-4 space-y-4">
        {hasRole('FixedAssetAdmin') && (
          <Link to="/admin/fixed-assets" className="block p-2 rounded hover:bg-gray-800">
            📦 Fixed Assets
          </Link>
        )}
        {hasRole('LoanOfficer') && (
          <Link to="/admin/loans" className="block p-2 rounded hover:bg-gray-800">
            💰 Loans
          </Link>
        )}
        {hasRole('Accountant') && (
          <Link to="/admin/accounting" className="block p-2 rounded hover:bg-gray-800">
            📊 Accounting
          </Link>
        )}
      </nav>
    </aside>
  );
};
```

## 2. Data Grid / List Component

```typescript
// src/components/DataGrid.tsx
import { useState, useEffect } from 'react';
import { useAuth } from '@/services/authService';

interface DataGridProps<T> {
  columns: ColumnDef<T>[];
  data: T[];
  loading: boolean;
  onDelete: (id: string) => Promise<void>;
  onEdit: (item: T) => void;
  canDelete?: boolean;
  canEdit?: boolean;
}

interface ColumnDef<T> {
  key: keyof T;
  label: string;
  render?: (value: any, item: T) => React.ReactNode;
  width?: string;
}

export const DataGrid = <T extends { id: string }>({
  columns,
  data,
  loading,
  onDelete,
  onEdit,
  canDelete = true,
  canEdit = true,
}: DataGridProps<T>) => {
  const [deleting, setDeleting] = useState<string | null>(null);

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure?')) return;
    try {
      setDeleting(id);
      await onDelete(id);
    } finally {
      setDeleting(null);
    }
  };

  if (loading) return <div className="text-center py-8">Loading...</div>;
  if (data.length === 0) return <div className="text-center py-8 text-gray-500">No data found</div>;

  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full">
        <thead className="bg-gray-200 border-b">
          <tr>
            {columns.map(col => (
              <th key={String(col.key)} className="px-4 py-3 text-left font-semibold">
                {col.label}
              </th>
            ))}
            {(canEdit || canDelete) && <th className="px-4 py-3 text-center">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {data.map((item, idx) => (
            <tr key={item.id} className={idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
              {columns.map(col => (
                <td key={String(col.key)} className="px-4 py-3">
                  {col.render ? col.render(item[col.key], item) : String(item[col.key])}
                </td>
              ))}
              {(canEdit || canDelete) && (
                <td className="px-4 py-3 text-center space-x-2">
                  {canEdit && (
                    <button
                      onClick={() => onEdit(item)}
                      className="text-blue-600 hover:underline"
                    >
                      Edit
                    </button>
                  )}
                  {canDelete && (
                    <button
                      onClick={() => handleDelete(item.id)}
                      disabled={deleting === item.id}
                      className="text-red-600 hover:underline disabled:opacity-50"
                    >
                      {deleting === item.id ? 'Deleting...' : 'Delete'}
                    </button>
                  )}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
```

## 3. Smart Form Component

```typescript
// src/components/SmartForm.tsx
import { useForm, Controller, FieldValues, Path } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ZodSchema } from 'zod';

interface FormField<T extends FieldValues> {
  name: Path<T>;
  label: string;
  type: 'text' | 'email' | 'number' | 'date' | 'select' | 'textarea';
  required?: boolean;
  options?: { value: string | number; label: string }[];
  min?: number;
  max?: number;
  step?: number;
  placeholder?: string;
}

interface SmartFormProps<T extends FieldValues> {
  schema: ZodSchema;
  fields: FormField<T>[];
  onSubmit: (data: T) => Promise<void>;
  onCancel?: () => void;
  submitLabel?: string;
  defaultValues?: Partial<T>;
  isLoading?: boolean;
}

export const SmartForm = <T extends FieldValues>({
  schema,
  fields,
  onSubmit,
  onCancel,
  submitLabel = 'Submit',
  defaultValues,
  isLoading = false,
}: SmartFormProps<T>) => {
  const { control, handleSubmit, formState: { errors }, watch } = useForm<T>({
    resolver: zodResolver(schema),
    defaultValues: defaultValues as any,
  });

  const [serverError, setServerError] = React.useState<string | null>(null);

  const handleFormSubmit = async (data: T) => {
    try {
      setServerError(null);
      await onSubmit(data);
    } catch (error) {
      setServerError(error instanceof Error ? error.message : 'An error occurred');
    }
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4 max-w-2xl">
      {serverError && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {serverError}
        </div>
      )}

      {fields.map(field => (
        <FormField
          key={String(field.name)}
          control={control}
          field={field}
          error={errors[field.name]?.message as string}
        />
      ))}

      <div className="flex gap-2 pt-4">
        <button
          type="submit"
          disabled={isLoading}
          className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {isLoading ? 'Loading...' : submitLabel}
        </button>
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="bg-gray-300 text-gray-800 px-6 py-2 rounded hover:bg-gray-400"
          >
            Cancel
          </button>
        )}
      </div>
    </form>
  );
};

const FormField = ({ control, field, error }: any) => {
  return (
    <div>
      <label className="block font-medium mb-1">
        {field.label}
        {field.required && <span className="text-red-500">*</span>}
      </label>

      <Controller
        name={field.name}
        control={control}
        render={({ field: fieldProps }) => {
          const baseClasses = 'w-full border rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500';

          if (field.type === 'select') {
            return (
              <select {...fieldProps} className={baseClasses}>
                <option value="">Select {field.label.toLowerCase()}</option>
                {field.options?.map(opt => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            );
          }

          if (field.type === 'textarea') {
            return (
              <textarea
                {...fieldProps}
                placeholder={field.placeholder}
                className={`${baseClasses} min-h-24`}
              />
            );
          }

          return (
            <input
              {...fieldProps}
              type={field.type}
              placeholder={field.placeholder}
              min={field.min}
              max={field.max}
              step={field.step}
              className={baseClasses}
            />
          );
        }}
      />

      {error && <p className="text-red-500 text-sm mt-1">{error}</p>}
    </div>
  );
};
```

## 4. CRUD Page Template

```typescript
// src/features/admin/FixedAssets/FixedAssetsPage.tsx
import { useState, useEffect } from 'react';
import { useAuth } from '@/services/authService';
import { DataGrid } from '@/components/DataGrid';
import { CreateAssetModal } from './components/CreateAssetModal';
import { EditAssetModal } from './components/EditAssetModal';

interface FixedAsset {
  id: string;
  assetCode: string;
  assetName: string;
  currentBookValue: number;
  status: 'Active' | 'Inactive' | 'Disposed';
}

export const FixedAssetsPage = () => {
  const { hasRole } = useAuth();
  const [assets, setAssets] = useState<FixedAsset[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedAsset, setSelectedAsset] = useState<FixedAsset | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);

  useEffect(() => {
    fetchAssets();
  }, []);

  const fetchAssets = async () => {
    try {
      const response = await fetch('/api/v1/fixedassets?pageSize=100');
      const data = await response.json();
      setAssets(data.items || []);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    const response = await fetch(`/api/v1/fixedassets/${id}`, { method: 'DELETE' });
    if (response.ok) {
      setAssets(assets.filter(a => a.id !== id));
    } else {
      throw new Error('Failed to delete asset');
    }
  };

  const handleCreateSuccess = (newAsset: FixedAsset) => {
    setAssets([...assets, newAsset]);
    setShowCreateModal(false);
  };

  const handleEditSuccess = (updatedAsset: FixedAsset) => {
    setAssets(assets.map(a => a.id === updatedAsset.id ? updatedAsset : a));
    setShowEditModal(false);
    setSelectedAsset(null);
  };

  const columns = [
    { key: 'assetCode' as const, label: 'Asset Code' },
    { key: 'assetName' as const, label: 'Name' },
    {
      key: 'currentBookValue' as const,
      label: 'Book Value',
      render: (value: number) => `$${value.toFixed(2)}`
    },
    {
      key: 'status' as const,
      label: 'Status',
      render: (value: string) => (
        <span className={`px-2 py-1 rounded text-sm font-medium
          ${value === 'Active' ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-700'}
        `}>
          {value}
        </span>
      )
    }
  ];

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Fixed Assets</h1>
        {hasRole('FixedAssetAdmin') && (
          <button
            onClick={() => setShowCreateModal(true)}
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
          >
            + New Asset
          </button>
        )}
      </div>

      <DataGrid
        columns={columns}
        data={assets}
        loading={loading}
        onDelete={handleDelete}
        onEdit={(asset) => {
          setSelectedAsset(asset);
          setShowEditModal(true);
        }}
        canDelete={hasRole('Admin')}
        canEdit={hasRole('FixedAssetAdmin')}
      />

      {showCreateModal && (
        <CreateAssetModal
          onClose={() => setShowCreateModal(false)}
          onSuccess={handleCreateSuccess}
        />
      )}

      {showEditModal && selectedAsset && (
        <EditAssetModal
          asset={selectedAsset}
          onClose={() => setShowEditModal(false)}
          onSuccess={handleEditSuccess}
        />
      )}
    </div>
  );
};
```

## 5. Modal Wrapper Component

```typescript
// src/components/Modal.tsx
interface ModalProps {
  isOpen: boolean;
  title: string;
  onClose: () => void;
  children: React.ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

export const Modal = ({ isOpen, title, onClose, children, size = 'md' }: ModalProps) => {
  if (!isOpen) return null;

  const sizeClasses = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-2xl',
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className={`bg-white rounded-lg shadow-xl ${sizeClasses[size]} w-full mx-4`}>
        <div className="flex justify-between items-center p-6 border-b">
          <h2 className="text-xl font-bold">{title}</h2>
          <button onClick={onClose} className="text-gray-500 hover:text-gray-700 text-2xl">
            ×
          </button>
        </div>
        <div className="p-6 max-h-[calc(100vh-200px)] overflow-y-auto">
          {children}
        </div>
      </div>
    </div>
  );
};
```

## 6. API Service Layer

```typescript
// src/services/api.ts
const API_BASE = '/api/v1';

export const apiService = {
  async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const response = await fetch(`${API_BASE}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ error: { description: 'Unknown error' } }));
      throw new Error(error.error?.description || 'Request failed');
    }

    return response.json();
  },

  get<T>(endpoint: string) {
    return this.request<T>(endpoint, { method: 'GET' });
  },

  post<T>(endpoint: string, body: any) {
    return this.request<T>(endpoint, { method: 'POST', body: JSON.stringify(body) });
  },

  put<T>(endpoint: string, body: any) {
    return this.request<T>(endpoint, { method: 'PUT', body: JSON.stringify(body) });
  },

  delete(endpoint: string) {
    return this.request<void>(endpoint, { method: 'DELETE' });
  },
};

// Usage
export const fixedAssetService = {
  list: () => apiService.get('/fixedassets'),
  create: (data: any) => apiService.post('/fixedassets', data),
  update: (id: string, data: any) => apiService.put(`/fixedassets/${id}`, data),
  delete: (id: string) => apiService.delete(`/fixedassets/${id}`),
  getById: (id: string) => apiService.get(`/fixedassets/${id}`),
};

export const loanService = {
  list: () => apiService.get('/loans'),
  create: (data: any) => apiService.post('/loans', data),
  approve: (id: string) => apiService.post(`/loans/${id}/approve`, {}),
  reject: (id: string, reason: string) => apiService.post(`/loans/${id}/reject`, { reason }),
};

export const accountingService = {
  createEntry: (data: any) => apiService.post('/accounting/journal-entries', data),
  postEntry: (id: string) => apiService.post(`/accounting/journal-entries/${id}/post`, {}),
  getTrialBalance: () => apiService.get('/accounting/trial-balance'),
};
```

## 7. Status Badge Component

```typescript
// src/components/StatusBadge.tsx
interface StatusBadgeProps {
  status: string;
  variants?: { [key: string]: { bg: string; text: string; icon?: string } };
}

export const StatusBadge = ({ status, variants }: StatusBadgeProps) => {
  const defaultVariants = {
    Active: { bg: 'bg-green-100', text: 'text-green-700', icon: '✓' },
    Pending: { bg: 'bg-yellow-100', text: 'text-yellow-700', icon: '⏳' },
    Approved: { bg: 'bg-blue-100', text: 'text-blue-700', icon: '✓' },
    Rejected: { bg: 'bg-red-100', text: 'text-red-700', icon: '✗' },
    Drafted: { bg: 'bg-gray-100', text: 'text-gray-700', icon: '✎' },
    Posted: { bg: 'bg-green-100', text: 'text-green-700', icon: '✓' },
    Disposed: { bg: 'bg-red-100', text: 'text-red-700', icon: '🗑' },
  };

  const variant = variants?.[status] || defaultVariants[status as keyof typeof defaultVariants];
  if (!variant) return <span>{status}</span>;

  return (
    <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-sm font-medium ${variant.bg} ${variant.text}`}>
      {variant.icon && <span>{variant.icon}</span>}
      {status}
    </span>
  );
};
```

## 8. Confirmation Dialog

```typescript
// src/components/ConfirmDialog.tsx
interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  isDangerous?: boolean;
  isLoading?: boolean;
}

export const ConfirmDialog = ({
  isOpen,
  title,
  message,
  onConfirm,
  onCancel,
  isDangerous = false,
  isLoading = false,
}: ConfirmDialogProps) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
        <div className="p-6">
          <h2 className={`text-lg font-bold mb-2 ${isDangerous ? 'text-red-700' : ''}`}>
            {title}
          </h2>
          <p className="text-gray-600 mb-6">{message}</p>

          <div className="flex gap-2 justify-end">
            <button
              onClick={onCancel}
              disabled={isLoading}
              className="px-4 py-2 text-gray-700 bg-gray-200 rounded hover:bg-gray-300 disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              onClick={onConfirm}
              disabled={isLoading}
              className={`px-4 py-2 text-white rounded disabled:opacity-50 ${
                isDangerous
                  ? 'bg-red-600 hover:bg-red-700'
                  : 'bg-blue-600 hover:bg-blue-700'
              }`}
            >
              {isLoading ? 'Loading...' : 'Confirm'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
```

## 9. Pagination Component

```typescript
// src/components/Pagination.tsx
interface PaginationProps {
  page: number;
  pageSize: number;
  totalItems: number;
  onPageChange: (page: number) => void;
}

export const Pagination = ({ page, pageSize, totalItems, onPageChange }: PaginationProps) => {
  const totalPages = Math.ceil(totalItems / pageSize);

  return (
    <div className="flex items-center gap-2">
      <button
        onClick={() => onPageChange(Math.max(1, page - 1))}
        disabled={page === 1}
        className="px-3 py-1 rounded border disabled:opacity-50"
      >
        ← Prev
      </button>

      {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
        const pageNum = Math.max(1, page - 2) + i;
        if (pageNum > totalPages) return null;
        return (
          <button
            key={pageNum}
            onClick={() => onPageChange(pageNum)}
            className={`px-3 py-1 rounded border ${
              page === pageNum ? 'bg-blue-600 text-white' : 'hover:bg-gray-100'
            }`}
          >
            {pageNum}
          </button>
        );
      })}

      <button
        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        disabled={page === totalPages}
        className="px-3 py-1 rounded border disabled:opacity-50"
      >
        Next →
      </button>
      <span className="ml-4 text-sm text-gray-600">
        Page {page} of {totalPages} ({totalItems} items)
      </span>
    </div>
  );
};
```

