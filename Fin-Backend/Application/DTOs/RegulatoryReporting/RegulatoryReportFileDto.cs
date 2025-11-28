namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportFileDto
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileContent { get; set; }
    }
}
