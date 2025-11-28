using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class CreateRegulatoryReportFieldDto
    {
        public Guid SectionId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
        public int DisplayOrder { get; set; }
        public string DefaultValue { get; set; }
        public string Formula { get; set; }
        public string MappingQuery { get; set; }
    }
}
