using System;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
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
}
