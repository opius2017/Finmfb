// Favorites and Recent Items Types
export interface FavoriteItem {
  id: string;
  type: ItemType;
  entityId: string;
  title: string;
  subtitle?: string;
  url: string;
  icon?: string;
  metadata?: Record<string, any>;
  createdAt: Date;
  order?: number;
}

export interface RecentItem {
  id: string;
  type: ItemType;
  entityId: string;
  title: string;
  subtitle?: string;
  url: string;
  icon?: string;
  metadata?: Record<string, any>;
  accessedAt: Date;
  accessCount: number;
}

export type ItemType = 
  | 'customer' 
  | 'vendor' 
  | 'invoice' 
  | 'payment' 
  | 'product' 
  | 'transaction' 
  | 'report' 
  | 'page';

export interface FavoriteFolder {
  id: string;
  name: string;
  description?: string;
  items: string[]; // Favorite IDs
  color?: string;
  icon?: string;
  createdAt: Date;
  order: number;
}

export interface QuickAccessMenu {
  favorites: FavoriteItem[];
  recent: RecentItem[];
  folders: FavoriteFolder[];
}
