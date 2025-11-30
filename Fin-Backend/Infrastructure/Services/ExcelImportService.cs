using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FinTech.Core.Application.DTOs.Loans;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Service for importing data from Excel files using EPPlus
    /// </summary>
    public class ExcelImportService
    {
        private readonly ILogger<ExcelImportService> _logger;

        public ExcelImportService(ILogger<ExcelImportService> logger)
        {
            _logger = logger;
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Import actual deductions from Excel file
        /// Expected format: Member Number, Loan Number, Amount, Payroll Reference, Status
        /// </summary>
        public List<ActualDeductionRecord> ImportActualDeductions(byte[] fileContent, string fileName)
        {
            try
            {
                _logger.LogInformation("Starting import of actual deductions from file: {FileName}", fileName);

                var records = new List<ActualDeductionRecord>();

                using var stream = new MemoryStream(fileContent);
                using var package = new ExcelPackage(stream);

                if (package.Workbook.Worksheets.Count == 0)
                {
                    throw new InvalidOperationException("Excel file contains no worksheets");
                }

                var worksheet = package.Workbook.Worksheets[0]; // Use first worksheet
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                {
                    throw new InvalidOperationException("Excel file contains no data rows");
                }

                _logger.LogInformation("Processing {RowCount} rows from worksheet: {WorksheetName}", 
                    rowCount, worksheet.Name);

                // Validate headers (row 1)
                ValidateHeaders(worksheet);

                // Process data rows (starting from row 2)
                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var memberNumber = worksheet.Cells[row, 1].Text?.Trim();
                        var loanNumber = worksheet.Cells[row, 2].Text?.Trim();
                        var amountText = worksheet.Cells[row, 3].Text?.Trim();
                        var payrollRef = worksheet.Cells[row, 4].Text?.Trim();
                        var status = worksheet.Cells[row, 5].Text?.Trim()?.ToUpper();

                        // Skip empty rows
                        if (string.IsNullOrWhiteSpace(memberNumber) && 
                            string.IsNullOrWhiteSpace(loanNumber))
                        {
                            continue;
                        }

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(memberNumber))
                        {
                            errors.Add($"Row {row}: Member Number is required");
                            errorCount++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(loanNumber))
                        {
                            errors.Add($"Row {row}: Loan Number is required");
                            errorCount++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(amountText))
                        {
                            errors.Add($"Row {row}: Amount is required");
                            errorCount++;
                            continue;
                        }

                        // Parse amount
                        if (!decimal.TryParse(amountText.Replace("₦", "").Replace(",", ""), out decimal amount))
                        {
                            errors.Add($"Row {row}: Invalid amount format: {amountText}");
                            errorCount++;
                            continue;
                        }

                        if (amount < 0)
                        {
                            errors.Add($"Row {row}: Amount cannot be negative");
                            errorCount++;
                            continue;
                        }

                        // Validate status
                        var validStatuses = new[] { "SUCCESS", "FAILED", "PENDING", "PARTIAL" };
                        if (!string.IsNullOrWhiteSpace(status) && !validStatuses.Contains(status))
                        {
                            errors.Add($"Row {row}: Invalid status: {status}. Must be one of: {string.Join(", ", validStatuses)}");
                            errorCount++;
                            continue;
                        }

                        var record = new ActualDeductionRecord
                        {
                            MemberNumber = memberNumber,
                            LoanNumber = loanNumber,
                            ActualAmount = amount,
                            PayrollReference = payrollRef,
                            DeductionStatus = status ?? "SUCCESS",
                            RowNumber = row
                        };

                        records.Add(record);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {row}: {ex.Message}");
                        errorCount++;
                        _logger.LogWarning(ex, "Error processing row {Row}", row);
                    }
                }

                _logger.LogInformation(
                    "Import completed. Success: {SuccessCount}, Errors: {ErrorCount}", 
                    successCount, errorCount);

                if (errors.Any())
                {
                    _logger.LogWarning("Import errors: {Errors}", string.Join("; ", errors.Take(10)));
                }

                if (successCount == 0)
                {
                    throw new InvalidOperationException(
                        $"No valid records imported. Errors: {string.Join("; ", errors.Take(5))}");
                }

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing actual deductions from Excel");
                throw;
            }
        }

        /// <summary>
        /// Validate Excel headers
        /// </summary>
        private void ValidateHeaders(ExcelWorksheet worksheet)
        {
            var expectedHeaders = new Dictionary<int, string>
            {
                { 1, "Member Number" },
                { 2, "Loan Number" },
                { 3, "Amount" },
                { 4, "Payroll Reference" },
                { 5, "Status" }
            };

            var errors = new List<string>();

            foreach (var header in expectedHeaders)
            {
                var actualHeader = worksheet.Cells[1, header.Key].Text?.Trim();
                
                if (string.IsNullOrWhiteSpace(actualHeader))
                {
                    errors.Add($"Column {header.Key}: Missing header (expected '{header.Value}')");
                }
                else if (!actualHeader.Equals(header.Value, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Column {Column}: Header mismatch. Expected '{Expected}', found '{Actual}'",
                        header.Key, header.Value, actualHeader);
                }
            }

            if (errors.Any())
            {
                throw new InvalidOperationException(
                    $"Invalid Excel format. {string.Join("; ", errors)}. " +
                    $"Expected headers: {string.Join(", ", expectedHeaders.Values)}");
            }
        }

        /// <summary>
        /// Create a template Excel file for actual deductions import
        /// </summary>
        public byte[] CreateImportTemplate()
        {
            try
            {
                _logger.LogInformation("Creating import template for actual deductions");

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Actual Deductions");

                // Headers
                worksheet.Cells[1, 1].Value = "Member Number";
                worksheet.Cells[1, 2].Value = "Loan Number";
                worksheet.Cells[1, 3].Value = "Amount";
                worksheet.Cells[1, 4].Value = "Payroll Reference";
                worksheet.Cells[1, 5].Value = "Status";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Add sample data
                worksheet.Cells[2, 1].Value = "MEM001";
                worksheet.Cells[2, 2].Value = "LN-2024-001";
                worksheet.Cells[2, 3].Value = 50000;
                worksheet.Cells[2, 4].Value = "PAY-2024-11-001";
                worksheet.Cells[2, 5].Value = "SUCCESS";

                worksheet.Cells[3, 1].Value = "MEM002";
                worksheet.Cells[3, 2].Value = "LN-2024-002";
                worksheet.Cells[3, 3].Value = 75000;
                worksheet.Cells[3, 4].Value = "PAY-2024-11-002";
                worksheet.Cells[3, 5].Value = "SUCCESS";

                // Format amount column
                worksheet.Cells[2, 3, 3, 3].Style.Numberformat.Format = "₦#,##0.00";

                // Add instructions
                worksheet.Cells[5, 1].Value = "INSTRUCTIONS:";
                worksheet.Cells[5, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 1].Value = "1. Member Number: Required - Member's unique identifier";
                worksheet.Cells[7, 1].Value = "2. Loan Number: Required - Loan reference number";
                worksheet.Cells[8, 1].Value = "3. Amount: Required - Actual deduction amount (numeric)";
                worksheet.Cells[9, 1].Value = "4. Payroll Reference: Optional - Payroll system reference";
                worksheet.Cells[10, 1].Value = "5. Status: Optional - SUCCESS, FAILED, PENDING, or PARTIAL (default: SUCCESS)";

                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating import template");
                throw;
            }
        }
    }
}
