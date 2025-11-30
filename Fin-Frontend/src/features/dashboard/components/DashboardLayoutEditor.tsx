import React, { useState } from 'react';
import { clsx } from 'clsx';
import { Plus, Save, Download, Upload, Trash2, Settings } from 'lucide-react';
import { Button, Modal, Input, toastService } from '@/design-system';
import { Grid, GridItem } from '@/design-system';
import { WidgetRenderer } from './WidgetRenderer';
import { WidgetConfigModal } from './WidgetConfigModal';
import type { DashboardLayout, Widget } from '../types/widget.types';
import { dashboardService } from '../services/dashboardService';

export interface DashboardLayoutEditorProps {
  layout: DashboardLayout;
  onLayoutChange: (layout: DashboardLayout) => void;
  isEditMode?: boolean;
}

export const DashboardLayoutEditor: React.FC<DashboardLayoutEditorProps> = ({
  layout,
  onLayoutChange,
  isEditMode = false,
}) => {
  const [editMode, setEditMode] = useState(isEditMode);
  const [selectedWidget, setSelectedWidget] = useState<Widget | null>(null);
  const [showConfigModal, setShowConfigModal] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);

  const handleAddWidget = async (widgetData: Partial<Widget>) => {
    const newWidget: Widget = {
      id: `widget-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      type: widgetData.type || 'metric',
      title: widgetData.title || 'New Widget',
      description: widgetData.description,
      size: { w: 3, h: 2 },
      position: { x: 0, y: 0 },
      config: {},
      ...widgetData,
    };

    try {
      const updatedLayout = await dashboardService.addWidget(
        layout.id,
        layout.userId,
        newWidget
      );
      onLayoutChange(updatedLayout);
      toastService.success('Widget added successfully');
    } catch (error) {
      toastService.error('Failed to add widget');
    }
  };

  const handleUpdateWidget = async (widgetId: string, updates: Partial<Widget>) => {
    try {
      const updatedLayout = await dashboardService.updateWidget(
        layout.id,
        layout.userId,
        widgetId,
        updates
      );
      onLayoutChange(updatedLayout);
      toastService.success('Widget updated successfully');
    } catch (error) {
      toastService.error('Failed to update widget');
    }
  };

  const handleRemoveWidget = async (widgetId: string) => {
    try {
      const updatedLayout = await dashboardService.removeWidget(
        layout.id,
        layout.userId,
        widgetId
      );
      onLayoutChange(updatedLayout);
      toastService.success('Widget removed successfully');
    } catch (error) {
      toastService.error('Failed to remove widget');
    }
  };

  const handleSaveLayout = async () => {
    try {
      await dashboardService.saveLayout(layout);
      toastService.success('Dashboard layout saved');
      setEditMode(false);
    } catch (error) {
      toastService.error('Failed to save layout');
    }
  };

  const handleExportLayout = async () => {
    try {
      const json = await dashboardService.exportLayout(layout.id, layout.userId);
      const blob = new Blob([json], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `dashboard-${layout.name}-${Date.now()}.json`;
      a.click();
      URL.revokeObjectURL(url);
      toastService.success('Dashboard exported');
    } catch (error) {
      toastService.error('Failed to export dashboard');
    }
  };

  const handleImportLayout = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    try {
      const text = await file.text();
      await dashboardService.importLayout(text, layout.userId);
      toastService.success('Dashboard imported');
      // Reload layouts
      window.location.reload();
    } catch (error) {
      toastService.error('Failed to import dashboard');
    }
  };

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex items-center justify-between p-4 bg-white dark:bg-neutral-900 rounded-lg border border-neutral-200 dark:border-neutral-700">
        <div className="flex items-center gap-2">
          <h2 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">
            {layout.name}
          </h2>
          {layout.isDefault && (
            <span className="px-2 py-1 text-xs font-medium rounded-full bg-primary-100 text-primary-800 dark:bg-primary-900 dark:text-primary-200">
              Default
            </span>
          )}
        </div>

        <div className="flex items-center gap-2">
          {editMode ? (
            <>
              <Button
                size="sm"
                variant="outline"
                icon={<Plus className="h-4 w-4" />}
                onClick={() => setShowAddModal(true)}
              >
                Add Widget
              </Button>
              <Button
                size="sm"
                variant="primary"
                icon={<Save className="h-4 w-4" />}
                onClick={handleSaveLayout}
              >
                Save
              </Button>
              <Button
                size="sm"
                variant="outline"
                onClick={() => setEditMode(false)}
              >
                Cancel
              </Button>
            </>
          ) : (
            <>
              <Button
                size="sm"
                variant="outline"
                icon={<Settings className="h-4 w-4" />}
                onClick={() => setEditMode(true)}
              >
                Edit
              </Button>
              <Button
                size="sm"
                variant="ghost"
                icon={<Download className="h-4 w-4" />}
                onClick={handleExportLayout}
              >
                Export
              </Button>
              <label>
                <input
                  type="file"
                  accept=".json"
                  className="hidden"
                  onChange={handleImportLayout}
                />
                <Button
                  size="sm"
                  variant="ghost"
                  icon={<Upload className="h-4 w-4" />}
                  as="span"
                >
                  Import
                </Button>
              </label>
            </>
          )}
        </div>
      </div>

      {/* Widget Grid */}
      <Grid cols={12} gap="md">
        {layout.widgets.map((widget) => (
          <GridItem key={widget.id} span={widget.size.w as any}>
            <div className={clsx(
              'h-full',
              editMode && 'ring-2 ring-primary-500 ring-opacity-50 rounded-lg'
            )}>
              <WidgetRenderer
                widget={widget}
                onRefresh={(id) => console.log('Refresh', id)}
                onRemove={editMode ? handleRemoveWidget : undefined}
                onConfigure={editMode ? (id) => {
                  setSelectedWidget(widget);
                  setShowConfigModal(true);
                } : undefined}
              />
            </div>
          </GridItem>
        ))}
      </Grid>

      {/* Empty State */}
      {layout.widgets.length === 0 && (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <div className="w-16 h-16 mb-4 rounded-full bg-neutral-100 dark:bg-neutral-800 flex items-center justify-center">
            <Plus className="h-8 w-8 text-neutral-400" />
          </div>
          <h3 className="text-lg font-medium text-neutral-900 dark:text-neutral-100 mb-2">
            No widgets yet
          </h3>
          <p className="text-neutral-600 dark:text-neutral-400 mb-4">
            Add your first widget to get started
          </p>
          <Button
            variant="primary"
            icon={<Plus className="h-4 w-4" />}
            onClick={() => setShowAddModal(true)}
          >
            Add Widget
          </Button>
        </div>
      )}

      {/* Add Widget Modal */}
      <WidgetConfigModal
        isOpen={showAddModal}
        onClose={() => setShowAddModal(false)}
        onSave={handleAddWidget}
      />

      {/* Configure Widget Modal */}
      {selectedWidget && (
        <WidgetConfigModal
          isOpen={showConfigModal}
          onClose={() => {
            setShowConfigModal(false);
            setSelectedWidget(null);
          }}
          widget={selectedWidget}
          onSave={(updates) => {
            handleUpdateWidget(selectedWidget.id, updates);
            setShowConfigModal(false);
            setSelectedWidget(null);
          }}
        />
      )}
    </div>
  );
};
