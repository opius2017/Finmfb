import React, { useState, useCallback } from 'react';
import { Upload, File, X, Check, AlertCircle } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { UploadProgress } from '../types/document.types';
import { documentService } from '../services/documentService';

interface DocumentUploadProps {
  onUploadComplete?: (documentId: string) => void;
  category?: string;
}

export const DocumentUpload: React.FC<DocumentUploadProps> = ({ onUploadComplete, category }) => {
  const [files, setFiles] = useState<File[]>([]);
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState<Record<string, UploadProgress>>({});
  const [dragActive, setDragActive] = useState(false);

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    const droppedFiles = Array.from(e.dataTransfer.files);
    handleFiles(droppedFiles);
  }, []);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const selectedFiles = Array.from(e.target.files);
      handleFiles(selectedFiles);
    }
  };

  const handleFiles = (newFiles: File[]) => {
    const validFiles = newFiles.filter(file => {
      const validation = documentService.validateFile(file);
      if (!validation.valid) {
        alert(`${file.name}: ${validation.error}`);
        return false;
      }
      return true;
    });

    setFiles(prev => [...prev, ...validFiles]);
  };

  const removeFile = (index: number) => {
    setFiles(prev => prev.filter((_, i) => i !== index));
  };

  const handleUpload = async () => {
    setUploading(true);

    for (const file of files) {
      try {
        await documentService.uploadDocument(
          file,
          { category: category as any, name: file.name },
          (prog) => {
            setProgress(prev => ({ ...prev, [file.name]: prog }));
          }
        );

        setProgress(prev => ({
          ...prev,
          [file.name]: { ...prev[file.name], status: 'complete' },
        }));
      } catch (error) {
        setProgress(prev => ({
          ...prev,
          [file.name]: { ...prev[file.name], status: 'error', error: 'Upload failed' },
        }));
      }
    }

    setUploading(false);
    setTimeout(() => {
      setFiles([]);
      setProgress({});
      if (onUploadComplete) onUploadComplete('');
    }, 2000);
  };

  return (
    <Card className="p-6">
      <h2 className="text-xl font-bold mb-6">Upload Documents</h2>

      <div
        className={`border-2 border-dashed rounded-lg p-12 text-center transition-colors ${
          dragActive
            ? 'border-primary-500 bg-primary-50'
            : 'border-neutral-300 hover:border-primary-400'
        }`}
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
      >
        <Upload className="w-16 h-16 text-neutral-400 mx-auto mb-4" />
        <p className="text-lg font-semibold mb-2">Drag and drop files here</p>
        <p className="text-sm text-neutral-600 mb-4">
          or click to browse (PDF, Images, Word, Excel - Max 50MB)
        </p>
        <input
          type="file"
          id="file-upload"
          className="hidden"
          multiple
          onChange={handleFileSelect}
          accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,.xls,.xlsx"
        />
        <label htmlFor="file-upload">
          <Button variant="primary">
            Browse Files
          </Button>
        </label>
      </div>

      {files.length > 0 && (
        <div className="mt-6">
          <h3 className="font-semibold mb-3">Selected Files ({files.length})</h3>
          <div className="space-y-2">
            {files.map((file, index) => {
              const fileProgress = progress[file.name];
              return (
                <div key={index} className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg">
                  <div className="flex items-center space-x-3 flex-1">
                    <File className="w-5 h-5 text-neutral-600" />
                    <div className="flex-1">
                      <div className="font-medium">{file.name}</div>
                      <div className="text-sm text-neutral-600">
                        {documentService.formatFileSize(file.size)}
                      </div>
                      {fileProgress && (
                        <div className="mt-1">
                          {fileProgress.status === 'uploading' && (
                            <div className="w-full bg-neutral-200 rounded-full h-2">
                              <div
                                className="bg-primary-600 h-2 rounded-full transition-all"
                                style={{ width: `${fileProgress.progress}%` }}
                              />
                            </div>
                          )}
                          {fileProgress.status === 'complete' && (
                            <div className="flex items-center text-success-600 text-sm">
                              <Check className="w-4 h-4 mr-1" />
                              Uploaded
                            </div>
                          )}
                          {fileProgress.status === 'error' && (
                            <div className="flex items-center text-error-600 text-sm">
                              <AlertCircle className="w-4 h-4 mr-1" />
                              {fileProgress.error}
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                  {!uploading && (
                    <button
                      onClick={() => removeFile(index)}
                      className="text-neutral-400 hover:text-error-600"
                    >
                      <X className="w-5 h-5" />
                    </button>
                  )}
                </div>
              );
            })}
          </div>

          <div className="mt-6 flex justify-end space-x-3">
            <Button variant="outline" onClick={() => setFiles([])} disabled={uploading}>
              Clear All
            </Button>
            <Button variant="primary" onClick={handleUpload} disabled={uploading || files.length === 0}>
              {uploading ? 'Uploading...' : `Upload ${files.length} File(s)`}
            </Button>
          </div>
        </div>
      )}
    </Card>
  );
};
