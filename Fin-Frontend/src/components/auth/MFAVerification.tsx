import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Loader2, ShieldCheck, KeyRound, HelpCircle } from 'lucide-react';
import toast from 'react-hot-toast';

interface MFAVerificationProps {
  email: string;
  mfaToken: string;
  onVerify: (code: string, token: string) => Promise<any>;
  onUseBackupCode: () => void;
}

const MFAVerification: React.FC<MFAVerificationProps> = ({ 
  email, 
  mfaToken, 
  onVerify, 
  onUseBackupCode 
}) => {
  const [verificationCode, setVerificationCode] = useState(['', '', '', '', '', '']);
  const [isVerifying, setIsVerifying] = useState(false);
  const [countdown, setCountdown] = useState(30);
  const [showResend, setShowResend] = useState(false);
  const [inputRefs, setInputRefs] = useState<Array<HTMLInputElement | null>>([]);
  
  // Initialize input refs
  useEffect(() => {
    setInputRefs(Array(6).fill(null));
  }, []);

  // Countdown timer for OTP expiration
  useEffect(() => {
    const timer = countdown > 0 && setInterval(() => setCountdown(countdown - 1), 1000);
    if (countdown === 0) {
      setShowResend(true);
    }
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
  
  const handleVerify = async () => {
    const code = verificationCode.join('');
    if (code.length !== 6) {
      toast.error('Please enter a 6-digit verification code');
      return;
    }
    
    setIsVerifying(true);
    
    try {
      await onVerify(code, mfaToken);
    } catch (error: any) {
      console.error('MFA verification error:', error);
      toast.error(error?.data?.message || 'Invalid verification code. Please try again.');
      
      // Clear the code on error
      setVerificationCode(['', '', '', '', '', '']);
      inputRefs[0]?.focus();
    } finally {
      setIsVerifying(false);
    }
  };
  
  const handleResendCode = () => {
    toast.success('A new verification code has been sent');
    setCountdown(30);
    setShowResend(false);
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
  
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="bg-white rounded-xl shadow-lg p-6 w-full max-w-md mx-auto"
    >
      <div className="text-center mb-6">
        <div className="flex justify-center mb-4">
          <div className="bg-emerald-100 p-3 rounded-full">
            <ShieldCheck className="w-8 h-8 text-emerald-600" />
          </div>
        </div>
        <h2 className="text-2xl font-semibold text-gray-800">Two-Factor Authentication</h2>
        <p className="text-gray-600 mt-2">
          Enter the verification code from your authenticator app
        </p>
        <p className="text-sm text-gray-500 mt-1">
          For account: <span className="font-medium">{email}</span>
        </p>
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
              className="w-12 h-14 text-center text-2xl font-bold border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              required
            />
          ))}
        </div>
        
        <div className="text-center mt-3">
          {showResend ? (
            <button
              type="button"
              onClick={handleResendCode}
              className="text-sm text-emerald-600 hover:text-emerald-700 font-medium"
            >
              Resend verification code
            </button>
          ) : (
            <p className="text-sm text-gray-500">
              Code expires in <span className="font-medium">{countdown}s</span>
            </p>
          )}
        </div>
      </div>
      
      <button
        type="button"
        disabled={isVerifying || verificationCode.some(digit => digit === '')}
        onClick={handleVerify}
        className="w-full bg-emerald-600 text-white py-3 px-4 rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
      >
        {isVerifying ? (
          <>
            <Loader2 className="w-5 h-5 animate-spin mr-2" />
            Verifying...
          </>
        ) : (
          'Verify Code'
        )}
      </button>
      
      <div className="mt-6 flex justify-between items-center">
        <button
          type="button"
          onClick={onUseBackupCode}
          className="text-sm text-emerald-600 hover:text-emerald-700 flex items-center"
        >
          <KeyRound className="w-4 h-4 mr-1" />
          Use backup code
        </button>
        
        <button
          type="button"
          className="text-sm text-gray-500 hover:text-gray-700 flex items-center"
        >
          <HelpCircle className="w-4 h-4 mr-1" />
          Need help?
        </button>
      </div>
    </motion.div>
  );
};

export default MFAVerification;