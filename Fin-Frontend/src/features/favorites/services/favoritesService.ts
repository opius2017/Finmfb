import { FavoriteItem, RecentItem, FavoriteFolder } from '../types/favorites.types';

export class FavoritesService {
  private favorites: FavoriteItem[] = [];
  private recent: RecentItem[] = [];
  private folders: FavoriteFolder[] = [];
  private maxRecentItems = 20;

  constructor() {
    this.loadFromStorage();
  }

  // Favorites
  addFavorite(item: Omit<FavoriteItem, 'id' | 'createdAt'>): FavoriteItem {
    const favorite: FavoriteItem = {
      ...item,
      id: Date.now().toString(),
      createdAt: new Date(),
      order: this.favorites.length,
    };

    this.favorites.push(favorite);
    this.saveToStorage();
    return favorite;
  }

  removeFavorite(id: string): void {
    this.favorites = this.favorites.filter(f => f.id !== id);
    this.saveToStorage();
  }

  getFavorites(): FavoriteItem[] {
    return this.favorites.sort((a, b) => (a.order || 0) - (b.order || 0));
  }

  isFavorite(entityId: string, type: string): boolean {
    return this.favorites.some(f => f.entityId === entityId && f.type === type);
  }

  reorderFavorites(favoriteIds: string[]): void {
    favoriteIds.forEach((id, index) => {
      const favorite = this.favorites.find(f => f.id === id);
      if (favorite) {
        favorite.order = index;
      }
    });
    this.saveToStorage();
  }

  // Recent Items
  addRecentItem(item: Omit<RecentItem, 'id' | 'accessedAt' | 'accessCount'>): void {
    const existing = this.recent.find(r => r.entityId === item.entityId && r.type === item.type);

    if (existing) {
      existing.accessedAt = new Date();
      existing.accessCount++;
      existing.title = item.title; // Update title in case it changed
      existing.subtitle = item.subtitle;
    } else {
      const recentItem: RecentItem = {
        ...item,
        id: Date.now().toString(),
        accessedAt: new Date(),
        accessCount: 1,
      };
      this.recent.unshift(recentItem);
    }

    // Limit size
    if (this.recent.length > this.maxRecentItems) {
      this.recent = this.recent.slice(0, this.maxRecentItems);
    }

    this.saveToStorage();
  }

  getRecentItems(limit?: number): RecentItem[] {
    const sorted = this.recent.sort((a, b) => 
      b.accessedAt.getTime() - a.accessedAt.getTime()
    );
    return limit ? sorted.slice(0, limit) : sorted;
  }

  clearRecentItems(): void {
    this.recent = [];
    this.saveToStorage();
  }

  // Folders
  createFolder(name: string, description?: string): FavoriteFolder {
    const folder: FavoriteFolder = {
      id: Date.now().toString(),
      name,
      description,
      items: [],
      createdAt: new Date(),
      order: this.folders.length,
    };

    this.folders.push(folder);
    this.saveToStorage();
    return folder;
  }

  updateFolder(id: string, updates: Partial<FavoriteFolder>): void {
    const folder = this.folders.find(f => f.id === id);
    if (folder) {
      Object.assign(folder, updates);
      this.saveToStorage();
    }
  }

  deleteFolder(id: string): void {
    this.folders = this.folders.filter(f => f.id !== id);
    this.saveToStorage();
  }

  getFolders(): FavoriteFolder[] {
    return this.folders.sort((a, b) => a.order - b.order);
  }

  addToFolder(folderId: string, favoriteId: string): void {
    const folder = this.folders.find(f => f.id === folderId);
    if (folder && !folder.items.includes(favoriteId)) {
      folder.items.push(favoriteId);
      this.saveToStorage();
    }
  }

  removeFromFolder(folderId: string, favoriteId: string): void {
    const folder = this.folders.find(f => f.id === folderId);
    if (folder) {
      folder.items = folder.items.filter(id => id !== favoriteId);
      this.saveToStorage();
    }
  }

  getFolderItems(folderId: string): FavoriteItem[] {
    const folder = this.folders.find(f => f.id === folderId);
    if (!folder) return [];

    return folder.items
      .map(id => this.favorites.find(f => f.id === id))
      .filter(Boolean) as FavoriteItem[];
  }

  // Storage
  private loadFromStorage(): void {
    try {
      const favoritesData = localStorage.getItem('favorites');
      const recentData = localStorage.getItem('recentItems');
      const foldersData = localStorage.getItem('favoriteFolders');

      if (favoritesData) {
        this.favorites = JSON.parse(favoritesData).map((f: any) => ({
          ...f,
          createdAt: new Date(f.createdAt),
        }));
      }

      if (recentData) {
        this.recent = JSON.parse(recentData).map((r: any) => ({
          ...r,
          accessedAt: new Date(r.accessedAt),
        }));
      }

      if (foldersData) {
        this.folders = JSON.parse(foldersData).map((f: any) => ({
          ...f,
          createdAt: new Date(f.createdAt),
        }));
      }
    } catch (error) {
      console.error('Failed to load from storage:', error);
    }
  }

  private saveToStorage(): void {
    try {
      localStorage.setItem('favorites', JSON.stringify(this.favorites));
      localStorage.setItem('recentItems', JSON.stringify(this.recent));
      localStorage.setItem('favoriteFolders', JSON.stringify(this.folders));
    } catch (error) {
      console.error('Failed to save to storage:', error);
    }
  }

  // Utility methods
  getItemIcon(type: string): string {
    const icons: Record<string, string> = {
      customer: '👤',
      vendor: '🏢',
      invoice: '📄',
      payment: '💳',
      product: '📦',
      transaction: '💰',
      report: '📊',
      page: '📑',
    };
    return icons[type] || '📄';
  }

  exportFavorites(): string {
    return JSON.stringify({
      favorites: this.favorites,
      folders: this.folders,
    }, null, 2);
  }

  importFavorites(data: string): void {
    try {
      const parsed = JSON.parse(data);
      if (parsed.favorites) {
        this.favorites = parsed.favorites.map((f: any) => ({
          ...f,
          createdAt: new Date(f.createdAt),
        }));
      }
      if (parsed.folders) {
        this.folders = parsed.folders.map((f: any) => ({
          ...f,
          createdAt: new Date(f.createdAt),
        }));
      }
      this.saveToStorage();
    } catch (error) {
      console.error('Failed to import favorites:', error);
      throw new Error('Invalid import data');
    }
  }
}

export const favoritesService = new FavoritesService();
