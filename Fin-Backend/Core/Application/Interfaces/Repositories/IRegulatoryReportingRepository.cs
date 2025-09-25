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
        Task<RegulatoryReportTemplate> GetTemplateByIdAsync(Guid id);
        Task<RegulatoryReportTemplate> GetTemplateWithSectionsAndFieldsAsync(Guid id);
        Task<IEnumerable<RegulatoryReportTemplate>> GetAllTemplatesAsync();
        Task<IEnumerable<RegulatoryReportTemplate>> GetTemplatesByRegulatoryBodyAsync(RegulatoryBody regulatoryBody);
        Task<bool> DeleteTemplateAsync(Guid id);

        // Section Management
        Task<RegulatoryReportSection> AddSectionAsync(RegulatoryReportSection section);
        Task<RegulatoryReportSection> UpdateSectionAsync(RegulatoryReportSection section);
        Task<bool> DeleteSectionAsync(Guid id);
        Task<RegulatoryReportSection> GetSectionByIdAsync(Guid id);
        Task<IEnumerable<RegulatoryReportSection>> GetSectionsByTemplateIdAsync(Guid templateId);

        // Field Management
        Task<RegulatoryReportField> AddFieldAsync(RegulatoryReportField field);
        Task<RegulatoryReportField> UpdateFieldAsync(RegulatoryReportField field);
        Task<bool> DeleteFieldAsync(Guid id);
        Task<RegulatoryReportField> GetFieldByIdAsync(Guid id);
        Task<IEnumerable<RegulatoryReportField>> GetFieldsBySectionIdAsync(Guid sectionId);

        // Submission Management
        Task<RegulatoryReportSubmission> AddSubmissionAsync(RegulatoryReportSubmission submission);
        Task<RegulatoryReportSubmission> UpdateSubmissionAsync(RegulatoryReportSubmission submission);
        Task<RegulatoryReportSubmission> GetSubmissionByIdAsync(Guid id);
        Task<RegulatoryReportSubmission> GetSubmissionWithDataAsync(Guid id);
        Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByTemplateIdAsync(Guid templateId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByStatusAsync(SubmissionStatus status);

        // Report Data Management
        Task<RegulatoryReportData> AddReportDataAsync(RegulatoryReportData data);
        Task<RegulatoryReportData> UpdateReportDataAsync(RegulatoryReportData data);
        Task<IEnumerable<RegulatoryReportData>> GetReportDataBySubmissionIdAsync(Guid submissionId);
        Task<RegulatoryReportData> GetReportDataBySubmissionAndFieldIdAsync(Guid submissionId, Guid fieldId);
        Task<bool> DeleteReportDataAsync(Guid id);
        Task<bool> DeleteAllReportDataForSubmissionAsync(Guid submissionId);

        // Validation Management
        Task<RegulatoryReportValidation> AddValidationAsync(RegulatoryReportValidation validation);
        Task<RegulatoryReportValidation> UpdateValidationAsync(RegulatoryReportValidation validation);
        Task<IEnumerable<RegulatoryReportValidation>> GetValidationsBySubmissionIdAsync(Guid submissionId);
        Task<bool> DeleteValidationAsync(Guid id);
        Task<bool> DeleteAllValidationsForSubmissionAsync(Guid submissionId);

        // Schedule Management
        Task<RegulatoryReportSchedule> AddScheduleAsync(RegulatoryReportSchedule schedule);
        Task<RegulatoryReportSchedule> UpdateScheduleAsync(RegulatoryReportSchedule schedule);
        Task<RegulatoryReportSchedule> GetScheduleByTemplateIdAsync(Guid templateId);
        Task<IEnumerable<RegulatoryReportSchedule>> GetAllSchedulesAsync();
        Task<IEnumerable<RegulatoryReportSchedule>> GetSchedulesDueByDateAsync(DateTime dueDate);
        Task<bool> DeleteScheduleAsync(Guid id);
    }
}
