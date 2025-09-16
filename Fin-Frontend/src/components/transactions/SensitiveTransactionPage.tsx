import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Shield, AlertTriangle, Loader2 } from 'lucide-react';
import { toast } from 'react-hot-toast';
import useStepUpAuth from '../../hooks/useStepUpAuth';
import MFAChallenge from '../auth/MFAChallenge';

interface TransferFormData {
  amount: string;
  accountNumber: string;
  description: string;
}

const SensitiveTransactionPage: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState<TransferFormData>({
    amount: '',
    accountNumber: '',
    description: ''
  });
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  // Initialize step-up authentication
  const { 
    showMfaChallenge, 
    verifyIdentity, 
    verifyChallenge, 
    cancelVerification,
    withStepUpAuth
  } = useStepUpAuth({
    operation: 'transfer_funds',
    onSuccess: () => {
      // This will be called after successful MFA verification
      processTransaction(formData);
    },
    onCancel: () => {
      toast.error('Transaction cancelled');
      setIsSubmitting(false);
    }
  });
  
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value
    });
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    // For amounts over a threshold, require step-up authentication
    if (parseFloat(formData.amount) > 50000) {
      // Initiate step-up authentication
      await withStepUpAuth(processTransaction, formData);
    } else {
      // For smaller amounts, proceed without additional verification
      await processTransaction(formData);
    }
  };
  
  // Function to process the transaction after verification (if needed)
  const processTransaction = async (data: TransferFormData) => {
    try {
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1500));
      
      toast.success('Transaction processed successfully');
      setIsSubmitting(false);
      
      // Navigate to confirmation page
      navigate('/transactions/confirmation', { 
        state: { 
          transactionId: 'TRX' + Math.floor(Math.random() * 1000000),
          ...data
        } 
      });
    } catch (error) {
      console.error('Transaction error:', error);
      toast.error('Transaction failed. Please try again.');
      setIsSubmitting(false);
    }
  };
  
  return (
    <div className="container mx-auto px-4 py-8 max-w-3xl">
      <div className="bg-white rounded-xl shadow-lg p-6 mb-6">
        <div className="flex items-center mb-6">
          <div className="bg-emerald-100 p-3 rounded-full mr-4">
            <Shield className="w-6 h-6 text-emerald-600" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold text-gray-800">Funds Transfer</h1>
            <p className="text-gray-600">Transfer funds to another account</p>
          </div>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label htmlFor="amount" className="block text-sm font-medium text-gray-700 mb-2">
              Amount (₦)
            </label>
            <input
              id="amount"
              name="amount"
              type="number"
              min="1"
              step="0.01"
              value={formData.amount}
              onChange={handleChange}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              placeholder="Enter amount"
              required
            />
          </div>
          
          <div>
            <label htmlFor="accountNumber" className="block text-sm font-medium text-gray-700 mb-2">
              Recipient Account Number
            </label>
            <input
              id="accountNumber"
              name="accountNumber"
              type="text"
              pattern="[0-9]*"
              maxLength={10}
              value={formData.accountNumber}
              onChange={handleChange}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              placeholder="10-digit account number"
              required
            />
          </div>
          
          <div>
            <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-2">
              Description
            </label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows={3}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              placeholder="Purpose of transfer"
              required
            />
          </div>
          
          {parseFloat(formData.amount) > 50000 && (
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <div className="flex items-start">
                <AlertTriangle className="w-5 h-5 text-amber-600 mr-3 mt-0.5 flex-shrink-0" />
                <div className="text-sm text-amber-800">
                  <p className="font-medium mb-1">Additional verification required</p>
                  <p>Transfers over ₦50,000 require additional security verification for your protection.</p>
                </div>
              </div>
            </div>
          )}
          
          <div className="flex justify-end space-x-4">
            <button
              type="button"
              onClick={() => navigate(-1)}
              className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
            >
              Cancel
            </button>
            
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-6 py-3 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-5 h-5 animate-spin mr-2" />
                  Processing...
                </>
              ) : (
                'Transfer Funds'
              )}
            </button>
          </div>
        </form>
      </div>
      
      {/* MFA Challenge Modal */}
      {showMfaChallenge && (
        <MFAChallenge
          title="Verify Your Identity"
          message="For your security, please confirm your identity to complete this high-value transaction."
          onVerify={verifyChallenge}
          onCancel={cancelVerification}
        />
      )}
    </div>
  );
};

export default SensitiveTransactionPage;