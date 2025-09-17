import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Eye, EyeOff, Loader2 } from 'lucide-react';
import toast from 'react-hot-toast';
import Logo from '../common/Logo';
import { 
  useLoginMutation, 
  useVerifyMfaLoginMutation,
  useVerifyBackupCodeLoginMutation 
} from '../../services/authApi';
import { loginSuccess, mfaRequired as setMfaRequired } from '../../store/slices/authSlice';
import MFAChallenge from './MFAChallenge';
import BackupCodeRecovery from './BackupCodeRecovery';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('admin@demo.com');
  const [password, setPassword] = useState('Password123!');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);

  const [login, { isLoading }] = useLoginMutation();
  const [verifyMfa, { isLoading: isVerifyingMfa }] = useVerifyMfaLoginMutation();
  const [verifyBackupCode, { isLoading: isVerifyingBackupCode }] = useVerifyBackupCodeLoginMutation();
  
  const dispatch = useDispatch();
  const navigate = useNavigate();

  // MFA State
  const [showMfaPrompt, setShowMfaPrompt] = useState(false);
  const [showBackupCodePrompt, setShowBackupCodePrompt] = useState(false);
  const [mfaInfo, setMfaInfo] = useState<{
    userId: string;
    challengeId: string;
    email: string;
    username: string;
  } | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const response = await login({ 
        email, 
        password
      }).unwrap();
      
      if (response.success && response.data) {
        if (response.data.requiresMfa) {
          // MFA is required, show MFA verification
          setMfaInfo({
            userId: response.data.userId,
            challengeId: response.data.mfaChallengeId!,
            email: response.data.email,
            username: response.data.username,
          });
          
          // Store MFA information in Redux
          dispatch(setMfaRequired({
            userId: response.data.userId,
            username: response.data.username,
            email: response.data.email,
            mfaType: response.data.mfaType || 'totp',
            mfaChallengeId: response.data.mfaChallengeId!,
            mfaChallenge: response.data.mfaChallenge!
          }));
          
          setShowMfaPrompt(true);
          toast('Please enter your verification code to continue');
          return;
        }
        
        // No MFA required, proceed with login
        dispatch(loginSuccess({
          token: response.data.token!,
          userId: response.data.userId,
          username: response.data.username,
          email: response.data.email,
          fullName: response.data.username || response.data.email,
          roles: response.data.roles || []
        }));
        
        toast.success('Login successful! Welcome back.');
        navigate('/dashboard');
      }
    } catch (error: any) {
      console.error('Login error:', error);
      toast.error(error?.data?.message || 'Login failed. Please try again.');
    }
  };
  
  // Handle MFA verification
  const handleVerifyMfa = async (code: string): Promise<boolean> => {
    if (!mfaInfo) return false;
    
    try {
      const response = await verifyMfa({
        userId: mfaInfo.userId,
        challengeId: mfaInfo.challengeId,
        code,
        rememberDevice: rememberMe
      }).unwrap();
      
      if (response.success && response.data) {
        // Store user authentication in Redux
        dispatch(loginSuccess({
          token: response.data.token!,
          userId: response.data.userId,
          username: response.data.username,
          email: response.data.email,
          fullName: response.data.username || response.data.email,
          roles: response.data.roles || []
        }));
        
        // Redirect to dashboard after successful verification
        setTimeout(() => {
          navigate('/dashboard');
        }, 1500);
        
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('MFA verification error:', error);
      return false;
    }
  };
  
  // Handle backup code verification
  const handleVerifyBackupCode = async (backupCode: string): Promise<boolean> => {
    if (!mfaInfo) return false;
    
    try {
      const response = await verifyBackupCode({
        userId: mfaInfo.userId,
        backupCode,
        rememberDevice: rememberMe
      }).unwrap();
      
      if (response.success && response.data) {
        // Store user authentication in Redux
        dispatch(loginSuccess({
          token: response.data.token!,
          userId: response.data.userId,
          username: response.data.username,
          email: response.data.email,
          fullName: response.data.username || response.data.email,
          roles: response.data.roles || []
        }));
        
        // Redirect to dashboard after successful verification
        setTimeout(() => {
          navigate('/dashboard');
        }, 1500);
        
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('Backup code verification error:', error);
      return false;
    }
  };
  
  // Switch between MFA verification and backup code recovery
  const switchToBackupCode = () => {
    setShowMfaPrompt(false);
    setShowBackupCodePrompt(true);
  };
  
  const switchToMfa = () => {
    setShowBackupCodePrompt(false);
    setShowMfaPrompt(true);
  };
  
  const cancelMfa = () => {
    setShowMfaPrompt(false);
    setShowBackupCodePrompt(false);
    setMfaInfo(null);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-emerald-50 via-white to-teal-50 flex items-center justify-center p-4">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="w-full max-w-md"
      >
        {/* Header */}
        <div className="text-center mb-8">
          <Logo size="xl" showText={true} className="justify-center mb-6" />
          <p className="text-gray-600 text-lg">Enterprise Financial Management System</p>
          <p className="text-sm text-emerald-600 font-medium mt-1">Empowering Nigerian MFBs & SMEs</p>
        </div>

        {/* Login Form */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.3 }}
          className="bg-white rounded-2xl shadow-xl p-8"
        >
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-2">
                Email Address
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
                placeholder="Enter your email"
                required
              />
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-2">
                Password
              </label>
              <div className="relative">
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full px-4 py-3 pr-12 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
                  placeholder="Enter your password"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? (
                    <EyeOff className="w-5 h-5" />
                  ) : (
                    <Eye className="w-5 h-5" />
                  )}
                </button>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <input
                  id="remember-me"
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="h-4 w-4 text-emerald-600 focus:ring-emerald-500 border-gray-300 rounded"
                />
                <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-700">
                  Remember me
                </label>
              </div>

              <button
                type="button"
                className="text-sm text-emerald-600 hover:text-emerald-500 font-medium"
              >
                Forgot password?
              </button>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-emerald-600 text-white py-3 px-4 rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-5 h-5 animate-spin mr-2" />
                  Signing in...
                </>
              ) : (
                'Sign In'
              )}
            </button>
          </form>

          {/* Demo Credentials */}
          <div className="mt-6 p-4 bg-amber-50 border border-amber-200 rounded-lg">
            <p className="text-xs text-amber-800 font-medium mb-2">Demo Credentials:</p>
            <p className="text-xs text-amber-700">
              Email: admin@demo.com<br />
              Password: Password123!
            </p>
          </div>
        </motion.div>

        {/* Footer */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="text-center mt-8 text-sm text-gray-600"
        >
          <p>© 2025 Soar-Fin+ Technologies. All rights reserved.</p>
          <p className="mt-1">CBN Licensed • NDIC Insured • FIRS Compliant</p>
        </motion.div>
      </motion.div>
      
      {/* MFA Challenge Modal */}
      {showMfaPrompt && mfaInfo && (
        <MFAChallenge
          title="Security Verification Required"
          message="Please enter the verification code from your authenticator app to complete login."
          onVerify={handleVerifyMfa}
          onCancel={cancelMfa}
          onUseBackupCode={switchToBackupCode}
        />
      )}
      
      {/* Backup Code Recovery Modal */}
      {showBackupCodePrompt && mfaInfo && (
        <BackupCodeRecovery
          mfaToken={mfaInfo.challengeId}
          email={mfaInfo.email}
          onReturn={switchToMfa}
          onSuccess={(token) => {
            dispatch(loginSuccess({
              token,
              userId: mfaInfo.userId,
              username: mfaInfo.username,
              email: mfaInfo.email,
              fullName: mfaInfo.username || mfaInfo.email,
              roles: []
            }));
            toast.success('Login successful! Welcome back.');
            navigate('/dashboard');
          }}
        />
      )}
    </div>
  );
};

export default LoginPage;