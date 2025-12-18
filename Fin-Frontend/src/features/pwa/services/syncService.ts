import { offlineStorageService } from './offlineStorageService';

export class SyncService {
  private syncing = false;

  async syncPendingOperations(): Promise<{ success: number; failed: number }> {
    if (this.syncing) {
      console.log('Sync already in progress');
      return { success: 0, failed: 0 };
    }

    if (!navigator.onLine) {
      console.log('Device is offline, skipping sync');
      return { success: 0, failed: 0 };
    }

    this.syncing = true;
    let success = 0;
    let failed = 0;

    try {
      const pending = await offlineStorageService.getPendingSync();

      for (const operation of pending) {
        try {
          await this.syncOperation(operation);
          await offlineStorageService.markSynced(operation.id);
          success++;
        } catch (error) {
          console.error('Failed to sync operation:', operation, error);
          failed++;
        }
      }
    } finally {
      this.syncing = false;
    }

    return { success, failed };
  }

  private async syncOperation(operation: any): Promise<void> {
    const { type, endpoint, method, data } = operation;

    const response = await fetch(endpoint, {
      method: method || 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.getAuthToken()}`,
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`Sync failed: ${response.statusText}`);
    }
  }

  private getAuthToken(): string {
    return localStorage.getItem('auth_token') || '';
  }

  async registerBackgroundSync(): Promise<void> {
    if ('serviceWorker' in navigator && 'sync' in ServiceWorkerRegistration.prototype) {
      const registration = await navigator.serviceWorker.ready;
      const reg = registration as any;
      if (reg.sync) {
        await reg.sync.register('sync-transactions');
      }
    }
  }

  isOnline(): boolean {
    return navigator.onLine;
  }

  onOnline(callback: () => void): void {
    window.addEventListener('online', callback);
  }

  onOffline(callback: () => void): void {
    window.addEventListener('offline', callback);
  }
}

export const syncService = new SyncService();
