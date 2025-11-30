import { dashboardService } from '../services/dashboardService';
import type { DashboardLayout, Widget } from '../types/widget.types';

describe('Dashboard Service', () => {
  const userId = 'test-user-123';
  
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
  });

  describe('Layout Management', () => {
    it('should create a new layout', async () => {
      const layout = await dashboardService.createLayout(userId, 'Test Dashboard');
      
      expect(layout).toBeDefined();
      expect(layout.name).toBe('Test Dashboard');
      expect(layout.userId).toBe(userId);
      expect(layout.widgets).toEqual([]);
      expect(layout.isDefault).toBe(false);
    });

    it('should save and retrieve layouts', async () => {
      const layout = await dashboardService.createLayout(userId, 'Test Dashboard');
      const retrieved = await dashboardService.getLayout(layout.id, userId);
      
      expect(retrieved).toBeDefined();
      expect(retrieved?.id).toBe(layout.id);
      expect(retrieved?.name).toBe('Test Dashboard');
    });

    it('should get all layouts for a user', async () => {
      await dashboardService.createLayout(userId, 'Dashboard 1');
      await dashboardService.createLayout(userId, 'Dashboard 2');
      
      const layouts = await dashboardService.getLayouts(userId);
      
      expect(layouts).toHaveLength(2);
      expect(layouts[0].name).toBe('Dashboard 1');
      expect(layouts[1].name).toBe('Dashboard 2');
    });

    it('should delete a layout', async () => {
      const layout = await dashboardService.createLayout(userId, 'Test Dashboard');
      await dashboardService.deleteLayout(layout.id, userId);
      
      const layouts = await dashboardService.getLayouts(userId);
      expect(layouts).toHaveLength(0);
    });

    it('should set default layout', async () => {
      const layout1 = await dashboardService.createLayout(userId, 'Dashboard 1');
      const layout2 = await dashboardService.createLayout(userId, 'Dashboard 2');
      
      await dashboardService.setDefaultLayout(layout2.id, userId);
      
      const layouts = await dashboardService.getLayouts(userId);
      expect(layouts.find(l => l.id === layout1.id)?.isDefault).toBe(false);
      expect(layouts.find(l => l.id === layout2.id)?.isDefault).toBe(true);
    });

    it('should set and get active layout', async () => {
      const layout = await dashboardService.createLayout(userId, 'Test Dashboard');
      await dashboardService.setActiveLayout(layout.id, userId);
      
      const activeLayout = await dashboardService.getActiveLayout(userId);
      expect(activeLayout?.id).toBe(layout.id);
    });
  });

  describe('Widget Management', () => {
    let layout: DashboardLayout;

    beforeEach(async () => {
      layout = await dashboardService.createLayout(userId, 'Test Dashboard');
    });

    it('should add widget to layout', async () => {
      const widget: Widget = {
        id: 'widget-1',
        type: 'metric',
        title: 'Test Widget',
        size: { w: 3, h: 2 },
        position: { x: 0, y: 0 },
        config: { label: 'Test', value: 100 },
      };

      const updated = await dashboardService.addWidget(layout.id, userId, widget);
      
      expect(updated.widgets).toHaveLength(1);
      expect(updated.widgets[0].id).toBe('widget-1');
      expect(updated.widgets[0].title).toBe('Test Widget');
    });

    it('should update widget in layout', async () => {
      const widget: Widget = {
        id: 'widget-1',
        type: 'metric',
        title: 'Test Widget',
        size: { w: 3, h: 2 },
        position: { x: 0, y: 0 },
        config: {},
      };

      await dashboardService.addWidget(layout.id, userId, widget);
      
      const updated = await dashboardService.updateWidget(
        layout.id,
        userId,
        'widget-1',
        { title: 'Updated Widget' }
      );

      expect(updated.widgets[0].title).toBe('Updated Widget');
    });

    it('should remove widget from layout', async () => {
      const widget: Widget = {
        id: 'widget-1',
        type: 'metric',
        title: 'Test Widget',
        size: { w: 3, h: 2 },
        position: { x: 0, y: 0 },
        config: {},
      };

      await dashboardService.addWidget(layout.id, userId, widget);
      const updated = await dashboardService.removeWidget(layout.id, userId, 'widget-1');

      expect(updated.widgets).toHaveLength(0);
    });

    it('should update widget positions', async () => {
      const widget1: Widget = {
        id: 'widget-1',
        type: 'metric',
        title: 'Widget 1',
        size: { w: 3, h: 2 },
        position: { x: 0, y: 0 },
        config: {},
      };

      const widget2: Widget = {
        id: 'widget-2',
        type: 'metric',
        title: 'Widget 2',
        size: { w: 3, h: 2 },
        position: { x: 3, y: 0 },
        config: {},
      };

      await dashboardService.addWidget(layout.id, userId, widget1);
      await dashboardService.addWidget(layout.id, userId, widget2);

      const updated = await dashboardService.updateWidgetPositions(
        layout.id,
        userId,
        [
          { id: 'widget-1', position: { x: 6, y: 0 } },
          { id: 'widget-2', position: { x: 0, y: 0 } },
        ]
      );

      expect(updated.widgets[0].position).toEqual({ x: 6, y: 0 });
      expect(updated.widgets[1].position).toEqual({ x: 0, y: 0 });
    });
  });

  describe('Widget Templates', () => {
    it('should return widget templates', () => {
      const templates = dashboardService.getWidgetTemplates();
      
      expect(templates.length).toBeGreaterThan(0);
      expect(templates[0]).toHaveProperty('id');
      expect(templates[0]).toHaveProperty('name');
      expect(templates[0]).toHaveProperty('type');
      expect(templates[0]).toHaveProperty('defaultSize');
      expect(templates[0]).toHaveProperty('defaultConfig');
    });
  });

  describe('Import/Export', () => {
    it('should export layout as JSON', async () => {
      const layout = await dashboardService.createLayout(userId, 'Test Dashboard');
      const json = await dashboardService.exportLayout(layout.id, userId);
      
      expect(json).toBeDefined();
      const parsed = JSON.parse(json);
      expect(parsed.name).toBe('Test Dashboard');
    });

    it('should import layout from JSON', async () => {
      const layout = await dashboardService.createLayout(userId, 'Original Dashboard');
      const json = await dashboardService.exportLayout(layout.id, userId);
      
      const imported = await dashboardService.importLayout(json, 'new-user-456');
      
      expect(imported.name).toBe('Original Dashboard');
      expect(imported.userId).toBe('new-user-456');
      expect(imported.id).not.toBe(layout.id); // Should have new ID
    });
  });
});
