// @ts-nocheck
import { api } from './api';

interface InventoryItem {
  id: string;
  itemCode: string;
  itemName: string;
  category: string;
  currentStock: number;
  reorderLevel: number;
  unitCost: number;
  sellingPrice: number;
  totalValue: number;
  status: string;
  lastUpdated: string;
}

interface InventoryItemDetail {
  id: string;
  itemCode: string;
  itemName: string;
  description?: string;
  category: string;
  brand?: string;
  model?: string;
  sku?: string;
  barcode?: string;
  unitOfMeasure: string;
  unitCost: number;
  sellingPrice: number;
  currentStock: number;
  reorderLevel: number;
  maximumLevel: number;
  reservedStock: number;
  availableStock: number;
  valuationMethod: string;
  status: string;
  supplier?: string;
  location?: string;
  notes?: string;
  recentTransactions: Array<{
    transactionNumber: string;
    transactionType: string;
    transactionDate: string;
    quantity: number;
    unitCost: number;
    totalCost: number;
    description?: string;
  }>;
  createdAt: string;
}

interface CreateInventoryItemRequest {
  itemCode: string;
  itemName: string;
  description?: string;
  itemType: number;
  category: number;
  brand?: string;
  model?: string;
  sku?: string;
  barcode?: string;
  unitOfMeasure: string;
  unitCost: number;
  sellingPrice: number;
  reorderLevel: number;
  maximumLevel: number;
  initialStock: number;
  valuationMethod: number;
  trackStock: boolean;
  supplier?: string;
  location?: string;
  notes?: string;
  inventoryGLAccountId: string;
  cogsGLAccountId: string;
}

interface InventoryResponse {
  success: boolean;
  data: InventoryItem[];
}

interface InventoryDetailResponse {
  success: boolean;
  data: InventoryItemDetail;
}

interface CreateInventoryResponse {
  success: boolean;
  data: InventoryItem;
  message: string;
}

export const inventoryApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getInventoryItems: builder.query<InventoryResponse, void>({
      query: () => '/inventory',
      providesTags: ['Inventory'],
    }),
    getInventoryItem: builder.query<InventoryDetailResponse, string>({
      query: (id) => `/inventory/${id}`,
      providesTags: ['Inventory'],
    }),
    createInventoryItem: builder.mutation<CreateInventoryResponse, CreateInventoryItemRequest>({
      query: (item) => ({
        url: '/inventory',
        method: 'POST',
        body: item,
      }),
      invalidatesTags: ['Inventory'],
    }),
    getLowStockItems: builder.query<InventoryResponse, void>({
      query: () => '/inventory/low-stock',
      providesTags: ['Inventory'],
    }),
    createStockAdjustment: builder.mutation<{ success: boolean; data: any }, { id: string; adjustment: any }>({
      query: ({ id, adjustment }) => ({
        url: `/inventory/${id}/adjustment`,
        method: 'POST',
        body: adjustment,
      }),
      invalidatesTags: ['Inventory'],
    }),
    createInventoryTransaction: builder.mutation<{ success: boolean; data: any }, { id: string; transaction: any }>({
      query: ({ id, transaction }) => ({
        url: `/inventory/${id}/transaction`,
        method: 'POST',
        body: transaction,
      }),
      invalidatesTags: ['Inventory'],
    }),
  }),
  overrideExisting: false,
});

export const { 
  useGetInventoryItemsQuery,
  useGetInventoryItemQuery,
  useCreateInventoryItemMutation,
  useGetLowStockItemsQuery,
  useCreateStockAdjustmentMutation,
  useCreateInventoryTransactionMutation
} = inventoryApi;