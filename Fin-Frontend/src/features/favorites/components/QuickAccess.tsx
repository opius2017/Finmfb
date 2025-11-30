import React, { useState, useEffect } from 'react';
import { Star, Clock, Folder, Plus, X } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { FavoriteItem, RecentItem, FavoriteFolder } from '../types/favorites.types';
import { favoritesService } from '../services/favoritesService';

export const QuickAccess: React.FC = () => {
  const [favorites, setFavorites] = useState<FavoriteItem[]>([]);
  const [recent, setRecent] = useState<RecentItem[]>([]);
  const [folders, setFolders] = useState<FavoriteFolder[]>([]);
  const [activeTab, setActiveTab] = useState<'favorites' | 'recent' | 'folders'>('favorites');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = () => {
    setFavorites(favoritesService.getFavorites());
    setRecent(favoritesService.getRecentItems(10));
    setFolders(favoritesService.getFolders());
  };

  const handleRemoveFavorite = (id: string) => {
    favoritesService.removeFavorite(id);
    loadData();
  };

  const handleClearRecent = () => {
    if (confirm('Clear all recent items?')) {
      favoritesService.clearRecentItems();
      loadData();
    }
  };

  const handleCreateFolder = () => {
    const name = prompt('Enter folder name:');
    if (name) {
      favoritesService.createFolder(name);
      loadData();
    }
  };

  const handleDeleteFolder = (id: string) => {
    if (confirm('Delete this folder?')) {
      favoritesService.deleteFolder(id);
      loadData();
    }
  };

  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold">Quick Access</h2>
        {activeTab === 'folders' && (
          <Button variant="outline" size="sm" onClick={handleCreateFolder}>
            <Plus className="w-4 h-4 mr-2" />
            New Folder
          </Button>
        )}
        {activeTab === 'recent' && recent.length > 0 && (
          <Button variant="ghost" size="sm" onClick={handleClearRecent}>
            Clear
          </Button>
        )}
      </div>

      {/* Tabs */}
      <div className="flex space-x-2 mb-4 border-b border-neutral-200">
        <button
          onClick={() => setActiveTab('favorites')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === 'favorites'
              ? 'text-primary-600 border-b-2 border-primary-600'
              : 'text-neutral-600 hover:text-neutral-900'
          }`}
        >
          <Star className="w-4 h-4 inline mr-2" />
          Favorites ({favorites.length})
        </button>
        <button
          onClick={() => setActiveTab('recent')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === 'recent'
              ? 'text-primary-600 border-b-2 border-primary-600'
              : 'text-neutral-600 hover:text-neutral-900'
          }`}
        >
          <Clock className="w-4 h-4 inline mr-2" />
          Recent ({recent.length})
        </button>
        <button
          onClick={() => setActiveTab('folders')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === 'folders'
              ? 'text-primary-600 border-b-2 border-primary-600'
              : 'text-neutral-600 hover:text-neutral-900'
          }`}
        >
          <Folder className="w-4 h-4 inline mr-2" />
          Folders ({folders.length})
        </button>
      </div>

      {/* Content */}
      <div className="space-y-2">
        {activeTab === 'favorites' && (
          favorites.length === 0 ? (
            <div className="text-center py-8 text-neutral-600">
              <Star className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p className="text-sm">No favorites yet</p>
              <p className="text-xs mt-1">Click the star icon on any item to add it here</p>
            </div>
          ) : (
            favorites.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between p-3 hover:bg-neutral-50 rounded-lg group"
              >
                <a href={item.url} className="flex items-center space-x-3 flex-1">
                  <span className="text-xl">{favoritesService.getItemIcon(item.type)}</span>
                  <div className="flex-1">
                    <div className="font-medium text-sm">{item.title}</div>
                    {item.subtitle && (
                      <div className="text-xs text-neutral-600">{item.subtitle}</div>
                    )}
                  </div>
                </a>
                <button
                  onClick={() => handleRemoveFavorite(item.id)}
                  className="opacity-0 group-hover:opacity-100 p-1 hover:bg-neutral-200 rounded transition-opacity"
                >
                  <X className="w-4 h-4 text-neutral-600" />
                </button>
              </div>
            ))
          )
        )}

        {activeTab === 'recent' && (
          recent.length === 0 ? (
            <div className="text-center py-8 text-neutral-600">
              <Clock className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p className="text-sm">No recent items</p>
            </div>
          ) : (
            recent.map((item) => (
              <a
                key={item.id}
                href={item.url}
                className="flex items-center space-x-3 p-3 hover:bg-neutral-50 rounded-lg"
              >
                <span className="text-xl">{favoritesService.getItemIcon(item.type)}</span>
                <div className="flex-1">
                  <div className="font-medium text-sm">{item.title}</div>
                  {item.subtitle && (
                    <div className="text-xs text-neutral-600">{item.subtitle}</div>
                  )}
                </div>
                <div className="text-xs text-neutral-500">
                  {new Date(item.accessedAt).toLocaleDateString()}
                </div>
              </a>
            ))
          )
        )}

        {activeTab === 'folders' && (
          folders.length === 0 ? (
            <div className="text-center py-8 text-neutral-600">
              <Folder className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p className="text-sm">No folders yet</p>
              <p className="text-xs mt-1">Create folders to organize your favorites</p>
            </div>
          ) : (
            folders.map((folder) => (
              <div
                key={folder.id}
                className="p-3 hover:bg-neutral-50 rounded-lg group"
              >
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center space-x-2">
                    <Folder className="w-5 h-5 text-primary-600" />
                    <span className="font-medium text-sm">{folder.name}</span>
                    <span className="text-xs text-neutral-500">
                      ({folder.items.length} items)
                    </span>
                  </div>
                  <button
                    onClick={() => handleDeleteFolder(folder.id)}
                    className="opacity-0 group-hover:opacity-100 p-1 hover:bg-neutral-200 rounded transition-opacity"
                  >
                    <X className="w-4 h-4 text-neutral-600" />
                  </button>
                </div>
                {folder.description && (
                  <p className="text-xs text-neutral-600 ml-7">{folder.description}</p>
                )}
              </div>
            ))
          )
        )}
      </div>
    </Card>
  );
};
