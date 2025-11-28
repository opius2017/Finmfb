import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface CommitteeApproval {
  id: number;
  approvalRefNumber: string;
  loanApplicationId: number;
  memberName: string;
  requestedAmount: number;
  riskRating: string;
  status: string;
  submittedToCommitteeDate: string;
  creditOfficerRecommendation: string;
}

interface CommitteeApprovalDetail extends CommitteeApproval {
  referralReason: string;
  committeeSummary: string;
  loanRepaymentHistoryScore: number | null;
  previousSuccessfulLoans: number;
  previousDefaultCount: number;
  hasDefaultHistory: boolean;
  guarantor1Name: string;
  guarantor1MemberSavings: number | null;
  guarantor2Name: string;
  guarantor2MemberSavings: number | null;
  allGuarantorsApproved: boolean;
  committeeMeetingDate: string | null;
  committeeReviewers: string;
  committeeDecisionDate: string | null;
  committeeDecision: string;
  committeeNotes: string;
  approvalConditions: string;
}

/**
 * Loan Committee Approval Dashboard Component
 * Manages high-value and high-risk loan approvals with governance workflow
 */
export const LoanCommitteeApprovalDashboard: React.FC = () => {
  const [pendingApprovals, setPendingApprovals] = useState<CommitteeApproval[]>([]);
  const [selectedApproval, setSelectedApproval] = useState<CommitteeApprovalDetail | null>(null);
  const [filterRiskRating, setFilterRiskRating] = useState<string>('');
  const [filterStatus, setFilterStatus] = useState<string>('');
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [showApprovalForm, setShowApprovalForm] = useState<boolean>(false);
  const [approvalDecision, setApprovalDecision] = useState<string>('Approved');
  const [approvalNotes, setApprovalNotes] = useState<string>('');
  const [approvalConditions, setApprovalConditions] = useState<string>('');
  const [submittingApproval, setSubmittingApproval] = useState<boolean>(false);

  useEffect(() => {
    fetchPendingApprovals();
  }, [pageNumber, pageSize, filterRiskRating, filterStatus]);

  const fetchPendingApprovals = async () => {
    setLoading(true);
    setError('');
    try {
      const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
      });
      if (filterRiskRating) params.append('riskRating', filterRiskRating);
      if (filterStatus) params.append('status', filterStatus);

      const response = await axios.get(`/api/v1/loan-committee/pending-approvals?${params}`);
      setPendingApprovals(response.data.items || []);
      setTotalCount(response.data.totalCount || 0);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to fetch pending approvals');
    } finally {
      setLoading(false);
    }
  };

  const fetchApprovalDetail = async (refNumber: string) => {
    setLoading(true);
    setError('');
    try {
      const response = await axios.get(`/api/v1/loan-committee/approval/${refNumber}`);
      setSelectedApproval(response.data);
      setShowApprovalForm(false);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to fetch approval detail');
    } finally {
      setLoading(false);
    }
  };

  const submitApproval = async () => {
    if (!selectedApproval) return;

    setSubmittingApproval(true);
    setError('');
    try {
      await axios.post('/api/v1/loan-committee/approve-application', {
        loanApplicationId: selectedApproval.loanApplicationId,
        committeeDecision: approvalDecision,
        committeeNotes: approvalNotes,
        approvalConditions: approvalConditions,
        approvedByOfficerId: 'current-user-id', // Replace with actual user ID
        committeeMeetingDate: new Date().toISOString(),
        committeeReviewers: 'committee-member-ids' // Replace with actual member IDs
      });

      alert(`Loan application ${approvalDecision} successfully`);
      setShowApprovalForm(false);
      fetchPendingApprovals();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to submit approval');
    } finally {
      setSubmittingApproval(false);
    }
  };

  const getRiskRatingColor = (rating: string): string => {
    switch (rating) {
      case 'Low': return 'bg-green-100 text-green-800';
      case 'Medium': return 'bg-yellow-100 text-yellow-800';
      case 'High': return 'bg-orange-100 text-orange-800';
      case 'Critical': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'Approved': return 'bg-green-100 text-green-800';
      case 'Rejected': return 'bg-red-100 text-red-800';
      case 'Pending': return 'bg-blue-100 text-blue-800';
      case 'InReview': return 'bg-purple-100 text-purple-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg p-6 max-w-6xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-800 mb-6">Loan Committee Approval Dashboard</h2>

      {/* Error Message */}
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      {!selectedApproval ? (
        <>
          {/* Filters */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Risk Rating</label>
              <select
                value={filterRiskRating}
                onChange={(e) => {
                  setFilterRiskRating(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
              >
                <option value="">All Risk Ratings</option>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Critical">Critical</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
              <select
                value={filterStatus}
                onChange={(e) => {
                  setFilterStatus(e.target.value);
                  setPageNumber(1);
                }}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
              >
                <option value="">All Status</option>
                <option value="Pending">Pending</option>
                <option value="InReview">In Review</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
            </div>
            <div className="flex items-end">
              <button
                onClick={() => fetchPendingApprovals()}
                disabled={loading}
                className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                {loading ? 'Loading...' : 'Apply Filters'}
              </button>
            </div>
          </div>

          {/* Pending Approvals Table */}
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse border border-gray-300">
              <thead className="bg-gray-100">
                <tr>
                  <th className="border border-gray-300 px-4 py-2 text-left">Ref #</th>
                  <th className="border border-gray-300 px-4 py-2 text-left">Member Name</th>
                  <th className="border border-gray-300 px-4 py-2 text-right">Amount (₦)</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Risk Rating</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Status</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Officer Recommendation</th>
                  <th className="border border-gray-300 px-4 py-2 text-center">Action</th>
                </tr>
              </thead>
              <tbody>
                {pendingApprovals.map((approval) => (
                  <tr key={approval.id} className="hover:bg-gray-50">
                    <td className="border border-gray-300 px-4 py-2 font-medium">{approval.approvalRefNumber}</td>
                    <td className="border border-gray-300 px-4 py-2">{approval.memberName}</td>
                    <td className="border border-gray-300 px-4 py-2 text-right font-semibold">
                      ₦{approval.requestedAmount.toLocaleString()}
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getRiskRatingColor(approval.riskRating)}`}>
                        {approval.riskRating}
                      </span>
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getStatusColor(approval.status)}`}>
                        {approval.status}
                      </span>
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center text-sm">
                      {approval.creditOfficerRecommendation}
                    </td>
                    <td className="border border-gray-300 px-4 py-2 text-center">
                      <button
                        onClick={() => fetchApprovalDetail(approval.approvalRefNumber)}
                        className="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700 text-sm"
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <div className="mt-4 flex justify-between items-center">
            <div className="text-sm text-gray-600">
              Showing {((pageNumber - 1) * pageSize) + 1} to {Math.min(pageNumber * pageSize, totalCount)} of {totalCount} total
            </div>
            <div className="space-x-2">
              <button
                onClick={() => setPageNumber(Math.max(1, pageNumber - 1))}
                disabled={pageNumber === 1}
                className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Previous
              </button>
              <button
                onClick={() => setPageNumber(pageNumber + 1)}
                disabled={pageNumber * pageSize >= totalCount}
                className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </div>
        </>
      ) : (
        <>
          {/* Approval Detail View */}
          <button
            onClick={() => setSelectedApproval(null)}
            className="mb-4 text-blue-600 hover:text-blue-800 font-semibold"
          >
            ← Back to List
          </button>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            {/* Member Info */}
            <div className="bg-blue-50 p-4 rounded-lg">
              <h3 className="font-semibold text-gray-800 mb-3">Member Information</h3>
              <p><span className="font-medium">Name:</span> {selectedApproval.memberName}</p>
              <p><span className="font-medium">Requested Amount:</span> ₦{selectedApproval.requestedAmount.toLocaleString()}</p>
              <p><span className="font-medium">Risk Rating:</span> <span className={`px-2 py-1 rounded text-sm font-semibold ${getRiskRatingColor(selectedApproval.riskRating)}`}>{selectedApproval.riskRating}</span></p>
              <p><span className="font-medium">Referral Reason:</span> {selectedApproval.referralReason}</p>
            </div>

            {/* Repayment History */}
            <div className="bg-green-50 p-4 rounded-lg">
              <h3 className="font-semibold text-gray-800 mb-3">Repayment History</h3>
              <p><span className="font-medium">History Score:</span> {selectedApproval.loanRepaymentHistoryScore?.toFixed(1)} / 100</p>
              <p><span className="font-medium">Successful Loans:</span> {selectedApproval.previousSuccessfulLoans}</p>
              <p><span className="font-medium">Defaults:</span> <span className={selectedApproval.previousDefaultCount > 0 ? 'text-red-600 font-semibold' : 'text-green-600'}>{selectedApproval.previousDefaultCount}</span></p>
              <p><span className="font-medium">Default History:</span> {selectedApproval.hasDefaultHistory ? '❌ Yes' : '✅ No'}</p>
            </div>

            {/* Guarantor 1 */}
            <div className="bg-purple-50 p-4 rounded-lg">
              <h3 className="font-semibold text-gray-800 mb-3">Guarantor 1</h3>
              <p><span className="font-medium">Name:</span> {selectedApproval.guarantor1Name}</p>
              <p><span className="font-medium">Member Savings:</span> ₦{(selectedApproval.guarantor1MemberSavings || 0).toLocaleString()}</p>
              <p><span className="font-medium">Equity Check (50% of loan):</span> {selectedApproval.guarantor1MemberSavings && selectedApproval.guarantor1MemberSavings >= (selectedApproval.requestedAmount * 0.5) ? '✅ Pass' : '❌ Fail'}</p>
            </div>

            {/* Guarantor 2 */}
            <div className="bg-purple-50 p-4 rounded-lg">
              <h3 className="font-semibold text-gray-800 mb-3">Guarantor 2</h3>
              <p><span className="font-medium">Name:</span> {selectedApproval.guarantor2Name || 'N/A'}</p>
              <p><span className="font-medium">Member Savings:</span> ₦{(selectedApproval.guarantor2MemberSavings || 0).toLocaleString()}</p>
              <p><span className="font-medium">Equity Check (50% of loan):</span> {selectedApproval.guarantor2MemberSavings && selectedApproval.guarantor2MemberSavings >= (selectedApproval.requestedAmount * 0.5) ? '✅ Pass' : '❌ Fail'}</p>
            </div>
          </div>

          {/* Committee Summary */}
          <div className="bg-yellow-50 p-4 rounded-lg mb-6 border-l-4 border-yellow-500">
            <h3 className="font-semibold text-gray-800 mb-2">Credit Officer Recommendation</h3>
            <p className="text-gray-700">{selectedApproval.committeeSummary}</p>
          </div>

          {/* Approval Decision Form */}
          {!selectedApproval.committeeDecision && (
            <div className="bg-gray-50 p-6 rounded-lg">
              <h3 className="font-semibold text-gray-800 mb-4">Committee Decision</h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Decision</label>
                  <select
                    value={approvalDecision}
                    onChange={(e) => setApprovalDecision(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  >
                    <option value="Approved">Approved</option>
                    <option value="ApprovedWithConditions">Approved With Conditions</option>
                    <option value="Rejected">Rejected</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Committee Notes</label>
                  <textarea
                    value={approvalNotes}
                    onChange={(e) => setApprovalNotes(e.target.value)}
                    rows={3}
                    placeholder="Document committee discussion and reasoning..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>
                {approvalDecision === 'ApprovedWithConditions' && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Conditions</label>
                    <textarea
                      value={approvalConditions}
                      onChange={(e) => setApprovalConditions(e.target.value)}
                      rows={2}
                      placeholder="E.g., Higher interest rate, Lower amount, Additional guarantor..."
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                )}
                <button
                  onClick={submitApproval}
                  disabled={submittingApproval}
                  className="w-full bg-green-600 text-white py-2 rounded-md hover:bg-green-700 disabled:bg-gray-400 font-semibold"
                >
                  {submittingApproval ? 'Submitting...' : 'Submit Committee Decision'}
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default LoanCommitteeApprovalDashboard;
