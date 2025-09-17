
jest.mock('../services/customersApi', () => ({
  useGetCustomerQuery: () => ({
    data: { id: 1, name: 'John Doe', email: 'john@example.com' },
    isLoading: false,
    isError: false,
  }),
}));

describe('CustomerDetailPage', () => {
  it('renders customer details UI', () => {
    renderWithProvider(<CustomerDetailPage />);
    expect(screen.getByText(/Customer/i)).toBeInTheDocument();
  });
});
