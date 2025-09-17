import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { RootState } from '../store/store';

const baseQuery = fetchBaseQuery({
  baseUrl: 'https://localhost:5001/api', // Update with your backend URL
  prepareHeaders: (headers, { getState }) => {
    const token = (getState() as RootState).auth.token;
    
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    
    headers.set('Content-Type', 'application/json');
    return headers;
  },
});

export const api = createApi({
  baseQuery,
  tagTypes: ['Dashboard'],
  endpoints: () => ({}),
});