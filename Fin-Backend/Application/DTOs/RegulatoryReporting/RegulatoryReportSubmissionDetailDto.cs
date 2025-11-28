using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportSubmissionDetailDto : RegulatoryReportSubmissionDto
    {
        public List<RegulatoryReportDataDto> ReportData { get; set; } = new List<RegulatoryReportDataDto>();
        public List<RegulatoryReportValidationDto> Validations { get; set; } = new List<RegulatoryReportValidationDto>();
    }
}
