using System;
using System.Collections.Generic;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Users;
using FinTech.Domain.Entities.Identity;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents a submission of a regulatory report
    /// </summary>
    public class RegulatoryReportSubmission : AuditableEntity
    {
        /// <summary>
        /// Reference to the report template
        /// </summary>
        public int RegulatoryReportTemplateId { get; set; }
        
        /// <summary>
        /// Navigation property for the report template
        /// </summary>
        public virtual RegulatoryReportTemplate Template { get; set; }
        
        /// <summary>
        /// Reference number for the submission
        /// </summary>
        public string ReferenceNumber { get; set; }
        
        /// <summary>
        /// Status of the submission
        /// </summary>
        public SubmissionStatus Status { get; set; }
        
        /// <summary>
        /// Reporting period start date
        /// </summary>
        public DateTime ReportingPeriodStart { get; set; }
        
        /// <summary>
        /// Reporting period end date
        /// </summary>
        public DateTime ReportingPeriodEnd { get; set; }
        
        /// <summary>
        /// Due date for the submission
        /// </summary>
        public DateTime DueDate { get; set; }
        
        /// <summary>
        /// Date when the report was submitted
        /// </summary>
        public DateTime? SubmissionDate { get; set; }
        
        /// <summary>
        /// Date when the report was acknowledged by the regulatory body
        /// </summary>
        public DateTime? AcknowledgementDate { get; set; }
        
        /// <summary>
        /// Acknowledgement reference from the regulatory body
        /// </summary>
        public string AcknowledgementReference { get; set; }
        
        /// <summary>
        /// Comments from the regulatory body
        /// </summary>
        public string RegulatoryComments { get; set; }
        
        /// <summary>
        /// Internal comments about the submission
        /// </summary>
        public string InternalComments { get; set; }
        
        /// <summary>
        /// User who prepared the report
        /// </summary>
        public string PreparedById { get; set; }
        
        /// <summary>
        /// Navigation property for the user who prepared the report
        /// </summary>
        public virtual ApplicationUser PreparedBy { get; set; }
        
        /// <summary>
        /// User who reviewed the report
        /// </summary>
        public string ReviewedById { get; set; }
        
        /// <summary>
        /// Navigation property for the user who reviewed the report
        /// </summary>
        public virtual ApplicationUser ReviewedBy { get; set; }
        
        /// <summary>
        /// User who approved the report
        /// </summary>
        public string ApprovedById { get; set; }
        
        /// <summary>
        /// Navigation property for the user who approved the report
        /// </summary>
        public virtual ApplicationUser ApprovedBy { get; set; }
        
        /// <summary>
        /// User who submitted the report
        /// </summary>
        public string SubmittedById { get; set; }
        
        /// <summary>
        /// Navigation property for the user who submitted the report
        /// </summary>
        public virtual ApplicationUser SubmittedBy { get; set; }
        
        /// <summary>
        /// Collection of report data sections in this submission
        /// </summary>
        public virtual ICollection<RegulatoryReportData> ReportData { get; set; }
        
        /// <summary>
        /// Collection of validation results for this submission
        /// </summary>
        public virtual ICollection<RegulatoryReportValidation> ValidationResults { get; set; }
        
        /// <summary>
        /// Collection of file attachments for this submission
        /// </summary>
        public virtual ICollection<RegulatoryReportAttachment> Attachments { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RegulatoryReportSubmission()
        {
            ReportData = new HashSet<RegulatoryReportData>();
            ValidationResults = new HashSet<RegulatoryReportValidation>();
            Attachments = new HashSet<RegulatoryReportAttachment>();
        }
    }
}