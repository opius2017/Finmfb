// @ts-nocheck
import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { motion } from 'framer-motion';
import {
  Building2,
  Home,
  Users,
  Wallet,
  CreditCard,
  FileText,
  Package,
  UserCheck,
  BarChart3,
  Settings,
  ChevronLeft,
  ChevronRight,
  Receipt,
  ShoppingCart,
  Briefcase,
  Shield,
  TrendingUp,
  DollarSign,
  Calculator,
} from 'lucide-react';
import { RootState } from '../../store/store';
import PermissionGuard from '../common/PermissionGuard';
import { toggleSidebar } from '../../store/slices/themeSlice';
import { useGetUserPermissionsQuery } from '../../services/roleApi';

interface NavItem {
  name: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  badge?: number;
  permission?: string;
  children?: NavItem[];
}

const Sidebar: React.FC = () => {
  const dispatch = useDispatch();
  const location = useLocation();
  const { sidebarCollapsed, organizationType, companyName } = useSelector(
    (state: RootState) => state.theme
  );
  const { data: userPermissions } = useGetUserPermissionsQuery();

  const mfbNavItems: NavItem[] = [
    { 
      name: 'Dashboard', 
      href: '/dashboard', 
      icon: Home,
      children: [
        { name: 'Overview', href: '/dashboard', icon: Home, permission: 'dashboard.read' },
        { name: 'Executive', href: '/dashboard/executive', icon: TrendingUp, permission: 'executive.dashboard.read' },
        { name: 'Loans', href: '/dashboard/loans', icon: CreditCard, permission: 'loans.dashboard.read' },
        { name: 'Deposits', href: '/dashboard/deposits', icon: Wallet, permission: 'deposits.dashboard.read' },
      ]
    },
    { name: 'Customers', href: '/customers', icon: Users, permission: 'customers.read' },
    { name: 'Deposits', href: '/deposits', icon: Wallet, permission: 'deposits.read' },
    { name: 'Loan Management', href: '/loans/management', icon: CreditCard, permission: 'loans.read' },
    { name: 'Maker-Checker', href: '/maker-checker', icon: Shield, permission: 'maker_checker.read' },
    { name: 'Loans', href: '/loans', icon: CreditCard, permission: 'loans.read' },
    { name: 'Accounts Payable', href: '/accounts-payable', icon: Receipt, permission: 'accounts_payable.read' },
    { name: 'General Ledger', href: '/general-ledger', icon: FileText, permission: 'general_ledger.read' },
    { name: 'Multi-Currency', href: '/multi-currency', icon: DollarSign, permission: 'multi_currency.read' },
    { name: 'Payroll', href: '/payroll', icon: Briefcase, permission: 'payroll.read' },
    { name: 'Reports', href: '/reports', icon: BarChart3, permission: 'reports.read' },
    { name: 'Compliance', href: '/compliance', icon: UserCheck, permission: 'compliance.read' },
    { name: 'Financial Reports', href: '/financial-reports', icon: FileText, permission: 'financial_reports.read' },
    { name: 'Security', href: '/security', icon: Shield, permission: 'security.read' },
    { name: 'Settings', href: '/settings', icon: Settings, permission: 'settings.read' },
  ];

  const smeNavItems: NavItem[] = [
    { 
      name: 'Dashboard', 
      href: '/dashboard', 
      icon: Home,
      children: [
        { name: 'Overview', href: '/dashboard', icon: Home, permission: 'dashboard.read' },
        { name: 'Executive', href: '/dashboard/executive', icon: TrendingUp, permission: 'executive.dashboard.read' },
        { name: 'Inventory', href: '/dashboard/inventory', icon: Package, permission: 'inventory.dashboard.read' },
        { name: 'Payroll', href: '/dashboard/payroll', icon: Briefcase, permission: 'payroll.dashboard.read' },
      ]
    },
    { name: 'Customers', href: '/customers', icon: Users, permission: 'customers.read' },
    { name: 'Accounts Receivable', href: '/accounts-receivable', icon: Receipt, permission: 'accounts_receivable.read' },
    { name: 'Accounts Payable', href: '/accounts-payable', icon: ShoppingCart, permission: 'accounts_payable.read' },
    { name: 'Invoicing', href: '/invoicing', icon: Receipt, permission: 'invoicing.read' },
    { name: 'Inventory Management', href: '/inventory/management', icon: Package, permission: 'inventory.read' },
    { name: 'Purchases', href: '/purchases', icon: ShoppingCart, permission: 'purchases.read' },
    { name: 'Payroll Management', href: '/payroll/management', icon: Briefcase, permission: 'payroll.read' },
    { name: 'General Ledger', href: '/general-ledger', icon: FileText, permission: 'general_ledger.read' },
    { name: 'Multi-Currency', href: '/multi-currency', icon: DollarSign, permission: 'multi_currency.read' },
    { name: 'Payroll', href: '/payroll', icon: Briefcase, permission: 'payroll.read' },
    { name: 'Tax Management', href: '/tax-management', icon: Calculator, permission: 'tax.read' },
    { name: 'Reports', href: '/reports', icon: BarChart3, permission: 'reports.read' },
    { name: 'Settings', href: '/settings', icon: Settings, permission: 'settings.read' },
  ];

  const navItems = organizationType === 'MFB' ? mfbNavItems : smeNavItems;
  
  const hasPermission = (permission?: string) => {
    if (!permission) return true;
    return userPermissions?.data?.permissions?.some(p => p.permissionName === permission) || false;
  };

  const filterNavItems = (items: NavItem[]): NavItem[] => {
    return items.filter(item => hasPermission(item.permission)).map(item => ({
      ...item,
      children: item.children ? filterNavItems(item.children) : undefined
    }));
  };

  const filteredNavItems = filterNavItems(navItems);
  const [expandedItems, setExpandedItems] = React.useState<string[]>(['Dashboard']);

  const toggleExpanded = (itemName: string) => {
    setExpandedItems(prev => 
      prev.includes(itemName) 
        ? prev.filter(name => name !== itemName)
        : [...prev, itemName]
    );
  };

  return (
    <motion.div
      initial={false}
      animate={{ width: sidebarCollapsed ? 64 : 256 }}
      transition={{ duration: 0.3, ease: 'easeInOut' }}
      className="bg-white border-r border-gray-200 flex flex-col fixed left-0 top-0 h-full z-40 shadow-lg"
    >
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-gray-200">
        {!sidebarCollapsed && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="flex items-center space-x-3"
          >
            <div className="w-8 h-8 bg-emerald-600 rounded-lg flex items-center justify-center">
              <div className="relative">
                <Building2 className="w-5 h-5 text-white" />
                <div className="absolute -top-0.5 -right-0.5 w-2 h-2 bg-yellow-400 rounded-full"></div>
              </div>
            </div>
            <div>
              <h1 className="text-sm font-semibold text-gray-900 truncate">
                Soar-Fin+
              </h1>
              <p className="text-xs text-gray-500">{organizationType}</p>
            </div>
          </motion.div>
        )}
        
        <button
          onClick={() => dispatch(toggleSidebar())}
          className="p-1.5 rounded-lg hover:bg-gray-100 transition-colors"
        >
          {sidebarCollapsed ? (
            <ChevronRight className="w-4 h-4 text-gray-600" />
          ) : (
            <ChevronLeft className="w-4 h-4 text-gray-600" />
          )}
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4 space-y-2">
        {filteredNavItems.map((item) => (
          <div key={item.name}>
            {item.children ? (
              <div>
                <button
                  onClick={() => !sidebarCollapsed && toggleExpanded(item.name)}
                  className="flex items-center w-full px-3 py-2 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-100 transition-colors"
                >
                  <item.icon className="w-5 h-5 flex-shrink-0" />
                  {!sidebarCollapsed && (
                    <>
                      <motion.span
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="ml-3 truncate"
                      >
                        {item.name}
                      </motion.span>
                      <ChevronRight 
                        className={`w-4 h-4 ml-auto transition-transform ${
                          expandedItems.includes(item.name) ? 'rotate-90' : ''
                        }`} 
                      />
                    </>
                  )}
                </button>
                {!sidebarCollapsed && expandedItems.includes(item.name) && (
                  <motion.div
                    initial={{ opacity: 0, height: 0 }}
                    animate={{ opacity: 1, height: 'auto' }}
                    exit={{ opacity: 0, height: 0 }}
                    className="ml-6 mt-1 space-y-1"
                  >
                    {item.children.map((child) => (
                      <NavLink
                        key={child.name}
                        to={child.href}
                        className={({ isActive }) =>
                          `flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                            isActive
                              ? 'bg-emerald-50 text-emerald-700 border-l-2 border-emerald-600'
                              : 'text-gray-600 hover:bg-gray-50'
                          }`
                        }
                      >
                        <child.icon className="w-4 h-4 flex-shrink-0" />
                        <span className="ml-3 truncate">{child.name}</span>
                      </NavLink>
                    ))}
                  </motion.div>
                )}
              </div>
            ) : (
              <NavLink
                to={item.href}
                className={({ isActive }) =>
                  `flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors group ${
                    isActive
                      ? 'bg-emerald-50 text-emerald-700 border-r-2 border-emerald-600'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`
                }
              >
                <PermissionGuard permission={item.permission}>
                <item.icon className="w-5 h-5 flex-shrink-0" />
                {!sidebarCollapsed && (
                  <motion.span
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    className="ml-3 truncate"
                  >
                    {item.name}
                  </motion.span>
                )}
                {!sidebarCollapsed && item.badge && (
                  <motion.span
                    initial={{ opacity: 0, scale: 0 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="ml-auto bg-emerald-100 text-emerald-800 text-xs px-2 py-0.5 rounded-full"
                  >
                    {item.badge}
                  </motion.span>
                )}
                </PermissionGuard>
              </NavLink>
            )}
          </div>
        ))}
      </nav>

      {/* Footer */}
      {!sidebarCollapsed && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="p-4 border-t border-gray-200"
        >
          <div className="text-xs text-gray-500 text-center">
            <p>Version 1.0.0</p>
            <p className="mt-1">© 2025 FinTech</p>
          </div>
        </motion.div>
      )}
    </motion.div>
  );
};

export default Sidebar;