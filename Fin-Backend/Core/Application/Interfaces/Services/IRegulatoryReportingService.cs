using FinTech.Core.Domain.Entities.RegulatoryReporting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IRegulatoryReportingService
    {
        // Template Management
        Task<RegulatoryReportTemplate> CreateTemplateAsync(RegulatoryReportTemplate template);
        Task<RegulatoryReportTemplate> UpdateTemplateAsync(RegulatoryReportTemplate template);
        Task<RegulatoryReportTemplate> GetTemplateByIdAsync(string id);
        Task<IEnumerable<RegulatoryReportTemplate>> GetAllTemplatesAsync();
        Task<IEnumerable<RegulatoryReportTemplate>> GetTemplatesByRegulatoryBodyAsync(RegulatoryBody regulatoryBody);
        Task<bool> DeleteTemplateAsync(string id);
        
        // Section Management
        Task<RegulatoryReportSection> CreateSectionAsync(RegulatoryReportSection section);
        Task<RegulatoryReportSection> UpdateSectionAsync(RegulatoryReportSection section);
        Task<bool> DeleteSectionAsync(string id);
        Task<IEnumerable<RegulatoryReportSection>> GetSectionsByTemplateIdAsync(string templateId);
        
        // Field Management
        Task<RegulatoryReportField> CreateFieldAsync(RegulatoryReportField field);
        Task<RegulatoryReportField> UpdateFieldAsync(RegulatoryReportField field);
        Task<bool> DeleteFieldAsync(string id);
        Task<IEnumerable<RegulatoryReportField>> GetFieldsBySectionIdAsync(string sectionId);
        
        // Report Generation
        Task<RegulatoryReportSubmission> InitiateReportSubmissionAsync(string templateId, DateTime reportingPeriodStart, DateTime reportingPeriodEnd);
        Task<RegulatoryReportSubmission> PopulateReportDataAsync(string submissionId);
        Task<IEnumerable<RegulatoryReportValidation>> ValidateReportAsync(string submissionId);
        Task<string> GenerateReportFileAsync(string submissionId);
        Task<RegulatoryReportSubmission> SubmitReportAsync(string submissionId);
        
        // Report Submission Management
        Task<RegulatoryReportSubmission> GetSubmissionByIdAsync(string id);
        Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByTemplateIdAsync(string templateId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<RegulatoryReportSubmission>> GetPendingSubmissionsAsync();
        Task<RegulatoryReportSubmission> ApproveSubmissionAsync(string submissionId, Guid approverId, string comments);
        Task<RegulatoryReportSubmission> RejectSubmissionAsync(string submissionId, string reason);
        
        // Schedule Management
        Task<RegulatoryReportSchedule> CreateScheduleAsync(RegulatoryReportSchedule schedule);
        Task<RegulatoryReportSchedule> UpdateScheduleAsync(RegulatoryReportSchedule schedule);
        Task<bool> DeleteScheduleAsync(string id);
        Task<IEnumerable<RegulatoryReportSchedule>> GetUpcomingSchedulesAsync(int daysAhead);
        
        // CBN Specific Reports
        Task<byte[]> GenerateCBNMonthlyReturnAsync(DateTime reportingPeriod);
        Task<byte[]> GenerateCBNQuarterlyReturnAsync(DateTime reportingPeriod);
        
        // NDIC Specific Reports
        Task<byte[]> GenerateNDICPremiumReturnAsync(DateTime reportingPeriod);
        Task<byte[]> GenerateNDICDepositReturnsAsync(DateTime reportingPeriod);
        
        // FIRS Specific Reports
        Task<byte[]> GenerateFIRSMonthlyTaxReturnAsync(DateTime reportingPeriod);
        Task<byte[]> GenerateFIRSAnnualReturnAsync(int year);
    }
}
