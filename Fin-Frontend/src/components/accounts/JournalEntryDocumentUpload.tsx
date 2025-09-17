import React, { useState } from 'react';
import toast from 'react-hot-toast';

interface Props {
  journalEntryId: string;
}

const JournalEntryDocumentUpload: React.FC<Props> = ({ journalEntryId }) => {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    }
  };

  const handleUpload = async () => {
    if (!file) return;
    setUploading(true);
    const formData = new FormData();
    formData.append('file', file);
    try {
      const res = await fetch(`/api/journalentries/${journalEntryId}/upload-document`, {
        method: 'POST',
        body: formData,
      });
      if (res.ok) {
        toast.success('Document uploaded and attached');
        setFile(null);
      } else {
        toast.error('Upload failed');
      }
    } catch {
      toast.error('Upload failed');
    }
    setUploading(false);
  };

  return (
    <div className="space-y-2">
      <input type="file" onChange={handleFileChange} disabled={uploading} />
      <button
        onClick={handleUpload}
        disabled={!file || uploading}
        className="px-4 py-2 bg-emerald-600 text-white rounded hover:bg-emerald-700"
      >
        {uploading ? 'Uploading...' : 'Upload Document'}
      </button>
    </div>
  );
};

export default JournalEntryDocumentUpload;
