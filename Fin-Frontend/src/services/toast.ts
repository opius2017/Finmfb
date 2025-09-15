type ToastStatus = 'success' | 'error' | 'warning' | 'info';

interface ToastOptions {
  title: string;
  description: string;
  status: ToastStatus;
  duration?: number;
  isClosable?: boolean;
}

// Basic toast notification implementation
export const toast = (options: ToastOptions) => {
  const { title, description, status, duration = 5000, isClosable = true } = options;

  // Create toast container if it doesn't exist
  let toastContainer = document.getElementById('toast-container');
  if (!toastContainer) {
    toastContainer = document.createElement('div');
    toastContainer.id = 'toast-container';
    toastContainer.className = 'fixed top-4 right-4 z-50 space-y-4';
    document.body.appendChild(toastContainer);
  }

  // Create toast element
  const toast = document.createElement('div');
  toast.className = `
    p-4 rounded-md shadow-lg transform transition-all duration-300 ease-in-out
    ${status === 'success' ? 'bg-green-50 text-green-800 border border-green-200' :
      status === 'error' ? 'bg-red-50 text-red-800 border border-red-200' :
      status === 'warning' ? 'bg-yellow-50 text-yellow-800 border border-yellow-200' :
      'bg-blue-50 text-blue-800 border border-blue-200'}
  `;

  // Add content
  toast.innerHTML = `
    <div class="flex items-start">
      <div class="ml-3 w-0 flex-1 pt-0.5">
        <p class="text-sm font-medium">${title}</p>
        <p class="mt-1 text-sm opacity-80">${description}</p>
      </div>
      ${isClosable ? `
        <div class="ml-4 flex-shrink-0 flex">
          <button class="inline-flex text-gray-400 hover:text-gray-500">
            <span class="sr-only">Close</span>
            <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
            </svg>
          </button>
        </div>
      ` : ''}
    </div>
  `;

  // Add close button functionality
  if (isClosable) {
    const closeButton = toast.querySelector('button');
    closeButton?.addEventListener('click', () => {
      toast.remove();
    });
  }

  // Add to container
  toastContainer.appendChild(toast);

  // Auto remove after duration
  setTimeout(() => {
    if (toast.parentElement) {
      toast.classList.add('opacity-0', 'translate-x-full');
      setTimeout(() => toast.remove(), 300);
    }
  }, duration);
};