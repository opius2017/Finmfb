import axios from 'axios';
import { api } from './api';

const paymentApi = api.injectEndpoints({
  endpoints: (builder) => ({
    initiatePayment: builder.mutation({
      query: (data) => ({
        url: '/payments/initiate',
        method: 'POST',
        body: data,
      }),
    }),
    
    verifyPayment: builder.query({
      query: (reference) => `/payments/verify/${reference}`,
    }),
    
    initiateRefund: builder.mutation({
      query: (data) => ({
        url: '/payments/refund',
        method: 'POST',
        body: data,
      }),
    }),
    
    setupRecurringBilling: builder.mutation({
      query: (data) => ({
        url: '/payments/recurring/setup',
        method: 'POST',
        body: data,
      }),
    }),
    
    cancelRecurringBilling: builder.mutation({
      query: (recurringId) => ({
        url: `/payments/recurring/cancel/${recurringId}`,
        method: 'POST',
      }),
    }),
  }),
});

export const {
  useInitiatePaymentMutation,
  useVerifyPaymentQuery,
  useLazyVerifyPaymentQuery,
  useInitiateRefundMutation,
  useSetupRecurringBillingMutation,
  useCancelRecurringBillingMutation,
} = paymentApi;

// Utility functions for client-side payment integration
export const initializePaystackPopup = (publicKey, email, amount, reference, callback, onClose) => {
  // Use this function for client-side payment integration with Paystack
  // This requires the Paystack JS SDK to be loaded in the page
  if (window.PaystackPop) {
    const handler = window.PaystackPop.setup({
      key: publicKey,
      email,
      amount: amount * 100, // Convert to kobo
      ref: reference,
      callback,
      onClose,
    });
    
    handler.openIframe();
    return true;
  }
  
  console.error("Paystack SDK not loaded");
  return false;
};

export const initializeFlutterwaveCheckout = (publicKey, email, amount, reference, name, phone, callback, onClose) => {
  // Use this function for client-side payment integration with Flutterwave
  // This requires the Flutterwave JS SDK to be loaded in the page
  if (window.FlutterwaveCheckout) {
    window.FlutterwaveCheckout({
      public_key: publicKey,
      tx_ref: reference,
      amount,
      currency: "NGN",
      customer: {
        email,
        name,
        phone_number: phone,
      },
      callback,
      onclose: onClose,
    });
    return true;
  }
  
  console.error("Flutterwave SDK not loaded");
  return false;
};

// Function to load payment SDK scripts dynamically
export const loadPaymentSdk = (provider) => {
  return new Promise((resolve, reject) => {
    const script = document.createElement('script');
    
    script.onload = () => resolve(true);
    script.onerror = () => reject(new Error(`Failed to load ${provider} SDK`));
    
    if (provider.toLowerCase() === 'paystack') {
      script.src = 'https://js.paystack.co/v1/inline.js';
    } else if (provider.toLowerCase() === 'flutterwave') {
      script.src = 'https://checkout.flutterwave.com/v3.js';
    } else {
      reject(new Error(`Unknown payment provider: ${provider}`));
      return;
    }
    
    document.body.appendChild(script);
  });
};

export default {
  useInitiatePaymentMutation,
  useVerifyPaymentQuery,
  useLazyVerifyPaymentQuery,
  useInitiateRefundMutation,
  useSetupRecurringBillingMutation,
  useCancelRecurringBillingMutation,
  initializePaystackPopup,
  initializeFlutterwaveCheckout,
  loadPaymentSdk,
};