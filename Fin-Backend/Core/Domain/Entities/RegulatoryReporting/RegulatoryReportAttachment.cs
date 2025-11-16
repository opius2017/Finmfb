using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents an attachment for a regulatory report submission
    /// </summary>
    public class RegulatoryReportAttachment : AuditableEntity
    {
        /// <summary>
        /// Reference to the report submission
        /// </summary>
        public int RegulatoryReportSubmissionId { get; set; }
        
        /// <summary>
        /// Navigation property for the report submission
        /// </summary>
        public virtual RegulatoryReportSubmission Submission { get; set; }
        
        /// <summary>
        /// Filename of the attachment
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// Original filename as uploaded
        /// </summary>
        public string OriginalFileName { get; set; }
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// MIME type of the file
        /// </summary>
        public string MimeType { get; set; }
        
        /// <summary>
        /// Type of attachment (e.g., supporting document, submission file, evidence)
        /// </summary>
        public string AttachmentType { get; set; }
        
        /// <summary>
        /// Description of the attachment
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Storage path or reference
        /// </summary>
        public string StoragePath { get; set; }
        
        /// <summary>
        /// Whether the attachment is submitted to the regulatory body
        /// </summary>
        public bool IsSubmittedToRegulator { get; set; }
        
        /// <summary>
        /// User who uploaded the attachment
        /// </summary>
        public string UploadedById { get; set; }
        
        /// <summary>
        /// Navigation property for the user who uploaded the attachment
        /// </summary>
        public virtual ApplicationUser UploadedBy { get; set; }
        
        /// <summary>
        /// Date and time when the attachment was uploaded
        /// </summary>
        public DateTime UploadTimestamp { get; set; }
    }
}
