import React, { useState } from 'react';
import { clsx } from 'clsx';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import type { TableConfig } from '../types/widget.types';

export interface TableWidgetProps {
  config: TableConfig;
}

export const TableWidget: React.FC<TableWidgetProps> = ({ config }) => {
  const pageSize = config.pageSize || 5;
  const [currentPage, setCurrentPage] = useState(1);

  const totalPages = Math.ceil(config.data.length / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const currentData = config.data.slice(startIndex, endIndex);

  const goToNextPage = () => {
    if (currentPage < totalPages) {
      setCurrentPage(currentPage + 1);
    }
  };

  const goToPreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage(currentPage - 1);
    }
  };

  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-x-auto">
        <table className="min-w-full divide-y divide-neutral-200 dark:divide-neutral-700">
          <thead className="bg-neutral-50 dark:bg-neutral-800">
            <tr>
              {config.columns.map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  style={{ width: column.width }}
                  className={clsx(
                    'px-4 py-2 text-left text-xs font-semibold',
                    'text-neutral-700 dark:text-neutral-300',
                    'uppercase tracking-wider'
                  )}
                >
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white dark:bg-neutral-900 divide-y divide-neutral-200 dark:divide-neutral-800">
            {currentData.length === 0 ? (
              <tr>
                <td
                  colSpan={config.columns.length}
                  className="px-4 py-8 text-center text-sm text-neutral-500 dark:text-neutral-400"
                >
                  No data available
                </td>
              </tr>
            ) : (
              currentData.map((row, rowIndex) => (
                <tr
                  key={rowIndex}
                  className="hover:bg-neutral-50 dark:hover:bg-neutral-800 transition-colors"
                >
                  {config.columns.map((column) => (
                    <td
                      key={column.key}
                      className="px-4 py-2 whitespace-nowrap text-sm text-neutral-900 dark:text-neutral-100"
                    >
                      {row[column.key]}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between px-4 py-3 border-t border-neutral-200 dark:border-neutral-700">
          <div className="text-sm text-neutral-700 dark:text-neutral-300">
            Page {currentPage} of {totalPages}
          </div>
          <div className="flex gap-2">
            <button
              onClick={goToPreviousPage}
              disabled={currentPage === 1}
              className={clsx(
                'p-1 rounded-md transition-colors',
                'disabled:opacity-50 disabled:cursor-not-allowed',
                'text-neutral-600 dark:text-neutral-400',
                'hover:bg-neutral-100 dark:hover:bg-neutral-800',
                'focus:outline-none focus:ring-2 focus:ring-primary-500'
              )}
              aria-label="Previous page"
            >
              <ChevronLeft className="h-5 w-5" />
            </button>
            <button
              onClick={goToNextPage}
              disabled={currentPage === totalPages}
              className={clsx(
                'p-1 rounded-md transition-colors',
                'disabled:opacity-50 disabled:cursor-not-allowed',
                'text-neutral-600 dark:text-neutral-400',
                'hover:bg-neutral-100 dark:hover:bg-neutral-800',
                'focus:outline-none focus:ring-2 focus:ring-primary-500'
              )}
              aria-label="Next page"
            >
              <ChevronRight className="h-5 w-5" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
