import { Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'sonner';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import LoanCalculator from './pages/LoanCalculator';
import LoanApplications from './pages/LoanApplications';
import NewLoanApplication from './pages/NewLoanApplication';
import EligibilityCheck from './pages/EligibilityCheck';
import GuarantorDashboard from './pages/GuarantorDashboard';
import CommitteeDashboard from './pages/CommitteeDashboard';
import DeductionSchedules from './pages/DeductionSchedules';
import Reconciliation from './pages/Reconciliation';
import CommodityVouchers from './pages/CommodityVouchers';
import Reports from './pages/Reports';
import Login from './pages/Login';
import { useAuthStore } from './store/authStore';

function App() {
  const { isAuthenticated } = useAuthStore();

  if (!isAuthenticated) {
    return (
      <>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
        <Toaster position="top-right" />
      </>
    );
  }

  return (
    <>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/calculator" element={<LoanCalculator />} />
          <Route path="/eligibility" element={<EligibilityCheck />} />
          <Route path="/applications" element={<LoanApplications />} />
          <Route path="/applications/new" element={<NewLoanApplication />} />
          <Route path="/guarantor" element={<GuarantorDashboard />} />
          <Route path="/committee" element={<CommitteeDashboard />} />
          <Route path="/schedules" element={<DeductionSchedules />} />
          <Route path="/reconciliation" element={<Reconciliation />} />
          <Route path="/vouchers" element={<CommodityVouchers />} />
          <Route path="/reports" element={<Reports />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Layout>
      <Toaster position="top-right" />
    </>
  );
}

export default App;
