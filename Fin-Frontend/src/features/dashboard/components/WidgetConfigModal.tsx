import React, { useState } from 'react';
import { Modal, Button, Input } from '@/design-system';
import type { Widget, WidgetType } from '../types/widget.types';

export interface WidgetConfigModalProps {
  isOpen: boolean;
  onClose: () => void;
  widget?: Widget;
  onSave: (widget: Partial<Widget>) => void;
}

export const WidgetConfigModal: React.FC<WidgetConfigModalProps> = ({
  isOpen,
  onClose,
  widget,
  onSave,
}) => {
  const [title, setTitle] = useState(widget?.title || '');
  const [description, setDescription] = useState(widget?.description || '');
  const [type, setType] = useState<WidgetType>(widget?.type || 'metric');
  const [refreshInterval, setRefreshInterval] = useState(
    widget?.refreshInterval?.toString() || '60'
  );

  const handleSave = () => {
    onSave({
      ...widget,
      title,
      description,
      type,
      refreshInterval: parseInt(refreshInterval, 10),
    });
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={widget ? 'Configure Widget' : 'Add Widget'}
      description="Customize your widget settings"
      size="md"
    >
      <div className="space-y-4">
        <Input
          label="Widget Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Enter widget title"
          required
        />

        <Input
          label="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Enter widget description (optional)"
        />

        <div>
          <label className="block text-sm font-medium text-neutral-700 dark:text-neutral-300 mb-1">
            Widget Type
          </label>
          <select
            value={type}
            onChange={(e) => setType(e.target.value as WidgetType)}
            className="w-full px-3 py-2 border border-neutral-300 dark:border-neutral-700 rounded-lg bg-white dark:bg-neutral-800 text-neutral-900 dark:text-neutral-100 focus:outline-none focus:ring-2 focus:ring-primary-500"
          >
            <option value="metric">Metric</option>
            <option value="chart">Chart</option>
            <option value="table">Table</option>
            <option value="list">List</option>
            <option value="custom">Custom</option>
          </select>
        </div>

        <Input
          label="Refresh Interval (seconds)"
          type="number"
          value={refreshInterval}
          onChange={(e) => setRefreshInterval(e.target.value)}
          placeholder="60"
          hint="How often to refresh widget data (0 for manual only)"
        />

        <div className="flex justify-end gap-3 pt-4">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleSave}>
            {widget ? 'Save Changes' : 'Add Widget'}
          </Button>
        </div>
      </div>
    </Modal>
  );
};
