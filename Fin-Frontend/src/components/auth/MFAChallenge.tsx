import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { ShieldAlert, Loader2, XCircle, CheckCircle, Key, HelpCircle } from 'lucide-react';
import toast from 'react-hot-toast';

interface MFAChallengeProps {
  title?: string;
  message?: string;
  onVerify: (code: string) => Promise<boolean>;
  onCancel: () => void;
  onUseBackupCode?: () => void;
}

const MFAChallenge: React.FC<MFAChallengeProps> = ({
  title = 'Security Verification Required',
  message = 'For your security, please confirm your identity by entering the verification code from your authenticator app.',
  onVerify,
  onCancel,
  onUseBackupCode
}) => {
  const [verificationCode, setVerificationCode] = useState(['', '', '', '', '', '']);
  const [isVerifying, setIsVerifying] = useState(false);
  const [verificationStatus, setVerificationStatus] = useState<'pending' | 'success' | 'error'>('pending');
  const [inputRefs, setInputRefs] = useState<Array<HTMLInputElement | null>>([]);
  const [countdown, setCountdown] = useState(30);
  
  // Initialize input refs
  useEffect(() => {
    setInputRefs(Array(6).fill(null));
  }, []);
  
  // Countdown timer for OTP expiration
  useEffect(() => {
    const timer = countdown > 0 && setInterval(() => setCountdown(countdown - 1), 1000);
    return () => clearInterval(timer as NodeJS.Timeout);
  }, [countdown]);
  
  // Auto-submit when all digits are entered
  useEffect(() => {
    if (verificationCode.every(digit => digit !== '')) {
      handleVerify();
    }
  }, [verificationCode]);
  
  const handleDigitChange = (index: number, value: string) => {
    // Allow only digits
    if (!/^\d*$/.test(value)) return;
    
    const newVerificationCode = [...verificationCode];
    newVerificationCode[index] = value;
    setVerificationCode(newVerificationCode);
    
    // Auto-focus next input
    if (value && index < 5 && inputRefs[index + 1]) {
      inputRefs[index + 1]?.focus();
    }
  };
  
  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    // Handle backspace: clear current field and focus previous
    if (e.key === 'Backspace' && !verificationCode[index] && index > 0) {
      const newVerificationCode = [...verificationCode];
      newVerificationCode[index - 1] = '';
      setVerificationCode(newVerificationCode);
      inputRefs[index - 1]?.focus();
    }
  };
  
  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData('text');
    
    // If pasted data is a 6-digit number
    if (/^\d{6}$/.test(pastedData)) {
      const digits = pastedData.split('');
      setVerificationCode(digits);
    }
  };
  
  const handleVerify = async () => {
    const code = verificationCode.join('');
    if (code.length !== 6) {
      toast.error('Please enter a 6-digit verification code');
      return;
    }
    
    setIsVerifying(true);
    
    try {
      const isVerified = await onVerify(code);
      setVerificationStatus(isVerified ? 'success' : 'error');
      
      if (isVerified) {
        toast.success('Verification successful');
        // Auto close after success
        setTimeout(() => {
          onCancel();
        }, 1500);
      } else {
        toast.error('Invalid verification code');
        // Clear the code on error
        setVerificationCode(['', '', '', '', '', '']);
        inputRefs[0]?.focus();
        setVerificationStatus('pending');
      }
    } catch (error) {
      console.error('MFA verification error:', error);
      toast.error('Verification failed. Please try again.');
      setVerificationStatus('error');
      
      // Clear the code on error
      setVerificationCode(['', '', '', '', '', '']);
      inputRefs[0]?.focus();
      
      // Reset status after a delay
      setTimeout(() => {
        setVerificationStatus('pending');
      }, 1500);
    } finally {
      setIsVerifying(false);
    }
  };
  
  const renderVerificationStatus = () => {
    if (verificationStatus === 'success') {
      return (
        <motion.div
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          className="absolute inset-0 flex flex-col items-center justify-center bg-white bg-opacity-90 rounded-xl z-10"
        >
          <CheckCircle className="w-16 h-16 text-emerald-500 mb-4" />
          <h3 className="text-xl font-semibold text-emerald-700">Verification Successful</h3>
          <p className="text-emerald-600 mt-2">Proceeding with your request...</p>
        </motion.div>
      );
    }
    
    if (verificationStatus === 'error' && !isVerifying) {
      return (
        <motion.div
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          className="absolute inset-0 flex flex-col items-center justify-center bg-white bg-opacity-90 rounded-xl z-10"
        >
          <XCircle className="w-16 h-16 text-red-500 mb-4" />
          <h3 className="text-xl font-semibold text-red-700">Verification Failed</h3>
          <p className="text-red-600 mt-2">Please try again with a valid code</p>
        </motion.div>
      );
    }
    
    return null;
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="fixed inset-0 flex items-center justify-center z-50 p-4 bg-black bg-opacity-50"
    >
      <motion.div
        initial={{ y: 20 }}
        animate={{ y: 0 }}
        className="bg-white rounded-xl shadow-xl p-6 w-full max-w-md relative overflow-hidden"
      >
        {renderVerificationStatus()}
        
        <button
          type="button"
          onClick={onCancel}
          className="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
          disabled={isVerifying}
        >
          <XCircle className="w-5 h-5" />
        </button>
        
        <div className="text-center mb-6">
          <div className="flex justify-center mb-4">
            <div className="bg-amber-100 p-3 rounded-full">
              <ShieldAlert className="w-8 h-8 text-amber-600" />
            </div>
          </div>
          <h2 className="text-xl font-semibold text-gray-800">{title}</h2>
          <p className="text-gray-600 mt-2">{message}</p>
        </div>
        
        <div className="mb-6">
          <div className="flex justify-center space-x-2">
            {verificationCode.map((digit, index) => (
              <input
                key={index}
                type="text"
                inputMode="numeric"
                maxLength={1}
                value={digit}
                onChange={(e) => handleDigitChange(index, e.target.value)}
                onKeyDown={(e) => handleKeyDown(index, e)}
                onPaste={index === 0 ? handlePaste : undefined}
                ref={(ref) => {
                  inputRefs[index] = ref;
                }}
                className="w-10 h-12 text-center text-xl font-bold border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-amber-500 transition-colors"
                disabled={isVerifying || verificationStatus !== 'pending'}
                autoFocus={index === 0}
                required
              />
            ))}
          </div>
          
          <div className="text-center mt-3">
            <p className="text-sm text-gray-500">
              Code expires in <span className="font-medium">{countdown}s</span>
            </p>
          </div>
        </div>
        
        <div className="flex flex-col space-y-3">
          <button
            type="button"
            onClick={handleVerify}
            disabled={isVerifying || verificationCode.some(digit => digit === '') || verificationStatus !== 'pending'}
            className="w-full bg-amber-600 text-white py-2 px-4 rounded-lg hover:bg-amber-700 focus:ring-2 focus:ring-amber-500 focus:ring-offset-2 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {isVerifying ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin mr-2" />
                Verifying...
              </>
            ) : (
              'Verify'
            )}
          </button>
          
          {onUseBackupCode && (
            <button
              type="button"
              onClick={onUseBackupCode}
              disabled={isVerifying || verificationStatus !== 'pending'}
              className="text-amber-600 hover:text-amber-700 text-sm font-medium py-2 flex items-center justify-center"
            >
              <Key className="w-4 h-4 mr-1" />
              Use backup code instead
            </button>
          )}
          
          <button
            type="button"
            onClick={onCancel}
            disabled={isVerifying}
            className="text-gray-500 hover:text-gray-700 text-sm py-2 flex items-center justify-center"
          >
            Cancel this operation
          </button>
        </div>
        
        <div className="mt-6 text-center">
          <button
            type="button"
            className="text-gray-500 hover:text-gray-700 text-xs flex items-center justify-center mx-auto"
          >
            <HelpCircle className="w-3 h-3 mr-1" />
            Need help?
          </button>
        </div>
      </motion.div>
    </motion.div>
  );
};

export default MFAChallenge;