using System;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportValidationDto
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid? FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string SeverityName => Severity.ToString();
        public bool IsResolved { get; set; }
        public string ResolutionComments { get; set; }
    }
}
