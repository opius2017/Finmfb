import React from 'react';
import { Widget } from './Widget';
import { MetricWidget } from './MetricWidget';
import { ChartWidget } from './ChartWidget';
import { TableWidget } from './TableWidget';
import { ListWidget } from './ListWidget';
import type { Widget as WidgetType, MetricConfig, ChartConfig, TableConfig, ListConfig } from '../types/widget.types';

export interface WidgetRendererProps {
  widget: WidgetType;
  onRefresh?: (widgetId: string) => void;
  onRemove?: (widgetId: string) => void;
  onConfigure?: (widgetId: string) => void;
  onResize?: (widgetId: string, size: { w: number; h: number }) => void;
  isDragging?: boolean;
}

export const WidgetRenderer: React.FC<WidgetRendererProps> = ({
  widget,
  onRefresh,
  onRemove,
  onConfigure,
  onResize,
  isDragging,
}) => {
  const renderWidgetContent = () => {
    switch (widget.type) {
      case 'metric':
        return <MetricWidget config={widget.config as MetricConfig} />;
      
      case 'chart':
        return <ChartWidget config={widget.config as ChartConfig} />;
      
      case 'table':
        return <TableWidget config={widget.config as TableConfig} />;
      
      case 'list':
        return <ListWidget config={widget.config as ListConfig} />;
      
      case 'custom':
        return (
          <div className="flex items-center justify-center h-full text-neutral-500 dark:text-neutral-400">
            Custom widget content
          </div>
        );
      
      default:
        return (
          <div className="flex items-center justify-center h-full text-neutral-500 dark:text-neutral-400">
            Unknown widget type: {widget.type}
          </div>
        );
    }
  };

  return (
    <Widget
      widget={widget}
      onRefresh={onRefresh}
      onRemove={onRemove}
      onConfigure={onConfigure}
      onResize={onResize}
      isDragging={isDragging}
    >
      {renderWidgetContent()}
    </Widget>
  );
};
