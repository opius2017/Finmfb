// Keyboard Shortcuts Types
export interface KeyboardShortcut {
  id: string;
  key: string;
  modifiers: Modifier[];
  action: string;
  description: string;
  category: ShortcutCategory;
  enabled: boolean;
  customizable: boolean;
}

export type Modifier = 'ctrl' | 'alt' | 'shift' | 'meta';

export type ShortcutCategory = 
  | 'navigation' 
  | 'editing' 
  | 'search' 
  | 'actions' 
  | 'general';

export interface ShortcutConfig {
  shortcuts: KeyboardShortcut[];
  globalEnabled: boolean;
  conflictResolution: 'first' | 'last' | 'prompt';
}

export interface ShortcutEvent {
  shortcut: KeyboardShortcut;
  event: KeyboardEvent;
  timestamp: Date;
}

export interface ShortcutConflict {
  shortcut1: KeyboardShortcut;
  shortcut2: KeyboardShortcut;
  key: string;
}
