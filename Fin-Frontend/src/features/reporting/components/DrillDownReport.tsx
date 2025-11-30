import React, { useState } from 'react';
import { ChevronRight, ArrowLeft, FileText } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';

interface DrillDownLevel {
  title: string;
  data: any[];
  columns: string[];
}

interface DrillDownReportProps {
  initialData: any[];
  onDrillDown: (item: any, level: number) => Promise<any[]>;
}

export const DrillDownReport: React.FC<DrillDownReportProps> = ({
  initialData,
  onDrillDown,
}) => {
  const [levels, setLevels] = useState<DrillDownLevel[]>([
    {
      title: 'Summary',
      data: initialData,
      columns: Object.keys(initialData[0] || {}),
    },
  ]);
  const [currentLevel, setCurrentLevel] = useState(0);
  const [loading, setLoading] = useState(false);

  const handleDrillDown = async (item: any) => {
    setLoading(true);
    try {
      const detailData = await onDrillDown(item, currentLevel);
      const newLevel: DrillDownLevel = {
        title: `Details: ${item.name || item.accountName || 'Item'}`,
        data: detailData,
        columns: Object.keys(detailData[0] || {}),
      };
      setLevels([...levels, newLevel]);
      setCurrentLevel(currentLevel + 1);
    } catch (error) {
      console.error('Failed to drill down:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDrillUp = () => {
    if (currentLevel > 0) {
      setLevels(levels.slice(0, -1));
      setCurrentLevel(currentLevel - 1);
    }
  };

  const currentData = levels[currentLevel];

  return (
    <Card className="p-6">
      {/* Breadcrumb */}
      <div className="flex items-center space-x-2 mb-4">
        {levels.map((level, index) => (
          <React.Fragment key={index}>
            {index > 0 && <ChevronRight className="w-4 h-4 text-neutral-400" />}
            <button
              onClick={() => {
                setLevels(levels.slice(0, index + 1));
                setCurrentLevel(index);
              }}
              className={`text-sm ${
                index === currentLevel
                  ? 'font-semibold text-primary-600'
                  : 'text-neutral-600 hover:text-primary-600'
              }`}
            >
              {level.title}
            </button>
          </React.Fragment>
        ))}
      </div>

      {/* Back Button */}
      {currentLevel > 0 && (
        <Button variant="outline" size="sm" onClick={handleDrillUp} className="mb-4">
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back
        </Button>
      )}

      {/* Data Table */}
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-neutral-200">
              {currentData.columns.map((col) => (
                <th key={col} className="text-left py-3 px-4 font-semibold capitalize">
                  {col.replace(/_/g, ' ')}
                </th>
              ))}
              <th className="text-right py-3 px-4 font-semibold">Actions</th>
            </tr>
          </thead>
          <tbody>
            {currentData.data.map((row, idx) => (
              <tr key={idx} className="border-b border-neutral-100 hover:bg-neutral-50">
                {currentData.columns.map((col) => (
                  <td key={col} className="py-3 px-4">
                    {typeof row[col] === 'number'
                      ? row[col].toLocaleString()
                      : row[col]}
                  </td>
                ))}
                <td className="py-3 px-4 text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDrillDown(row)}
                    disabled={loading}
                  >
                    <FileText className="w-4 h-4" />
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  );
};
