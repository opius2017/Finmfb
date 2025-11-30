import React, { useState, useEffect } from 'react';
import { Lock, Plus, Unlock, FileText } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { LegalHold } from '../types/retention.types';
import { retentionService } from '../services/retentionService';

export const LegalHolds: React.FC = () => {
  const [holds, setHolds] = useState<LegalHold[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadHolds();
  }, []);

  const loadHolds = async () => {
    setLoading(true);
    try {
      const data = await retentionService.getLegalHolds();
      setHolds(data);
    } catch (error) {
      console.error('Failed to load legal holds:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleReleaseHold = async (holdId: string) => {
    if (!confirm('Are you sure you want to release this legal hold?')) return;
    
    try {
      await retentionService.releaseLegalHold(holdId);
      await loadHolds();
    } catch (error) {
      console.error('Failed to release legal hold:', error);
    }
  };

  return (
    <div className=\"p-6\">
      <div className=\"flex items-center justify-between mb-6\">
        <h1 className=\"text-2xl font-bold\">Legal Holds</h1>
        <Button variant=\"primary\">
          <Plus className=\"w-4 h-4 mr-2\" />
          Create Legal Hold
        </Button>
      </div>

      <Card className=\"p-6\">
        {loading ? (
          <div className=\"text-center py-8\">
            <div className=\"animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto\"></div>
          </div>
        ) : holds.length === 0 ? (
          <div className=\"text-center py-8 text-neutral-600\">
            <Lock className=\"w-12 h-12 mx-auto mb-3 text-neutral-400\" />
            <p>No active legal holds</p>
          </div>
        ) : (
          <div className=\"space-y-4\">
            {holds.map((hold) => (
              <div
                key={hold.id}
                className=\"p-4 border border-neutral-200 rounded-lg\"
              >
                <div className=\"flex items-start justify-between mb-3\">
                  <div className=\"flex-1\">
                    <div className=\"flex items-center space-x-3 mb-2\">
                      <Lock className=\"w-5 h-5 text-error-600\" />
                      <h3 className=\"font-semibold text-lg\">{hold.name}</h3>
                      {hold.isActive ? (
                        <span className=\"px-2 py-1 text-xs bg-error-100 text-error-800 rounded-full\">
                          Active
                        </span>
                      ) : (
                        <span className=\"px-2 py-1 text-xs bg-neutral-200 text-neutral-700 rounded-full\">
                          Released
                        </span>
                      )}
                    </div>
                    <p className=\"text-sm text-neutral-600 mb-2\">{hold.description}</p>
                    <div className=\"text-sm text-neutral-700 mb-2\">
                      <span className=\"font-medium\">Reason:</span> {hold.reason}
                    </div>
                  </div>
                  {hold.isActive && (
                    <Button
                      variant=\"outline\"
                      size=\"sm\"
                      onClick={() => handleReleaseHold(hold.id)}
                    >
                      <Unlock className=\"w-4 h-4 mr-2\" />
                      Release Hold
                    </Button>
                  )}
                </div>

                <div className=\"grid grid-cols-3 gap-4 text-sm\">
                  <div>
                    <div className=\"text-neutral-600\">Documents</div>
                    <div className=\"font-medium flex items-center\">
                      <FileText className=\"w-4 h-4 mr-1\" />
                      {hold.documentIds.length}
                    </div>
                  </div>
                  <div>
                    <div className=\"text-neutral-600\">Start Date</div>
                    <div className=\"font-medium\">
                      {new Date(hold.startDate).toLocaleDateString()}
                    </div>
                  </div>
                  <div>
                    <div className=\"text-neutral-600\">Created By</div>
                    <div className=\"font-medium\">{hold.createdBy}</div>
                  </div>
                </div>

                {hold.approvedBy && (
                  <div className=\"mt-3 pt-3 border-t border-neutral-200 text-sm\">
                    <span className=\"text-neutral-600\">Approved by:</span>{' '}
                    <span className=\"font-medium\">{hold.approvedBy}</span>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
};
