import React, { useState, useEffect } from 'react';
import { Building2, Plus, Edit, Trash2, Users, TrendingUp, MapPin } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { Branch } from '../types/branch.types';
import { branchService } from '../services/branchService';

export const BranchManagement: React.FC = () => {
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<Branch | null>(null);

  useEffect(() => {
    loadBranches();
  }, []);

  const loadBranches = async () => {
    setLoading(true);
    try {
      const data = await branchService.getBranches();
      setBranches(data);
    } catch (error) {
      console.error('Failed to load branches:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (branchId: string) => {
    if (!confirm('Are you sure you want to delete this branch?')) return;
    
    try {
      await branchService.deleteBranch(branchId);
      await loadBranches();
    } catch (error) {
      console.error('Failed to delete branch:', error);
    }
  };

  const handleSetDefault = async (branchId: string) => {
    try {
      await branchService.setDefaultBranch(branchId);
      alert('Default branch updated');
    } catch (error) {
      console.error('Failed to set default branch:', error);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'bg-success-100 text-success-800';
      case 'inactive': return 'bg-neutral-100 text-neutral-800';
      case 'suspended': return 'bg-error-100 text-error-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'headquarters': return '🏢';
      case 'regional': return '🏛️';
      case 'local': return '🏪';
      case 'warehouse': return '📦';
      case 'retail': return '🛒';
      default: return '🏢';
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Branch Management</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Manage branches and locations
          </p>
        </div>
        <Button variant="primary">
          <Plus className="w-4 h-4 mr-2" />
          Add Branch
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Total Branches</div>
            <Building2 className="w-5 h-5 text-primary-600" />
          </div>
          <div className="text-2xl font-bold">{branches.length}</div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Active</div>
            <TrendingUp className="w-5 h-5 text-success-600" />
          </div>
          <div className="text-2xl font-bold text-success-600">
            {branches.filter(b => b.status === 'active').length}
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Headquarters</div>
            <Building2 className="w-5 h-5 text-primary-600" />
          </div>
          <div className="text-2xl font-bold">
            {branches.filter(b => b.type === 'headquarters').length}
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="text-sm text-neutral-600">Regional</div>
            <MapPin className="w-5 h-5 text-warning-600" />
          </div>
          <div className="text-2xl font-bold">
            {branches.filter(b => b.type === 'regional').length}
          </div>
        </Card>
      </div>

      {/* Branches List */}
      <div className="space-y-4">
        {loading ? (
          <Card className="p-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          </Card>
        ) : branches.length === 0 ? (
          <Card className="p-8">
            <div className="text-center text-neutral-600">
              <Building2 className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No branches found</p>
            </div>
          </Card>
        ) : (
          branches.map((branch) => (
            <Card key={branch.id} className="p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3 mb-2">
                    <span className="text-2xl">{getTypeIcon(branch.type)}</span>
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="text-lg font-semibold">{branch.name}</h3>
                        <span className="text-sm text-neutral-600 font-mono">
                          {branch.code}
                        </span>
                        <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(branch.status)}`}>
                          {branch.status}
                        </span>
                      </div>
                      <p className="text-sm text-neutral-600">{branch.description}</p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
                    <div>
                      <div className="text-xs text-neutral-600">Type</div>
                      <div className="font-medium capitalize">{branch.type}</div>
                    </div>
                    <div>
                      <div className="text-xs text-neutral-600">Manager</div>
                      <div className="font-medium">{branch.manager}</div>
                    </div>
                    <div>
                      <div className="text-xs text-neutral-600">Location</div>
                      <div className="font-medium">
                        {branch.address.city}, {branch.address.state}
                      </div>
                    </div>
                    <div>
                      <div className="text-xs text-neutral-600">Contact</div>
                      <div className="font-medium text-sm">{branch.contact.phone}</div>
                    </div>
                  </div>

                  <div className="mt-3 flex items-center space-x-4 text-xs text-neutral-500">
                    <span>Currency: {branch.currency}</span>
                    <span>•</span>
                    <span>Timezone: {branch.timezone}</span>
                    {branch.settings.allowInterBranchTransfers && (
                      <>
                        <span>•</span>
                        <span className="text-success-600">Inter-branch transfers enabled</span>
                      </>
                    )}
                  </div>
                </div>

                <div className="flex items-center space-x-2 ml-4">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setSelectedBranch(branch)}
                    title="View Details"
                  >
                    <TrendingUp className="w-4 h-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    title="Manage Users"
                  >
                    <Users className="w-4 h-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    title="Edit"
                  >
                    <Edit className="w-4 h-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDelete(branch.id)}
                    title="Delete"
                  >
                    <Trash2 className="w-4 h-4 text-error-600" />
                  </Button>
                </div>
              </div>
            </Card>
          ))
        )}
      </div>
    </div>
  );
};
