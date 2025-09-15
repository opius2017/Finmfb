import React, { useState } from 'react';
import { useParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Upload, AlertCircle, CheckCircle, XCircle, Loader2 } from 'lucide-react';
import toast from 'react-hot-toast';
import {
  useGetCustomerQuery,
  useGetCustomerDocumentsQuery,
  useUploadKycDocumentMutation,
  useUpdateKycDocumentMutation,
  KycDocument,
} from '../../services/customersApi';

interface DocumentTypeInfo {
  title: string;
  description: string;
  required: boolean;
  maxSize: number; // in MB
  allowedTypes: string[];
}

const documentTypes: Record<KycDocument['documentType'], DocumentTypeInfo> = {
  id_card: {
    title: 'National ID Card',
    description: 'Upload a valid national ID card (front and back)',
    required: true,
    maxSize: 5,
    allowedTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  },
  passport: {
    title: 'International Passport',
    description: 'Upload your international passport data page',
    required: false,
    maxSize: 5,
    allowedTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  },
  drivers_license: {
    title: "Driver's License",
    description: "Upload your valid driver's license (front and back)",
    required: false,
    maxSize: 5,
    allowedTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  },
  utility_bill: {
    title: 'Utility Bill',
    description: 'Recent utility bill (not older than 3 months)',
    required: true,
    maxSize: 2,
    allowedTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  },
  business_reg: {
    title: 'Business Registration',
    description: 'Certificate of incorporation or business registration',
    required: false,
    maxSize: 5,
    allowedTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  },
  tax_cert: {
    title: 'Tax Certificate',
    description: 'Valid tax clearance certificate',
    required: false,
    maxSize: 2,
    allowedTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  },
};

interface UploadFormData {
  documentNumber: string;
  issueDate: string;
  expiryDate?: string;
  issuingAuthority: string;
}

const DocumentUploadForm: React.FC<{
  documentType: KycDocument['documentType'];
  customerId: string;
  onSuccess: () => void;
}> = ({ documentType, customerId, onSuccess }) => {
  const [formData, setFormData] = useState<UploadFormData>({
    documentNumber: '',
    issueDate: '',
    issuingAuthority: '',
  });
  const [file, setFile] = useState<File | null>(null);
  const [uploadKycDocument, { isLoading }] = useUploadKycDocumentMutation();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) return;

    try {
      await uploadKycDocument({
        customerId,
        documentType,
        file,
        ...formData,
      }).unwrap();
      toast.success('Document uploaded successfully');
      onSuccess();
    } catch (error) {
      toast.error('Failed to upload document');
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (!selectedFile) return;

    const { maxSize, allowedTypes } = documentTypes[documentType];
    const fileSize = selectedFile.size / (1024 * 1024); // Convert to MB

    if (!allowedTypes.includes(selectedFile.type)) {
      toast.error('Invalid file type');
      return;
    }

    if (fileSize > maxSize) {
      toast.error(`File size must be less than ${maxSize}MB`);
      return;
    }

    setFile(selectedFile);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700">Document File</label>
        <div className="mt-1">
          <input
            type="file"
            onChange={handleFileChange}
            accept={documentTypes[documentType].allowedTypes.join(',')}
            className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-medium file:bg-emerald-50 file:text-emerald-700 hover:file:bg-emerald-100"
          />
        </div>
        <p className="mt-1 text-xs text-gray-500">
          Max size: {documentTypes[documentType].maxSize}MB. Accepted formats: JPG, PNG, PDF
        </p>
      </div>

      <div>
        <label htmlFor="documentNumber" className="block text-sm font-medium text-gray-700">
          Document Number
        </label>
        <input
          type="text"
          id="documentNumber"
          value={formData.documentNumber}
          onChange={(e) => setFormData({ ...formData, documentNumber: e.target.value })}
          required
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
        />
      </div>

      <div>
        <label htmlFor="issueDate" className="block text-sm font-medium text-gray-700">
          Issue Date
        </label>
        <input
          type="date"
          id="issueDate"
          value={formData.issueDate}
          onChange={(e) => setFormData({ ...formData, issueDate: e.target.value })}
          required
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
        />
      </div>

      <div>
        <label htmlFor="expiryDate" className="block text-sm font-medium text-gray-700">
          Expiry Date
        </label>
        <input
          type="date"
          id="expiryDate"
          value={formData.expiryDate || ''}
          onChange={(e) => setFormData({ ...formData, expiryDate: e.target.value })}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
        />
      </div>

      <div>
        <label htmlFor="issuingAuthority" className="block text-sm font-medium text-gray-700">
          Issuing Authority
        </label>
        <input
          type="text"
          id="issuingAuthority"
          value={formData.issuingAuthority}
          onChange={(e) => setFormData({ ...formData, issuingAuthority: e.target.value })}
          required
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
        />
      </div>

      <div className="pt-4">
        <button
          type="submit"
          disabled={isLoading || !file}
          className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-emerald-600 hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin mr-2" />
              Uploading...
            </>
          ) : (
            <>
              <Upload className="w-5 h-5 mr-2" />
              Upload Document
            </>
          )}
        </button>
      </div>
    </form>
  );
};

const DocumentVerification: React.FC<{
  document: KycDocument;
}> = ({ document }) => {
  const [updateDocument] = useUpdateKycDocumentMutation();

  const handleVerification = async (status: KycDocument['verificationStatus'], notes?: string) => {
    try {
      await updateDocument({
        id: document.id,
        verificationStatus: status,
        verificationNotes: notes,
      }).unwrap();
      toast.success('Document verification status updated');
    } catch (error) {
      toast.error('Failed to update verification status');
    }
  };

  return (
    <div className="p-4 bg-gray-50 rounded-lg">
      <h3 className="font-medium text-gray-900">
        {documentTypes[document.documentType].title}
      </h3>
      <div className="mt-2 flex items-center justify-between">
        <div className="flex items-center space-x-2">
          {document.verificationStatus === 'verified' && (
            <CheckCircle className="w-5 h-5 text-green-500" />
          )}
          {document.verificationStatus === 'rejected' && (
            <XCircle className="w-5 h-5 text-red-500" />
          )}
          {document.verificationStatus === 'pending' && (
            <AlertCircle className="w-5 h-5 text-yellow-500" />
          )}
          <span className="text-sm text-gray-700">{document.documentNumber}</span>
        </div>
        <div className="flex space-x-2">
          <button
            onClick={() => handleVerification('verified')}
            className="px-3 py-1 text-sm bg-green-100 text-green-800 rounded-md hover:bg-green-200"
          >
            Verify
          </button>
          <button
            onClick={() => handleVerification('rejected', 'Document invalid or unclear')}
            className="px-3 py-1 text-sm bg-red-100 text-red-800 rounded-md hover:bg-red-200"
          >
            Reject
          </button>
        </div>
      </div>
      <div className="mt-2 text-xs text-gray-500">
        <p>Issued: {new Date(document.issueDate).toLocaleDateString()}</p>
        {document.expiryDate && (
          <p>Expires: {new Date(document.expiryDate).toLocaleDateString()}</p>
        )}
      </div>
    </div>
  );
};

const KycVerification: React.FC = () => {
  const { customerId } = useParams<{ customerId: string }>();
  const { data: customer } = useGetCustomerQuery(customerId!);
  const { data: documents } = useGetCustomerDocumentsQuery(customerId!);
  const [selectedDocumentType, setSelectedDocumentType] = useState<KycDocument['documentType'] | null>(null);

  const getRequiredDocuments = () => {
    const required = Object.entries(documentTypes)
      .filter(([, info]) => info.required)
      .map(([type]) => type as KycDocument['documentType']);

    if (customer?.accountType === 'business' || customer?.accountType === 'corporate') {
      required.push('business_reg', 'tax_cert');
    }

    return required;
  };

  const missingDocuments = getRequiredDocuments().filter(
    (type) => !documents?.some((doc) => doc.documentType === type)
  );

  return (
    <div className="p-6 space-y-6">
      <div>
        <h2 className="text-lg font-medium text-gray-900">KYC Documents</h2>
        <p className="mt-1 text-sm text-gray-500">
          Upload and manage customer verification documents
        </p>
      </div>

      {missingDocuments.length > 0 && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <h3 className="text-sm font-medium text-yellow-800">Missing Required Documents</h3>
          <ul className="mt-2 text-sm text-yellow-700 list-disc list-inside">
            {missingDocuments.map((type) => (
              <li key={type}>{documentTypes[type].title}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="space-y-4">
        {documents?.map((doc) => (
          <DocumentVerification key={doc.id} document={doc} />
        ))}
      </div>

      <div>
        <h3 className="text-md font-medium text-gray-900 mb-4">Upload New Document</h3>
        <select
          value={selectedDocumentType || ''}
          onChange={(e) => setSelectedDocumentType(e.target.value as KycDocument['documentType'])}
          className="block w-full rounded-md border-gray-300 shadow-sm focus:border-emerald-500 focus:ring-emerald-500"
        >
          <option value="">Select Document Type</option>
          {Object.entries(documentTypes).map(([type, info]) => (
            <option key={type} value={type}>
              {info.title}
            </option>
          ))}
        </select>

        {selectedDocumentType && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            className="mt-4"
          >
            <DocumentUploadForm
              documentType={selectedDocumentType}
              customerId={customerId!}
              onSuccess={() => setSelectedDocumentType(null)}
            />
          </motion.div>
        )}
      </div>
    </div>
  );
};

export default KycVerification;