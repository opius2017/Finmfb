import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { ThemeProvider, useTheme } from '../ThemeContext';

// Test component that uses the theme hook
const TestComponent = () => {
  const { theme, actualTheme, setTheme, toggleTheme } = useTheme();
  
  return (
    <div>
      <div data-testid="theme">{theme}</div>
      <div data-testid="actual-theme">{actualTheme}</div>
      <button onClick={() => setTheme('light')}>Set Light</button>
      <button onClick={() => setTheme('dark')}>Set Dark</button>
      <button onClick={() => setTheme('system')}>Set System</button>
      <button onClick={toggleTheme}>Toggle</button>
    </div>
  );
};

describe('ThemeContext', () => {
  beforeEach(() => {
    localStorage.clear();
    // Reset document classes
    document.documentElement.classList.remove('light', 'dark');
  });

  describe('Theme Provider', () => {
    it('provides theme context to children', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      expect(screen.getByTestId('theme')).toBeInTheDocument();
      expect(screen.getByTestId('actual-theme')).toBeInTheDocument();
    });

    it('defaults to system theme', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      expect(screen.getByTestId('theme')).toHaveTextContent('system');
    });

    it('loads theme from localStorage', () => {
      localStorage.setItem('fintech-theme', 'dark');
      
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      expect(screen.getByTestId('theme')).toHaveTextContent('dark');
    });
  });

  describe('Theme Switching', () => {
    it('switches to light theme', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      fireEvent.click(screen.getByText('Set Light'));
      
      expect(screen.getByTestId('theme')).toHaveTextContent('light');
      expect(screen.getByTestId('actual-theme')).toHaveTextContent('light');
    });

    it('switches to dark theme', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      fireEvent.click(screen.getByText('Set Dark'));
      
      expect(screen.getByTestId('theme')).toHaveTextContent('dark');
      expect(screen.getByTestId('actual-theme')).toHaveTextContent('dark');
    });

    it('persists theme to localStorage', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      fireEvent.click(screen.getByText('Set Dark'));
      
      expect(localStorage.getItem('fintech-theme')).toBe('dark');
    });

    it('toggles between light and dark', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      // Start with light
      fireEvent.click(screen.getByText('Set Light'));
      expect(screen.getByTestId('actual-theme')).toHaveTextContent('light');
      
      // Toggle to dark
      fireEvent.click(screen.getByText('Toggle'));
      expect(screen.getByTestId('actual-theme')).toHaveTextContent('dark');
      
      // Toggle back to light
      fireEvent.click(screen.getByText('Toggle'));
      expect(screen.getByTestId('actual-theme')).toHaveTextContent('light');
    });
  });

  describe('Document Class Application', () => {
    it('applies light class to document', async () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      fireEvent.click(screen.getByText('Set Light'));
      
      await waitFor(() => {
        expect(document.documentElement.classList.contains('light')).toBe(true);
        expect(document.documentElement.classList.contains('dark')).toBe(false);
      });
    });

    it('applies dark class to document', async () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );
      
      fireEvent.click(screen.getByText('Set Dark'));
      
      await waitFor(() => {
        expect(document.documentElement.classList.contains('dark')).toBe(true);
        expect(document.documentElement.classList.contains('light')).toBe(false);
      });
    });
  });

  describe('Error Handling', () => {
    it('throws error when useTheme is used outside provider', () => {
      // Suppress console.error for this test
      const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
      
      expect(() => {
        render(<TestComponent />);
      }).toThrow('useTheme must be used within a ThemeProvider');
      
      consoleSpy.mockRestore();
    });
  });
});
