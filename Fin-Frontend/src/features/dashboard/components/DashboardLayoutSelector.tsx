import React, { useState, useEffect } from 'react';
import { clsx } from 'clsx';
import { Check, Plus, Trash2, Star } from 'lucide-react';
import { Button, Modal, Input, toastService } from '@/design-system';
import type { DashboardLayout } from '../types/widget.types';
import { dashboardService } from '../services/dashboardService';

export interface DashboardLayoutSelectorProps {
  userId: string;
  currentLayoutId?: string;
  onLayoutChange: (layout: DashboardLayout) => void;
}

export const DashboardLayoutSelector: React.FC<DashboardLayoutSelectorProps> = ({
  userId,
  currentLayoutId,
  onLayoutChange,
}) => {
  const [layouts, setLayouts] = useState<DashboardLayout[]>([]);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newLayoutName, setNewLayoutName] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadLayouts();
  }, [userId]);

  const loadLayouts = async () => {
    try {
      setLoading(true);
      const data = await dashboardService.getLayouts(userId);
      setLayouts(data);
    } catch (error) {
      toastService.error('Failed to load layouts');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateLayout = async () => {
    if (!newLayoutName.trim()) {
      toastService.error('Please enter a layout name');
      return;
    }

    try {
      const newLayout = await dashboardService.createLayout(userId, newLayoutName);
      setLayouts([...layouts, newLayout]);
      setNewLayoutName('');
      setShowCreateModal(false);
      onLayoutChange(newLayout);
      toastService.success('Layout created successfully');
    } catch (error) {
      toastService.error('Failed to create layout');
    }
  };

  const handleSelectLayout = async (layout: DashboardLayout) => {
    try {
      await dashboardService.setActiveLayout(layout.id, userId);
      onLayoutChange(layout);
      toastService.success(`Switched to ${layout.name}`);
    } catch (error) {
      toastService.error('Failed to switch layout');
    }
  };

  const handleSetDefault = async (layoutId: string) => {
    try {
      await dashboardService.setDefaultLayout(layoutId, userId);
      await loadLayouts();
      toastService.success('Default layout updated');
    } catch (error) {
      toastService.error('Failed to set default layout');
    }
  };

  const handleDeleteLayout = async (layoutId: string) => {
    if (layouts.length <= 1) {
      toastService.error('Cannot delete the last layout');
      return;
    }

    if (!confirm('Are you sure you want to delete this layout?')) {
      return;
    }

    try {
      await dashboardService.deleteLayout(layoutId, userId);
      await loadLayouts();
      
      // If deleted layout was active, switch to first available
      if (layoutId === currentLayoutId && layouts.length > 0) {
        const nextLayout = layouts.find(l => l.id !== layoutId);
        if (nextLayout) {
          onLayoutChange(nextLayout);
        }
      }
      
      toastService.success('Layout deleted');
    } catch (error) {
      toastService.error('Failed to delete layout');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center p-4">
        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary-600" />
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">
          Dashboard Layouts
        </h3>
        <Button
          size="xs"
          variant="outline"
          icon={<Plus className="h-3 w-3" />}
          onClick={() => setShowCreateModal(true)}
        >
          New
        </Button>
      </div>

      <div className="space-y-1">
        {layouts.map((layout) => (
          <div
            key={layout.id}
            className={clsx(
              'flex items-center justify-between p-3 rounded-lg transition-colors cursor-pointer',
              'hover:bg-neutral-50 dark:hover:bg-neutral-800',
              currentLayoutId === layout.id
                ? 'bg-primary-50 dark:bg-primary-900/20 border border-primary-200 dark:border-primary-800'
                : 'border border-transparent'
            )}
            onClick={() => handleSelectLayout(layout)}
          >
            <div className="flex items-center gap-3 flex-1 min-w-0">
              {currentLayoutId === layout.id && (
                <Check className="h-4 w-4 text-primary-600 dark:text-primary-400 flex-shrink-0" />
              )}
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100 truncate">
                    {layout.name}
                  </p>
                  {layout.isDefault && (
                    <Star className="h-3 w-3 text-warning-500 fill-current flex-shrink-0" />
                  )}
                </div>
                <p className="text-xs text-neutral-500 dark:text-neutral-400">
                  {layout.widgets.length} widgets
                </p>
              </div>
            </div>

            <div className="flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
              {!layout.isDefault && (
                <button
                  onClick={() => handleSetDefault(layout.id)}
                  className="p-1 rounded hover:bg-neutral-200 dark:hover:bg-neutral-700 text-neutral-500 dark:text-neutral-400"
                  title="Set as default"
                >
                  <Star className="h-4 w-4" />
                </button>
              )}
              {layouts.length > 1 && (
                <button
                  onClick={() => handleDeleteLayout(layout.id)}
                  className="p-1 rounded hover:bg-error-100 dark:hover:bg-error-900/20 text-error-600 dark:text-error-400"
                  title="Delete layout"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Create Layout Modal */}
      <Modal
        isOpen={showCreateModal}
        onClose={() => {
          setShowCreateModal(false);
          setNewLayoutName('');
        }}
        title="Create New Layout"
        description="Enter a name for your new dashboard layout"
        size="sm"
      >
        <div className="space-y-4">
          <Input
            label="Layout Name"
            value={newLayoutName}
            onChange={(e) => setNewLayoutName(e.target.value)}
            placeholder="e.g., Sales Dashboard"
            autoFocus
            onKeyPress={(e) => {
              if (e.key === 'Enter') {
                handleCreateLayout();
              }
            }}
          />

          <div className="flex justify-end gap-3">
            <Button
              variant="outline"
              onClick={() => {
                setShowCreateModal(false);
                setNewLayoutName('');
              }}
            >
              Cancel
            </Button>
            <Button variant="primary" onClick={handleCreateLayout}>
              Create
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};
