using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
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
}
