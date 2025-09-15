import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
}

interface Tenant {
  id: string;
  name: string;
  code: string;
  logoUrl: string;
  organizationType: string;
}

interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  user: User | null;
  tenant: Tenant | null;
  expiryDate: string | null;
}

const initialState: AuthState = {
  isAuthenticated: false,
  token: localStorage.getItem('fintech_token'),
  user: null,
  tenant: null,
  expiryDate: localStorage.getItem('fintech_expiry'),
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loginSuccess: (state, action: PayloadAction<{
      token: string;
      user: User;
      tenant: Tenant;
      expiryDate: string;
    }>) => {
      state.isAuthenticated = true;
      state.token = action.payload.token;
      state.user = action.payload.user;
      state.tenant = action.payload.tenant;
      state.expiryDate = action.payload.expiryDate;
      
      localStorage.setItem('fintech_token', action.payload.token);
      localStorage.setItem('fintech_expiry', action.payload.expiryDate);
    },
    logout: (state) => {
      state.isAuthenticated = false;
      state.token = null;
      state.user = null;
      state.tenant = null;
      state.expiryDate = null;
      
      localStorage.removeItem('fintech_token');
      localStorage.removeItem('fintech_expiry');
    },
    checkAuthStatus: (state) => {
      const token = localStorage.getItem('fintech_token');
      const expiry = localStorage.getItem('fintech_expiry');
      
      if (token && expiry && new Date(expiry) > new Date()) {
        state.isAuthenticated = true;
        state.token = token;
        state.expiryDate = expiry;
      } else {
        state.isAuthenticated = false;
        state.token = null;
        state.user = null;
        state.tenant = null;
        state.expiryDate = null;
        localStorage.removeItem('fintech_token');
        localStorage.removeItem('fintech_expiry');
      }
    },
  },
});

export const { loginSuccess, logout, checkAuthStatus } = authSlice.actions;
export default authSlice.reducer;