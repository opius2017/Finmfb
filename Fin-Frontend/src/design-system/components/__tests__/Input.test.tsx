import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Input } from '../Input';

describe('Input Component', () => {
  describe('Basic Rendering', () => {
    it('renders input field correctly', () => {
      render(<Input placeholder="Enter text" />);
      const input = screen.getByPlaceholderText('Enter text');
      expect(input).toBeInTheDocument();
    });

    it('renders with label', () => {
      render(<Input label="Username" />);
      expect(screen.getByText('Username')).toBeInTheDocument();
    });

    it('shows required indicator when required', () => {
      render(<Input label="Email" required />);
      expect(screen.getByText('*')).toBeInTheDocument();
    });
  });

  describe('Validation', () => {
    it('displays error message', () => {
      render(<Input error="This field is required" />);
      expect(screen.getByText('This field is required')).toBeInTheDocument();
    });

    it('applies error styling when error is present', () => {
      render(<Input error="Error message" />);
      const input = screen.getByRole('textbox');
      expect(input).toHaveClass('border-error-300');
      expect(input).toHaveAttribute('aria-invalid', 'true');
    });

    it('displays hint text when no error', () => {
      render(<Input hint="Enter your email address" />);
      expect(screen.getByText('Enter your email address')).toBeInTheDocument();
    });

    it('does not display hint when error is present', () => {
      render(<Input hint="Hint text" error="Error message" />);
      expect(screen.queryByText('Hint text')).not.toBeInTheDocument();
      expect(screen.getByText('Error message')).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('associates label with input using htmlFor', () => {
      render(<Input label="Email" id="email-input" />);
      const label = screen.getByText('Email');
      const input = screen.getByRole('textbox');
      expect(label).toHaveAttribute('for', 'email-input');
      expect(input).toHaveAttribute('id', 'email-input');
    });

    it('sets aria-describedby for error', () => {
      render(<Input error="Error message" id="test-input" />);
      const input = screen.getByRole('textbox');
      expect(input).toHaveAttribute('aria-describedby', 'test-input-error');
    });

    it('sets aria-describedby for hint', () => {
      render(<Input hint="Hint text" id="test-input" />);
      const input = screen.getByRole('textbox');
      expect(input).toHaveAttribute('aria-describedby', 'test-input-hint');
    });
  });

  describe('States', () => {
    it('handles disabled state', () => {
      render(<Input disabled />);
      const input = screen.getByRole('textbox');
      expect(input).toBeDisabled();
      expect(input).toHaveClass('disabled:bg-neutral-50');
    });

    it('handles value changes', () => {
      const handleChange = jest.fn();
      render(<Input onChange={(e) => handleChange(e.target.value)} />);
      const input = screen.getByRole('textbox');
      fireEvent.change(input, { target: { value: 'test value' } });
      expect(handleChange).toHaveBeenCalledWith('test value');
    });
  });

  describe('Icons', () => {
    it('renders icon on the left', () => {
      const icon = <span data-testid="test-icon">Icon</span>;
      render(<Input icon={icon} iconPosition="left" />);
      expect(screen.getByTestId('test-icon')).toBeInTheDocument();
    });

    it('renders icon on the right', () => {
      const icon = <span data-testid="test-icon">Icon</span>;
      render(<Input icon={icon} iconPosition="right" />);
      expect(screen.getByTestId('test-icon')).toBeInTheDocument();
    });

    it('applies correct padding for left icon', () => {
      const icon = <span>Icon</span>;
      render(<Input icon={icon} iconPosition="left" />);
      const input = screen.getByRole('textbox');
      expect(input).toHaveClass('pl-10');
    });

    it('applies correct padding for right icon', () => {
      const icon = <span>Icon</span>;
      render(<Input icon={icon} iconPosition="right" />);
      const input = screen.getByRole('textbox');
      expect(input).toHaveClass('pr-10');
    });
  });

  describe('Layout', () => {
    it('renders full width when specified', () => {
      render(<Input fullWidth />);
      const container = screen.getByRole('textbox').parentElement?.parentElement;
      expect(container).toHaveClass('w-full');
    });
  });
});
