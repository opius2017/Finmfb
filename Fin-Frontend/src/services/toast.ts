import { toast as reactToast } from 'react-toastify';

// Create wrapper for react-toastify
export const toast = {
  success: (message: string) => reactToast.success(message),
  error: (message: string) => reactToast.error(message),
  info: (message: string) => reactToast.info(message),
  warning: (message: string) => reactToast.warning(message),
};

export default toast;