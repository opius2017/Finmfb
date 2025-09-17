import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  Shield,
  Smartphone,
  Check,
  Copy,
  Info,
  ArrowRight,
  ArrowLeft,
  Download,
  Printer,
  Loader2
} from 'lucide-react';
import { QRCodeSVG } from 'qrcode.react';
import toast from 'react-hot-toast';

interface MFASetupWizardProps {
  userId: string;
  userEmail: string;
  onComplete: (isEnabled: boolean) => void;
  onGenerateMfaSecret: () => Promise<{ secret: string; qrCodeUrl: string; backupCodes: string[] }>;
  onVerifyMfaSetup: (code: string) => Promise<boolean>;
}

const MFASetupWizard: React.FC<MFASetupWizardProps> = ({
  userId,
  userEmail,
  onComplete,
  onGenerateMfaSecret,
  onVerifyMfaSetup
}) => {
  const [currentStep, setCurrentStep] = useState(1);
  const [mfaSecret, setMfaSecret] = useState('');
  const [qrCodeUrl, setQrCodeUrl] = useState('');
  const [backupCodes, setBackupCodes] = useState<string[]>([]);
  const [verificationCode, setVerificationCode] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [hasVerified, setHasVerified] = useState(false);
  const [hasCopiedSecret, setHasCopiedSecret] = useState(false);
  const [hasSavedBackupCodes, setHasSavedBackupCodes] = useState(false);
  
  const steps = [
    'Introduction',
    'Generate Secret',
    'Verify Setup',
    'Backup Codes',
    'Complete'
  ];
  
  useEffect(() => {
    if (currentStep === 2 && !mfaSecret) {
      generateMfaSecret();
    }
  }, [currentStep]);
  
  const generateMfaSecret = async () => {
    setIsLoading(true);
    try {
      const result = await onGenerateMfaSecret();
      setMfaSecret(result.secret);
      setQrCodeUrl(result.qrCodeUrl);
      setBackupCodes(result.backupCodes);
    } catch (error) {
      console.error('Error generating MFA secret:', error);
      toast.error('Failed to generate MFA setup information');
    } finally {
      setIsLoading(false);
    }
  };
  
  const verifyMfaSetup = async () => {
    if (verificationCode.length !== 6) {
      toast.error('Please enter a 6-digit verification code');
      return;
    }
    
    setIsLoading(true);
    try {
      const isVerified = await onVerifyMfaSetup(verificationCode);
      if (isVerified) {
        setHasVerified(true);
        setCurrentStep(4);
        toast.success('MFA verification successful');
      } else {
        toast.error('Invalid verification code. Please try again.');
        setVerificationCode('');
      }
    } catch (error) {
      console.error('Error verifying MFA setup:', error);
      toast.error('Failed to verify MFA setup');
    } finally {
      setIsLoading(false);
    }
  };
  
  const copyToClipboard = (text: string, type: 'secret' | 'codes') => {
    navigator.clipboard.writeText(text).then(
      () => {
        if (type === 'secret') {
          setHasCopiedSecret(true);
          toast.success('Secret copied to clipboard');
        } else {
          toast.success('Backup codes copied to clipboard');
        }
      },
      (err) => {
        console.error('Could not copy text: ', err);
        toast.error('Failed to copy to clipboard');
      }
    );
  };
  
  const downloadBackupCodes = () => {
    const element = document.createElement('a');
    const content = `FINMFB MFA BACKUP CODES\n\nKeep these backup codes in a safe place. Each code can only be used once.\n\n${backupCodes.join('\n')}\n\nGenerated on: ${new Date().toLocaleString()}\nFor account: ${userEmail}`;
    const file = new Blob([content], { type: 'text/plain' });
    element.href = URL.createObjectURL(file);
    element.download = `finmfb-backup-codes-${userId}.txt`;
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
    setHasSavedBackupCodes(true);
    toast.success('Backup codes downloaded');
  };
  
  const handleNextStep = () => {
    if (currentStep < steps.length) {
      setCurrentStep(currentStep + 1);
    }
  };
  
  const handlePreviousStep = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };
  
  const handleSkip = () => {
    onComplete(false);
  };
  
  const handleComplete = () => {
    onComplete(true);
  };
  
  const renderStepIndicator = () => {
    return (
      <div className="flex items-center justify-center mb-6">
        {steps.map((step, index) => (
          <React.Fragment key={index}>
            <div 
              className={`w-8 h-8 rounded-full flex items-center justify-center ${
                index + 1 < currentStep
                  ? 'bg-emerald-500 text-white'
                  : index + 1 === currentStep
                  ? 'bg-emerald-100 border-2 border-emerald-500 text-emerald-700'
                  : 'bg-gray-100 text-gray-400'
              }`}
            >
              {index + 1 < currentStep ? (
                <Check className="w-4 h-4" />
              ) : (
                <span className="text-sm font-medium">{index + 1}</span>
              )}
            </div>
            {index < steps.length - 1 && (
              <div 
                className={`w-12 h-1 ${
                  index + 1 < currentStep ? 'bg-emerald-500' : 'bg-gray-200'
                }`}
              />
            )}
          </React.Fragment>
        ))}
      </div>
    );
  };
  
  const renderStep1 = () => {
    return (
      <div className="text-center">
        <div className="mx-auto w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mb-4">
          <Shield className="w-8 h-8 text-emerald-600" />
        </div>
        <h2 className="text-2xl font-semibold text-gray-800 mb-3">
          Set Up Two-Factor Authentication
        </h2>
        <p className="text-gray-600 mb-6">
          Protect your account with an extra layer of security. When 2FA is enabled, you'll need both your password and a verification code to sign in.
        </p>
        
        <div className="bg-blue-50 border border-blue-100 rounded-lg p-4 mb-6 text-left">
          <div className="flex">
            <Info className="w-5 h-5 text-blue-500 mr-2 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="font-medium text-blue-800 mb-1">You'll need:</h3>
              <ul className="text-blue-700 text-sm space-y-1">
                <li className="flex items-center">
                  <Check className="w-4 h-4 mr-1 text-emerald-500" />
                  A smartphone with an authenticator app like Google Authenticator, Authy, or Microsoft Authenticator
                </li>
                <li className="flex items-center">
                  <Check className="w-4 h-4 mr-1 text-emerald-500" />
                  A few minutes to complete the setup
                </li>
              </ul>
            </div>
          </div>
        </div>
        
        <div className="flex justify-between">
          <button
            type="button"
            onClick={handleSkip}
            className="px-6 py-2 text-gray-500 hover:text-gray-700 font-medium rounded-lg transition-colors"
          >
            Skip for now
          </button>
          <button
            type="button"
            onClick={handleNextStep}
            className="px-6 py-2 bg-emerald-600 text-white font-medium rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors flex items-center"
          >
            Get Started
            <ArrowRight className="ml-2 w-4 h-4" />
          </button>
        </div>
      </div>
    );
  };
  
  const renderStep2 = () => {
    return (
      <div>
        <h2 className="text-xl font-semibold text-gray-800 mb-4">
          Scan QR Code with Authenticator App
        </h2>
        <p className="text-gray-600 mb-6">
          Use an authenticator app like Google Authenticator, Microsoft Authenticator, or Authy to scan this QR code.
        </p>
        
        <div className="flex flex-col md:flex-row gap-6 mb-6">
          <div className="flex-1 flex flex-col items-center justify-center p-4 border border-gray-200 rounded-lg bg-gray-50">
            {isLoading ? (
              <div className="flex flex-col items-center justify-center p-10">
                <Loader2 className="w-10 h-10 text-emerald-600 animate-spin mb-4" />
                <p className="text-gray-600">Generating your secure key...</p>
              </div>
            ) : (
              <>
                <div className="bg-white p-3 rounded-lg shadow-sm mb-3">
                  <QRCodeSVG 
                    value={qrCodeUrl || 'https://example.com'} 
                    size={180} 
                    level="H"
                    includeMargin={true}
                  />
                </div>
                <p className="text-sm text-gray-500">Scan with your authenticator app</p>
              </>
            )}
          </div>
          
          <div className="flex-1">
            <div className="mb-4">
              <h3 className="text-sm font-medium text-gray-700 mb-1">Manual Setup</h3>
              <p className="text-sm text-gray-600 mb-2">
                If you can't scan the QR code, enter this key manually in your app:
              </p>
              <div className="flex items-center">
                <div className="bg-gray-100 px-3 py-2 rounded-lg text-gray-800 font-mono text-sm flex-grow">
                  {mfaSecret}
                </div>
                <button
                  type="button"
                  onClick={() => copyToClipboard(mfaSecret, 'secret')}
                  className="ml-2 p-2 text-gray-500 hover:text-emerald-600 hover:bg-emerald-50 rounded-full transition-colors"
                >
                  <Copy className="w-5 h-5" />
                </button>
              </div>
              {hasCopiedSecret && (
                <p className="text-xs text-emerald-600 mt-1 flex items-center">
                  <Check className="w-3 h-3 mr-1" /> Secret copied to clipboard
                </p>
              )}
            </div>
            
            <div className="bg-yellow-50 border border-yellow-100 rounded-lg p-4 text-sm">
              <h3 className="font-medium text-yellow-800 mb-1">Important:</h3>
              <ul className="text-yellow-700 space-y-1">
                <li className="flex items-start">
                  <Info className="w-4 h-4 mr-1 mt-0.5 text-yellow-500" />
                  Make sure to keep your authenticator app backed up or use a cloud-based authenticator.
                </li>
                <li className="flex items-start">
                  <Info className="w-4 h-4 mr-1 mt-0.5 text-yellow-500" />
                  Losing access to your authenticator app without backup codes will lock you out of your account.
                </li>
              </ul>
            </div>
          </div>
        </div>
        
        <div className="flex justify-between mt-6">
          <button
            type="button"
            onClick={handlePreviousStep}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors flex items-center"
          >
            <ArrowLeft className="mr-2 w-4 h-4" />
            Back
          </button>
          <button
            type="button"
            onClick={handleNextStep}
            className="px-6 py-2 bg-emerald-600 text-white font-medium rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors flex items-center"
            disabled={isLoading || !mfaSecret}
          >
            Continue
            <ArrowRight className="ml-2 w-4 h-4" />
          </button>
        </div>
      </div>
    );
  };
  
  const renderStep3 = () => {
    return (
      <div>
        <h2 className="text-xl font-semibold text-gray-800 mb-4">
          Verify Your Setup
        </h2>
        <p className="text-gray-600 mb-6">
          Enter the 6-digit verification code from your authenticator app to verify the setup.
        </p>
        
        <div className="mb-6">
          <label htmlFor="verificationCode" className="block text-sm font-medium text-gray-700 mb-1">
            Verification Code
          </label>
          <input
            type="text"
            id="verificationCode"
            inputMode="numeric"
            pattern="[0-9]*"
            maxLength={6}
            value={verificationCode}
            onChange={(e) => setVerificationCode(e.target.value.replace(/[^0-9]/g, ''))}
            placeholder="Enter 6-digit code"
            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors text-lg tracking-wider text-center"
          />
          <p className="text-sm text-gray-500 mt-2 flex items-center">
            <Info className="w-4 h-4 mr-1" />
            Open your authenticator app to get the code
          </p>
        </div>
        
        <div className="bg-emerald-50 border border-emerald-100 rounded-lg p-4 mb-6">
          <div className="flex">
            <Smartphone className="w-5 h-5 text-emerald-500 mr-2 flex-shrink-0" />
            <p className="text-sm text-emerald-700">
              Your authenticator app generates a new code every 30 seconds. The code is valid only for a short period of time.
            </p>
          </div>
        </div>
        
        <div className="flex justify-between mt-6">
          <button
            type="button"
            onClick={handlePreviousStep}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors flex items-center"
          >
            <ArrowLeft className="mr-2 w-4 h-4" />
            Back
          </button>
          <button
            type="button"
            onClick={verifyMfaSetup}
            disabled={verificationCode.length !== 6 || isLoading}
            className="px-6 py-2 bg-emerald-600 text-white font-medium rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Verifying...
              </>
            ) : (
              <>
                Verify
                <ArrowRight className="ml-2 w-4 h-4" />
              </>
            )}
          </button>
        </div>
      </div>
    );
  };
  
  const renderStep4 = () => {
    return (
      <div>
        <h2 className="text-xl font-semibold text-gray-800 mb-4">
          Save Your Backup Codes
        </h2>
        <p className="text-gray-600 mb-6">
          Use these backup codes if you lose access to your authenticator app. Each code can only be used once.
        </p>
        
        <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 mb-6">
          <div className="grid grid-cols-2 gap-2">
            {backupCodes.map((code, index) => (
              <div
                key={index}
                className="bg-white px-3 py-2 border border-gray-200 rounded text-gray-800 font-mono text-sm"
              >
                {code}
              </div>
            ))}
          </div>
        </div>
        
        <div className="flex flex-wrap gap-3 mb-6">
          <button
            type="button"
            onClick={() => copyToClipboard(backupCodes.join('\n'), 'codes')}
            className="flex items-center px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
          >
            <Copy className="w-4 h-4 mr-2" />
            Copy All Codes
          </button>
          <button
            type="button"
            onClick={downloadBackupCodes}
            className="flex items-center px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
          >
            <Download className="w-4 h-4 mr-2" />
            Download Codes
          </button>
          <button
            type="button"
            onClick={() => window.print()}
            className="flex items-center px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
          >
            <Printer className="w-4 h-4 mr-2" />
            Print Codes
          </button>
        </div>
        
        <div className="bg-red-50 border border-red-100 rounded-lg p-4 mb-6">
          <div className="flex">
            <Info className="w-5 h-5 text-red-500 mr-2 flex-shrink-0" />
            <div>
              <h3 className="font-medium text-red-800 mb-1">Important:</h3>
              <ul className="text-red-700 text-sm space-y-1">
                <li>
                  Store these codes in a secure location separate from your device.
                </li>
                <li>
                  Without your authenticator app or backup codes, you may lose access to your account.
                </li>
              </ul>
            </div>
          </div>
        </div>
        
        <div className="flex items-center mb-6">
          <input
            type="checkbox"
            id="confirmBackup"
            checked={hasSavedBackupCodes}
            onChange={(e) => setHasSavedBackupCodes(e.target.checked)}
            className="w-4 h-4 text-emerald-600 border-gray-300 rounded focus:ring-emerald-500"
          />
          <label htmlFor="confirmBackup" className="ml-2 text-gray-700">
            I have saved my backup codes in a secure location
          </label>
        </div>
        
        <div className="flex justify-between mt-6">
          <button
            type="button"
            onClick={handlePreviousStep}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors flex items-center"
          >
            <ArrowLeft className="mr-2 w-4 h-4" />
            Back
          </button>
          <button
            type="button"
            onClick={handleNextStep}
            disabled={!hasSavedBackupCodes}
            className="px-6 py-2 bg-emerald-600 text-white font-medium rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Continue
            <ArrowRight className="ml-2 w-4 h-4" />
          </button>
        </div>
      </div>
    );
  };
  
  const renderStep5 = () => {
    return (
      <div className="text-center">
        <div className="mx-auto w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mb-4">
          <Check className="w-8 h-8 text-emerald-600" />
        </div>
        <h2 className="text-2xl font-semibold text-gray-800 mb-3">
          Two-Factor Authentication Enabled
        </h2>
        <p className="text-gray-600 mb-6">
          Your account is now protected with two-factor authentication. You'll need to enter a verification code when signing in.
        </p>
        
        <div className="bg-emerald-50 border border-emerald-100 rounded-lg p-4 mb-6 text-left">
          <div className="flex">
            <Info className="w-5 h-5 text-emerald-500 mr-2 flex-shrink-0" />
            <div>
              <h3 className="font-medium text-emerald-800 mb-1">Security Tips:</h3>
              <ul className="text-emerald-700 text-sm space-y-1">
                <li className="flex items-center">
                  <Check className="w-4 h-4 mr-1 text-emerald-500" />
                  Keep your device and authenticator app updated
                </li>
                <li className="flex items-center">
                  <Check className="w-4 h-4 mr-1 text-emerald-500" />
                  Never share your verification codes with anyone
                </li>
                <li className="flex items-center">
                  <Check className="w-4 h-4 mr-1 text-emerald-500" />
                  Keep your backup codes in a secure location
                </li>
              </ul>
            </div>
          </div>
        </div>
        
        <button
          type="button"
          onClick={handleComplete}
          className="px-8 py-3 bg-emerald-600 text-white font-medium rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transition-colors"
        >
          Finish Setup
        </button>
      </div>
    );
  };
  
  const renderCurrentStep = () => {
    switch (currentStep) {
      case 1:
        return renderStep1();
      case 2:
        return renderStep2();
      case 3:
        return renderStep3();
      case 4:
        return renderStep4();
      case 5:
        return renderStep5();
      default:
        return null;
    }
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="bg-white rounded-xl shadow-lg p-6 w-full max-w-2xl mx-auto"
    >
      {renderStepIndicator()}
      {renderCurrentStep()}
    </motion.div>
  );
};

export default MFASetupWizard;