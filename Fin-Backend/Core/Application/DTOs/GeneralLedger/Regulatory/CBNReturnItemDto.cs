namespace FinTech.Core.Application.DTOs.GeneralLedger.Regulatory
{
    public class CBNReturnItemDto
    {
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Notes { get; set; }
    }
}
