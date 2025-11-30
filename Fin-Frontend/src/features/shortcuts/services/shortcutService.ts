import { KeyboardShortcut, Modifier, ShortcutEvent } from '../types/shortcuts.types';

export class ShortcutService {
  private shortcuts: Map<string, KeyboardShortcut> = new Map();
  private handlers: Map<string, (event: KeyboardEvent) => void> = new Map();
  private enabled: boolean = true;

  constructor() {
    this.initializeDefaultShortcuts();
    this.attachGlobalListener();
  }

  private initializeDefaultShortcuts(): void {
    const defaults: KeyboardShortcut[] = [
      // Navigation
      { id: 'nav-dashboard', key: 'd', modifiers: ['ctrl'], action: 'navigate-dashboard', description: 'Go to Dashboard', category: 'navigation', enabled: true, customizable: true },
      { id: 'nav-invoices', key: 'i', modifiers: ['ctrl'], action: 'navigate-invoices', description: 'Go to Invoices', category: 'navigation', enabled: true, customizable: true },
      { id: 'nav-customers', key: 'u', modifiers: ['ctrl'], action: 'navigate-customers', description: 'Go to Customers', category: 'navigation', enabled: true, customizable: true },
      
      // Editing
      { id: 'save', key: 's', modifiers: ['ctrl'], action: 'save', description: 'Save', category: 'editing', enabled: true, customizable: false },
      { id: 'new', key: 'n', modifiers: ['ctrl'], action: 'new', description: 'New Item', category: 'editing', enabled: true, customizable: true },
      { id: 'delete', key: 'Delete', modifiers: [], action: 'delete', description: 'Delete', category: 'editing', enabled: true, customizable: true },
      { id: 'undo', key: 'z', modifiers: ['ctrl'], action: 'undo', description: 'Undo', category: 'editing', enabled: true, customizable: false },
      { id: 'redo', key: 'y', modifiers: ['ctrl'], action: 'redo', description: 'Redo', category: 'editing', enabled: true, customizable: false },
      
      // Search
      { id: 'search', key: 'k', modifiers: ['ctrl'], action: 'search', description: 'Global Search', category: 'search', enabled: true, customizable: false },
      { id: 'find', key: 'f', modifiers: ['ctrl'], action: 'find', description: 'Find in Page', category: 'search', enabled: true, customizable: true },
      
      // Actions
      { id: 'print', key: 'p', modifiers: ['ctrl'], action: 'print', description: 'Print', category: 'actions', enabled: true, customizable: true },
      { id: 'export', key: 'e', modifiers: ['ctrl'], action: 'export', description: 'Export', category: 'actions', enabled: true, customizable: true },
      
      // General
      { id: 'help', key: '/', modifiers: ['ctrl'], action: 'help', description: 'Show Shortcuts', category: 'general', enabled: true, customizable: false },
      { id: 'escape', key: 'Escape', modifiers: [], action: 'escape', description: 'Close/Cancel', category: 'general', enabled: true, customizable: false },
    ];

    defaults.forEach(shortcut => {
      const key = this.getShortcutKey(shortcut);
      this.shortcuts.set(key, shortcut);
    });
  }

  private attachGlobalListener(): void {
    document.addEventListener('keydown', (event) => {
      if (!this.enabled) return;

      // Don't trigger shortcuts when typing in inputs
      const target = event.target as HTMLElement;
      if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable) {
        // Allow Ctrl+S and Escape even in inputs
        if (!(event.ctrlKey && event.key === 's') && event.key !== 'Escape') {
          return;
        }
      }

      const key = this.getKeyFromEvent(event);
      const shortcut = this.shortcuts.get(key);

      if (shortcut && shortcut.enabled) {
        const handler = this.handlers.get(shortcut.action);
        if (handler) {
          event.preventDefault();
          handler(event);
        }
      }
    });
  }

  registerShortcut(shortcut: KeyboardShortcut, handler: (event: KeyboardEvent) => void): void {
    const key = this.getShortcutKey(shortcut);
    this.shortcuts.set(key, shortcut);
    this.handlers.set(shortcut.action, handler);
  }

  unregisterShortcut(shortcutId: string): void {
    const shortcut = Array.from(this.shortcuts.values()).find(s => s.id === shortcutId);
    if (shortcut) {
      const key = this.getShortcutKey(shortcut);
      this.shortcuts.delete(key);
      this.handlers.delete(shortcut.action);
    }
  }

  registerHandler(action: string, handler: (event: KeyboardEvent) => void): void {
    this.handlers.set(action, handler);
  }

  unregisterHandler(action: string): void {
    this.handlers.delete(action);
  }

  getShortcuts(category?: string): KeyboardShortcut[] {
    const shortcuts = Array.from(this.shortcuts.values());
    return category 
      ? shortcuts.filter(s => s.category === category)
      : shortcuts;
  }

  updateShortcut(shortcutId: string, updates: Partial<KeyboardShortcut>): void {
    const shortcut = Array.from(this.shortcuts.values()).find(s => s.id === shortcutId);
    if (shortcut) {
      const oldKey = this.getShortcutKey(shortcut);
      this.shortcuts.delete(oldKey);

      const updated = { ...shortcut, ...updates };
      const newKey = this.getShortcutKey(updated);
      this.shortcuts.set(newKey, updated);
    }
  }

  enable(): void {
    this.enabled = true;
  }

  disable(): void {
    this.enabled = false;
  }

  isEnabled(): boolean {
    return this.enabled;
  }

  private getShortcutKey(shortcut: KeyboardShortcut): string {
    const modifiers = shortcut.modifiers.sort().join('+');
    return modifiers ? `${modifiers}+${shortcut.key.toLowerCase()}` : shortcut.key.toLowerCase();
  }

  private getKeyFromEvent(event: KeyboardEvent): string {
    const modifiers: string[] = [];
    if (event.ctrlKey) modifiers.push('ctrl');
    if (event.altKey) modifiers.push('alt');
    if (event.shiftKey) modifiers.push('shift');
    if (event.metaKey) modifiers.push('meta');

    modifiers.sort();
    const key = event.key.toLowerCase();
    return modifiers.length > 0 ? `${modifiers.join('+')}+${key}` : key;
  }

  formatShortcut(shortcut: KeyboardShortcut): string {
    const modifiers = shortcut.modifiers.map(m => {
      switch (m) {
        case 'ctrl': return 'Ctrl';
        case 'alt': return 'Alt';
        case 'shift': return 'Shift';
        case 'meta': return '⌘';
        default: return m;
      }
    });

    const key = shortcut.key.length === 1 
      ? shortcut.key.toUpperCase() 
      : shortcut.key;

    return [...modifiers, key].join(' + ');
  }

  detectConflicts(): Array<{ shortcut1: KeyboardShortcut; shortcut2: KeyboardShortcut }> {
    const conflicts: Array<{ shortcut1: KeyboardShortcut; shortcut2: KeyboardShortcut }> = [];
    const shortcuts = Array.from(this.shortcuts.values());

    for (let i = 0; i < shortcuts.length; i++) {
      for (let j = i + 1; j < shortcuts.length; j++) {
        const key1 = this.getShortcutKey(shortcuts[i]);
        const key2 = this.getShortcutKey(shortcuts[j]);

        if (key1 === key2) {
          conflicts.push({
            shortcut1: shortcuts[i],
            shortcut2: shortcuts[j],
          });
        }
      }
    }

    return conflicts;
  }
}

export const shortcutService = new ShortcutService();
