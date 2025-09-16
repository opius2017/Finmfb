using FinTech.Domain.Entities.RegulatoryReporting;
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
        Task<RegulatoryReportTemplate> GetTemplateByIdAsync(Guid id);
        Task<IEnumerable<RegulatoryReportTemplate>> GetAllTemplatesAsync();
        Task<IEnumerable<RegulatoryReportTemplate>> GetTemplatesByRegulatoryBodyAsync(RegulatoryBody regulatoryBody);
        Task<bool> DeleteTemplateAsync(Guid id);
        
        // Section Management
        Task<RegulatoryReportSection> CreateSectionAsync(RegulatoryReportSection section);
        Task<RegulatoryReportSection> UpdateSectionAsync(RegulatoryReportSection section);
        Task<bool> DeleteSectionAsync(Guid id);
        Task<IEnumerable<RegulatoryReportSection>> GetSectionsByTemplateIdAsync(Guid templateId);
        
        // Field Management
        Task<RegulatoryReportField> CreateFieldAsync(RegulatoryReportField field);
        Task<RegulatoryReportField> UpdateFieldAsync(RegulatoryReportField field);
        Task<bool> DeleteFieldAsync(Guid id);
        Task<IEnumerable<RegulatoryReportField>> GetFieldsBySectionIdAsync(Guid sectionId);
        
        // Report Generation
        Task<RegulatoryReportSubmission> InitiateReportSubmissionAsync(Guid templateId, DateTime reportingPeriodStart, DateTime reportingPeriodEnd);
        Task<RegulatoryReportSubmission> PopulateReportDataAsync(Guid submissionId);
        Task<IEnumerable<RegulatoryReportValidation>> ValidateReportAsync(Guid submissionId);
        Task<string> GenerateReportFileAsync(Guid submissionId);
        Task<RegulatoryReportSubmission> SubmitReportAsync(Guid submissionId);
        
        // Report Submission Management
        Task<RegulatoryReportSubmission> GetSubmissionByIdAsync(Guid id);
        Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByTemplateIdAsync(Guid templateId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<RegulatoryReportSubmission>> GetPendingSubmissionsAsync();
        Task<RegulatoryReportSubmission> ApproveSubmissionAsync(Guid submissionId, Guid approverId, string comments);
        Task<RegulatoryReportSubmission> RejectSubmissionAsync(Guid submissionId, string reason);
        
        // Schedule Management
        Task<RegulatoryReportSchedule> CreateScheduleAsync(RegulatoryReportSchedule schedule);
        Task<RegulatoryReportSchedule> UpdateScheduleAsync(RegulatoryReportSchedule schedule);
        Task<bool> DeleteScheduleAsync(Guid id);
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