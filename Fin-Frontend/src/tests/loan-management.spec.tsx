import React from 'react';
import '@testing-library/jest-dom';
import { screen } from '@testing-library/react';
import LoanManagementPage from '../components/loans/LoanManagementPage';
import { renderWithProvider } from './test-utils';

jest.mock('../services/loansApi', () => ({
  useGetLoansQuery: () => ({
    data: [
      { id: '1', accountNumber: 'LN001', customerName: 'John Doe', productName: 'Personal Loan', principalAmount: 10000, outstandingPrincipal: 5000, outstandingInterest: 200, interestRate: 5, disbursementDate: '2025-01-01', maturityDate: '2025-12-31', status: 'Active', classification: 'Standard', daysPastDue: 0, provisionAmount: 0, createdAt: '2025-01-01' },
      { id: '2', accountNumber: 'LN002', customerName: 'Jane Smith', productName: 'Business Loan', principalAmount: 5000, outstandingPrincipal: 0, outstandingInterest: 0, interestRate: 6, disbursementDate: '2024-01-01', maturityDate: '2024-12-31', status: 'Closed', classification: 'Standard', daysPastDue: 0, provisionAmount: 0, createdAt: '2024-01-01' },
    ],
    isLoading: false,
    isError: false,
  }),
}));

describe('LoanManagementPage', () => {
  it('renders loan management UI', async () => {
    renderWithProvider(<LoanManagementPage />);
    expect(await screen.findByText(/John Doe/i)).toBeInTheDocument();
    expect(screen.getByText(/Personal Loan/i)).toBeInTheDocument();
    expect(screen.getByText(/Jane Smith/i)).toBeInTheDocument();
    expect(screen.getByText(/Business Loan/i)).toBeInTheDocument();
  });
});
