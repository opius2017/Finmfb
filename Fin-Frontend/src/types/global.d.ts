// Global type declarations for external payment libraries
declare global {
  interface Window {
    PaystackPop?: {
      setup: (options: {
        key: string;
        email: string;
        amount: number;
        currency?: string;
        ref?: string;
        callback: (response: any) => void;
        onClose: () => void;
      }) => {
        openIframe: () => void;
      };
    };
    
    FlutterwaveCheckout?: (options: {
      public_key: string;
      tx_ref: string;
      amount: number;
      currency: string;
      payment_options: string;
      customer: {
        email: string;
        phone_number?: string;
        name?: string;
      };
      callback: (response: any) => void;
      onclose: () => void;
      customizations?: {
        title?: string;
        description?: string;
        logo?: string;
      };
    }) => void;
  }
}

export {};