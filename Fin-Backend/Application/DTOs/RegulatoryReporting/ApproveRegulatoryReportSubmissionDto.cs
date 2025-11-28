using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class ApproveRegulatoryReportSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid ApprovedById { get; set; }
        public string Comments { get; set; }
    }
}
