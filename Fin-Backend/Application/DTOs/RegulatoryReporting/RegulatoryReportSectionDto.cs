using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
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
}
