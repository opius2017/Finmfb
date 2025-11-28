using System;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class UpdateRegulatoryReportSubmissionStatusDto
    {
        public Guid Id { get; set; }
        public SubmissionStatus Status { get; set; }
        public string Comments { get; set; }
    }
}
