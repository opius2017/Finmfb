import { OCRData, OCRField } from '../types/invoice.types';

export class OCRService {
  private apiEndpoint = '/api/ocr';

  /**
   * Extract data from invoice image/PDF using OCR
   */
  async extractInvoiceData(file: File): Promise<OCRData> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${this.apiEndpoint}/extract`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error('OCR extraction failed');
    }

    return response.json();
  }

  /**
   * Parse OCR data to extract structured invoice fields
   */
  parseInvoiceFields(ocrData: OCRData): {
    vendorName?: string;
    invoiceNumber?: string;
    invoiceDate?: Date;
    dueDate?: Date;
    total?: number;
    subtotal?: number;
    taxAmount?: number;
  } {
    const fields: Record<string, any> = {};

    ocrData.fields.forEach((field) => {
      switch (field.name.toLowerCase()) {
        case 'vendor_name':
        case 'supplier_name':
          fields.vendorName = field.value;
          break;
        case 'invoice_number':
        case 'invoice_no':
          fields.invoiceNumber = field.value;
          break;
        case 'invoice_date':
        case 'date':
          fields.invoiceDate = this.parseDate(field.value);
          break;
        case 'due_date':
        case 'payment_due':
          fields.dueDate = this.parseDate(field.value);
          break;
        case 'total':
        case 'total_amount':
          fields.total = this.parseAmount(field.value);
          break;
        case 'subtotal':
        case 'sub_total':
          fields.subtotal = this.parseAmount(field.value);
          break;
        case 'tax':
        case 'vat':
        case 'tax_amount':
          fields.taxAmount = this.parseAmount(field.value);
          break;
      }
    });

    return fields;
  }

  /**
   * Parse date string to Date object
   */
  private parseDate(dateStr: string): Date | undefined {
    try {
      // Try various date formats
      const formats = [
        /(\d{1,2})\/(\d{1,2})\/(\d{4})/,  // DD/MM/YYYY or MM/DD/YYYY
        /(\d{4})-(\d{2})-(\d{2})/,         // YYYY-MM-DD
        /(\d{1,2})-(\d{1,2})-(\d{4})/,    // DD-MM-YYYY
      ];

      for (const format of formats) {
        const match = dateStr.match(format);
        if (match) {
          return new Date(dateStr);
        }
      }

      return undefined;
    } catch {
      return undefined;
    }
  }

  /**
   * Parse amount string to number
   */
  private parseAmount(amountStr: string): number | undefined {
    try {
      // Remove currency symbols and commas
      const cleaned = amountStr.replace(/[₦$€£,]/g, '').trim();
      const amount = parseFloat(cleaned);
      return isNaN(amount) ? undefined : amount;
    } catch {
      return undefined;
    }
  }

  /**
   * Validate OCR confidence score
   */
  isConfidenceAcceptable(ocrData: OCRData, threshold: number = 0.8): boolean {
    return ocrData.confidence >= threshold;
  }

  /**
   * Get low confidence fields that need manual review
   */
  getLowConfidenceFields(ocrData: OCRData, threshold: number = 0.7): OCRField[] {
    return ocrData.fields.filter(field => field.confidence < threshold);
  }
}

export const ocrService = new OCRService();
