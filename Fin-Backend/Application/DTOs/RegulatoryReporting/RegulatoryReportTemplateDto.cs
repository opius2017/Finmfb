using System;
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
}
