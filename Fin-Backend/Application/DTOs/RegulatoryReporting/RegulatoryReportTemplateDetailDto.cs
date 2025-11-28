using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportTemplateDetailDto : RegulatoryReportTemplateDto
    {
        public string TemplateStructure { get; set; }
        public List<RegulatoryReportSectionDto> Sections { get; set; } = new List<RegulatoryReportSectionDto>();
    }
}
