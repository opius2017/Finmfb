import React, { useEffect } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { checkAuthStatus } from '../../store/slices/authSlice';
import { RootState } from '../../store/store';
import LoginPage from '../auth/LoginPage';
import DashboardLayout from '../layout/DashboardLayout';
import DashboardHome from '../dashboard/DashboardHome';
import ExecutiveDashboard from '../dashboard/ExecutiveDashboard';
import LoanDashboard from '../dashboard/LoanDashboard';
import CustomersPage from '../customers/CustomersPage';
import MakerCheckerPage from '../makerchecker/MakerCheckerPage';
import FinancialReportsPage from '../reports/FinancialReportsPage';
import AccountsPayablePage from '../accounts/AccountsPayablePage';
import AccountsReceivablePage from '../accounts/AccountsReceivablePage';
import LoanManagementPage from '../loans/LoanManagementPage';
import InventoryManagementPage from '../inventory/InventoryManagementPage';
import PayrollManagementPage from '../payroll/PayrollManagementPage';
import CustomerDetailPage from '../customers/CustomerDetailPage';
import LoansPage from '../loans/LoansPage';
import LoanApplication from '../loans/LoanApplication';
import InventoryPage from '../inventory/InventoryPage';
import PayrollPage from '../payroll/PayrollPage';
import { useGetUserPermissionsQuery } from '../../services/roleApi';
import DepositDashboard from '../dashboard/DepositDashboard';

const AppRouter: React.FC = () => {
  const dispatch = useDispatch();
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated);
  const { data: userPermissions } = useGetUserPermissionsQuery(undefined, {
    skip: !isAuthenticated
  });

  useEffect(() => {
    dispatch(checkAuthStatus());
  }, [dispatch]);

  const getDefaultRoute = () => {
    if (!userPermissions?.data) return '/dashboard';
    
    const { defaultModule } = userPermissions.data;
    
    if (defaultModule === 'executive') return '/dashboard/executive';
    if (defaultModule === 'loans') return '/dashboard/loans';
    if (defaultModule === 'deposits') return '/dashboard/deposits';
    if (defaultModule === 'inventory') return '/dashboard/inventory';
    if (defaultModule === 'payroll') return '/dashboard/payroll';
    
    return '/dashboard';
  };

  return (
    <Routes>
      <Route
        path="/login"
        element={
          isAuthenticated ? <Navigate to={getDefaultRoute()} replace /> : <LoginPage />
        }
      />
      
      <Route
        path="/dashboard"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <DashboardHome />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/dashboard/executive"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <ExecutiveDashboard />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/dashboard/loans"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <LoanDashboard />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/dashboard/deposits"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <DepositDashboard />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/customers"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <CustomersPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/customers/:id"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <CustomerDetailPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/loans/management"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <LoanManagementPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />

      <Route
        path="/loans/apply/:customerId"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <LoanApplication />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/maker-checker"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <MakerCheckerPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/financial-reports"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <FinancialReportsPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/accounts-payable"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <AccountsPayablePage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/accounts-receivable"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <AccountsReceivablePage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/inventory/management"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <InventoryManagementPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/loans"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <LoansPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/inventory"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <InventoryPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/payroll/management"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <PayrollManagementPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/payroll"
        element={
          isAuthenticated ? (
            <DashboardLayout>
              <PayrollPage />
            </DashboardLayout>
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      <Route
        path="/"
        element={<Navigate to={isAuthenticated ? getDefaultRoute() : "/login"} replace />}
      />
      
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

export default AppRouter;