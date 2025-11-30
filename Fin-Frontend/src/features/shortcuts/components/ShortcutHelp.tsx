import React, { useState, useEffect } from 'react';
import { Keyboard, X } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { KeyboardShortcut } from '../types/shortcuts.types';
import { shortcutService } from '../services/shortcutService';

interface ShortcutHelpProps {
  isOpen: boolean;
  onClose: () => void;
}

export const ShortcutHelp: React.FC<ShortcutHelpProps> = ({ isOpen, onClose }) => {
  const [shortcuts, setShortcuts] = useState<KeyboardShortcut[]>([]);

  useEffect(() => {
    if (isOpen) {
      setShortcuts(shortcutService.getShortcuts());
    }
  }, [isOpen]);

  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isOpen) {
        onClose();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const groupedShortcuts = shortcuts.reduce((acc, shortcut) => {
    if (!acc[shortcut.category]) {
      acc[shortcut.category] = [];
    }
    acc[shortcut.category].push(shortcut);
    return acc;
  }, {} as Record<string, KeyboardShortcut[]>);

  const categoryLabels: Record<string, string> = {
    navigation: 'Navigation',
    editing: 'Editing',
    search: 'Search',
    actions: 'Actions',
    general: 'General',
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <Card className="w-full max-w-3xl max-h-[80vh] overflow-hidden flex flex-col">
        <div className="p-6 border-b border-neutral-200 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <Keyboard className="w-6 h-6 text-primary-600" />
            <h2 className="text-xl font-bold">Keyboard Shortcuts</h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-neutral-100 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6 overflow-y-auto flex-1">
          <div className="space-y-6">
            {Object.entries(groupedShortcuts).map(([category, categoryShortcuts]) => (
              <div key={category}>
                <h3 className="text-lg font-semibold mb-3">
                  {categoryLabels[category] || category}
                </h3>
                <div className="space-y-2">
                  {categoryShortcuts.map((shortcut) => (
                    <div
                      key={shortcut.id}
                      className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
                    >
                      <span className="text-sm text-neutral-700">
                        {shortcut.description}
                      </span>
                      <div className="flex items-center space-x-1">
                        {shortcutService.formatShortcut(shortcut).split(' + ').map((key, index, arr) => (
                          <React.Fragment key={index}>
                            <kbd className="px-2 py-1 text-xs font-mono bg-white border border-neutral-300 rounded shadow-sm">
                              {key}
                            </kbd>
                            {index < arr.length - 1 && (
                              <span className="text-neutral-400">+</span>
                            )}
                          </React.Fragment>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="p-4 border-t border-neutral-200 bg-neutral-50">
          <p className="text-sm text-neutral-600 text-center">
            Press <kbd className="px-2 py-1 text-xs font-mono bg-white border border-neutral-300 rounded">Esc</kbd> to close
          </p>
        </div>
      </Card>
    </div>
  );
};

// Hook for using shortcuts in components
export function useShortcut(
  action: string,
  handler: (event: KeyboardEvent) => void,
  deps: React.DependencyList = []
): void {
  useEffect(() => {
    shortcutService.registerHandler(action, handler);
    return () => shortcutService.unregisterHandler(action);
  }, deps);
}
