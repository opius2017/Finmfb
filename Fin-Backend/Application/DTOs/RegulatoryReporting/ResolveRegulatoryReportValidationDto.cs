using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class ResolveRegulatoryReportValidationDto
    {
        public Guid Id { get; set; }
        public bool IsResolved { get; set; }
        public string ResolutionComments { get; set; }
    }
}
