import { PortfolioReport } from '../types/loan.types';
import { api } from '../../../services/api';

export const loanReportService = {
    getPortfolioReport: async (): Promise<PortfolioReport> => {
        const response = await fetch('http://localhost:5002/api/reports/portfolio');
        if (!response.ok) {
            throw new Error('Failed to fetch portfolio report');
        }
        return response.json();
    },

    // Placeholder for generate report which returns a blob
    generateReport: async (reportType: string, params: any): Promise<Blob> => {
        const response = await fetch('http://localhost:5002/api/reports/generate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ reportType, ...params })
        });
        if (!response.ok) {
            throw new Error('Failed to generate report');
        }
        return response.blob();
    }
};
