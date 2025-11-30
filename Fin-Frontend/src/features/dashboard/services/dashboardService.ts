/**
 * Dashboard Service
 * Handles dashboard layout persistence and management
 */

import type { DashboardLayout, Widget, WidgetTemplate } from '../types/widget.types';

const STORAGE_KEY = 'dashboard-layouts';
const ACTIVE_LAYOUT_KEY = 'active-dashboard-layout';

class DashboardService {
  /**
   * Get all dashboard layouts for a user
   */
  async getLayouts(userId: string): Promise<DashboardLayout[]> {
    try {
      const stored = localStorage.getItem(`${STORAGE_KEY}-${userId}`);
      if (!stored) return [];
      
      const layouts = JSON.parse(stored);
      return layouts.map((layout: any) => ({
        ...layout,
        createdAt: new Date(layout.createdAt),
        updatedAt: new Date(layout.updatedAt),
      }));
    } catch (error) {
      console.error('[DashboardService] Error loading layouts:', error);
      return [];
    }
  }

  /**
   * Get a specific layout by ID
   */
  async getLayout(layoutId: string, userId: string): Promise<DashboardLayout | null> {
    const layouts = await this.getLayouts(userId);
    return layouts.find(l => l.id === layoutId) || null;
  }

  /**
   * Get the active layout for a user
   */
  async getActiveLayout(userId: string): Promise<DashboardLayout | null> {
    const activeLayoutId = localStorage.getItem(`${ACTIVE_LAYOUT_KEY}-${userId}`);
    
    if (activeLayoutId) {
      const layout = await this.getLayout(activeLayoutId, userId);
      if (layout) return layout;
    }

    // Return default layout if no active layout
    const layouts = await this.getLayouts(userId);
    return layouts.find(l => l.isDefault) || layouts[0] || null;
  }

  /**
   * Save a dashboard layout
   */
  async saveLayout(layout: DashboardLayout): Promise<DashboardLayout> {
    try {
      const layouts = await this.getLayouts(layout.userId);
      const existingIndex = layouts.findIndex(l => l.id === layout.id);

      const updatedLayout = {
        ...layout,
        updatedAt: new Date(),
      };

      if (existingIndex >= 0) {
        layouts[existingIndex] = updatedLayout;
      } else {
        layouts.push(updatedLayout);
      }

      localStorage.setItem(
        `${STORAGE_KEY}-${layout.userId}`,
        JSON.stringify(layouts)
      );

      return updatedLayout;
    } catch (error) {
      console.error('[DashboardService] Error saving layout:', error);
      throw error;
    }
  }

  /**
   * Create a new dashboard layout
   */
  async createLayout(
    userId: string,
    name: string,
    widgets: Widget[] = []
  ): Promise<DashboardLayout> {
    const layout: DashboardLayout = {
      id: `layout-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      name,
      userId,
      widgets,
      isDefault: false,
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    return this.saveLayout(layout);
  }

  /**
   * Delete a dashboard layout
   */
  async deleteLayout(layoutId: string, userId: string): Promise<void> {
    try {
      const layouts = await this.getLayouts(userId);
      const filtered = layouts.filter(l => l.id !== layoutId);
      
      localStorage.setItem(
        `${STORAGE_KEY}-${userId}`,
        JSON.stringify(filtered)
      );

      // Clear active layout if it was deleted
      const activeLayoutId = localStorage.getItem(`${ACTIVE_LAYOUT_KEY}-${userId}`);
      if (activeLayoutId === layoutId) {
        localStorage.removeItem(`${ACTIVE_LAYOUT_KEY}-${userId}`);
      }
    } catch (error) {
      console.error('[DashboardService] Error deleting layout:', error);
      throw error;
    }
  }

  /**
   * Set active layout
   */
  async setActiveLayout(layoutId: string, userId: string): Promise<void> {
    localStorage.setItem(`${ACTIVE_LAYOUT_KEY}-${userId}`, layoutId);
  }

  /**
   * Set default layout
   */
  async setDefaultLayout(layoutId: string, userId: string): Promise<void> {
    const layouts = await this.getLayouts(userId);
    
    const updated = layouts.map(layout => ({
      ...layout,
      isDefault: layout.id === layoutId,
    }));

    localStorage.setItem(
      `${STORAGE_KEY}-${userId}`,
      JSON.stringify(updated)
    );
  }

  /**
   * Add widget to layout
   */
  async addWidget(layoutId: string, userId: string, widget: Widget): Promise<DashboardLayout> {
    const layout = await this.getLayout(layoutId, userId);
    if (!layout) throw new Error('Layout not found');

    layout.widgets.push(widget);
    return this.saveLayout(layout);
  }

  /**
   * Update widget in layout
   */
  async updateWidget(
    layoutId: string,
    userId: string,
    widgetId: string,
    updates: Partial<Widget>
  ): Promise<DashboardLayout> {
    const layout = await this.getLayout(layoutId, userId);
    if (!layout) throw new Error('Layout not found');

    const widgetIndex = layout.widgets.findIndex(w => w.id === widgetId);
    if (widgetIndex === -1) throw new Error('Widget not found');

    layout.widgets[widgetIndex] = {
      ...layout.widgets[widgetIndex],
      ...updates,
    };

    return this.saveLayout(layout);
  }

  /**
   * Remove widget from layout
   */
  async removeWidget(layoutId: string, userId: string, widgetId: string): Promise<DashboardLayout> {
    const layout = await this.getLayout(layoutId, userId);
    if (!layout) throw new Error('Layout not found');

    layout.widgets = layout.widgets.filter(w => w.id !== widgetId);
    return this.saveLayout(layout);
  }

  /**
   * Update widget positions
   */
  async updateWidgetPositions(
    layoutId: string,
    userId: string,
    positions: Array<{ id: string; position: { x: number; y: number } }>
  ): Promise<DashboardLayout> {
    const layout = await this.getLayout(layoutId, userId);
    if (!layout) throw new Error('Layout not found');

    positions.forEach(({ id, position }) => {
      const widget = layout.widgets.find(w => w.id === id);
      if (widget) {
        widget.position = position;
      }
    });

    return this.saveLayout(layout);
  }

  /**
   * Get widget templates
   */
  getWidgetTemplates(): WidgetTemplate[] {
    return [
      {
        id: 'total-revenue',
        name: 'Total Revenue',
        description: 'Display total revenue with trend',
        type: 'metric',
        defaultSize: { w: 3, h: 2 },
        defaultConfig: {
          label: 'Total Revenue',
          value: 0,
          format: 'currency',
          icon: '💰',
        },
        category: 'financial',
      },
      {
        id: 'active-users',
        name: 'Active Users',
        description: 'Number of active users',
        type: 'metric',
        defaultSize: { w: 3, h: 2 },
        defaultConfig: {
          label: 'Active Users',
          value: 0,
          format: 'number',
          icon: '👥',
        },
        category: 'operational',
      },
      {
        id: 'revenue-chart',
        name: 'Revenue Chart',
        description: 'Revenue over time',
        type: 'chart',
        defaultSize: { w: 6, h: 4 },
        defaultConfig: {
          type: 'line',
          data: [],
          xKey: 'date',
          yKey: 'revenue',
          showLegend: true,
          showGrid: true,
        },
        category: 'analytics',
      },
      {
        id: 'recent-transactions',
        name: 'Recent Transactions',
        description: 'Latest transactions',
        type: 'table',
        defaultSize: { w: 6, h: 4 },
        defaultConfig: {
          columns: [
            { key: 'date', header: 'Date' },
            { key: 'description', header: 'Description' },
            { key: 'amount', header: 'Amount' },
          ],
          data: [],
          pageSize: 5,
        },
        category: 'operational',
      },
    ];
  }

  /**
   * Export dashboard layout
   */
  async exportLayout(layoutId: string, userId: string): Promise<string> {
    const layout = await this.getLayout(layoutId, userId);
    if (!layout) throw new Error('Layout not found');

    return JSON.stringify(layout, null, 2);
  }

  /**
   * Import dashboard layout
   */
  async importLayout(layoutJson: string, userId: string): Promise<DashboardLayout> {
    try {
      const layout = JSON.parse(layoutJson);
      
      // Generate new ID and update user
      const newLayout: DashboardLayout = {
        ...layout,
        id: `layout-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        userId,
        isDefault: false,
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      return this.saveLayout(newLayout);
    } catch (error) {
      console.error('[DashboardService] Error importing layout:', error);
      throw new Error('Invalid layout format');
    }
  }
}

// Export singleton instance
export const dashboardService = new DashboardService();
