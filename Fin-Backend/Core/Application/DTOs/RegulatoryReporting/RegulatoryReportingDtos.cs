using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public string Description { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public string RegulatoryBodyName => RegulatoryBody.ToString();
        public ReportingFrequency Frequency { get; set; }
        public string FrequencyName => Frequency.ToString();
        public string FileFormat { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string SchemaVersion { get; set; }
        public int SectionCount { get; set; }
        public int FieldCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RegulatoryReportTemplateDetailDto : RegulatoryReportTemplateDto
    {
        public string TemplateStructure { get; set; }
        public List<RegulatoryReportSectionDto> Sections { get; set; } = new List<RegulatoryReportSectionDto>();
    }

    public class CreateRegulatoryReportTemplateDto
    {
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public string Description { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public ReportingFrequency Frequency { get; set; }
        public string FileFormat { get; set; }
        public bool IsActive { get; set; } = true;
        public string SchemaVersion { get; set; }
        public string TemplateStructure { get; set; }
    }

    public class UpdateRegulatoryReportTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public string Description { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public ReportingFrequency Frequency { get; set; }
        public string FileFormat { get; set; }
        public bool IsActive { get; set; }
        public string SchemaVersion { get; set; }
        public string TemplateStructure { get; set; }
    }

    public class RegulatoryReportSectionDto
    {
        public Guid Id { get; set; }
        public Guid ReportTemplateId { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
        public List<RegulatoryReportFieldDto> Fields { get; set; } = new List<RegulatoryReportFieldDto>();
    }

    public class CreateRegulatoryReportSectionDto
    {
        public Guid ReportTemplateId { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
    }

    public class UpdateRegulatoryReportSectionDto
    {
        public Guid Id { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
    }

    public class RegulatoryReportFieldDto
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
        public int DisplayOrder { get; set; }
        public string DefaultValue { get; set; }
        public string Formula { get; set; }
        public string MappingQuery { get; set; }
    }

    public class CreateRegulatoryReportFieldDto
    {
        public Guid SectionId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
        public int DisplayOrder { get; set; }
        public string DefaultValue { get; set; }
        public string Formula { get; set; }
        public string MappingQuery { get; set; }
    }

    public class UpdateRegulatoryReportFieldDto
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
        public int DisplayOrder { get; set; }
        public string DefaultValue { get; set; }
        public string Formula { get; set; }
        public string MappingQuery { get; set; }
    }

    public class RegulatoryReportSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid ReportTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public string RegulatoryBodyName => RegulatoryBody.ToString();
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public Guid SubmittedById { get; set; }
        public string SubmittedByName { get; set; }
        public Guid? ApprovedById { get; set; }
        public string ApprovedByName { get; set; }
        public SubmissionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string Comments { get; set; }
        public string SubmissionReference { get; set; }
        public string FilePath { get; set; }
        public int ValidationErrorCount { get; set; }
        public int ValidationWarningCount { get; set; }
    }

    public class RegulatoryReportSubmissionDetailDto : RegulatoryReportSubmissionDto
    {
        public List<RegulatoryReportDataDto> ReportData { get; set; } = new List<RegulatoryReportDataDto>();
        public List<RegulatoryReportValidationDto> Validations { get; set; } = new List<RegulatoryReportValidationDto>();
    }

    public class CreateRegulatoryReportSubmissionDto
    {
        public Guid ReportTemplateId { get; set; }
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
        public Guid SubmittedById { get; set; }
        public string Comments { get; set; }
    }

    public class UpdateRegulatoryReportSubmissionStatusDto
    {
        public Guid Id { get; set; }
        public SubmissionStatus Status { get; set; }
        public string Comments { get; set; }
    }

    public class ApproveRegulatoryReportSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid ApprovedById { get; set; }
        public string Comments { get; set; }
    }

    public class RegulatoryReportDataDto
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string SectionName { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public bool IsCalculated { get; set; }
        public string Comments { get; set; }
        public bool HasException { get; set; }
        public string ExceptionReason { get; set; }
    }

    public class UpdateRegulatoryReportDataDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public string Comments { get; set; }
        public bool HasException { get; set; }
        public string ExceptionReason { get; set; }
    }

    public class RegulatoryReportValidationDto
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid? FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string SeverityName => Severity.ToString();
        public bool IsResolved { get; set; }
        public string ResolutionComments { get; set; }
    }

    public class ResolveRegulatoryReportValidationDto
    {
        public Guid Id { get; set; }
        public bool IsResolved { get; set; }
        public string ResolutionComments { get; set; }
    }

    public class RegulatoryReportScheduleDto
    {
        public Guid Id { get; set; }
        public Guid ReportTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public string RegulatoryBodyName => RegulatoryBody.ToString();
        public ReportingFrequency Frequency { get; set; }
        public string FrequencyName => Frequency.ToString();
        public DateTime NextGenerationDate { get; set; }
        public DateTime NextSubmissionDeadline { get; set; }
        public bool IsAutoGenerate { get; set; }
        public bool IsAutoSubmit { get; set; }
        public string NotificationEmails { get; set; }
        public int ReminderDays { get; set; }
        public int DaysUntilDeadline => (int)(NextSubmissionDeadline - DateTime.UtcNow).TotalDays;
    }

    public class CreateRegulatoryReportScheduleDto
    {
        public Guid ReportTemplateId { get; set; }
        public DateTime NextGenerationDate { get; set; }
        public DateTime NextSubmissionDeadline { get; set; }
        public bool IsAutoGenerate { get; set; }
        public bool IsAutoSubmit { get; set; }
        public string NotificationEmails { get; set; }
        public int ReminderDays { get; set; }
    }

    public class UpdateRegulatoryReportScheduleDto
    {
        public Guid Id { get; set; }
        public DateTime NextGenerationDate { get; set; }
        public DateTime NextSubmissionDeadline { get; set; }
        public bool IsAutoGenerate { get; set; }
        public bool IsAutoSubmit { get; set; }
        public string NotificationEmails { get; set; }
        public int ReminderDays { get; set; }
    }

    public class RegulatoryReportFileDto
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileContent { get; set; }
    }

    public class GenerateReportRequestDto
    {
        public RegulatoryBody RegulatoryBody { get; set; }
        public string ReportType { get; set; }
        public DateTime ReportingPeriod { get; set; }
    }
}
