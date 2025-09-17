import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { Loader2, AlertTriangle, ArrowLeft } from 'lucide-react';
import toast from 'react-hot-toast';
import { useValidateBackupCodeMutation } from '../../services/authApi';

interface BackupCodeRecoveryProps {
  mfaToken: string;
  email: string;
  onReturn: () => void;
  onSuccess: (token: string) => void;
}

const BackupCodeRecovery: React.FC<BackupCodeRecoveryProps> = ({ 
  mfaToken,
  email,
  onReturn,
  onSuccess 
}) => {
  const [backupCode, setBackupCode] = useState('');
  const [validateBackupCode, { isLoading }] = useValidateBackupCodeMutation();
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!backupCode.trim()) {
      toast.error('Please enter a backup code');
      return;
    }
    try {
      const response = await validateBackupCode({
        mfaToken,
        backupCode: backupCode.trim().toUpperCase(),
      }).unwrap();
      if (response.success) {
        toast.success('Backup code validated successfully');
        onSuccess(response.data?.token || response.token || '');
      } else {
        toast.error('Invalid backup code. Please try again.');
      }
    } catch (error: any) {
      console.error('Backup code validation error:', error);
      toast.error(error?.data?.message || 'Failed to validate backup code');
    }
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="bg-white rounded-xl shadow-lg p-6 w-full max-w-md mx-auto"
    >
      <div className="mb-5 flex items-center">
        <button 
          onClick={onReturn}
          className="mr-3 text-gray-500 hover:text-gray-700 transition-colors"
          aria-label="Go back to MFA entry"
        >
          <ArrowLeft size={20} />
        </button>
        <h2 className="text-2xl font-semibold text-gray-800">Use Backup Code</h2>
      </div>
      
      <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6 flex items-start">
        <AlertTriangle className="w-5 h-5 text-amber-600 mr-3 mt-0.5 flex-shrink-0" />
        <div className="text-sm text-amber-800">
          <p className="font-medium mb-1">Use your backup code to access your account</p>
          <p>If you've lost access to your authenticator app, you can use one of your backup codes to sign in.</p>
        </div>
      </div>
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="backup-code" className="block text-sm font-medium text-gray-700 mb-2">
            Backup Recovery Code
          </label>
          <input
            id="backup-code"
            type="text"
            value={backupCode}
            onChange={(e) => setBackupCode(e.target.value.replace(/\s/g, ''))}
            placeholder="Enter your backup code"
            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
            autoComplete="one-time-code"
            maxLength={16}
            required
          />
          <p className="mt-2 text-sm text-gray-500">
            Backup codes are case-insensitive and should be 8-16 characters long
          </p>
        </div>
        
        <button
          type="submit"
          disabled={isLoading || backupCode.length < 8}
          className="w-full bg-emerald-600 text-white py-3 px-4 rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
        >
          {isLoading ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin mr-2" />
              Verifying...
            </>
          ) : (
            'Verify Backup Code'
          )}
        </button>
      </form>
      
      <div className="mt-5 text-center">
        <button
          type="button"
          onClick={onReturn}
          className="text-sm text-emerald-600 hover:text-emerald-700"
        >
          Return to verification code entry
        </button>
      </div>
      
      <div className="mt-6 text-xs text-gray-500 border-t pt-4">
        <p>If you don't have access to your backup codes, please contact support for assistance.</p>
      </div>
    </motion.div>
  );
};

export default BackupCodeRecovery;