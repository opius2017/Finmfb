/// <reference types="vite/client" />

declare global {
  interface Window {
    PaystackPop?: {
      setup: (config: any) => {
        openIframe: () => void;
      };
    };
    FlutterwaveCheckout?: (config: any) => void;
  }
}

export {};