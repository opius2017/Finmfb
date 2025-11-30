/**
 * Dashboard Widget Types
 * Type definitions for the widget system
 */

export type WidgetType = 'metric' | 'chart' | 'table' | 'list' | 'custom';

export type ChartType = 'line' | 'bar' | 'pie' | 'area' | 'donut';

export interface WidgetSize {
  w: number; // width in grid units
  h: number; // height in grid units
  minW?: number;
  minH?: number;
  maxW?: number;
  maxH?: number;
}

export interface WidgetPosition {
  x: number;
  y: number;
}

export interface MetricConfig {
  value: number | string;
  label: string;
  trend?: {
    value: number;
    direction: 'up' | 'down';
    isPositive?: boolean;
  };
  icon?: string;
  color?: string;
  format?: 'number' | 'currency' | 'percentage';
}

export interface ChartConfig {
  type: ChartType;
  data: any[];
  xKey?: string;
  yKey?: string;
  colors?: string[];
  showLegend?: boolean;
  showGrid?: boolean;
}

export interface TableConfig {
  columns: Array<{
    key: string;
    header: string;
    width?: string;
  }>;
  data: any[];
  pageSize?: number;
}

export interface ListConfig {
  items: Array<{
    id: string;
    title: string;
    subtitle?: string;
    icon?: string;
    value?: string;
  }>;
  maxItems?: number;
}

export interface DataSourceConfig {
  type: 'api' | 'static' | 'realtime';
  endpoint?: string;
  params?: Record<string, any>;
  refreshInterval?: number; // in seconds
  transform?: (data: any) => any;
}

export type WidgetConfig = MetricConfig | ChartConfig | TableConfig | ListConfig | Record<string, any>;

export interface Widget {
  id: string;
  type: WidgetType;
  title: string;
  description?: string;
  size: WidgetSize;
  position: WidgetPosition;
  config: WidgetConfig;
  dataSource?: DataSourceConfig;
  refreshInterval?: number;
  isLoading?: boolean;
  error?: string;
  lastUpdated?: Date;
}

export interface DashboardLayout {
  id: string;
  name: string;
  userId: string;
  role?: string;
  widgets: Widget[];
  isDefault: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface WidgetTemplate {
  id: string;
  name: string;
  description: string;
  type: WidgetType;
  defaultSize: WidgetSize;
  defaultConfig: WidgetConfig;
  category: 'financial' | 'operational' | 'analytics' | 'custom';
  icon?: string;
}
