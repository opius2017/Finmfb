import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface User {
  userId: string;
  username: string;
  email: string;
  fullName: string;
  roles: string[];
}

interface MfaChallenge {
  challengeId: string;
  expiresAt: string;
}

interface TrustedDevice {
  id: string;
  deviceName: string;
  deviceType: string;
  browser: string;
  operatingSystem: string;
  lastUsed: string;
  location: string;
  ipAddress: string;
  isCurrent: boolean;
}

interface SecurityPreferences {
  emailNotificationsEnabled: boolean;
  loginNotificationsEnabled: boolean;
  unusualActivityNotificationsEnabled: boolean;
}

interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  user: User | null;
  tenant: { name: string; code: string } | null;
  expiryDate: string | null;
  mfaRequired: boolean;
  mfaType: string | null;
  mfaChallenge: MfaChallenge | null;
  mfaPending: boolean;
  trustedDevices: TrustedDevice[];
  securityPreferences: SecurityPreferences | null;
  lastMfaVerification: string | null;
}

const initialState: AuthState = {
  isAuthenticated: false,
  token: localStorage.getItem('fintech_token'),
  user: null,
  tenant: null,
  expiryDate: localStorage.getItem('fintech_expiry'),
  mfaRequired: false,
  mfaType: null,
  mfaChallenge: null,
  mfaPending: false,
  trustedDevices: [],
  securityPreferences: null,
  lastMfaVerification: localStorage.getItem('fintech_last_mfa')
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loginSuccess: (state, action: PayloadAction<{
      token: string;
      userId: string;
      username: string;
      email: string;
      fullName: string;
      roles: string[];
      tenant?: { name: string; code: string };
    }>) => {
      state.isAuthenticated = true;
      state.token = action.payload.token;
      state.user = {
        userId: action.payload.userId,
        username: action.payload.username,
        email: action.payload.email,
        fullName: action.payload.fullName,
        roles: action.payload.roles
      };
      state.tenant = action.payload.tenant || null;
      
      // Set expiry to 3 hours from now (matches token expiry set in AuthController)
      const expiryDate = new Date(new Date().getTime() + 3 * 60 * 60 * 1000).toISOString();
      state.expiryDate = expiryDate;
      
      // Reset MFA-related state
      state.mfaRequired = false;
      state.mfaPending = false;
      state.mfaChallenge = null;
      
      // Store authentication data in localStorage
      localStorage.setItem('fintech_token', action.payload.token);
      localStorage.setItem('fintech_expiry', expiryDate);
      localStorage.setItem('fintech_last_login', new Date().toISOString());
    },
    
    mfaRequired: (state, action: PayloadAction<{
      userId: string;
      username: string;
      email: string;
      mfaType: string;
      mfaChallengeId: string;
      mfaChallenge: MfaChallenge;
    }>) => {
      // Set user information for the MFA verification
      state.user = {
        userId: action.payload.userId,
        username: action.payload.username,
        email: action.payload.email,
        fullName: action.payload.username, // fallback
        roles: []
      };
      
      // Set MFA challenge information
      state.mfaRequired = true;
      state.mfaPending = true;
      state.mfaType = action.payload.mfaType;
      state.mfaChallenge = action.payload.mfaChallenge;
      
      // Store the user email for recovery purposes
      localStorage.setItem('fintech_mfa_email', action.payload.email);
    },
    
    mfaVerified: (state, action: PayloadAction<{
      token: string;
      userId: string;
      username: string;
      email: string;
      fullName: string;
      roles: string[];
    }>) => {
      state.isAuthenticated = true;
      state.token = action.payload.token;
      state.user = {
        userId: action.payload.userId,
        username: action.payload.username,
        email: action.payload.email,
        fullName: action.payload.fullName,
        roles: action.payload.roles
      };
      
      // Set expiry to 3 hours from now
      const expiryDate = new Date(new Date().getTime() + 3 * 60 * 60 * 1000).toISOString();
      state.expiryDate = expiryDate;
      
      // Reset MFA state
      state.mfaRequired = false;
      state.mfaPending = false;
      state.mfaType = null;
      state.mfaChallenge = null;
      
      // Record last MFA verification time
      const now = new Date().toISOString();
      state.lastMfaVerification = now;
      
      // Store updated authentication data
      localStorage.setItem('fintech_token', action.payload.token);
      localStorage.setItem('fintech_expiry', expiryDate);
      localStorage.setItem('fintech_last_mfa', now);
      localStorage.setItem('fintech_last_login', now);
      localStorage.removeItem('fintech_mfa_email');
    },
    
    setTrustedDevices: (state, action: PayloadAction<TrustedDevice[]>) => {
      state.trustedDevices = action.payload;
    },
    
    addTrustedDevice: (state, action: PayloadAction<TrustedDevice>) => {
      state.trustedDevices.push(action.payload);
    },
    
    removeTrustedDevice: (state, action: PayloadAction<string>) => {
      state.trustedDevices = state.trustedDevices.filter(device => device.id !== action.payload);
    },
    
    setSecurityPreferences: (state, action: PayloadAction<SecurityPreferences>) => {
      state.securityPreferences = action.payload;
    },
    
    newMfaChallenge: (state, action: PayloadAction<MfaChallenge>) => {
      state.mfaChallenge = action.payload;
    },
    
    mfaChallengeVerified: (state) => {
      const now = new Date().toISOString();
      state.lastMfaVerification = now;
      localStorage.setItem('fintech_last_mfa', now);
    },
    
    logout: (state) => {
      state.isAuthenticated = false;
      state.token = null;
      state.user = null;
      state.tenant = null;
      state.expiryDate = null;
      state.mfaRequired = false;
      state.mfaType = null;
      state.mfaChallenge = null;
      state.mfaPending = false;
      
      // Clear authentication data
      localStorage.removeItem('fintech_token');
      localStorage.removeItem('fintech_expiry');
      localStorage.removeItem('fintech_last_login');
      localStorage.removeItem('fintech_last_mfa');
      localStorage.removeItem('fintech_mfa_email');
      // Keep trusted device info on normal logout for better UX
    },
    
    forceLogout: (state) => {
      state.isAuthenticated = false;
      state.token = null;
      state.user = null;
      state.tenant = null;
      state.expiryDate = null;
      state.mfaRequired = false;
      state.mfaType = null;
      state.mfaChallenge = null;
      state.mfaPending = false;
      state.trustedDevices = [];
      state.securityPreferences = null;
      
      // Clear ALL stored data including trusted device info
      localStorage.removeItem('fintech_token');
      localStorage.removeItem('fintech_expiry');
      localStorage.removeItem('fintech_last_login');
      localStorage.removeItem('fintech_last_mfa');
      localStorage.removeItem('fintech_mfa_email');
      localStorage.removeItem('fintech_trusted_device');
    },
    
    checkAuthStatus: (state) => {
      const token = localStorage.getItem('fintech_token');
      const expiry = localStorage.getItem('fintech_expiry');
      
      if (token && expiry && new Date(expiry) > new Date()) {
        state.isAuthenticated = true;
        state.token = token;
        state.expiryDate = expiry;
        
        // Check if MFA re-verification should be tracked
        const lastMfa = localStorage.getItem('fintech_last_mfa');
        if (lastMfa) {
          state.lastMfaVerification = lastMfa;
        }
      } else {
        // Token expired or not present - logout
        state.isAuthenticated = false;
        state.token = null;
        state.user = null;
        state.tenant = null;
        state.expiryDate = null;
        state.mfaRequired = false;
        state.mfaType = null;
        state.mfaChallenge = null;
        state.mfaPending = false;
        
        localStorage.removeItem('fintech_token');
        localStorage.removeItem('fintech_expiry');
        localStorage.removeItem('fintech_last_mfa');
        localStorage.removeItem('fintech_mfa_email');
        // Keep trusted device info for smoother re-login
      }
    },
  },
});

export const { 
  loginSuccess, 
  logout, 
  forceLogout,
  checkAuthStatus, 
  mfaRequired, 
  mfaVerified,
  setTrustedDevices,
  addTrustedDevice,
  removeTrustedDevice,
  setSecurityPreferences,
  newMfaChallenge,
  mfaChallengeVerified
} = authSlice.actions;

export default authSlice.reducer;