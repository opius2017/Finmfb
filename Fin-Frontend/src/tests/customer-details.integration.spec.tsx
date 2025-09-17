// ...existing code...
import '@testing-library/jest-dom';
import { screen, fireEvent } from '@testing-library/react';
import CustomerDetailPage from '../components/customers/CustomerDetailPage';
import { renderWithProvider } from './test-utils';

jest.mock('../services/customersApi', () => ({
  useGetCustomerQuery: () => ({
    data: { id: 1, name: 'John Doe', email: 'john@example.com' },
    isLoading: false,
    isError: false,
  }),
}));

describe('CustomerDetailPage Integration', () => {
  it('renders and interacts with customer details', async () => {
    renderWithProvider(<CustomerDetailPage />);
    expect(await screen.findByText(/John Doe/i)).toBeInTheDocument();
    expect(screen.getByText(/john@example.com/i)).toBeInTheDocument();
    // Example interaction: simulate edit button click if present
    const editButton = screen.queryByText(/Edit Customer/i);
    if (editButton) {
      fireEvent.click(editButton);
      // Add assertions for edit modal/dialog if implemented
    }
  });
});
