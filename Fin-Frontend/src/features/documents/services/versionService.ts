import { DocumentVersion } from '../types/document.types';

export class VersionService {
  private apiEndpoint = '/api/documents';

  async getVersions(documentId: string): Promise<DocumentVersion[]> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}/versions`);
    if (!response.ok) throw new Error('Failed to fetch versions');
    return response.json();
  }

  async uploadNewVersion(documentId: string, file: File, changes: string): Promise<DocumentVersion> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('changes', changes);

    const response = await fetch(`${this.apiEndpoint}/${documentId}/versions`, {
      method: 'POST',
      body: formData,
    });
    if (!response.ok) throw new Error('Failed to upload new version');
    return response.json();
  }

  async restoreVersion(documentId: string, version: number): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}/versions/${version}/restore`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to restore version');
  }

  async compareVersions(documentId: string, version1: number, version2: number): Promise<any> {
    const response = await fetch(
      `${this.apiEndpoint}/${documentId}/versions/compare?v1=${version1}&v2=${version2}`
    );
    if (!response.ok) throw new Error('Failed to compare versions');
    return response.json();
  }

  async downloadVersion(documentId: string, version: number): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}/versions/${version}/download`);
    if (!response.ok) throw new Error('Failed to download version');
    return response.blob();
  }
}

export const versionService = new VersionService();
