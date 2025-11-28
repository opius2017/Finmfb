using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportDataDto
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string SectionName { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public bool IsCalculated { get; set; }
        public string Comments { get; set; }
        public bool HasException { get; set; }
        public string ExceptionReason { get; set; }
    }
}
