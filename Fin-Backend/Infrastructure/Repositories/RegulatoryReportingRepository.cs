using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Data
{
    public class RegulatoryReportingRepository : IRegulatoryReportingRepository
    {
        private readonly IApplicationDbContext _context;

        public RegulatoryReportingRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        #region Template Management

        public async Task<RegulatoryReportTemplate> AddTemplateAsync(RegulatoryReportTemplate template)
        {
            _context.RegulatoryReportTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<RegulatoryReportTemplate> UpdateTemplateAsync(RegulatoryReportTemplate template)
        {
            _context.RegulatoryReportTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<RegulatoryReportTemplate> GetTemplateByIdAsync(string id)
        {
            return await _context.RegulatoryReportTemplates.FindAsync(id);
        }

        public async Task<RegulatoryReportTemplate> GetTemplateWithSectionsAndFieldsAsync(string id)
        {
            return await _context.RegulatoryReportTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Fields)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<RegulatoryReportTemplate>> GetAllTemplatesAsync()
        {
            return await _context.RegulatoryReportTemplates.ToListAsync();
        }

        public async Task<IEnumerable<RegulatoryReportTemplate>> GetTemplatesByRegulatoryBodyAsync(RegulatoryBody regulatoryBody)
        {
            return await _context.RegulatoryReportTemplates
                .Where(t => t.RegulatoryBody == regulatoryBody)
                .ToListAsync();
        }

        public async Task<bool> DeleteTemplateAsync(string id)
        {
            var template = await _context.RegulatoryReportTemplates.FindAsync(id);
            if (template == null)
                return false;

            _context.RegulatoryReportTemplates.Remove(template);
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Section Management

        public async Task<RegulatoryReportSection> AddSectionAsync(RegulatoryReportSection section)
        {
            _context.RegulatoryReportSections.Add(section);
            await _context.SaveChangesAsync();
            return section;
        }

        public async Task<RegulatoryReportSection> UpdateSectionAsync(RegulatoryReportSection section)
        {
            _context.RegulatoryReportSections.Update(section);
            await _context.SaveChangesAsync();
            return section;
        }

        public async Task<bool> DeleteSectionAsync(string id)
        {
            var section = await _context.RegulatoryReportSections.FindAsync(id);
            if (section == null)
                return false;

            _context.RegulatoryReportSections.Remove(section);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<RegulatoryReportSection> GetSectionByIdAsync(string id)
        {
            return await _context.RegulatoryReportSections.FindAsync(id);
        }

        public async Task<IEnumerable<RegulatoryReportSection>> GetSectionsByTemplateIdAsync(string templateId)
        {
            return await _context.RegulatoryReportSections
                .Where(s => s.RegulatoryReportTemplateId == templateId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Field Management

        public async Task<RegulatoryReportField> AddFieldAsync(RegulatoryReportField field)
        {
            _context.RegulatoryReportFields.Add(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task<RegulatoryReportField> UpdateFieldAsync(RegulatoryReportField field)
        {
            _context.RegulatoryReportFields.Update(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task<bool> DeleteFieldAsync(string id)
        {
            var field = await _context.RegulatoryReportFields.FindAsync(id);
            if (field == null)
                return false;

            _context.RegulatoryReportFields.Remove(field);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<RegulatoryReportField> GetFieldByIdAsync(string id)
        {
            return await _context.RegulatoryReportFields.FindAsync(id);
        }

        public async Task<IEnumerable<RegulatoryReportField>> GetFieldsBySectionIdAsync(string sectionId)
        {
            return await _context.RegulatoryReportFields
                .Where(f => f.SectionId == sectionId)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Submission Management

        public async Task<RegulatoryReportSubmission> AddSubmissionAsync(RegulatoryReportSubmission submission)
        {
            _context.RegulatoryReportSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<RegulatoryReportSubmission> UpdateSubmissionAsync(RegulatoryReportSubmission submission)
        {
            _context.RegulatoryReportSubmissions.Update(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<RegulatoryReportSubmission> GetSubmissionByIdAsync(string id)
        {
            return await _context.RegulatoryReportSubmissions.FindAsync(id);
        }

        public async Task<RegulatoryReportSubmission> GetSubmissionWithDataAsync(string id)
        {
            return await _context.RegulatoryReportSubmissions
                .Include(s => s.Template)
                .Include(s => s.ReportData)
                    .ThenInclude(d => d.Field)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByTemplateIdAsync(string templateId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.RegulatoryReportSubmissions
                .Where(s => s.RegulatoryReportTemplateId == templateId);

            if (fromDate.HasValue)
                query = query.Where(s => s.ReportingPeriodStart >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(s => s.ReportingPeriodEnd <= toDate.Value);

            return await query
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegulatoryReportSubmission>> GetSubmissionsByStatusAsync(SubmissionStatus status)
        {
            return await _context.RegulatoryReportSubmissions
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }

        #endregion

        #region Report Data Management

        public async Task<RegulatoryReportData> AddReportDataAsync(RegulatoryReportData data)
        {
            _context.RegulatoryReportData.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<RegulatoryReportData> UpdateReportDataAsync(RegulatoryReportData data)
        {
            _context.RegulatoryReportData.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<IEnumerable<RegulatoryReportData>> GetReportDataBySubmissionIdAsync(string submissionId)
        {
            return await _context.RegulatoryReportData
                .Where(d => d.SubmissionId == submissionId)
                .Include(d => d.Field)
                .ToListAsync();
        }

        public async Task<RegulatoryReportData> GetReportDataBySubmissionAndFieldIdAsync(string submissionId, string fieldId)
        {
            return await _context.RegulatoryReportData
                .FirstOrDefaultAsync(d => d.SubmissionId == submissionId && d.FieldId == fieldId);
        }

        public async Task<bool> DeleteReportDataAsync(string id)
        {
            var data = await _context.RegulatoryReportData.FindAsync(id);
            if (data == null)
                return false;

            _context.RegulatoryReportData.Remove(data);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAllReportDataForSubmissionAsync(string submissionId)
        {
            var dataItems = await _context.RegulatoryReportData
                .Where(d => d.SubmissionId == submissionId)
                .ToListAsync();

            if (!dataItems.Any())
                return false;

            _context.RegulatoryReportData.RemoveRange(dataItems);
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Validation Management

        public async Task<RegulatoryReportValidation> AddValidationAsync(RegulatoryReportValidation validation)
        {
            _context.RegulatoryReportValidations.Add(validation);
            await _context.SaveChangesAsync();
            return validation;
        }

        public async Task<RegulatoryReportValidation> UpdateValidationAsync(RegulatoryReportValidation validation)
        {
            _context.RegulatoryReportValidations.Update(validation);
            await _context.SaveChangesAsync();
            return validation;
        }

        public async Task<IEnumerable<RegulatoryReportValidation>> GetValidationsBySubmissionIdAsync(string submissionId)
        {
            return await _context.RegulatoryReportValidations
                .Where(v => v.SubmissionId == submissionId)
                .Include(v => v.Field)
                .ToListAsync();
        }

        public async Task<bool> DeleteValidationAsync(string id)
        {
            var validation = await _context.RegulatoryReportValidations.FindAsync(id);
            if (validation == null)
                return false;

            _context.RegulatoryReportValidations.Remove(validation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAllValidationsForSubmissionAsync(string submissionId)
        {
            var validations = await _context.RegulatoryReportValidations
                .Where(v => v.SubmissionId == submissionId)
                .ToListAsync();

            if (!validations.Any())
                return false;

            _context.RegulatoryReportValidations.RemoveRange(validations);
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion

        #region Schedule Management

        public async Task<RegulatoryReportSchedule> AddScheduleAsync(RegulatoryReportSchedule schedule)
        {
            _context.RegulatoryReportSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<RegulatoryReportSchedule> UpdateScheduleAsync(RegulatoryReportSchedule schedule)
        {
            _context.RegulatoryReportSchedules.Update(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<RegulatoryReportSchedule> GetScheduleByTemplateIdAsync(string templateId)
        {
            return await _context.RegulatoryReportSchedules
                .FirstOrDefaultAsync(s => s.RegulatoryReportTemplateId == templateId);
        }

        public async Task<IEnumerable<RegulatoryReportSchedule>> GetAllSchedulesAsync()
        {
            return await _context.RegulatoryReportSchedules
                .Include(s => s.ReportTemplate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegulatoryReportSchedule>> GetSchedulesDueByDateAsync(DateTime dueDate)
        {
            return await _context.RegulatoryReportSchedules
                .Where(s => s.NextGenerationDate.Value.Date <= dueDate.Date)
                .Include(s => s.ReportTemplate)
                .ToListAsync();
        }

        public async Task<bool> DeleteScheduleAsync(string id)
        {
            var schedule = await _context.RegulatoryReportSchedules.FindAsync(id);
            if (schedule == null)
                return false;

            _context.RegulatoryReportSchedules.Remove(schedule);
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}
