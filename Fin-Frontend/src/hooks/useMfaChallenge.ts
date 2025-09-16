import { useState, useCallback, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { RootState } from '../store/store';
import { 
  useCreateMfaChallengeMutation, 
  useVerifyMfaChallengeMutation 
} from '../services/authApi';
import { newMfaChallenge, mfaChallengeVerified } from '../store/slices/authSlice';
import { toast } from 'react-hot-toast';

interface UseMfaChallengeProps {
  operation?: string;
  onSuccess?: () => void;
  onCancel?: () => void;
  autoPrompt?: boolean;
}

export function useMfaChallenge(props?: UseMfaChallengeProps) {
  const {
    operation = 'verify_identity',
    onSuccess,
    onCancel,
    autoPrompt = false
  } = props || {};

  const [showMfaChallenge, setShowMfaChallenge] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [verificationCode, setVerificationCode] = useState('');
  
  const dispatch = useDispatch();
  const { user, mfaChallenge } = useSelector((state: RootState) => state.auth);
  
  const [createMfaChallenge] = useCreateMfaChallengeMutation();
  const [verifyMfaChallenge] = useVerifyMfaChallengeMutation();
  
  // Request a new MFA challenge
  const requestChallenge = useCallback(async (
    operationName?: string,
    successCallback?: () => void
  ) => {
    if (!user) {
      toast.error('You must be logged in to perform this action');
      return false;
    }
    
    // Use provided parameters or fall back to props
    const currentOperation = operationName || operation;
    const currentCallback = successCallback || onSuccess;
    
    try {
      setIsLoading(true);
      const response = await createMfaChallenge({ operation: currentOperation }).unwrap();
      
      if (response.success && response.data) {
        dispatch(newMfaChallenge(response.data));
        setShowMfaChallenge(true);
        
        // Store callback if provided
        if (currentCallback && typeof currentCallback === 'function') {
          // Store in component state if needed
          // This example assumes the callback is captured in the closure
        }
        
        return true;
      } else {
        toast.error(response.message || 'Failed to create MFA challenge');
        return false;
      }
    } catch (error) {
      console.error('Error requesting MFA challenge:', error);
      toast.error('An error occurred while creating MFA challenge');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [user, operation, createMfaChallenge, dispatch, onSuccess]);
  
  // Verify the MFA challenge with the provided code
  const verifyChallenge = useCallback(async (code?: string): Promise<boolean> => {
    if (!mfaChallenge?.challengeId) {
      toast.error('No active MFA challenge');
      return false;
    }
    
    const codeToVerify = code || verificationCode;
    if (!codeToVerify) {
      toast.error('Please enter a verification code');
      return false;
    }
    
    try {
      setIsLoading(true);
      const response = await verifyMfaChallenge({
        challengeId: mfaChallenge.challengeId,
        code: codeToVerify
      }).unwrap();
      
      if (response.success && response.data) {
        // Update store to record successful verification
        dispatch(mfaChallengeVerified());
        
        // Reset state
        setShowMfaChallenge(false);
        setVerificationCode('');
        
        // Execute success callback if provided
        if (onSuccess) {
          onSuccess();
        }
        
        return true;
      } else {
        toast.error(response.message || 'Invalid verification code');
        return false;
      }
    } catch (error) {
      console.error('Error verifying MFA challenge:', error);
      toast.error('An error occurred while verifying the code');
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [mfaChallenge, verificationCode, verifyMfaChallenge, dispatch, onSuccess]);
  
  // Cancel the MFA challenge
  const cancelChallenge = useCallback(() => {
    setShowMfaChallenge(false);
    setVerificationCode('');
    
    if (onCancel) {
      onCancel();
    }
  }, [onCancel]);
  
  // Auto-prompt for MFA verification when component mounts
  useEffect(() => {
    if (autoPrompt) {
      requestChallenge();
    }
  }, [autoPrompt, requestChallenge]);
  
  // Check if the challenge has expired
  useEffect(() => {
    if (mfaChallenge && showMfaChallenge) {
      const expiryTime = new Date(mfaChallenge.expiresAt).getTime();
      const currentTime = new Date().getTime();
      
      if (currentTime > expiryTime) {
        toast.error('MFA challenge has expired. Please try again.');
        setShowMfaChallenge(false);
      }
      
      // Set up expiration timer
      const timeoutId = setTimeout(() => {
        setShowMfaChallenge(false);
        toast.error('MFA challenge has expired. Please try again.');
      }, expiryTime - currentTime);
      
      return () => clearTimeout(timeoutId);
    }
  }, [mfaChallenge, showMfaChallenge]);
  
  return {
    showMfaChallenge,
    isLoading,
    verificationCode,
    setVerificationCode,
    mfaChallenge,
    requestChallenge,
    verifyChallenge,
    cancelChallenge
  };
}

export default useMfaChallenge;