import React, { useState } from 'react';
import { Shield, Smartphone, Mail, Key, Copy, Check } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { TwoFactorSetup as TwoFactorSetupType } from '../types/2fa.types';
import { twoFactorService } from '../services/twoFactorService';

type SetupStep = 'select' | 'configure' | 'verify' | 'backup-codes';

export const TwoFactorSetup: React.FC = () => {
  const [step, setStep] = useState<SetupStep>('select');
  const [selectedMethod, setSelectedMethod] = useState<string>('');
  const [setup, setSetup] = useState<TwoFactorSetupType | null>(null);
  const [verificationCode, setVerificationCode] = useState('');
  const [backupCodes, setBackupCodes] = useState<string[]>([]);
  const [copied, setCopied] = useState(false);

  const methods = [
    { id: 'authenticator', name: 'Authenticator App', icon: Smartphone, description: 'Use Google Authenticator or similar' },
    { id: 'sms', name: 'SMS', icon: Smartphone, description: 'Receive codes via text message' },
    { id: 'email', name: 'Email', icon: Mail, description: 'Receive codes via email' },
  ];

  const handleMethodSelect = async (method: string) => {
    setSelectedMethod(method);
    try {
      let setupData: TwoFactorSetupType;
      if (method === 'authenticator') {
        setupData = await twoFactorService.setupAuthenticator();
      } else if (method === 'sms') {
        setupData = await twoFactorService.setupSMS('+1234567890');
      } else {
        setupData = await twoFactorService.setupEmail('user@example.com');
      }
      setSetup(setupData);
      setStep('configure');
    } catch (error) {
      console.error('Setup failed:', error);
    }
  };

  const handleVerify = async () => {
    try {
      const result = await twoFactorService.verifySetup(selectedMethod, verificationCode);
      if (result.success && result.backupCodes) {
        setBackupCodes(result.backupCodes);
        setStep('backup-codes');
      }
    } catch (error) {
      console.error('Verification failed:', error);
    }
  };

  const handleCopyBackupCodes = () => {
    navigator.clipboard.writeText(backupCodes.join('\n'));
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  if (step === 'select') {
    return (
      <Card className="p-6">
        <h2 className="text-xl font-bold mb-6">Enable Two-Factor Authentication</h2>
        <p className="text-neutral-600 mb-6">
          Add an extra layer of security to your account by requiring a verification code in addition to your password.
        </p>

        <div className="space-y-3">
          {methods.map((method) => (
            <button
              key={method.id}
              onClick={() => handleMethodSelect(method.id)}
              className="w-full p-4 border-2 border-neutral-200 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors text-left"
            >
              <div className="flex items-center space-x-4">
                <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center">
                  <method.icon className="w-6 h-6 text-primary-600" />
                </div>
                <div>
                  <div className="font-semibold">{method.name}</div>
                  <div className="text-sm text-neutral-600">{method.description}</div>
                </div>
              </div>
            </button>
          ))}
        </div>
      </Card>
    );
  }

  if (step === 'configure' && setup) {
    return (
      <Card className="p-6">
        <h2 className="text-xl font-bold mb-6">Configure {selectedMethod === 'authenticator' ? 'Authenticator App' : selectedMethod.toUpperCase()}</h2>

        {selectedMethod === 'authenticator' && setup.qrCode && (
          <div className="mb-6">
            <p className="text-neutral-600 mb-4">Scan this QR code with your authenticator app:</p>
            <div className="flex justify-center mb-4">
              <img src={setup.qrCode} alt="QR Code" className="w-48 h-48" />
            </div>
            <p className="text-sm text-neutral-600 text-center mb-2">Or enter this code manually:</p>
            <div className="bg-neutral-100 p-3 rounded-lg text-center font-mono">
              {setup.secret}
            </div>
          </div>
        )}

        {selectedMethod !== 'authenticator' && (
          <div className="mb-6">
            <p className="text-neutral-600 mb-4">
              We've sent a verification code to your {selectedMethod}. Enter it below:
            </p>
          </div>
        )}

        <div className="mb-6">
          <label className="block text-sm font-medium mb-2">Verification Code</label>
          <Input
            value={verificationCode}
            onChange={(e) => setVerificationCode(e.target.value)}
            placeholder="Enter 6-digit code"
            maxLength={6}
          />
        </div>

        <div className="flex space-x-3">
          <Button variant="outline" onClick={() => setStep('select')}>
            Back
          </Button>
          <Button
            variant="primary"
            onClick={handleVerify}
            disabled={verificationCode.length !== 6}
            fullWidth
          >
            Verify & Enable
          </Button>
        </div>
      </Card>
    );
  }

  if (step === 'backup-codes') {
    return (
      <Card className="p-6">
        <div className="flex items-center space-x-3 mb-6">
          <div className="w-12 h-12 bg-success-100 rounded-lg flex items-center justify-center">
            <Check className="w-6 h-6 text-success-600" />
          </div>
          <div>
            <h2 className="text-xl font-bold">2FA Enabled Successfully!</h2>
            <p className="text-neutral-600">Save your backup codes</p>
          </div>
        </div>

        <div className="bg-warning-50 border border-warning-200 rounded-lg p-4 mb-6">
          <p className="text-sm text-warning-800">
            <strong>Important:</strong> Save these backup codes in a secure place. You can use them to access your account if you lose your device.
          </p>
        </div>

        <div className="bg-neutral-100 p-4 rounded-lg mb-4">
          <div className="grid grid-cols-2 gap-2 font-mono text-sm">
            {backupCodes.map((code, index) => (
              <div key={index} className="p-2 bg-white rounded">
                {twoFactorService.formatBackupCode(code)}
              </div>
            ))}
          </div>
        </div>

        <div className="flex space-x-3">
          <Button variant="outline" onClick={handleCopyBackupCodes} fullWidth>
            {copied ? <Check className="w-4 h-4 mr-2" /> : <Copy className="w-4 h-4 mr-2" />}
            {copied ? 'Copied!' : 'Copy Codes'}
          </Button>
          <Button variant="primary" onClick={() => window.location.reload()} fullWidth>
            Done
          </Button>
        </div>
      </Card>
    );
  }

  return null;
};
