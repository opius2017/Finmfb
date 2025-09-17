import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  ArrowLeft,
  User,
  Mail,
  Phone,
  MapPin,
  Building,
  CreditCard,
  FileText,
  Activity,
} from 'lucide-react';
import {
  useGetCustomerQuery,
  useGetRiskProfileQuery,
  useGetRelationshipMapQuery,
  useInitiateOnboardingMutation,
} from '../../services/customersApi';
import { format } from 'date-fns';

const CustomerDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data: customer, isLoading, error } = useGetCustomerQuery(id!);
  const { data: riskProfile, isLoading: riskLoading } = useGetRiskProfileQuery(id!);
  const { data: relationshipMap, isLoading: relLoading } = useGetRelationshipMapQuery(id!);
  const [initiateOnboarding, { isLoading: onboardingLoading, data: onboardingWorkflow }] = useInitiateOnboardingMutation();

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="h-8 bg-gray-200 rounded w-1/4"></div>
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 space-y-6">
            <div className="h-64 bg-gray-200 rounded-xl"></div>
            <div className="h-64 bg-gray-200 rounded-xl"></div>
          </div>
          <div className="space-y-6">
            <div className="h-48 bg-gray-200 rounded-xl"></div>
            <div className="h-32 bg-gray-200 rounded-xl"></div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !customer) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-red-600">Failed to load customer details</p>
      </div>
    );
  }

  // customer is already defined above
  const isIndividual = customer.customerType === 1; // Individual = 1, Corporate = 2

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1: // Active
        return 'bg-green-100 text-green-800';
      case 2: // Inactive
        return 'bg-gray-100 text-gray-800';
      case 3: // Suspended
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status: number) => {
    switch (status) {
      case 1:
        return 'Active';
      case 2:
        return 'Inactive';
      case 3:
        return 'Suspended';
      case 4:
        return 'Closed';
      default:
        return 'Unknown';
    }
  };

  const customerName = isIndividual 
    ? `${customer.firstName || ''} ${customer.middleName ? customer.middleName + ' ' : ''}${customer.lastName || ''}`.trim()
    : customer.companyName || 'Unknown Company';

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Link
            to="/customers"
            className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
          >
            <ArrowLeft className="w-5 h-5 text-gray-600" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{customerName}</h1>
            <p className="text-gray-600">Customer #{customer.customerNumber}</p>
          </div>
        </div>
        
        <div className="flex items-center space-x-3">
          <span className={`inline-flex px-3 py-1 text-sm font-semibold rounded-full ${
            getStatusColor(customer.status)
          }`}>
            {getStatusText(customer.status)}
          </span>
          <button className="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            Edit Customer
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Basic Information */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
              <User className="w-5 h-5 text-emerald-600 mr-2" />
              Basic Information
            </h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {isIndividual ? (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-1">
                      Full Name
                    </label>
                    <p className="text-gray-900">{customerName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-1">
                      Date of Birth
                    </label>
                    <p className="text-gray-900">
                      {customer.dateOfBirth 
                        ? format(new Date(customer.dateOfBirth), 'MMM dd, yyyy')
                        : 'Not provided'
                      }
                    </p>
                  </div>
                </>
              ) : (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-1">
                      Company Name
                    </label>
                    <p className="text-gray-900">{customer.companyName}</p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-1">
                      RC Number
                    </label>
                    <p className="text-gray-900">{customer.rcNumber || 'Not provided'}</p>
                  </div>
                </>
              )}
              
              <div>
                <label className="block text-sm font-medium text-gray-500 mb-1">
                  Customer Type
                </label>
                <p className="text-gray-900">{isIndividual ? 'Individual' : 'Corporate'}</p>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-500 mb-1">
                  Date Joined
                </label>
                <p className="text-gray-900">
                  {format(new Date(customer.createdAt), 'MMM dd, yyyy')}
                </p>
              </div>
            </div>
          </motion.div>

          {/* Contact Information */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
              <Mail className="w-5 h-5 text-emerald-600 mr-2" />
              Contact Information
            </h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="flex items-center">
                <Mail className="w-4 h-4 text-gray-400 mr-3" />
                <div>
                  <label className="block text-sm font-medium text-gray-500 mb-1">
                    Email Address
                  </label>
                  <p className="text-gray-900">{customer.email}</p>
                </div>
              </div>
              
              <div className="flex items-center">
                <Phone className="w-4 h-4 text-gray-400 mr-3" />
                <div>
                  <label className="block text-sm font-medium text-gray-500 mb-1">
                    Phone Number
                  </label>
                  <p className="text-gray-900">{customer.phoneNumber}</p>
                </div>
              </div>
              
              {customer.address && (
                <div className="md:col-span-2 flex items-start">
                  <MapPin className="w-4 h-4 text-gray-400 mr-3 mt-1" />
                  <div>
                    <label className="block text-sm font-medium text-gray-500 mb-1">
                      Address
                    </label>
                    <p className="text-gray-900">
                      {customer.address}
                      {customer.city && `, ${customer.city}`}
                      {customer.state && `, ${customer.state}`}
                    </p>
                  </div>
                </div>
              )}
            </div>
          </motion.div>

          {/* KYC Information */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
              <FileText className="w-5 h-5 text-emerald-600 mr-2" />
              KYC Information
            </h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {customer.bvn && (
                <div>
                  <label className="block text-sm font-medium text-gray-500 mb-1">
                    BVN
                  </label>
                  <p className="text-gray-900 font-mono">{customer.bvn}</p>
                </div>
              )}
              
              {customer.nin && (
                <div>
                  <label className="block text-sm font-medium text-gray-500 mb-1">
                    NIN
                  </label>
                  <p className="text-gray-900 font-mono">{customer.nin}</p>
                </div>
              )}
            </div>
          </motion.div>

          {/* Risk Profile */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
              <Activity className="w-5 h-5 text-emerald-600 mr-2" />
              Risk Profile
            </h3>
            {riskLoading ? (
              <div className="text-gray-500">Loading risk profile...</div>
            ) : riskProfile ? (
              <div>
                <div className="flex items-center space-x-4 mb-2">
                  <span className="font-semibold">Level:</span>
                  <span className="px-2 py-1 rounded bg-gray-100 text-gray-800">{riskProfile.riskLevel}</span>
                  <span className="font-semibold">Score:</span>
                  <span className="px-2 py-1 rounded bg-gray-100 text-gray-800">{riskProfile.riskScore}</span>
                </div>
                <div>
                  <span className="font-semibold">Factors:</span>
                  <ul className="list-disc ml-6 text-gray-700">
                    {riskProfile.factors.map((f, i) => <li key={i}>{f}</li>)}
                  </ul>
                </div>
                <div className="text-xs text-gray-400 mt-2">Evaluated: {riskProfile.evaluatedAt && new Date(riskProfile.evaluatedAt).toLocaleString()}</div>
              </div>
            ) : (
              <div className="text-gray-500">No risk profile available.</div>
            )}
          </motion.div>

          {/* Relationship Map */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.35 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
              <Building className="w-5 h-5 text-emerald-600 mr-2" />
              Relationship Map
            </h3>
            {relLoading ? (
              <div className="text-gray-500">Loading relationship map...</div>
            ) : relationshipMap ? (
              <div>
                <div className="mb-2 font-semibold">Entities:</div>
                <ul className="list-disc ml-6 text-gray-700">
                  {relationshipMap.nodes.map((node) => (
                    <li key={node.id}>{node.name} <span className="text-xs text-gray-500">({node.type})</span></li>
                  ))}
                </ul>
                <div className="mt-2 font-semibold">Relationships:</div>
                <ul className="list-disc ml-6 text-gray-700">
                  {relationshipMap.edges.map((edge, i) => (
                    <li key={i}>{edge.relationshipType}: {edge.sourceId} → {edge.targetId}</li>
                  ))}
                </ul>
              </div>
            ) : (
              <div className="text-gray-500">No relationship map available.</div>
            )}
          </motion.div>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Quick Actions */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h3>
            <div className="space-y-3">
              <button className="w-full flex items-center px-4 py-3 bg-emerald-50 text-emerald-700 rounded-lg hover:bg-emerald-100 transition-colors" onClick={() => initiateOnboarding({ customerId: id! })} disabled={onboardingLoading}>
                <Activity className="w-4 h-4 mr-3" />
                {onboardingLoading ? 'Initiating...' : 'Initiate Onboarding'}
              </button>
              <button className="w-full flex items-center px-4 py-3 bg-emerald-50 text-emerald-700 rounded-lg hover:bg-emerald-100 transition-colors">
                <CreditCard className="w-4 h-4 mr-3" />
                Open Account
              </button>
              <button className="w-full flex items-center px-4 py-3 border border-gray-200 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors">
                <Activity className="w-4 h-4 mr-3" />
                View Transactions
              </button>
              <button className="w-full flex items-center px-4 py-3 border border-gray-200 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors">
                <FileText className="w-4 h-4 mr-3" />
                Generate Report
              </button>
            </div>
            {onboardingWorkflow && (
              <div className="mt-4 text-sm text-gray-700">
                <div className="font-semibold">Onboarding Workflow:</div>
                <div>Status: {onboardingWorkflow.status}</div>
                <div>Started: {new Date(onboardingWorkflow.startedAt).toLocaleString()}</div>
                <div>Steps:</div>
                <ul className="list-disc ml-6">
                  {onboardingWorkflow.steps.map((step, i) => <li key={i}>{step}</li>)}
                </ul>
              </div>
            )}
          </motion.div>

          {/* Account Summary */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.1 }}
            className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
          >
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Account Summary</h3>
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <span className="text-gray-600">Total Accounts</span>
                <span className="font-semibold">0</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600">Total Balance</span>
                <span className="font-semibold">₦0.00</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600">Active Loans</span>
                <span className="font-semibold">0</span>
              </div>
            </div>
          </motion.div>
        </div>
      </div>
    </div>
  );
};

export default CustomerDetailPage;