using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace FinTech.Core.Application.Services.Regulatory
{
    public class RegulatoryReportingService : IRegulatoryReportingService
    {
        private readonly IRegulatoryReportingRepository _repository;
        private readonly ILogger<RegulatoryReportingService> _logger;

        public RegulatoryReportingService(IRegulatoryReportingRepository repository, ILogger<RegulatoryReportingService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        #region Template Management

        public async Task<RegulatoryReportTemplate> CreateTemplateAsync(RegulatoryReportTemplate template)
        {
            try
            {
                _logger.LogInformation("Creating new regulatory report template: {TemplateName}", template.TemplateName);
                return await _repository.AddTemplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating regulatory report template: {TemplateName}", template.TemplateName);
                throw;
            }
        }

        public async Task<RegulatoryReportTemplate> UpdateTemplateAsync(RegulatoryReportTemplate template)
        {
            try
            {
                _logger.LogInformation("Updating regulatory report template: {TemplateId}", template.Id);
                return await _repository.UpdateTemplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regulatory report template: {TemplateId}", template.Id);
                throw;
            }
        }

        public async Task<RegulatoryReportTemplate> GetTemplateByIdAsync(string id)
        {
            return await _repository.GetTemplateByIdAsync(id);
        }

        public async Task<IEnumerable<RegulatoryReportTemplate>> GetAllTemplatesAsync()
        {
            return await _repository.GetAllTemplatesAsync();
        }

        public async Task<IEnumerable<RegulatoryReportTemplate>> GetTemplatesByRegulatoryBodyAsync(RegulatoryBody regulatoryBody)
        {
            return await _repository.GetTemplatesByRegulatoryBodyAsync(regulatoryBody);
        }

        public async Task<bool> DeleteTemplateAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting regulatory report template: {TemplateId}", id);
                return await _repository.DeleteTemplateAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting regulatory report template: {TemplateId}", id);
                throw;
            }
        }

        #endregion

        #region Section Management

        public async Task<RegulatoryReportSection> CreateSectionAsync(RegulatoryReportSection section)
        {
            try
            {
                _logger.LogInformation("Creating new regulatory report section: {SectionName} for template {TemplateId}", 
                    section.SectionName, section.RegulatoryReportTemplateId);
                return await _repository.AddSectionAsync(section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating regulatory report section: {SectionName}", section.SectionName);
                throw;
            }
        }

        public async Task<RegulatoryReportSection> UpdateSectionAsync(RegulatoryReportSection section)
        {
            try
            {
                _logger.LogInformation("Updating regulatory report section: {SectionId}", section.Id);
                return await _repository.UpdateSectionAsync(section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regulatory report section: {SectionId}", section.Id);
                throw;
            }
        }

        public async Task<bool> DeleteSectionAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting regulatory report section: {SectionId}", id);
                return await _repository.DeleteSectionAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting regulatory report section: {SectionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RegulatoryReportSection>> GetSectionsByTemplateIdAsync(string templateId)
        {
            return await _repository.GetSectionsByTemplateIdAsync(templateId);
        }

        #endregion

        #region Field Management

        public async Task<RegulatoryReportField> CreateFieldAsync(RegulatoryReportField field)
        {
            try
            {
                _logger.LogInformation("Creating new regulatory report field: {FieldName} for section {SectionId}", 
                    field.FieldName, field.SectionId);
                return await _repository.AddFieldAsync(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating regulatory report field: {FieldName}", field.FieldName);
                throw;
            }
        }

        public async Task<RegulatoryReportField> UpdateFieldAsync(RegulatoryReportField field)
        {
            try
            {
                _logger.LogInformation("Updating regulatory report field: {FieldId}", field.Id);
                return await _repository.UpdateFieldAsync(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regulatory report field: {FieldId}", field.Id);
                throw;
            }
        }

        public async Task<bool> DeleteFieldAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting regulatory report field: {FieldId}", id);
                return await _repository.DeleteFieldAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting regulatory report field: {FieldId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RegulatoryReportField>> GetFieldsBySectionIdAsync(string sectionId)
        {
            return await _repository.GetFieldsBySectionIdAsync(sectionId);
        }

        #endregion

        #region Report Generation

        public async Task<RegulatoryReportSubmission> InitiateReportSubmissionAsync(string templateId, DateTime reportingPeriodStart, DateTime reportingPeriodEnd)
        {
            try
            {
                var template = await _repository.GetTemplateWithSectionsAndFieldsAsync(templateId);
                if (template == null)
                {
                    throw new Exception($"Template with ID {templateId} not found");
                }

                var submission = new RegulatoryReportSubmission
                {
                    RegulatoryReportTemplateId = templateId,
                    ReportingPeriodStart = reportingPeriodStart,
                    ReportingPeriodEnd = reportingPeriodEnd,
                    SubmissionDate = DateTime.UtcNow,
                    Status = SubmissionStatus.Draft,
                    SubmittedById = string.Empty, // Set this from the authenticated user
                    ReportData = new List<RegulatoryReportData>()
                };

                // Create empty data entries for all fields in the template
                foreach (var section in template.Sections)
                {
                    foreach (var field in section.Fields)
                    {
                        var reportData = new RegulatoryReportData
                        {
                            FieldId = field.Id,
                            IsCalculated = !string.IsNullOrEmpty(field.Formula),
                            HasException = false
                        };

                        // If there's a default value, use it
                        if (!string.IsNullOrEmpty(field.DefaultValue))
                        {
                            reportData.Value = field.DefaultValue;
                        }

                        ((List<RegulatoryReportData>)submission.ReportData).Add(reportData);
                    }
                }

                _logger.LogInformation("Initiating regulatory report submission for template: {TemplateId}, period: {Start} to {End}",
                    templateId, reportingPeriodStart, reportingPeriodEnd);

                return await _repository.AddSubmissionAsync(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating regulatory report submission for template: {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<RegulatoryReportSubmission> PopulateReportDataAsync(string submissionId)
        {
            try
            {
                var submission = await _repository.GetSubmissionWithDataAsync(submissionId);
                if (submission == null)
                {
                    throw new Exception($"Submission with ID {submissionId} not found");
                }

                // Process each field's data
                foreach (var data in submission.ReportData)
                {
                    var field = data.Field;

                    // Skip if data already exists and wasn't calculated
                    if (!string.IsNullOrEmpty(data.Value) && !data.IsCalculated)
                    {
                        continue;
                    }

                    // If there's a mapping query, execute it to get the data
                    if (!string.IsNullOrEmpty(field.MappingQuery))
                    {
                        data.Value = await ExecuteMappingQueryAsync(field.MappingQuery, submission.ReportingPeriodStart, submission.ReportingPeriodEnd);
                        data.IsCalculated = true;
                    }
                    // If there's a formula, calculate it
                    else if (!string.IsNullOrEmpty(field.Formula))
                    {
                        data.Value = await CalculateFormulaAsync(field.Formula, submission.ReportData);
                        data.IsCalculated = true;
                    }
                }

                _logger.LogInformation("Populated data for regulatory report submission: {SubmissionId}", submissionId);

                return await _repository.UpdateSubmissionAsync(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating data for regulatory report submission: {SubmissionId}", submissionId);
                throw;
            }
        }

        private async Task<string> ExecuteMappingQueryAsync(string mappingQuery, DateTime startDate, DateTime endDate)
        {
            // This is a placeholder for executing a query against the database
            // In a real implementation, this would execute the query and return the result
            
            // Mock implementation for now
            await Task.Delay(10); // Simulate async operation
            return "Query Result";
        }

        private async Task<string> CalculateFormulaAsync(string formula, IEnumerable<RegulatoryReportData> reportData)
        {
            // This is a placeholder for calculating a formula
            // In a real implementation, this would parse the formula and calculate it using the report data
            
            // Mock implementation for now
            await Task.Delay(10); // Simulate async operation
            return "Calculated Value";
        }

        public async Task<IEnumerable<RegulatoryReportValidation>> ValidateReportAsync(string submissionId)
        {
            try
            {
                var submission = await _repository.GetSubmissionWithDataAsync(submissionId);
                if (submission == null)
                {
                    throw new Exception($"Submission with ID {submissionId} not found");
                }

                // Clear any existing validations
                await _repository.DeleteAllValidationsForSubmissionAsync(submissionId);

                var validations = new List<RegulatoryReportValidation>();

                // Validate each field's data
                foreach (var data in submission.ReportData)
                {
                    var field = data.Field;

                    // Skip validation if the field is not required and value is empty
                    if (!field.IsRequired && string.IsNullOrEmpty(data.Value))
                    {
                        continue;
                    }

                    // Check if required field has a value
                    if (field.IsRequired && string.IsNullOrEmpty(data.Value))
                    {
                        var validation = new RegulatoryReportValidation
                        {
                            SubmissionId = submissionId,
                            FieldId = field.Id,
                            ErrorMessage = $"Required field '{field.FieldName}' has no value",
                            ErrorCode = "REQUIRED_FIELD",
                            Severity = ValidationSeverity.Error,
                            IsResolved = false
                        };
                        
                        validations.Add(validation);
                        await _repository.AddValidationAsync(validation);
                    }

                    // If there are validation rules, apply them
                    if (!string.IsNullOrEmpty(field.ValidationRules))
                    {
                        var fieldValidations = await ValidateFieldAsync(data.Value, field.ValidationRules, field.Id, submissionId);
                        validations.AddRange(fieldValidations);
                        
                        foreach (var validation in fieldValidations)
                        {
                            await _repository.AddValidationAsync(validation);
                        }
                    }
                }

                _logger.LogInformation("Validated regulatory report submission: {SubmissionId}, found {ValidationCount} issues",
                    submissionId, validations.Count);

                return validations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating regulatory report submission: {SubmissionId}", submissionId);
                throw;
            }
        }

        private async Task<IEnumerable<RegulatoryReportValidation>> ValidateFieldAsync(string value, string validationRules, string fieldId, string submissionId)
        {
            var validations = new List<RegulatoryReportValidation>();

            try
            {
                // Parse validation rules from JSON
                var rules = JsonConvert.DeserializeObject<Dictionary<string, object>>(validationRules);

                // Apply validation rules (simplified example)
                if (rules.ContainsKey("minLength") && int.TryParse(rules["minLength"].ToString(), out int minLength))
                {
                    if (value?.Length < minLength)
                    {
                        validations.Add(new RegulatoryReportValidation
                        {
                            SubmissionId = submissionId,
                            FieldId = fieldId,
                            ErrorMessage = $"Value must be at least {minLength} characters long",
                            ErrorCode = "MIN_LENGTH",
                            Severity = ValidationSeverity.Error,
                            IsResolved = false
                        });
                    }
                }

                if (rules.ContainsKey("maxLength") && int.TryParse(rules["maxLength"].ToString(), out int maxLength))
                {
                    if (value?.Length > maxLength)
                    {
                        validations.Add(new RegulatoryReportValidation
                        {
                            SubmissionId = submissionId,
                            FieldId = fieldId,
                            ErrorMessage = $"Value must not exceed {maxLength} characters",
                            ErrorCode = "MAX_LENGTH",
                            Severity = ValidationSeverity.Error,
                            IsResolved = false
                        });
                    }
                }

                // Add more validation rules as needed...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating field {FieldId} with rules: {Rules}", fieldId, validationRules);
                validations.Add(new RegulatoryReportValidation
                {
                    SubmissionId = submissionId,
                    FieldId = fieldId,
                    ErrorMessage = $"Error validating field: {ex.Message}",
                    ErrorCode = "VALIDATION_ERROR",
                    Severity = ValidationSeverity.Error,
                    IsResolved = false
                });
            }

            await Task.CompletedTask; // Just to make the method async
            return validations;
        }

        public async Task<string> GenerateReportFileAsync(string submissionId)
        {
            try
            {
                var submission = await _repository.GetSubmissionWithDataAsync(submissionId);
                if (submission == null)
                {
                    throw new Exception($"Submission with ID {submissionId} not found");
                }

                // Determine the file format to generate
                string fileFormat = submission.Template.FileFormat.ToLower();
                string fileName = $"{submission.Template.TemplateCode}_{submission.ReportingPeriodEnd:yyyyMMdd}";
                string filePath = Path.Combine("Regulatory_Reports", submission.Template.RegulatoryBody.ToString(), fileName);

                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Generate the file based on format
                switch (fileFormat)
                {
                    case "xml":
                        filePath = await GenerateXmlReportAsync(submission, filePath);
                        break;
                    case "excel":
                        filePath = await GenerateExcelReportAsync(submission, filePath);
                        break;
                    case "pdf":
                        filePath = await GeneratePdfReportAsync(submission, filePath);
                        break;
                    default:
                        filePath = await GenerateTextReportAsync(submission, filePath);
                        break;
                }

                // Update the submission with the file path
                submission.FilePath = filePath;
                await _repository.UpdateSubmissionAsync(submission);

                _logger.LogInformation("Generated report file for submission: {SubmissionId}, file: {FilePath}", submissionId, filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report file for submission: {SubmissionId}", submissionId);
                throw;
            }
        }

        private async Task<string> GenerateXmlReportAsync(RegulatoryReportSubmission submission, string filePath)
        {
            // This is a placeholder for generating an XML report
            filePath += ".xml";
            
            // In a real implementation, this would create an XML document
            // and write it to the file path
            
            await Task.Delay(10); // Simulate async operation
            return filePath;
        }

        private async Task<string> GenerateExcelReportAsync(RegulatoryReportSubmission submission, string filePath)
        {
            // This is a placeholder for generating an Excel report
            filePath += ".xlsx";
            
            // In a real implementation, this would create an Excel file
            // and write it to the file path
            
            await Task.Delay(10); // Simulate async operation
            return filePath;
        }

        private async Task<string> GeneratePdfReportAsync(RegulatoryReportSubmission submission, string filePath)
        {
            // This is a placeholder for generating a PDF report
            filePath += ".pdf";
            
            // In a real implementation, this would create a PDF document
            // and write it to the file path
            
            await Task.Delay(10); // Simulate async operation
            return filePath;
        }

        private async Task<string> GenerateTextReportAsync(RegulatoryReportSubmission submission, string filePath)
        {
            // This is a placeholder for generating a text report
            filePath += ".txt";
            
            // In a real implementation, this would create a text file
            // and write it to the file path
            
            await Task.Delay(10); // Simulate async operation
            return filePath;
        }

        public async Task<RegulatoryReportSubmission> SubmitReportAsync(string submissionId)
        {
            try
            {
                var submission = await _repository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    throw new Exception($"Submission with ID {submissionId} not found");
                }

                // Check if report has been validated and generated
                if (string.IsNullOrEmpty(submission.FilePath))
                {
                    throw new Exception("Report file must be generated before submission");
                }

                // Check for validation errors
                var validations = await _repository.GetValidationsBySubmissionIdAsync(submissionId);
                var errors = validations.Where(v => v.Severity == ValidationSeverity.Error && !v.IsResolved).ToList();
                
                if (errors.Any())
                {
                    throw new Exception($"Cannot submit report with {errors.Count} unresolved validation errors");
                }

                // Update submission status
                submission.Status = SubmissionStatus.Submitted;
                submission.SubmissionDate = DateTime.UtcNow;

                _logger.LogInformation("Submitted regulatory report: {SubmissionId}", submissionId);

                return await _repository.UpdateSubmissionAsync(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting regulatory report: {SubmissionId}", submissionId);
                throw;
            }
        }

        #endregion

        #region Report Submission Management

        public async Task<RegulatoryReportSubmission> GetSubmissionByIdAsync(string id)
        {
            return await _repository.GetSubmissionByIdAsync(id);
        }

        public async Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByTemplateIdAsync(string templateId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _repository.GetSubmissionsByTemplateIdAsync(templateId, fromDate, toDate);
        }

        public async Task<IEnumerable<RegulatoryReportSubmission>> GetPendingSubmissionsAsync()
        {
            return await _repository.GetSubmissionsByStatusAsync(SubmissionStatus.PendingApproval);
        }

        public async Task<RegulatoryReportSubmission> ApproveSubmissionAsync(string submissionId, string approverId, string comments)
        {
            try
            {
                var submission = await _repository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    throw new Exception($"Submission with ID {submissionId} not found");
                }

                if (submission.Status != SubmissionStatus.PendingApproval)
                {
                    throw new Exception($"Submission is not in PendingApproval status. Current status: {submission.Status}");
                }

                submission.Status = SubmissionStatus.Approved;
                submission.ApprovalDate = DateTime.UtcNow;
                submission.ApprovedById = approverId;
                submission.Comments = comments;

                _logger.LogInformation("Approved regulatory report submission: {SubmissionId}", submissionId);

                return await _repository.UpdateSubmissionAsync(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving regulatory report submission: {SubmissionId}", submissionId);
                throw;
            }
        }

        public async Task<RegulatoryReportSubmission> RejectSubmissionAsync(string submissionId, string reason)
        {
            try
            {
                var submission = await _repository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    throw new Exception($"Submission with ID {submissionId} not found");
                }

                submission.Status = SubmissionStatus.Rejected;
                submission.Comments = reason;

                _logger.LogInformation("Rejected regulatory report submission: {SubmissionId}", submissionId);

                return await _repository.UpdateSubmissionAsync(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting regulatory report submission: {SubmissionId}", submissionId);
                throw;
            }
        }

        #endregion

        #region Schedule Management

        public async Task<RegulatoryReportSchedule> CreateScheduleAsync(RegulatoryReportSchedule schedule)
        {
            try
            {
                _logger.LogInformation("Creating new regulatory report schedule for template: {TemplateId}", schedule.RegulatoryReportTemplateId);
                return await _repository.AddScheduleAsync(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating regulatory report schedule for template: {TemplateId}", schedule.ReportTemplateId);
                throw;
            }
        }

        public async Task<RegulatoryReportSchedule> UpdateScheduleAsync(RegulatoryReportSchedule schedule)
        {
            try
            {
                _logger.LogInformation("Updating regulatory report schedule: {ScheduleId}", schedule.Id);
                return await _repository.UpdateScheduleAsync(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating regulatory report schedule: {ScheduleId}", schedule.Id);
                throw;
            }
        }

        public async Task<bool> DeleteScheduleAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting regulatory report schedule: {ScheduleId}", id);
                return await _repository.DeleteScheduleAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting regulatory report schedule: {ScheduleId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<RegulatoryReportSchedule>> GetUpcomingSchedulesAsync(int daysAhead)
        {
            var dueDate = DateTime.UtcNow.AddDays(daysAhead);
            return await _repository.GetSchedulesDueByDateAsync(dueDate);
        }

        #endregion

        #region Regulatory Body Specific Reports

        public async Task<byte[]> GenerateCBNMonthlyReturnAsync(DateTime reportingPeriod)
        {
            try
            {
                _logger.LogInformation("Generating CBN Monthly Return for period: {Period}", reportingPeriod);

                // Find the appropriate template
                var templates = await _repository.GetTemplatesByRegulatoryBodyAsync(RegulatoryBody.CBN);
                var template = templates.FirstOrDefault(t => 
                    t.Frequency == ReportingFrequency.Monthly && 
                    t.TemplateCode.Contains("MONTHLY"));

                if (template == null)
                {
                    throw new Exception("CBN Monthly Return template not found");
                }

                // Initialize the reporting period
                var startDate = new DateTime(reportingPeriod.Year, reportingPeriod.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Create and populate a submission
                var submission = await InitiateReportSubmissionAsync(template.Id, startDate, endDate);
                await PopulateReportDataAsync(submission.Id);
                await ValidateReportAsync(submission.Id);
                var filePath = await GenerateReportFileAsync(submission.Id);

                // Read the file and return its contents
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CBN Monthly Return for period: {Period}", reportingPeriod);
                throw;
            }
        }

        public async Task<byte[]> GenerateCBNQuarterlyReturnAsync(DateTime reportingPeriod)
        {
            try
            {
                _logger.LogInformation("Generating CBN Quarterly Return for period: {Period}", reportingPeriod);

                // Calculate quarter dates
                int quarter = (reportingPeriod.Month - 1) / 3 + 1;
                var startDate = new DateTime(reportingPeriod.Year, (quarter - 1) * 3 + 1, 1);
                var endDate = startDate.AddMonths(3).AddDays(-1);

                // Find the appropriate template
                var templates = await _repository.GetTemplatesByRegulatoryBodyAsync(RegulatoryBody.CBN);
                var template = templates.FirstOrDefault(t => 
                    t.Frequency == ReportingFrequency.Quarterly && 
                    t.TemplateCode.Contains("QUARTERLY"));

                if (template == null)
                {
                    throw new Exception("CBN Quarterly Return template not found");
                }

                // Create and populate a submission
                var submission = await InitiateReportSubmissionAsync(template.Id, startDate, endDate);
                await PopulateReportDataAsync(submission.Id);
                await ValidateReportAsync(submission.Id);
                var filePath = await GenerateReportFileAsync(submission.Id);

                // Read the file and return its contents
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CBN Quarterly Return for period: {Period}", reportingPeriod);
                throw;
            }
        }

        public async Task<byte[]> GenerateNDICPremiumReturnAsync(DateTime reportingPeriod)
        {
            try
            {
                _logger.LogInformation("Generating NDIC Premium Return for period: {Period}", reportingPeriod);

                // Find the appropriate template
                var templates = await _repository.GetTemplatesByRegulatoryBodyAsync(RegulatoryBody.NDIC);
                var template = templates.FirstOrDefault(t => t.TemplateCode.Contains("PREMIUM"));

                if (template == null)
                {
                    throw new Exception("NDIC Premium Return template not found");
                }

                // Initialize the reporting period (quarterly)
                int quarter = (reportingPeriod.Month - 1) / 3 + 1;
                var startDate = new DateTime(reportingPeriod.Year, (quarter - 1) * 3 + 1, 1);
                var endDate = startDate.AddMonths(3).AddDays(-1);

                // Create and populate a submission
                var submission = await InitiateReportSubmissionAsync(template.Id, startDate, endDate);
                await PopulateReportDataAsync(submission.Id);
                await ValidateReportAsync(submission.Id);
                var filePath = await GenerateReportFileAsync(submission.Id);

                // Read the file and return its contents
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating NDIC Premium Return for period: {Period}", reportingPeriod);
                throw;
            }
        }

        public async Task<byte[]> GenerateNDICDepositReturnsAsync(DateTime reportingPeriod)
        {
            try
            {
                _logger.LogInformation("Generating NDIC Deposit Returns for period: {Period}", reportingPeriod);

                // Find the appropriate template
                var templates = await _repository.GetTemplatesByRegulatoryBodyAsync(RegulatoryBody.NDIC);
                var template = templates.FirstOrDefault(t => t.TemplateCode.Contains("DEPOSIT"));

                if (template == null)
                {
                    throw new Exception("NDIC Deposit Return template not found");
                }

                // Initialize the reporting period (monthly)
                var startDate = new DateTime(reportingPeriod.Year, reportingPeriod.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Create and populate a submission
                var submission = await InitiateReportSubmissionAsync(template.Id, startDate, endDate);
                await PopulateReportDataAsync(submission.Id);
                await ValidateReportAsync(submission.Id);
                var filePath = await GenerateReportFileAsync(submission.Id);

                // Read the file and return its contents
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating NDIC Deposit Returns for period: {Period}", reportingPeriod);
                throw;
            }
        }

        public async Task<byte[]> GenerateFIRSMonthlyTaxReturnAsync(DateTime reportingPeriod)
        {
            try
            {
                _logger.LogInformation("Generating FIRS Monthly Tax Return for period: {Period}", reportingPeriod);

                // Find the appropriate template
                var templates = await _repository.GetTemplatesByRegulatoryBodyAsync(RegulatoryBody.FIRS);
                var template = templates.FirstOrDefault(t => 
                    t.Frequency == ReportingFrequency.Monthly && 
                    t.TemplateCode.Contains("TAX"));

                if (template == null)
                {
                    throw new Exception("FIRS Monthly Tax Return template not found");
                }

                // Initialize the reporting period
                var startDate = new DateTime(reportingPeriod.Year, reportingPeriod.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Create and populate a submission
                var submission = await InitiateReportSubmissionAsync(template.Id, startDate, endDate);
                await PopulateReportDataAsync(submission.Id);
                await ValidateReportAsync(submission.Id);
                var filePath = await GenerateReportFileAsync(submission.Id);

                // Read the file and return its contents
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating FIRS Monthly Tax Return for period: {Period}", reportingPeriod);
                throw;
            }
        }

        public async Task<byte[]> GenerateFIRSAnnualReturnAsync(int year)
        {
            try
            {
                _logger.LogInformation("Generating FIRS Annual Return for year: {Year}", year);

                // Find the appropriate template
                var templates = await _repository.GetTemplatesByRegulatoryBodyAsync(RegulatoryBody.FIRS);
                var template = templates.FirstOrDefault(t => 
                    t.Frequency == ReportingFrequency.Annually && 
                    t.TemplateCode.Contains("ANNUAL"));

                if (template == null)
                {
                    throw new Exception("FIRS Annual Return template not found");
                }

                // Initialize the reporting period
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31);

                // Create and populate a submission
                var submission = await InitiateReportSubmissionAsync(template.Id, startDate, endDate);
                await PopulateReportDataAsync(submission.Id);
                await ValidateReportAsync(submission.Id);
                var filePath = await GenerateReportFileAsync(submission.Id);

                // Read the file and return its contents
                return await File.ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating FIRS Annual Return for year: {Year}", year);
                throw;
            }
        }

        #endregion
    }
}
