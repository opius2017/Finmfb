using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    public class RegulatoryReportValidationRule : AuditableEntity
    {
        public string RegulatoryReportSectionId { get; set; } = string.Empty;
        
        [ForeignKey("RegulatoryReportSectionId")]
        public virtual RegulatoryReportSection? Section { get; set; }
        
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Expression { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public ValidationSeverity Severity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
