import { importService } from '../services/importService';

describe('Import Service', () => {
  describe('Format Detection', () => {
    it('should detect CSV format', () => {
      const file = new File([''], 'statement.csv', { type: 'text/csv' });
      const formats = importService.getSupportedFormats();
      
      expect(formats).toContain('CSV');
    });

    it('should detect Excel format', () => {
      const file = new File([''], 'statement.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
      const formats = importService.getSupportedFormats();
      
      expect(formats).toContain('Excel');
    });

    it('should return all supported formats', () => {
      const formats = importService.getSupportedFormats();
      
      expect(formats).toContain('CSV');
      expect(formats).toContain('Excel');
      expect(formats).toContain('OFX');
      expect(formats).toContain('MT940');
      expect(formats).toContain('PDF');
    });
  });

  describe('CSV Import', () => {
    it('should import valid CSV file', async () => {
      const csvContent = `Date,Description,Debit,Credit,Balance
2024-01-15,Payment from Customer,0,5000,15000
2024-01-16,Office Supplies,500,0,14500`;

      const file = new File([csvContent], 'statement.csv', { type: 'text/csv' });
      
      const result = await importService.importStatement(
        file,
        'acc-1',
        'Cash Account',
        '1234567890'
      );

      expect(result.success).toBe(true);
      expect(result.statement).toBeDefined();
      expect(result.statement?.transactions.length).toBe(2);
      expect(result.transactionCount).toBe(2);
    });

    it('should handle empty CSV file', async () => {
      const csvContent = '';
      const file = new File([csvContent], 'empty.csv', { type: 'text/csv' });
      
      const result = await importService.importStatement(
        file,
        'acc-1',
        'Cash Account',
        '1234567890'
      );

      expect(result.success).toBe(false);
      expect(result.errors.length).toBeGreaterThan(0);
    });

    it('should handle CSV with missing required columns', async () => {
      const csvContent = `Date,Amount
2024-01-15,5000`;

      const file = new File([csvContent], 'invalid.csv', { type: 'text/csv' });
      
      const result = await importService.importStatement(
        file,
        'acc-1',
        'Cash Account',
        '1234567890'
      );

      expect(result.success).toBe(false);
      expect(result.errors.some(e => e.message.includes('Required columns'))).toBe(true);
    });

    it('should remove duplicate transactions', async () => {
      const csvContent = `Date,Description,Debit,Credit,Balance
2024-01-15,Payment,0,5000,15000
2024-01-15,Payment,0,5000,15000`;

      const file = new File([csvContent], 'duplicates.csv', { type: 'text/csv' });
      
      const result = await importService.importStatement(
        file,
        'acc-1',
        'Cash Account',
        '1234567890'
      );

      expect(result.duplicateCount).toBe(1);
      expect(result.transactionCount).toBe(1);
    });

    it('should validate transaction data', async () => {
      const csvContent = `Date,Description,Debit,Credit,Balance
invalid-date,Payment,0,5000,15000
2024-01-15,,0,5000,15000`;

      const file = new File([csvContent], 'invalid-data.csv', { type: 'text/csv' });
      
      const result = await importService.importStatement(
        file,
        'acc-1',
        'Cash Account',
        '1234567890'
      );

      expect(result.errors.length).toBeGreaterThan(0);
    });
  });

  describe('Format Descriptions', () => {
    it('should provide description for each format', () => {
      const formats = importService.getSupportedFormats();
      
      formats.forEach(format => {
        const description = importService.getFormatDescription(format);
        expect(description).toBeDefined();
        expect(description.length).toBeGreaterThan(0);
      });
    });
  });

  describe('Balance Calculations', () => {
    it('should calculate opening and closing balances', async () => {
      const csvContent = `Date,Description,Debit,Credit,Balance
2024-01-15,Opening,0,0,10000
2024-01-16,Payment,0,5000,15000
2024-01-17,Expense,2000,0,13000`;

      const file = new File([csvContent], 'balances.csv', { type: 'text/csv' });
      
      const result = await importService.importStatement(
        file,
        'acc-1',
        'Cash Account',
        '1234567890'
      );

      expect(result.statement?.openingBalance).toBeDefined();
      expect(result.statement?.closingBalance).toBeDefined();
      expect(result.statement?.closingBalance).toBe(13000);
    });
  });
});
