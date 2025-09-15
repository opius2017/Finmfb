import React, { useState } from 'react';
import { motion } from 'framer-motion';
import QRCode from 'qrcode.react';
import { Loader2 } from 'lucide-react';
import toast from 'react-hot-toast';

interface MFASetupProps {
  secret?: string;
  onVerify: (token: string) => Promise<boolean>;
  onCancel: () => void;
}

const MFASetup: React.FC<MFASetupProps> = ({ secret, onVerify, onCancel }) => {
  const [verificationCode, setVerificationCode] = useState('');
  const [isVerifying, setIsVerifying] = useState(false);

  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsVerifying(true);

    try {
      const isValid = await onVerify(verificationCode);
      if (isValid) {
        toast.success('MFA setup completed successfully!');
      } else {
        toast.error('Invalid verification code. Please try again.');
      }
    } catch (error) {
      toast.error('Failed to verify MFA setup. Please try again.');
    } finally {
      setIsVerifying(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="bg-white rounded-lg shadow-lg p-6 max-w-md mx-auto"
    >
      <h2 className="text-2xl font-semibold text-gray-800 mb-4">
        Set Up Two-Factor Authentication
      </h2>

      <div className="space-y-6">
        <div className="text-sm text-gray-600">
          <p className="mb-2">1. Install an authenticator app like Google Authenticator or Authy.</p>
          <p>2. Scan the QR code below or enter the secret key manually.</p>
        </div>

        {secret && (
          <div className="flex flex-col items-center space-y-4 py-4">
            <QRCode
              value={`otpauth://totp/SoarFin:${encodeURIComponent(
                'user@email.com'
              )}?secret=${secret}&issuer=SoarFin`}
              size={200}
              level="H"
              includeMargin={true}
            />
            <div className="text-sm text-gray-500 font-mono bg-gray-50 p-2 rounded">
              {secret}
            </div>
          </div>
        )}

        <form onSubmit={handleVerify} className="space-y-4">
          <div>
            <label htmlFor="code" className="block text-sm font-medium text-gray-700 mb-1">
              Verification Code
            </label>
            <input
              id="code"
              type="text"
              inputMode="numeric"
              pattern="[0-9]*"
              maxLength={6}
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, ''))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-emerald-500 focus:border-emerald-500"
              placeholder="Enter 6-digit code"
              required
            />
          </div>

          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-900"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isVerifying || verificationCode.length !== 6}
              className="px-4 py-2 bg-emerald-600 text-white rounded-md hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
            >
              {isVerifying ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin mr-2" />
                  Verifying...
                </>
              ) : (
                'Verify'
              )}
            </button>
          </div>
        </form>

        <div className="mt-6 text-sm text-gray-500">
          <p className="font-medium mb-2">Backup Codes</p>
          <p>
            Save these backup codes in a secure location. You can use them to access your account if you
            lose your authentication device.
          </p>
          <div className="grid grid-cols-2 gap-2 mt-2 font-mono text-xs">
            {[...Array(8)].map((_, i) => (
              <div key={i} className="bg-gray-50 p-1 rounded text-center">
                {Math.random().toString(36).substring(2, 10).toUpperCase()}
              </div>
            ))}
          </div>
        </div>
      </div>
    </motion.div>
  );
};

export default MFASetup;