// ...existing code...
import '@testing-library/jest-dom';
import { screen } from '@testing-library/react';
import CustomerDetailPage from '../components/customers/CustomerDetailPage';
import { renderWithProvider } from './test-utils';

jest.mock('../services/customersApi', () => ({
  useGetCustomerQuery: () => ({
    data: {
      id: '1',
      name: 'John Doe',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      customerType: 1,
      customerNumber: 'CUST-001',
      status: 1,
      createdAt: '2025-09-17T10:00:00Z',
      updatedAt: '2025-09-17T10:00:00Z',
    },
    isLoading: false,
    error: false,
  }),
  useGetRiskProfileQuery: () => ({
    data: {
      customerId: '1',
      riskLevel: 'Low',
      riskScore: 20,
      factors: ['Income', 'Employment'],
      evaluatedAt: '2025-09-17',
    },
    isLoading: false,
    error: false,
  }),
  useGetRelationshipMapQuery: () => ({
    data: {
      customerId: '1',
      relationships: [],
      nodes: [
        { id: 'n1', name: 'Entity One', type: 'TypeA' },
        { id: 'n2', name: 'Entity Two', type: 'TypeB' },
      ],
      edges: [
        { relationshipType: 'Parent', sourceId: 'n1', targetId: 'n2' },
        { relationshipType: 'Subsidiary', sourceId: 'n2', targetId: 'n1' },
      ],
    },
    isLoading: false,
    error: false,
  }),
  useInitiateOnboardingMutation: () => [jest.fn(), { isLoading: false, data: null }],
}));

describe('CustomerDetailPage', () => {
  it('renders customer details UI', async () => {
    renderWithProvider(<CustomerDetailPage />);
    const nameElements = await screen.findAllByText(/John Doe/i);
    expect(nameElements.length).toBeGreaterThan(0);
    expect(screen.getByText(/john@example.com/i)).toBeInTheDocument();
  });
});