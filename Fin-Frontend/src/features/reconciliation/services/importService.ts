/**
 * Bank Statement Import Service
 * Handles importing bank statements in various formats
 */

import type {
  BankStatement,
  BankTransaction,
  StatementFormat,
  ImportResult,
  ImportError,
  ImportWarning,
} from '../types/reconciliation.types';

class ImportService {
  /**
   * Import bank statement from file
   */
  async importStatement(
    file: File,
    accountId: string,
    accountName: string,
    accountNumber: string
  ): Promise<ImportResult> {
    const format = this.detectFormat(file);
    
    try {
      let transactions: BankTransaction[] = [];
      const errors: ImportError[] = [];
      const warnings: ImportWarning[] = [];

      switch (format) {
        case 'CSV':
          transactions = await this.parseCSV(file, errors, warnings);
          break;
        case 'Excel':
          transactions = await this.parseExcel(file, errors, warnings);
          break;
        case 'PDF':
          transactions = await this.parsePDF(file, errors, warnings);
          break;
        case 'OFX':
          transactions = await this.parseOFX(file, errors, warnings);
          break;
        case 'MT940':
          transactions = await this.parseMT940(file, errors, warnings);
          break;
        default:
          errors.push({
            message: `Unsupported file format: ${format}`,
            severity: 'error',
          });
          return {
            success: false,
            errors,
            warnings,
            transactionCount: 0,
            duplicateCount: 0,
          };
      }

      // Validate transactions
      const validatedTransactions = this.validateTransactions(transactions, errors, warnings);

      // Check for duplicates
      const { unique, duplicateCount } = this.removeDuplicates(validatedTransactions);

      if (duplicateCount > 0) {
        warnings.push({
          message: `${duplicateCount} duplicate transactions were removed`,
        });
      }

      // Create statement
      const statement: BankStatement = {
        id: `stmt-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        accountId,
        accountName,
        accountNumber,
        statementDate: new Date(),
        openingBalance: this.calculateOpeningBalance(unique),
        closingBalance: this.calculateClosingBalance(unique),
        format,
        fileName: file.name,
        uploadedBy: 'current-user', // Replace with actual user
        uploadedAt: new Date(),
        transactions: unique,
      };

      return {
        success: errors.filter(e => e.severity === 'error').length === 0,
        statement,
        errors,
        warnings,
        transactionCount: unique.length,
        duplicateCount,
      };
    } catch (error) {
      return {
        success: false,
        errors: [{
          message: `Failed to import statement: ${error instanceof Error ? error.message : 'Unknown error'}`,
          severity: 'error',
        }],
        warnings: [],
        transactionCount: 0,
        duplicateCount: 0,
      };
    }
  }

  /**
   * Detect file format
   */
  private detectFormat(file: File): StatementFormat {
    const extension = file.name.split('.').pop()?.toLowerCase();
    
    switch (extension) {
      case 'csv':
        return 'CSV';
      case 'xlsx':
      case 'xls':
        return 'Excel';
      case 'pdf':
        return 'PDF';
      case 'ofx':
        return 'OFX';
      case 'mt940':
      case 'sta':
        return 'MT940';
      default:
        return 'CSV'; // Default to CSV
    }
  }

  /**
   * Parse CSV file
   */
  private async parseCSV(
    file: File,
    errors: ImportError[],
    warnings: ImportWarning[]
  ): Promise<BankTransaction[]> {
    const text = await file.text();
    const lines = text.split('\n').filter(line => line.trim());
    
    if (lines.length === 0) {
      errors.push({ message: 'File is empty', severity: 'error' });
      return [];
    }

    // Parse header
    const header = lines[0].split(',').map(h => h.trim().toLowerCase());
    const transactions: BankTransaction[] = [];

    // Find column indices
    const dateIndex = header.findIndex(h => h.includes('date'));
    const descIndex = header.findIndex(h => h.includes('description') || h.includes('details'));
    const debitIndex = header.findIndex(h => h.includes('debit') || h.includes('withdrawal'));
    const creditIndex = header.findIndex(h => h.includes('credit') || h.includes('deposit'));
    const balanceIndex = header.findIndex(h => h.includes('balance'));
    const refIndex = header.findIndex(h => h.includes('reference') || h.includes('ref'));

    if (dateIndex === -1 || descIndex === -1) {
      errors.push({
        message: 'Required columns (date, description) not found',
        severity: 'error',
      });
      return [];
    }

    // Parse data rows
    for (let i = 1; i < lines.length; i++) {
      const values = lines[i].split(',').map(v => v.trim());
      
      try {
        const transaction: BankTransaction = {
          id: `bank-tx-${Date.now()}-${i}`,
          date: new Date(values[dateIndex]),
          description: values[descIndex],
          reference: refIndex >= 0 ? values[refIndex] : undefined,
          debit: debitIndex >= 0 ? this.parseAmount(values[debitIndex]) : undefined,
          credit: creditIndex >= 0 ? this.parseAmount(values[creditIndex]) : undefined,
          balance: balanceIndex >= 0 ? this.parseAmount(values[balanceIndex]) : undefined,
        };

        transactions.push(transaction);
      } catch (error) {
        errors.push({
          row: i + 1,
          message: `Failed to parse row: ${error instanceof Error ? error.message : 'Unknown error'}`,
          severity: 'warning',
        });
      }
    }

    return transactions;
  }

  /**
   * Parse Excel file (mock implementation)
   */
  private async parseExcel(
    file: File,
    errors: ImportError[],
    warnings: ImportWarning[]
  ): Promise<BankTransaction[]> {
    // In production, use a library like xlsx or exceljs
    warnings.push({
      message: 'Excel parsing is using CSV fallback. Install xlsx library for full support.',
    });
    
    return this.parseCSV(file, errors, warnings);
  }

  /**
   * Parse PDF file (mock implementation)
   */
  private async parsePDF(
    file: File,
    errors: ImportError[],
    warnings: ImportWarning[]
  ): Promise<BankTransaction[]> {
    // In production, use OCR service like Tesseract or cloud OCR
    warnings.push({
      message: 'PDF parsing requires OCR service. Please use CSV or Excel format.',
    });
    
    errors.push({
      message: 'PDF parsing not yet implemented. Please convert to CSV or Excel.',
      severity: 'error',
    });
    
    return [];
  }

  /**
   * Parse OFX file (mock implementation)
   */
  private async parseOFX(
    file: File,
    errors: ImportError[],
    warnings: ImportWarning[]
  ): Promise<BankTransaction[]> {
    warnings.push({
      message: 'OFX parsing is simplified. Full OFX support requires additional library.',
    });
    
    // Basic OFX parsing would go here
    return [];
  }

  /**
   * Parse MT940 file (mock implementation)
   */
  private async parseMT940(
    file: File,
    errors: ImportError[],
    warnings: ImportWarning[]
  ): Promise<BankTransaction[]> {
    warnings.push({
      message: 'MT940 parsing is simplified. Full MT940 support requires additional library.',
    });
    
    // Basic MT940 parsing would go here
    return [];
  }

  /**
   * Parse amount string to number
   */
  private parseAmount(value: string): number | undefined {
    if (!value || value === '') return undefined;
    
    // Remove currency symbols and commas
    const cleaned = value.replace(/[₦$,]/g, '').trim();
    const amount = parseFloat(cleaned);
    
    return isNaN(amount) ? undefined : amount;
  }

  /**
   * Validate transactions
   */
  private validateTransactions(
    transactions: BankTransaction[],
    errors: ImportError[],
    warnings: ImportWarning[]
  ): BankTransaction[] {
    return transactions.filter((tx, index) => {
      // Check required fields
      if (!tx.date || isNaN(tx.date.getTime())) {
        errors.push({
          row: index + 2,
          field: 'date',
          message: 'Invalid or missing date',
          severity: 'warning',
        });
        return false;
      }

      if (!tx.description || tx.description.trim() === '') {
        errors.push({
          row: index + 2,
          field: 'description',
          message: 'Missing description',
          severity: 'warning',
        });
        return false;
      }

      if (!tx.debit && !tx.credit) {
        errors.push({
          row: index + 2,
          message: 'Transaction must have either debit or credit amount',
          severity: 'warning',
        });
        return false;
      }

      return true;
    });
  }

  /**
   * Remove duplicate transactions
   */
  private removeDuplicates(transactions: BankTransaction[]): {
    unique: BankTransaction[];
    duplicateCount: number;
  } {
    const seen = new Set<string>();
    const unique: BankTransaction[] = [];
    let duplicateCount = 0;

    for (const tx of transactions) {
      const key = `${tx.date.toISOString()}-${tx.description}-${tx.debit || 0}-${tx.credit || 0}`;
      
      if (!seen.has(key)) {
        seen.add(key);
        unique.push(tx);
      } else {
        duplicateCount++;
      }
    }

    return { unique, duplicateCount };
  }

  /**
   * Calculate opening balance
   */
  private calculateOpeningBalance(transactions: BankTransaction[]): number {
    if (transactions.length === 0) return 0;
    
    // If first transaction has balance, use it
    const firstTx = transactions[0];
    if (firstTx.balance !== undefined) {
      const amount = (firstTx.debit || 0) - (firstTx.credit || 0);
      return firstTx.balance - amount;
    }

    return 0;
  }

  /**
   * Calculate closing balance
   */
  private calculateClosingBalance(transactions: BankTransaction[]): number {
    if (transactions.length === 0) return 0;
    
    // If last transaction has balance, use it
    const lastTx = transactions[transactions.length - 1];
    if (lastTx.balance !== undefined) {
      return lastTx.balance;
    }

    // Calculate from opening balance
    const openingBalance = this.calculateOpeningBalance(transactions);
    return transactions.reduce((balance, tx) => {
      return balance + (tx.credit || 0) - (tx.debit || 0);
    }, openingBalance);
  }

  /**
   * Get supported formats
   */
  getSupportedFormats(): StatementFormat[] {
    return ['CSV', 'Excel', 'OFX', 'MT940', 'PDF'];
  }

  /**
   * Get format description
   */
  getFormatDescription(format: StatementFormat): string {
    const descriptions: Record<StatementFormat, string> = {
      CSV: 'Comma-separated values (.csv)',
      Excel: 'Microsoft Excel (.xlsx, .xls)',
      OFX: 'Open Financial Exchange (.ofx)',
      MT940: 'SWIFT MT940 (.mt940, .sta)',
      PDF: 'Portable Document Format (.pdf) - requires OCR',
    };

    return descriptions[format];
  }
}

// Export singleton instance
export const importService = new ImportService();
