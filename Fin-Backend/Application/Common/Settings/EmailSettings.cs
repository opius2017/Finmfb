namespace FinTech.Core.Application.Common.Settings
{
    public class EmailSettings
    {
        /// <summary>
        /// SMTP server address
        /// </summary>
        public string SmtpServer { get; set; } = string.Empty;
        
        /// <summary>
        /// SMTP server port
        /// </summary>
        public int SmtpPort { get; set; } = 587;
        
        /// <summary>
        /// Enable SSL for SMTP connection
        /// </summary>
        public bool EnableSsl { get; set; } = true;
        
        /// <summary>
        /// SMTP username for authentication
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;
        
        /// <summary>
        /// SMTP password for authentication
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// Default sender email address
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Default sender display name
        /// </summary>
        public string FromName { get; set; } = string.Empty;
        
        /// <summary>
        /// Path to email templates directory
        /// </summary>
        public string TemplatesPath { get; set; } = "EmailTemplates";
    }
}