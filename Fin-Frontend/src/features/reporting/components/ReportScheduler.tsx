import React, { useState } from 'react';
import { Clock, Mail, Download, Calendar } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { ReportSchedule } from '../types/report.types';

interface ReportSchedulerProps {
  reportId: string;
  reportName: string;
  onSchedule: (schedule: ReportSchedule) => void;
  onCancel: () => void;
}

export const ReportScheduler: React.FC<ReportSchedulerProps> = ({
  reportId,
  reportName,
  onSchedule,
  onCancel,
}) => {
  const [schedule, setSchedule] = useState<Partial<ReportSchedule>>({
    enabled: true,
    frequency: 'monthly',
    time: '09:00',
    recipients: [],
    format: 'pdf',
  });
  const [recipientEmail, setRecipientEmail] = useState('');

  const handleAddRecipient = () => {
    if (recipientEmail && !schedule.recipients?.includes(recipientEmail)) {
      setSchedule({
        ...schedule,
        recipients: [...(schedule.recipients || []), recipientEmail],
      });
      setRecipientEmail('');
    }
  };

  const handleRemoveRecipient = (email: string) => {
    setSchedule({
      ...schedule,
      recipients: schedule.recipients?.filter(r => r !== email),
    });
  };

  const handleSubmit = () => {
    onSchedule(schedule as ReportSchedule);
  };

  return (
    <Card className="p-6">
      <h2 className="text-xl font-bold mb-6">Schedule Report: {reportName}</h2>

      <div className="space-y-6">
        <div>
          <label className="block text-sm font-medium mb-2">Frequency</label>
          <select
            value={schedule.frequency}
            onChange={(e) => setSchedule({ ...schedule, frequency: e.target.value as any })}
            className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
          >
            <option value="daily">Daily</option>
            <option value="weekly">Weekly</option>
            <option value="monthly">Monthly</option>
            <option value="quarterly">Quarterly</option>
          </select>
        </div>

        {schedule.frequency === 'weekly' && (
          <div>
            <label className="block text-sm font-medium mb-2">Day of Week</label>
            <select
              value={schedule.dayOfWeek || 1}
              onChange={(e) => setSchedule({ ...schedule, dayOfWeek: parseInt(e.target.value) })}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
            >
              <option value="1">Monday</option>
              <option value="2">Tuesday</option>
              <option value="3">Wednesday</option>
              <option value="4">Thursday</option>
              <option value="5">Friday</option>
            </select>
          </div>
        )}

        {schedule.frequency === 'monthly' && (
          <div>
            <label className="block text-sm font-medium mb-2">Day of Month</label>
            <Input
              type="number"
              min="1"
              max="31"
              value={schedule.dayOfMonth || 1}
              onChange={(e) => setSchedule({ ...schedule, dayOfMonth: parseInt(e.target.value) })}
            />
          </div>
        )}

        <div>
          <label className="block text-sm font-medium mb-2">Time</label>
          <Input
            type="time"
            value={schedule.time}
            onChange={(e) => setSchedule({ ...schedule, time: e.target.value })}
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Export Format</label>
          <div className="flex space-x-4">
            {['pdf', 'excel', 'csv'].map((format) => (
              <label key={format} className="flex items-center space-x-2">
                <input
                  type="radio"
                  value={format}
                  checked={schedule.format === format}
                  onChange={(e) => setSchedule({ ...schedule, format: e.target.value as any })}
                  className="rounded"
                />
                <span className="capitalize">{format}</span>
              </label>
            ))}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Recipients</label>
          <div className="flex space-x-2 mb-3">
            <Input
              type="email"
              value={recipientEmail}
              onChange={(e) => setRecipientEmail(e.target.value)}
              placeholder="Enter email address"
            />
            <Button variant="outline" onClick={handleAddRecipient}>
              Add
            </Button>
          </div>
          <div className="space-y-2">
            {schedule.recipients?.map((email) => (
              <div key={email} className="flex items-center justify-between p-2 bg-neutral-50 rounded">
                <span className="text-sm">{email}</span>
                <button
                  onClick={() => handleRemoveRecipient(email)}
                  className="text-error-600 text-sm hover:text-error-700"
                >
                  Remove
                </button>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="mt-6 flex justify-end space-x-3">
        <Button variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={!schedule.recipients || schedule.recipients.length === 0}
        >
          <Clock className="w-4 h-4 mr-2" />
          Schedule Report
        </Button>
      </div>
    </Card>
  );
};
