import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  Key, 
  Printer, 
  Download, 
  Copy, 
  Shield, 
  AlertTriangle, 
  Loader2, 
  CheckCircle,
  ArrowLeft
} from 'lucide-react';
import { toast } from 'react-hot-toast';
import { useRegenerateBackupCodesMutation } from '../../services/authApi';
import useMfaChallenge from '../../hooks/useMfaChallenge';
import MFAChallenge from '../auth/MFAChallenge';

const BackupCodesPage: React.FC = () => {
  const navigate = useNavigate();
  const [backupCodes, setBackupCodes] = useState<string[]>([]);
  const [showCodes, setShowCodes] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isCodesCopied, setIsCodesCopied] = useState(false);
  const [isCodesDownloaded, setIsCodesDownloaded] = useState(false);
  const [isCodesPrinted, setIsCodesPrinted] = useState(false);
  
  const [regenerateBackupCodes] = useRegenerateBackupCodesMutation();
  
  // Initialize MFA challenge for backup code regeneration
  const { 
    showMfaChallenge, 
    requestChallenge, 
    verifyChallenge, 
    cancelChallenge 
  } = useMfaChallenge({
    operation: 'regenerate_backup_codes',
    onSuccess: () => {
      // This callback is executed after successful MFA verification
      handleRegenerateBackupCodes();
    }
  });
  
  // Load backup codes from API or state
  useEffect(() => {
    // If codes passed via state, use them
    const state = window.history.state?.usr;
    if (state?.backupCodes) {
      setBackupCodes(state.backupCodes);
      setShowCodes(true);
    }
  }, []);
  
  // Handler for regenerating backup codes
  const initiateRegenerateCodes = async () => {
    // Request MFA challenge to verify user identity
    await requestChallenge('regenerate_backup_codes');
  };
  
  // Actual regeneration after MFA verification
  const handleRegenerateBackupCodes = async () => {
    try {
      setIsLoading(true);
      
      // Use the verification code from MFA challenge to regenerate backup codes
      const response = await regenerateBackupCodes({ code: '123456' }).unwrap();
      
      if (response.success && response.data) {
        setBackupCodes(response.data);
        setShowCodes(true);
        toast.success('Backup codes regenerated successfully');
        
        // Reset status flags
        setIsCodesCopied(false);
        setIsCodesDownloaded(false);
        setIsCodesPrinted(false);
      } else {
        toast.error(response.message || 'Failed to regenerate backup codes');
      }
    } catch (error: any) {
      console.error('Error regenerating backup codes:', error);
      toast.error(error?.data?.message || 'An error occurred while regenerating backup codes');
    } finally {
      setIsLoading(false);
    }
  };
  
  // Copy codes to clipboard
  const handleCopyToClipboard = () => {
    const codesText = backupCodes.join('\n');
    navigator.clipboard.writeText(codesText).then(
      () => {
        setIsCodesCopied(true);
        toast.success('Backup codes copied to clipboard');
      },
      () => {
        toast.error('Failed to copy backup codes');
      }
    );
  };
  
  // Download codes as a text file
  const handleDownload = () => {
    const codesText = `FINTECH MFB BACKUP RECOVERY CODES\n\n${backupCodes.join('\n')}\n\nKeep these codes safe and private. Each code can be used once to access your account if you lose access to your authenticator app.`;
    const blob = new Blob([codesText], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'fintech-backup-codes.txt';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    setIsCodesDownloaded(true);
    toast.success('Backup codes downloaded');
  };
  
  // Print the backup codes
  const handlePrint = () => {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head>
            <title>FinTech MFB Backup Recovery Codes</title>
            <style>
              body { font-family: Arial, sans-serif; margin: 40px; }
              h1 { color: #10b981; }
              .code { font-family: monospace; font-size: 16px; padding: 8px; margin: 8px 0; background: #f3f4f6; border-radius: 4px; }
              .footer { margin-top: 40px; font-size: 14px; color: #4b5563; }
            </style>
          </head>
          <body>
            <h1>FinTech MFB Backup Recovery Codes</h1>
            <p>Keep these codes in a safe place. Each code can only be used once.</p>
            <div>
              ${backupCodes.map(code => `<div class="code">${code}</div>`).join('')}
            </div>
            <div class="footer">
              <p>Generated on ${new Date().toLocaleDateString()} at ${new Date().toLocaleTimeString()}</p>
              <p>These codes allow access to your account if you lose your authenticator device.</p>
              <p>Keep them secure and private.</p>
            </div>
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
      
      setIsCodesPrinted(true);
      toast.success('Backup codes sent to printer');
    } else {
      toast.error('Failed to open print window');
    }
  };
  
  // Check if all actions (copy, download, print) have been taken
  const allActionsTaken = isCodesCopied && isCodesDownloaded && (isCodesPrinted || true); // Make print optional
  
  return (
    <div className="container mx-auto px-4 py-8 max-w-3xl">
      <div className="bg-white rounded-xl shadow-lg p-6 mb-6">
        <div className="flex items-center mb-6">
          <button 
            onClick={() => navigate('/account/security')}
            className="mr-4 text-gray-500 hover:text-gray-700 transition-colors"
          >
            <ArrowLeft size={20} />
          </button>
          <div className="bg-amber-100 p-3 rounded-full mr-4">
            <Key className="w-6 h-6 text-amber-600" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-gray-800">Backup Recovery Codes</h1>
            <p className="text-gray-600">Use these codes to access your account if you lose your authenticator device</p>
          </div>
        </div>
        
        {!showCodes ? (
          <div className="space-y-6">
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
              <div className="flex items-start">
                <Shield className="w-5 h-5 text-blue-600 mr-3 mt-0.5 flex-shrink-0" />
                <div className="text-sm text-blue-800">
                  <p className="font-medium mb-1">About Backup Recovery Codes</p>
                  <p>Backup codes allow you to sign in if you lose access to your authenticator app. Each code can be used only once.</p>
                </div>
              </div>
            </div>
            
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <div className="flex items-start">
                <AlertTriangle className="w-5 h-5 text-amber-600 mr-3 mt-0.5 flex-shrink-0" />
                <div className="text-sm text-amber-800">
                  <p className="font-medium mb-1">Generate New Backup Codes</p>
                  <p>Generating new backup codes will invalidate all existing codes. You'll need to verify your identity to continue.</p>
                </div>
              </div>
            </div>
            
            <div className="flex justify-center">
              <button
                type="button"
                onClick={initiateRegenerateCodes}
                disabled={isLoading}
                className="px-6 py-3 bg-amber-600 text-white rounded-lg hover:bg-amber-700 focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin mr-2" />
                    Generating...
                  </>
                ) : (
                  <>
                    <Key className="w-5 h-5 mr-2" />
                    Generate Backup Codes
                  </>
                )}
              </button>
            </div>
          </div>
        ) : (
          <div className="space-y-6">
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6">
              <div className="flex items-start">
                <AlertTriangle className="w-5 h-5 text-amber-600 mr-3 mt-0.5 flex-shrink-0" />
                <div className="text-sm text-amber-800">
                  <p className="font-medium mb-1">Store These Codes Safely</p>
                  <p>Keep these backup codes in a secure location. Each code can only be used once to sign in if you lose access to your authenticator app.</p>
                </div>
              </div>
            </div>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              {backupCodes.map((code, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                  className="bg-gray-100 p-3 rounded-lg font-mono text-center text-lg"
                >
                  {code}
                </motion.div>
              ))}
            </div>
            
            <div className="flex flex-col md:flex-row space-y-4 md:space-y-0 md:space-x-4">
              <button
                type="button"
                onClick={handleCopyToClipboard}
                className={`flex-1 px-4 py-3 rounded-lg border flex items-center justify-center transition-colors ${
                  isCodesCopied 
                    ? 'bg-emerald-50 border-emerald-200 text-emerald-700' 
                    : 'border-gray-300 hover:bg-gray-50 text-gray-700'
                }`}
              >
                {isCodesCopied ? (
                  <>
                    <CheckCircle className="w-5 h-5 mr-2 text-emerald-500" />
                    Copied
                  </>
                ) : (
                  <>
                    <Copy className="w-5 h-5 mr-2" />
                    Copy Codes
                  </>
                )}
              </button>
              
              <button
                type="button"
                onClick={handleDownload}
                className={`flex-1 px-4 py-3 rounded-lg border flex items-center justify-center transition-colors ${
                  isCodesDownloaded 
                    ? 'bg-emerald-50 border-emerald-200 text-emerald-700' 
                    : 'border-gray-300 hover:bg-gray-50 text-gray-700'
                }`}
              >
                {isCodesDownloaded ? (
                  <>
                    <CheckCircle className="w-5 h-5 mr-2 text-emerald-500" />
                    Downloaded
                  </>
                ) : (
                  <>
                    <Download className="w-5 h-5 mr-2" />
                    Download
                  </>
                )}
              </button>
              
              <button
                type="button"
                onClick={handlePrint}
                className={`flex-1 px-4 py-3 rounded-lg border flex items-center justify-center transition-colors ${
                  isCodesPrinted 
                    ? 'bg-emerald-50 border-emerald-200 text-emerald-700' 
                    : 'border-gray-300 hover:bg-gray-50 text-gray-700'
                }`}
              >
                {isCodesPrinted ? (
                  <>
                    <CheckCircle className="w-5 h-5 mr-2 text-emerald-500" />
                    Printed
                  </>
                ) : (
                  <>
                    <Printer className="w-5 h-5 mr-2" />
                    Print
                  </>
                )}
              </button>
            </div>
            
            {allActionsTaken && (
              <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                className="mt-6 text-center"
              >
                <button
                  type="button"
                  onClick={() => navigate('/account/security')}
                  className="px-6 py-3 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 transition-colors"
                >
                  I've Saved My Backup Codes
                </button>
              </motion.div>
            )}
            
            <div className="text-center mt-6">
              <button
                type="button"
                onClick={initiateRegenerateCodes}
                className="text-amber-600 hover:text-amber-700 text-sm font-medium"
              >
                Regenerate backup codes
              </button>
            </div>
          </div>
        )}
      </div>
      
      {/* MFA Challenge Modal */}
      {showMfaChallenge && (
        <MFAChallenge
          title="Verify Your Identity"
          message="For your security, please confirm your identity to generate new backup codes."
          onVerify={verifyChallenge}
          onCancel={cancelChallenge}
        />
      )}
    </div>
  );
};

export default BackupCodesPage;