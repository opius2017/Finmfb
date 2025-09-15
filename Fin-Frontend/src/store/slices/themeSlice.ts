import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface ThemeState {
  primaryColor: string;
  organizationType: 'MFB' | 'SME';
  companyName: string;
  logoUrl: string;
  sidebarCollapsed: boolean;
}

const initialState: ThemeState = {
  primaryColor: '#059669', // Emerald-600 for financial institutions
  organizationType: 'MFB',
  companyName: 'Soar-Fin+',
  logoUrl: '',
  sidebarCollapsed: false,
};

const themeSlice = createSlice({
  name: 'theme',
  initialState,
  reducers: {
    setPrimaryColor: (state, action: PayloadAction<string>) => {
      state.primaryColor = action.payload;
    },
    setOrganizationType: (state, action: PayloadAction<'MFB' | 'SME'>) => {
      state.organizationType = action.payload;
    },
    setCompanyName: (state, action: PayloadAction<string>) => {
      state.companyName = action.payload;
    },
    setLogoUrl: (state, action: PayloadAction<string>) => {
      state.logoUrl = action.payload;
    },
    toggleSidebar: (state) => {
      state.sidebarCollapsed = !state.sidebarCollapsed;
    },
    setSidebarCollapsed: (state, action: PayloadAction<boolean>) => {
      state.sidebarCollapsed = action.payload;
    },
  },
});

export const {
  setPrimaryColor,
  setOrganizationType,
  setCompanyName,
  setLogoUrl,
  toggleSidebar,
  setSidebarCollapsed,
} = themeSlice.actions;

export default themeSlice.reducer;