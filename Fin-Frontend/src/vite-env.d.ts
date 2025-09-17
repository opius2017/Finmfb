/// <reference types="vite/client" />

declare global {
  interface Window {
    PaystackPop?: {
      setup: (options: any) => {
        openIframe: () => void;
      };
    };
    FlutterwaveCheckout?: (options: any) => void;
  }
}
