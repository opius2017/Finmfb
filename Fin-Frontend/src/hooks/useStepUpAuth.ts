import { useState, useCallback } from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../store/store';
import useMfaChallenge from './useMfaChallenge';
import { toast } from 'react-hot-toast';

interface UseStepUpAuthOptions {
  operation: string;
  onSuccess?: () => void;
  onCancel?: () => void;
}

/**
 * Custom hook for implementing step-up authentication for sensitive operations
 * Step-up authentication requires additional verification for high-risk operations
 * even if the user is already authenticated
 */
export function useStepUpAuth(options: UseStepUpAuthOptions) {
  const { operation, onSuccess, onCancel } = options;
  
  const [isVerifying, setIsVerifying] = useState(false);
  const { user, lastMfaVerification } = useSelector((state: RootState) => state.auth);
  
  // Use the MFA challenge hook
  const { 
    showMfaChallenge, 
    requestChallenge, 
    verifyChallenge, 
    cancelChallenge 
  } = useMfaChallenge({
    operation,
    onSuccess: () => {
      setIsVerifying(false);
      if (onSuccess) {
        onSuccess();
      }
    },
    onCancel: () => {
      setIsVerifying(false);
      if (onCancel) {
        onCancel();
      }
    }
  });
  
  /**
   * Check if the operation requires step-up authentication
   * and initiate the verification process if needed
   */
  const verifyIdentity = useCallback(async (): Promise<boolean> => {
    // If user is not authenticated, operation cannot proceed
    if (!user) {
      toast.error('You must be logged in to perform this action');
      return false;
    }
    
    // Check if user has recent MFA verification (within the last 15 minutes)
    const hasRecentVerification = checkRecentVerification();
    if (hasRecentVerification) {
      // User has recently verified, allow operation without additional verification
      if (onSuccess) {
        onSuccess();
      }
      return true;
    }
    
    // User needs to verify identity
    setIsVerifying(true);
    return await requestChallenge(operation);
  }, [user, operation, requestChallenge, onSuccess]);
  
  /**
   * Check if the user has recently verified their identity
   * Returns true if verified within the last 15 minutes
   */
  const checkRecentVerification = useCallback((): boolean => {
    if (!lastMfaVerification) {
      return false;
    }
    
    const lastVerification = new Date(lastMfaVerification);
    const now = new Date();
    
    // Calculate the difference in minutes
    const diffInMinutes = (now.getTime() - lastVerification.getTime()) / (1000 * 60);
    
    // Return true if last verification was within the last 15 minutes
    return diffInMinutes <= 15;
  }, [lastMfaVerification]);
  
  /**
   * Execute a function that requires step-up authentication
   * @param fn Function to execute after successful verification
   */
  const withStepUpAuth = useCallback(
    async <T extends (...args: any[]) => any>(
      fn: T,
      ...args: Parameters<T>
    ): Promise<ReturnType<T> | null> => {
      const verified = await verifyIdentity();
      
      if (verified && !showMfaChallenge) {
        // If verified without showing MFA challenge, execute function immediately
        return await fn(...args);
      } else if (verified) {
        // If verification is in progress, the function will be called after successful verification
        // via the onSuccess callback
        return null;
      } else {
        // Verification failed or was cancelled
        return null;
      }
    },
    [verifyIdentity, showMfaChallenge]
  );
  
  return {
    isVerifying,
    showMfaChallenge,
    verifyIdentity,
    verifyChallenge,
    cancelVerification: cancelChallenge,
    withStepUpAuth
  };
}

export default useStepUpAuth;