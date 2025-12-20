using FinTech.Core.Domain.Entities.RegulatoryReporting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    public interface IRegulatoryReportingRepository
    {
        // Template Management
        Task<RegulatoryReportTemplate> AddTemplateAsync(RegulatoryReportTemplate template);
        Task<RegulatoryReportTemplate> UpdateTemplateAsync(RegulatoryReportTemplate template);
        Task<RegulatoryReportTemplate> GetTemplateByIdAsync(string id);
        Task<RegulatoryReportTemplate> GetTemplateWithSectionsAndFieldsAsync(string id);
        Task<IEnumerable<RegulatoryReportTemplate>> GetAllTemplatesAsync();
        Task<IEnumerable<RegulatoryReportTemplate>> GetTemplatesByRegulatoryBodyAsync(RegulatoryBody regulatoryBody);
        Task<bool> DeleteTemplateAsync(string id);

        // Section Management
        Task<RegulatoryReportSection> AddSectionAsync(RegulatoryReportSection section);
        Task<RegulatoryReportSection> UpdateSectionAsync(RegulatoryReportSection section);
        Task<bool> DeleteSectionAsync(string id);
        Task<RegulatoryReportSection> GetSectionByIdAsync(string id);
        Task<IEnumerable<RegulatoryReportSection>> GetSectionsByTemplateIdAsync(string templateId);

        // Field Management
        Task<RegulatoryReportField> AddFieldAsync(RegulatoryReportField field);
        Task<RegulatoryReportField> UpdateFieldAsync(RegulatoryReportField field);
        Task<bool> DeleteFieldAsync(string id);
        Task<RegulatoryReportField> GetFieldByIdAsync(string id);
        Task<IEnumerable<RegulatoryReportField>> GetFieldsBySectionIdAsync(string sectionId);

        // Submission Management
        Task<RegulatoryReportSubmission> AddSubmissionAsync(RegulatoryReportSubmission submission);
        Task<RegulatoryReportSubmission> UpdateSubmissionAsync(RegulatoryReportSubmission submission);
        Task<RegulatoryReportSubmission> GetSubmissionByIdAsync(string id);
        Task<RegulatoryReportSubmission> GetSubmissionWithDataAsync(string id);
        Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByTemplateIdAsync(string templateId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByStatusAsync(SubmissionStatus status);

        // Report Data Management
        Task<RegulatoryReportData> AddReportDataAsync(RegulatoryReportData data);
        Task<RegulatoryReportData> UpdateReportDataAsync(RegulatoryReportData data);
        Task<IEnumerable<RegulatoryReportData>> GetReportDataBySubmissionIdAsync(string submissionId);
        Task<RegulatoryReportData> GetReportDataBySubmissionAndFieldIdAsync(string submissionId, string fieldId);
        Task<bool> DeleteReportDataAsync(string id);
        Task<bool> DeleteAllReportDataForSubmissionAsync(string submissionId);

        // Validation Management
        Task<RegulatoryReportValidation> AddValidationAsync(RegulatoryReportValidation validation);
        Task<RegulatoryReportValidation> UpdateValidationAsync(RegulatoryReportValidation validation);
        Task<IEnumerable<RegulatoryReportValidation>> GetValidationsBySubmissionIdAsync(string submissionId);
        Task<bool> DeleteValidationAsync(string id);
        Task<bool> DeleteAllValidationsForSubmissionAsync(string submissionId);

        // Schedule Management
        Task<RegulatoryReportSchedule> AddScheduleAsync(RegulatoryReportSchedule schedule);
        Task<RegulatoryReportSchedule> UpdateScheduleAsync(RegulatoryReportSchedule schedule);
        Task<RegulatoryReportSchedule> GetScheduleByTemplateIdAsync(string templateId);
        Task<IEnumerable<RegulatoryReportSchedule>> GetAllSchedulesAsync();
        Task<IEnumerable<RegulatoryReportSchedule>> GetSchedulesDueByDateAsync(DateTime dueDate);
        Task<bool> DeleteScheduleAsync(string id);
    }
}
