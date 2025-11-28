using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class ApproveReportDto
    {
        public Guid ApproverId { get; set; }
        public string Comments { get; set; }
    }
}
