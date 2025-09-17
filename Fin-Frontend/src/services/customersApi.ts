import { api } from './api';

export interface Customer {
  id: string;
  customerNumber: string;
  firstName: string;
  lastName: string;
  otherNames?: string;
  dateOfBirth: string;
  gender: 'male' | 'female' | 'other';
  maritalStatus: 'single' | 'married' | 'divorced' | 'widowed';
  nationality: string;
  stateOfOrigin?: string;
  phoneNumber: string;
  email?: string;
  occupation: string;
  employerName?: string;
  monthlyIncome?: number;
  bvn?: string;
  nin?: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode?: string;
  country: string;
  kycLevel: 1 | 2 | 3;
  kycStatus: 'pending' | 'in-progress' | 'approved' | 'rejected';
  accountType: 'individual' | 'business' | 'corporate';
  createdAt: string;
  updatedAt: string;
}

export interface KycDocument {
  id: string;
  customerId: string;
  documentType: 'id_card' | 'passport' | 'drivers_license' | 'utility_bill' | 'business_reg' | 'tax_cert';
  documentNumber: string;
  issueDate: string;
  expiryDate?: string;
  issuingAuthority: string;
  documentUrl: string;
  verificationStatus: 'pending' | 'verified' | 'rejected';
  verificationNotes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface RiskProfile {
  customerId: string;
  riskLevel: string;
  riskScore: number;
  factors: string[];
  evaluatedAt: string;
}

export interface RelationshipMap {
  customerId: string;
  nodes: RelationshipNode[];
  edges: RelationshipEdge[];
}

export interface RelationshipNode {
  id: string;
  name: string;
  type: string;
}

export interface RelationshipEdge {
  sourceId: string;
  targetId: string;
  relationshipType: string;
}

export interface OnboardingWorkflow {
  workflowId: string;
  customerId: string;
  status: string;
  startedAt: string;
  steps: string[];
}

export interface OnboardingRequest {
  customerId: string;
  channel?: string;
  initiatedBy?: string;
  metadata?: Record<string, string>;
}

interface CustomerResponse {
  success: boolean;
  data: Customer;
}

interface CustomersResponse {
  success: boolean;
  data: {
    customers: Customer[];
    total: number;
    page: number;
    limit: number;
  };
}

interface KycDocumentResponse {
  success: boolean;
  data: KycDocument;
}

interface KycDocumentsResponse {
  success: boolean;
  data: KycDocument[];
}

interface CreateCustomerRequest {
  firstName: string;
  lastName: string;
  otherNames?: string;
  dateOfBirth: string;
  gender: Customer['gender'];
  maritalStatus: Customer['maritalStatus'];
  nationality: string;
  stateOfOrigin?: string;
  phoneNumber: string;
  email?: string;
  occupation: string;
  employerName?: string;
  monthlyIncome?: number;
  bvn?: string;
  nin?: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode?: string;
  country: string;
  accountType: Customer['accountType'];
}

interface UpdateCustomerRequest extends Partial<CreateCustomerRequest> {
  id: string;
}

interface UploadKycDocumentRequest {
  customerId: string;
  documentType: KycDocument['documentType'];
  documentNumber: string;
  issueDate: string;
  expiryDate?: string;
  issuingAuthority: string;
  file: File;
}

interface UpdateKycDocumentRequest {
  id: string;
  verificationStatus: KycDocument['verificationStatus'];
  verificationNotes?: string;
}

interface CustomerFilters {
  search?: string;
  kycLevel?: Customer['kycLevel'];
  kycStatus?: Customer['kycStatus'];
  accountType?: Customer['accountType'];
  page?: number;
  limit?: number;
}

export const customersApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getCustomers: builder.query<CustomersResponse['data'], CustomerFilters>({
      query: (filters) => ({
        url: '/customers',
        params: filters,
      }),
    }),
    getCustomer: builder.query<Customer, string>({
      query: (id) => `/customers/${id}`,
      transformResponse: (response: CustomerResponse) => response.data,
    }),
    createCustomer: builder.mutation<Customer, CreateCustomerRequest>({
      query: (data) => ({
        url: '/customers',
        method: 'POST',
        body: data,
      }),
      transformResponse: (response: CustomerResponse) => response.data,
    }),
    updateCustomer: builder.mutation<Customer, UpdateCustomerRequest>({
      query: ({ id, ...data }) => ({
        url: `/customers/${id}`,
        method: 'PUT',
        body: data,
      }),
      transformResponse: (response: CustomerResponse) => response.data,
    }),
    getCustomerDocuments: builder.query<KycDocument[], string>({
      query: (customerId) => `/customers/${customerId}/documents`,
      transformResponse: (response: KycDocumentsResponse) => response.data,
    }),
    uploadKycDocument: builder.mutation<KycDocument, UploadKycDocumentRequest>({
      query: ({ file, ...data }) => {
        const formData = new FormData();
        formData.append('file', file);
        Object.entries(data).forEach(([key, value]) => {
          if (value !== undefined) {
            formData.append(key, String(value));
          }
        });
        return {
          url: '/kyc/documents',
          method: 'POST',
          body: formData,
          formData: true,
        };
      },
      transformResponse: (response: KycDocumentResponse) => response.data,
    }),
    updateKycDocument: builder.mutation<KycDocument, UpdateKycDocumentRequest>({
      query: ({ id, ...data }) => ({
        url: `/kyc/documents/${id}`,
        method: 'PUT',
        body: data,
      }),
      transformResponse: (response: KycDocumentResponse) => response.data,
    }),
    getRiskProfile: builder.query<RiskProfile, string>({
      query: (customerId) => `/client/profile/risk-profile?customerId=${customerId}`,
    }),
    getRelationshipMap: builder.query<RelationshipMap, string>({
      query: (customerId) => `/client/profile/relationship-map?customerId=${customerId}`,
    }),
    initiateOnboarding: builder.mutation<OnboardingWorkflow, OnboardingRequest>({
      query: (data) => ({
        url: `/client/profile/onboarding/initiate`,
        method: 'POST',
        body: data,
      }),
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetCustomersQuery,
  useGetCustomerQuery,
  useCreateCustomerMutation,
  useUpdateCustomerMutation,
  useGetCustomerDocumentsQuery,
  useUploadKycDocumentMutation,
  useUpdateKycDocumentMutation,
  useGetRiskProfileQuery,
  useGetRelationshipMapQuery,
  useInitiateOnboardingMutation,
} = customersApi;