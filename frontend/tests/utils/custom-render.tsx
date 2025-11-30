import { ReactElement } from 'react';
import { render, RenderOptions as RTLRenderOptions, RenderResult } from '@testing-library/react';
import { BrowserRouter, MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from 'react-query';

/**
 * Custom render options for testing
 */
export interface RenderOptions extends Omit<RTLRenderOptions, 'wrapper'> {
  /**
   * Initial route for the router (e.g., '/dashboard')
   */
  initialRoute?: string;
  
  /**
   * Initial authentication state
   */
  authState?: {
    user: {
      id: string;
      email: string;
      name: string;
      role: string;
    } | null;
    token: string | null;
    isAuthenticated: boolean;
  };
  
  /**
   * Use BrowserRouter instead of MemoryRouter
   */
  useBrowserRouter?: boolean;
}

/**
 * Custom render function that wraps components with necessary providers
 * 
 * @param ui - React component to render
 * @param options - Render options including initial route and auth state
 * @returns Render result with additional utilities
 * 
 * @example
 * ```tsx
 * const { getByText } = renderWithProviders(<Dashboard />, {
 *   initialRoute: '/dashboard',
 *   authState: {
 *     user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'MEMBER' },
 *     token: 'test-token',
 *     isAuthenticated: true,
 *   },
 * });
 * ```
 */
export function renderWithProviders(
  ui: ReactElement,
  options: RenderOptions = {}
): RenderResult {
  const {
    initialRoute = '/',
    authState,
    useBrowserRouter = false,
    ...renderOptions
  } = options;

  // Create a new QueryClient for each test to ensure isolation
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        cacheTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });

  // Set up authentication state if provided
  if (authState) {
    if (authState.token) {
      localStorage.setItem('authToken', authState.token);
    }
    if (authState.user) {
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: authState.user,
          token: authState.token,
          isAuthenticated: authState.isAuthenticated,
        },
        version: 0,
      }));
    }
  }

  // Choose router based on options
  const Router = useBrowserRouter ? BrowserRouter : MemoryRouter;
  const routerProps = useBrowserRouter ? {} : { initialEntries: [initialRoute] };

  function Wrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <Router {...routerProps}>
          {children}
        </Router>
      </QueryClientProvider>
    );
  }

  return render(ui, { wrapper: Wrapper, ...renderOptions });
}

/**
 * Render with authenticated user
 */
export function renderWithAuth(
  ui: ReactElement,
  options: Omit<RenderOptions, 'authState'> = {}
): RenderResult {
  return renderWithProviders(ui, {
    ...options,
    authState: {
      user: {
        id: 'test-user-1',
        email: 'test@example.com',
        name: 'Test User',
        role: 'MEMBER',
      },
      token: 'test-auth-token',
      isAuthenticated: true,
    },
  });
}

/**
 * Render without authentication (logged out state)
 */
export function renderWithoutAuth(
  ui: ReactElement,
  options: Omit<RenderOptions, 'authState'> = {}
): RenderResult {
  return renderWithProviders(ui, {
    ...options,
    authState: {
      user: null,
      token: null,
      isAuthenticated: false,
    },
  });
}

/**
 * Render with committee member role
 */
export function renderWithCommitteeRole(
  ui: ReactElement,
  options: Omit<RenderOptions, 'authState'> = {}
): RenderResult {
  return renderWithProviders(ui, {
    ...options,
    authState: {
      user: {
        id: 'committee-user-1',
        email: 'committee@example.com',
        name: 'Committee Member',
        role: 'COMMITTEE',
      },
      token: 'committee-auth-token',
      isAuthenticated: true,
    },
  });
}

/**
 * Render with admin role
 */
export function renderWithAdminRole(
  ui: ReactElement,
  options: Omit<RenderOptions, 'authState'> = {}
): RenderResult {
  return renderWithProviders(ui, {
    ...options,
    authState: {
      user: {
        id: 'admin-user-1',
        email: 'admin@example.com',
        name: 'Admin User',
        role: 'ADMIN',
      },
      token: 'admin-auth-token',
      isAuthenticated: true,
    },
  });
}

/**
 * Clean up after tests
 */
export function cleanupAuth(): void {
  localStorage.clear();
  sessionStorage.clear();
}

// Re-export everything from @testing-library/react
export * from '@testing-library/react';
