import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { WidgetRenderer } from '../components/WidgetRenderer';
import type { Widget } from '../types/widget.types';

describe('WidgetRenderer Component', () => {
  const mockWidget: Widget = {
    id: 'test-widget-1',
    type: 'metric',
    title: 'Test Metric',
    description: 'Test Description',
    size: { w: 3, h: 2 },
    position: { x: 0, y: 0 },
    config: {
      label: 'Total Revenue',
      value: 50000,
      format: 'currency',
      trend: {
        value: 15,
        direction: 'up',
        isPositive: true,
      },
    },
  };

  describe('Rendering', () => {
    it('should render widget with title', () => {
      render(<WidgetRenderer widget={mockWidget} />);
      expect(screen.getByText('Test Metric')).toBeInTheDocument();
    });

    it('should render widget with description', () => {
      render(<WidgetRenderer widget={mockWidget} />);
      expect(screen.getByText('Test Description')).toBeInTheDocument();
    });

    it('should render metric widget content', () => {
      render(<WidgetRenderer widget={mockWidget} />);
      expect(screen.getByText('Total Revenue')).toBeInTheDocument();
    });
  });

  describe('Widget Types', () => {
    it('should render chart widget', () => {
      const chartWidget: Widget = {
        ...mockWidget,
        type: 'chart',
        config: {
          type: 'line',
          data: [
            { name: 'Jan', value: 100 },
            { name: 'Feb', value: 200 },
          ],
          xKey: 'name',
          yKey: 'value',
        },
      };

      render(<WidgetRenderer widget={chartWidget} />);
      expect(screen.getByText('Test Metric')).toBeInTheDocument();
    });

    it('should render table widget', () => {
      const tableWidget: Widget = {
        ...mockWidget,
        type: 'table',
        config: {
          columns: [
            { key: 'name', header: 'Name' },
            { key: 'value', header: 'Value' },
          ],
          data: [
            { name: 'Item 1', value: '100' },
            { name: 'Item 2', value: '200' },
          ],
        },
      };

      render(<WidgetRenderer widget={tableWidget} />);
      expect(screen.getByText('Name')).toBeInTheDocument();
      expect(screen.getByText('Value')).toBeInTheDocument();
    });

    it('should render list widget', () => {
      const listWidget: Widget = {
        ...mockWidget,
        type: 'list',
        config: {
          items: [
            { id: '1', title: 'Item 1', value: '100' },
            { id: '2', title: 'Item 2', value: '200' },
          ],
        },
      };

      render(<WidgetRenderer widget={listWidget} />);
      expect(screen.getByText('Item 1')).toBeInTheDocument();
      expect(screen.getByText('Item 2')).toBeInTheDocument();
    });
  });

  describe('Widget Actions', () => {
    it('should call onRefresh when refresh is clicked', () => {
      const onRefresh = jest.fn();
      render(<WidgetRenderer widget={mockWidget} onRefresh={onRefresh} />);

      // Open menu
      const menuButton = screen.getByLabelText('Widget menu');
      fireEvent.click(menuButton);

      // Click refresh
      const refreshButton = screen.getByText('Refresh');
      fireEvent.click(refreshButton);

      expect(onRefresh).toHaveBeenCalledWith(mockWidget.id);
    });

    it('should call onRemove when remove is clicked', () => {
      const onRemove = jest.fn();
      render(<WidgetRenderer widget={mockWidget} onRemove={onRemove} />);

      // Open menu
      const menuButton = screen.getByLabelText('Widget menu');
      fireEvent.click(menuButton);

      // Click remove
      const removeButton = screen.getByText('Remove');
      fireEvent.click(removeButton);

      expect(onRemove).toHaveBeenCalledWith(mockWidget.id);
    });

    it('should call onConfigure when configure is clicked', () => {
      const onConfigure = jest.fn();
      render(<WidgetRenderer widget={mockWidget} onConfigure={onConfigure} />);

      // Open menu
      const menuButton = screen.getByLabelText('Widget menu');
      fireEvent.click(menuButton);

      // Click configure
      const configureButton = screen.getByText('Configure');
      fireEvent.click(configureButton);

      expect(onConfigure).toHaveBeenCalledWith(mockWidget.id);
    });
  });

  describe('Loading and Error States', () => {
    it('should show loading state', () => {
      const loadingWidget: Widget = {
        ...mockWidget,
        isLoading: true,
      };

      render(<WidgetRenderer widget={loadingWidget} />);
      expect(screen.getByRole('progressbar', { hidden: true })).toBeInTheDocument();
    });

    it('should show error state', () => {
      const errorWidget: Widget = {
        ...mockWidget,
        error: 'Failed to load data',
      };

      render(<WidgetRenderer widget={errorWidget} />);
      expect(screen.getByText('Failed to load data')).toBeInTheDocument();
    });

    it('should show last updated time', () => {
      const widgetWithUpdate: Widget = {
        ...mockWidget,
        lastUpdated: new Date('2024-01-01T12:00:00'),
      };

      render(<WidgetRenderer widget={widgetWithUpdate} />);
      expect(screen.getByText(/Last updated:/)).toBeInTheDocument();
    });
  });
});
