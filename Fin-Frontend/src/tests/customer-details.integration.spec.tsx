import '@testing-library/jest-dom';
import React from 'react';

import { screen, fireEvent } from '@testing-library/react';
import CustomerDetailPage from '../components/customers/CustomerDetailPage';
import { renderWithProvider } from './test-utils';

describe('CustomerDetailPage Integration', () => {
  it('renders and interacts with customer details', () => {
    renderWithProvider(<CustomerDetailPage />);
    expect(screen.getByText(/Customer/i)).toBeInTheDocument();
    // Example interaction: simulate edit button click if present
    const editButton = screen.queryByText(/Edit Customer/i);
    if (editButton) {
      fireEvent.click(editButton);
      // Add assertions for edit modal/dialog if implemented
    }
  });
});
