// ...existing code...
import '@testing-library/jest-dom';
import { screen } from '@testing-library/react';

import LoanManagementPage from '../components/loans/LoanManagementPage';
import { renderWithProvider } from './test-utils';

jest.mock('axios');
import axios from 'axios';

const mockLoans = [
  { id: '1', accountNumber: 'LN001', customerName: 'John Doe', productName: 'Personal Loan', principalAmount: 10000, outstandingPrincipal: 5000, outstandingInterest: 200, interestRate: 5, disbursementDate: '2025-01-01', maturityDate: '2025-12-31', status: 'Active', classification: 'Standard', daysPastDue: 0, provisionAmount: 0, createdAt: '2025-01-01' },
  { id: '2', accountNumber: 'LN002', customerName: 'Jane Smith', productName: 'Business Loan', principalAmount: 5000, outstandingPrincipal: 0, outstandingInterest: 0, interestRate: 6, disbursementDate: '2024-01-01', maturityDate: '2024-12-31', status: 'Closed', classification: 'Standard', daysPastDue: 0, provisionAmount: 0, createdAt: '2024-01-01' },
];

beforeEach(() => {
  (axios.get as jest.Mock).mockImplementation((url: string) => {
    if (url === '/api/loans') {
      return Promise.resolve({ data: mockLoans });
    }
    if (url.startsWith('/api/loans/') && url.endsWith('/provisioning')) {
      return Promise.resolve({ data: { expectedCreditLoss: 0 } });
    }
    return Promise.reject(new Error('Not Found'));
  });
});

describe('LoanManagementPage', () => {
  it('renders loan management UI', async () => {
    renderWithProvider(<LoanManagementPage />);
    expect(await screen.findByText(/John Doe/i)).toBeInTheDocument();
    expect(await screen.findByText(/Personal Loan/i)).toBeInTheDocument();
    expect(await screen.findByText(/Jane Smith/i)).toBeInTheDocument();
    expect(await screen.findByText(/Business Loan/i)).toBeInTheDocument();
  });
});
